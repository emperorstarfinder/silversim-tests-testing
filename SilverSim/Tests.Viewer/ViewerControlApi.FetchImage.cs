// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3 with
// the following clarification and special exception.

// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU Affero General Public License cover the whole
// combination.

// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

using SilverSim.Http.Client;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Image;
using System;
using System.ComponentModel;
using System.Threading;
using System.Web;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        #region texture_received event
        [TranslatedScriptEvent("texture_received")]
        public class TextureReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;

            [TranslatedScriptEventParameter(1)]
            public LSLKey TextureID;

            [TranslatedScriptEventParameter(2)]
            public int Success;

            [TranslatedScriptEventParameter(3)]
            public ByteArrayApi.ByteArray Data;
        }

        [APIExtension(ExtensionName, "texture_received")]
        [StateEventDelegate]
        public delegate void TextureReceived(
            [Description("Agent info")]
            ViewerAgentAccessor agent,
            [Description("Texture ID")]
            LSLKey textureID,
            [Description("Result")]
            int success,
            [Description("Data")]
            ByteArrayApi.ByteArray data);
        #endregion

        private sealed class ImageReceiveInfo
        {
            public ImageData FirstPacket;
            public readonly RwLockedDictionary<ushort, ImagePacket> Segments = new RwLockedDictionary<ushort, ImagePacket>();

            public bool IsComplete
            {
                get
                {
                    if (FirstPacket == null)
                    {
                        return false;
                    }
                    for(ushort i = 1; i < FirstPacket.Packets; ++i)
                    {
                        if(!Segments.ContainsKey(i))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            public byte[] Data
            {
                get
                {
                    if (FirstPacket == null)
                    {
                        return null;
                    }
                    int byteLength = FirstPacket.Data.Length;
                    ImagePacket packet;
                    for (ushort i = 1; i < FirstPacket.Packets; ++i)
                    {
                        if (!Segments.TryGetValue(i, out packet))
                        {
                            return null;
                        }
                        byteLength += packet.Data.Length;
                    }

                    byte[] resdata = new byte[byteLength];
                    Buffer.BlockCopy(FirstPacket.Data, 0, resdata, 0, FirstPacket.Data.Length);
                    byteLength = FirstPacket.Data.Length;

                    for (ushort i = 1; i < FirstPacket.Packets; ++i)
                    {
                        if (!Segments.TryGetValue(i, out packet))
                        {
                            return null;
                        }
                        Buffer.BlockCopy(packet.Data, 0, resdata, byteLength, packet.Data.Length);
                        byteLength += packet.Data.Length;
                    }

                    return resdata;
                }
            }
        }
        private readonly RwLockedDictionary<UUID, ImageReceiveInfo> m_ActiveImageTransfers = new RwLockedDictionary<UUID, ImageReceiveInfo>();

        private void HandleImageNotInDatabase(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            ImageNotInDatabase res = (ImageNotInDatabase)m;
            m_ActiveImageTransfers.Remove(res.ID);
            vc.PostEvent(new TextureReceivedEvent
            {
                Agent = agent,
                TextureID = res.ID,
                Success = 0,
                Data = new ByteArrayApi.ByteArray()
            });
        }

        private void HandleImageData(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            ImageData res = (ImageData)m;
            ImageReceiveInfo info;
            if(m_ActiveImageTransfers.TryGetValue(res.ID, out info))
            {
                info.FirstPacket = res;
                if(info.IsComplete)
                {
                    vc.PostEvent(new TextureReceivedEvent
                    {
                        Agent = agent,
                        TextureID = res.ID,
                        Success = 1,
                        Data = new ByteArrayApi.ByteArray(info.Data)
                    });
                    m_ActiveImageTransfers.Remove(res.ID);
                }
            }
        }

        private void HandleImagePacket(Message m, ViewerConnection vc, ViewerAgentAccessor agent)
        {
            ImagePacket res = (ImagePacket)m;
            ImageReceiveInfo info;
            if (m_ActiveImageTransfers.TryGetValue(res.ID, out info))
            {
                info.Segments[res.Packet] = res;
                if (info.IsComplete)
                {
                    vc.PostEvent(new TextureReceivedEvent
                    {
                        Agent = agent,
                        TextureID = res.ID,
                        Success = 1,
                        Data = new ByteArrayApi.ByteArray(info.Data)
                    });
                    m_ActiveImageTransfers.Remove(res.ID);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int RequestTextureViaCircuit(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey textureID,
            int type)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    !m_ActiveImageTransfers.ContainsKey(textureID.AsUUID))
                {
                    m_ActiveImageTransfers[textureID.AsUUID] = new ImageReceiveInfo();
                    var reqImage = new RequestImage
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    reqImage.RequestImageList.Add(new RequestImage.RequestImageEntry
                    {
                        DiscardLevel = 1,
                        ImageID = textureID.AsUUID,
                        Type = (RequestImage.ImageType)type,
                        Packet = 0,
                        DownloadPriority = 1
                    });

                    viewerCircuit.SendMessage(reqImage);

                    return 1;
                }
                return 0;
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int AbortTextureViaCircuit(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey textureID,
            int type)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    m_ActiveImageTransfers.ContainsKey(textureID.AsUUID))
                {
                    m_ActiveImageTransfers.Remove(textureID.AsUUID);
                    var reqImage = new RequestImage
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    reqImage.RequestImageList.Add(new RequestImage.RequestImageEntry
                    {
                        DiscardLevel = -1,
                        ImageID = textureID.AsUUID,
                        Type = (RequestImage.ImageType)type,
                        Packet = 0,
                        DownloadPriority = 0
                    });

                    viewerCircuit.SendMessage(reqImage);

                    return 1;
                }
                return 0;
            }
        }

        private sealed class UrlDownloadInfoRequest
        {
            public ViewerConnection ViewerConn;
            public ViewerAgentAccessor Agent;
            public UUID TextureID;
            public string TextureUrl;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void RequestTextureViaCap(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string capsUrl,
            LSLKey textureID)
        {
            lock(instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    ThreadPool.QueueUserWorkItem(RequestTextureHandler, new UrlDownloadInfoRequest
                    {
                        ViewerConn = vc,
                        Agent = agent,
                        TextureID = textureID.AsUUID,
                        TextureUrl = capsUrl
                    });
                }
            }
        }

        private void RequestTextureHandler(object o)
        {
            UrlDownloadInfoRequest reqInfo = (UrlDownloadInfoRequest)o;
            try
            {
                try
                {
                    byte[] data = new HttpClient.Get($"{reqInfo.TextureUrl}?texture_id={reqInfo.TextureID}")
                    {
                        TimeoutMs = 20000
                    }.ExecuteBinaryRequest();

                    reqInfo.ViewerConn.PostEvent(new TextureReceivedEvent
                    {
                        Agent = reqInfo.Agent,
                        TextureID = reqInfo.TextureID,
                        Success = 1,
                        Data = new ByteArrayApi.ByteArray(data)
                    });
                }
                catch (HttpException)
                {
                    reqInfo.ViewerConn.PostEvent(new TextureReceivedEvent
                    {
                        Agent = reqInfo.Agent,
                        TextureID = reqInfo.TextureID,
                        Success = 0,
                        Data = new ByteArrayApi.ByteArray()
                    });
                }
            }
            catch
            {
                /* intentionally ignore */
            }
        }
    }
}
