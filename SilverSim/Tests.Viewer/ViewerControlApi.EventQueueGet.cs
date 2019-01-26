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
using SilverSim.Scene.Management.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.StructuredData.Llsd;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Circuit;
using SilverSim.Viewer.Messages.Console;
using SilverSim.Viewer.Messages.Object;
using SilverSim.Viewer.Messages.Parcel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        private static EQGDecoder m_EQGDecoder = new EQGDecoder(true);

        private class EventQueueGet
        {
            private readonly UUID m_ReqID;
            private readonly string m_Url;
            private readonly int m_TimeoutMs;
            private readonly SceneList m_Scenes;
            private readonly UUID m_SceneID;
            private readonly UUID m_PartID;
            private readonly UUID m_ItemID;
            private readonly uint m_CircuitCode;
            private readonly ViewerAgentAccessor m_ViewerAgent;
            private readonly ViewerConnection m_ViewerConnection;
            private Dictionary<MessageType, Action<Message>> m_MessageRouting;

            public EventQueueGet(
                UUID reqid,
                string url,
                uint circuitCode,
                int timeoutms,
                SceneList sceneList,
                UUID sceneID,
                UUID partID,
                UUID itemID,
                ViewerAgentAccessor agent,
                ViewerConnection vc,
                Dictionary<MessageType, Action<Message>> messageRouting)
            {
                m_ReqID = reqid;
                m_Url = url;
                m_CircuitCode = circuitCode;
                m_TimeoutMs = timeoutms;
                m_Scenes = sceneList;
                m_SceneID = sceneID;
                m_PartID = partID;
                m_ItemID = itemID;
                m_ViewerAgent = agent;
                m_ViewerConnection = vc;
                m_MessageRouting = messageRouting;
            }

            public void HandleRequest(object unused)
            {
                try
                {
                    var reqmap = new Map
                {
                    { "ack", new Undef() },
                    { "done", false }
                };
                    HttpStatusCode statusCode;

                    using (Stream s = new HttpClient.Post(m_Url, "application/llsd+xml", (Stream o) => LlsdXml.Serialize(reqmap, o))
                    {
                        TimeoutMs = m_TimeoutMs,
                        DisableExceptions = HttpClient.Request.DisableExceptionFlags.Disable5XX
                    }.ExecuteStreamRequest(out statusCode))
                    {
                        if (statusCode == HttpStatusCode.OK)
                        {
                            var resmap = (Map)LlsdXml.Deserialize(s);
                            foreach(Map evmap in ((AnArray)resmap["events"]).OfType<Map>())
                            {
                                IValue body;
                                string msgtype;
                                Func<IValue, Message> del;
                                if(evmap.TryGetValue("message", out msgtype) &&
                                    evmap.TryGetValue("body", out body) &&
                                    m_EQGDecoder.EQGDecoders.TryGetValue(msgtype, out del))
                                {
                                    Message m;
                                    try
                                    {
                                        m = del(body);
                                    }
                                    catch
                                    {
                                        continue;
                                    }

                                    Action<Message> handle;
                                    if(msgtype == "ObjectPhysicsProperties")
                                    {
                                        var msg = (ObjectPhysicsProperties)m;
                                        var ev = new ObjectPhysicsPropertiesReceivedEvent
                                        {
                                            Agent = m_ViewerAgent
                                        };
                                        foreach(ObjectPhysicsProperties.ObjectDataEntry d in msg.ObjectData)
                                        {
                                            ev.ObjectData.Add(new ObjectPhysicsPropertiesData
                                            {
                                                LocalID = (int)d.LocalID,
                                                PhysicsShapeType = (int)d.PhysicsShapeType,
                                                Density = d.Density,
                                                Friction = d.Friction,
                                                Restitution = d.Restitution,
                                                GravityMultiplier = d.GravityMultiplier
                                            });
                                        }
                                    }
                                    else if(msgtype == "EstablishAgentCommunication")
                                    {
                                        var msg = (EstablishAgentCommunication)m;
                                        m_ViewerConnection.PostEvent(new EstablishAgentCommunicationReceivedEvent
                                        {
                                            Agent = m_ViewerAgent,
                                            SimIpAndPort = msg.SimIpAndPort.ToString(),
                                            SeedCapability = msg.SeedCapability,
                                            GridPositionX = (int)msg.GridPosition.X,
                                            GridPositionY = (int)msg.GridPosition.Y,
                                            RegionSizeX = (int)msg.RegionSize.X,
                                            RegionSizeY = (int)msg.RegionSize.Y
                                        });
                                    }
                                    else if(msgtype == "ParcelVoiceInfo")
                                    {
                                        var msg = (ParcelVoiceInfo)m;
                                        m_ViewerConnection.PostEvent(new ParcelVoiceInfoReceivedEvent
                                        {
                                            Agent = m_ViewerAgent,
                                            ParcelLocalId = msg.ParcelLocalId,
                                            ChannelUri = msg.ChannelUri,
                                            ChannelCredentials = msg.ChannelCredentials
                                        });
                                    }
                                    else if(msgtype == "SimConsoleResponse")
                                    {
                                        var msg = (SimConsoleResponse)m;
                                        m_ViewerConnection.PostEvent(new SimConsoleResponseReceivedEvent
                                        {
                                            Agent = m_ViewerAgent,
                                            Response = msg.Message
                                        });
                                    }
                                    else if(m_MessageRouting.TryGetValue(m.Number, out handle))
                                    {
                                        handle(m);
                                    }
                                }
                            }
                        }
                        m_ViewerConnection.PostEvent(new EventQueueGetFinishedEvent(
                            m_ViewerAgent,
                            m_ReqID,
                            (int)statusCode));
                    }
                }
                catch
                {
                    m_ViewerConnection.PostEvent(new EventQueueGetFinishedEvent(
                        m_ViewerAgent,
                        m_ReqID,
                        499));
                }
            }
        }

        [TranslatedScriptEvent("simconsoleresponse_received")]
        public class SimConsoleResponseReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public string Response;
        }

        [APIExtension(ExtensionName, "simconsoleresponse_received")]
        [StateEventDelegate]
        public delegate void SimConsoleResponseReceived(
            ViewerAgentAccessor agent,
            string response);

        [TranslatedScriptEvent("parcelvoiceinfo_received")]
        public class ParcelVoiceInfoReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public int ParcelLocalId;
            [TranslatedScriptEventParameter(2)]
            public string ChannelUri;
            [TranslatedScriptEventParameter(3)]
            public string ChannelCredentials;
        }

        [APIExtension(ExtensionName, "parcelvoiceinfo_received")]
        [StateEventDelegate]
        public delegate void ParcelVoiceInfoReceived(
            ViewerAgentAccessor agent,
            int parcelLocalId,
            string channelUri,
            string channelCredentials);

        [APIExtension(ExtensionName)]
        [APIDisplayName("objectphysicsproperties")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class ObjectPhysicsPropertiesData
        {
            public int LocalID;
            public int PhysicsShapeType;
            public double Density;
            public double Friction;
            public double Restitution;
            public double GravityMultiplier;
        }

        [APIExtension(ExtensionName)]
        [APIDisplayName("objectphysicspropertieslist")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class ObjectPhysicsPropertiesDataList : List<ObjectPhysicsPropertiesData>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<ObjectPhysicsPropertiesData>
            {
                private readonly ObjectPhysicsPropertiesDataList Src;
                private int Position = -1;

                public LSLEnumerator(ObjectPhysicsPropertiesDataList src)
                {
                    Src = src;
                }

                public ObjectPhysicsPropertiesData Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }

        [TranslatedScriptEvent("objectphysicsproperties_received")]
        public class ObjectPhysicsPropertiesReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public ObjectPhysicsPropertiesDataList ObjectData = new ObjectPhysicsPropertiesDataList();
        }

        [APIExtension(ExtensionName, "objectphysicsproperties_received")]
        [StateEventDelegate]
        public delegate void ObjectPhysicsPropertiesReceived(
            ViewerAgentAccessor agent,
            ObjectPhysicsPropertiesDataList objectData);

        [TranslatedScriptEvent("establishagentcommunication_received")]
        public class EstablishAgentCommunicationReceivedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public string SimIpAndPort;
            [TranslatedScriptEventParameter(2)]
            public string SeedCapability;
            [TranslatedScriptEventParameter(3)]
            public int GridPositionX;
            [TranslatedScriptEventParameter(4)]
            public int GridPositionY;
            [TranslatedScriptEventParameter(5)]
            public int RegionSizeX;
            [TranslatedScriptEventParameter(6)]
            public int RegionSizeY;
        }

        [APIExtension(ExtensionName, "establishagentcommunication_received")]
        [StateEventDelegate]
        public delegate void EstablishAgentCommunicationRceived(
            ViewerAgentAccessor agent,
            string simipandport,
            string seedcapability,
            int gridPositionX,
            int gridPositionY,
            int regionSizeX,
            int regionSizeY);

        [TranslatedScriptEvent("eventqueueget_finished")]
        public class EventQueueGetFinishedEvent : IScriptEvent
        {
            [TranslatedScriptEventParameter(0)]
            public ViewerAgentAccessor Agent;
            [TranslatedScriptEventParameter(1)]
            public LSLKey RequestID;
            [TranslatedScriptEventParameter(2)]
            public int StatusCode;

            public EventQueueGetFinishedEvent(ViewerAgentAccessor agentInfo, UUID requestID, int statusCode)
            {
                Agent = agentInfo;
                RequestID = requestID;
                StatusCode = statusCode;
            }
        }

        [APIExtension(ExtensionName, "eventqueueget_finished")]
        [StateEventDelegate]
        public delegate void EventQueueGetFinished(
            ViewerAgentAccessor agent,
            LSLKey requestID,
            int httpStatusCode);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "RequestEventQueueGet")]
        public LSLKey SendEventQueueGet(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            string url,
            int timeoutms)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                UUID reqID = UUID.Zero;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    reqID = UUID.Random;
                    ThreadPool.QueueUserWorkItem(new EventQueueGet(
                        reqID,
                        url,
                        viewerCircuit.CircuitCode,
                        timeoutms,
                        m_Scenes,
                        instance.Part.ObjectGroup.Scene.ID,
                        instance.Part.ID,
                        instance.Item.ID,
                        agent,
                        vc,
                        viewerCircuit.MessageRouting).HandleRequest);
                }
                return reqID;
            }
        }
    }
}
