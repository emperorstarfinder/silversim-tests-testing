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

using SilverSim.Scene.Types.Agent;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Account;
using SilverSim.Types.Grid;
using SilverSim.Types.Presence;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Circuit;
using SilverSim.Viewer.Messages.Telehub;
using SilverSim.Viewer.Messages.Teleport;
using System;
using System.Net;
using System.Net.Sockets;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_SETHOMETOTARGET = 1;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_SETLASTTOTARGET = 2;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIALURE = 4;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIALANDMARK = 8;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIALOCATION = 16;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIAHOME = 32;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIATELEHUB = 64;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIALOGIN = 128;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIAGODLIKELURE = 256;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_GODLIKE = 512;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_NINEONEONE = 1024;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_DISABLECANCEL = 2048;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIAREGIONID = 4096;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_ISFLYING = 8192;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_RESETHOME = 16384;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_FORCEREDIRECT = 32768;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_FINISHEDVIALURE = 67108864;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_FINISHEDVIANEWSIM = 268435456;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_FINISHEDVIASAMESIM = 536870912;
        [APIExtension("ViewerControl")]
        public const int TELEPORT_FLAGS_VIAHGLOGIN = 1073741824;

        [APIExtension("ViewerControl", "vcCreateAccount")]
        public LSLKey CreateAccount(
            ScriptInstance instance,
            string firstName,
            string lastName)
        {
            lock(instance)
            {
                var acc = new UserAccount
                {
                    Principal = new UGUIWithName { ID = UUID.Random, FirstName = firstName, LastName = lastName }
                };
                m_UserAccountService.Add(acc);
                return acc.Principal.ID;
            }
        }

        [APIExtension("ViewerControl", "vcSetAgentLimit")]
        public void SetAgentLimit(
            ScriptInstance instance,
            int agentlimit)
        {
            lock(instance)
            {
                SceneInterface scene = instance.Part.ObjectGroup.Scene;
                scene.RegionSettings.AgentLimit = agentlimit;
                scene.TriggerRegionSettingsChanged();
            }
        }

        [APIExtension("ViewerControl", "vieweragent")]
        [APIDisplayName("vieweragent")]
        [APIAccessibleMembers]
        [Serializable]
        public class ViewerAgentAccessor
        {
            public UUID AgentID { get; }
            public uint CircuitCode { get; }
            public string CapsPath { get; }

            public ViewerAgentAccessor()
            {
            }

            public ViewerAgentAccessor(UUID agentID, uint circuitCode, string capsPath)
            {
                AgentID = agentID;
                CircuitCode = circuitCode;
                CapsPath = capsPath;
            }

            public static implicit operator bool(ViewerAgentAccessor vc) => vc.AgentID != UUID.Zero;
        }

        [APIExtension("ViewerControl", "vcLoginAgent")]
        public ViewerAgentAccessor LoginAgent(
            ScriptInstance instance,
            int circuitCode,
            LSLKey regionId,
            LSLKey agentId,
            LSLKey sessionId,
            LSLKey secureSessionId,
            string serviceSessionId,
            string viewerChannel,
            string viewerVersion,
            string id0,
            string mac,
            int teleportFlags,
            Vector3 position,
            Vector3 lookAt)
        {
            lock (instance)
            {
                string externalHostName = m_CapsRedirector.ExternalHostName;
                IPAddress[] addresses = DnsNameCache.GetHostAddresses(externalHostName);

                string clientIP = string.Empty;

                if (addresses.Length == 0)
                {
                    m_Log.InfoFormat("ExternalHostName \"{0}\" does not resolve", externalHostName);
                    return new ViewerAgentAccessor();
                }

                foreach (IPAddress addr in addresses)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        clientIP = addr.ToString();
                    }
                }

                if (string.IsNullOrEmpty(clientIP))
                {
                    m_Log.InfoFormat("ExternalHostName \"{0}\" does not resolve", externalHostName);
                    return new ViewerAgentAccessor();
                }

                UUID capsId = UUID.Random;
                string capsPath = m_CapsRedirector.ServerURI + "CAPS/" + capsId.ToString() + "0000/";
                SceneInterface scene;
                if (!m_Scenes.TryGetValue(regionId, out scene))
                {
                    return new ViewerAgentAccessor();
                }
                var clientInfo = new ClientInfo
                {
                    ClientIP = clientIP,
                    Channel = viewerChannel,
                    ClientVersion = viewerVersion,
                    ID0 = id0,
                    Mac = mac
                };
                UserAccount userAccount;
                if (!m_UserAccountService.TryGetValue(UUID.Zero, agentId, out userAccount))
                {
                    m_Log.InfoFormat("User account {0} does not exist", agentId.ToString());
                    return new ViewerAgentAccessor();
                }

                var presenceInfo = new PresenceInfo
                {
                    RegionID = regionId,
                    SecureSessionID = secureSessionId,
                    SessionID = sessionId,
                    UserID = userAccount.Principal
                };
                var serviceList = new AgentServiceList
                {
                    m_AgentAssetService,
                    m_AgentInventoryService,
                    m_AgentFriendsService,
                    m_AgentUserAgentService,
                    m_PresenceService,
                    m_GridUserService,
                    m_GridService
                };
                if (m_AgentProfileService != null)
                {
                    serviceList.Add(m_AgentProfileService);
                }
                if (m_OfflineIMService != null)
                {
                    serviceList.Add(m_OfflineIMService);
                }

                var agent = new ViewerAgent(
                    m_Scenes,
                    agentId,
                    userAccount.Principal.FirstName,
                    userAccount.Principal.LastName,
                    userAccount.Principal.HomeURI,
                    sessionId.AsUUID,
                    secureSessionId.AsUUID,
                    serviceSessionId,
                    clientInfo,
                    userAccount,
                    serviceList);

                try
                {
                    scene.DetermineInitialAgentLocation(agent, (TeleportFlags)teleportFlags, position, lookAt);
                }
                catch (Exception e)
                {
                    m_Log.InfoFormat("Failed to determine initial location for agent {0}: {1}: {2}", userAccount.Principal.FullName, e.GetType().FullName, e.Message);
                    return new ViewerAgentAccessor();
                }

                var udpServer = (UDPCircuitsManager)scene.UDPServer;

                IPAddress ipAddr;
                if (!IPAddress.TryParse(clientInfo.ClientIP, out ipAddr))
                {
                    m_Log.InfoFormat("Invalid IP address for agent {0}", userAccount.Principal.FullName);
                    return new ViewerAgentAccessor();
                }

                ViewerConnection vc = AddAgent(instance, userAccount.Principal.ID);

                var ep = new IPEndPoint(ipAddr, vc.ClientUDP.LocalPort);
                var regionEndPoint = new IPEndPoint(ipAddr, (int)scene.RegionPort);
                var circuit = new AgentCircuit(
                    m_Commands,
                    agent,
                    udpServer,
                    (uint)circuitCode,
                    m_CapsRedirector,
                    capsId,
                    agent.ServiceURLs,
                    string.Empty,
                    m_PacketHandlerPlugins,
                    ep)
                {
                    LastTeleportFlags = (TeleportFlags)teleportFlags,
                    Agent = agent,
                    AgentID = userAccount.Principal.ID,
                    SessionID = sessionId.AsUUID,
                    ForceUseCircuitCode = true
                };
                agent.Circuits.Add(circuit.Scene.ID, circuit);

                try
                {
                    scene.Add(agent);
                    try
                    {
                        udpServer.AddCircuit(circuit);
                    }
                    catch
                    {
                        scene.Remove(agent);
                        throw;
                    }
                }
                catch (Exception e)
                {
                    m_Log.Debug("Failed agent post", e);
                    agent.Circuits.Clear();
                    return new ViewerAgentAccessor();
                }
                /* make agent a root agent */
                agent.SceneID = scene.ID;
                if (null != m_GridUserService)
                {
                    try
                    {
                        m_GridUserService.SetPosition(agent.Owner, scene.ID, agent.GlobalPosition, agent.LookAt);
                    }
                    catch (Exception e)
                    {
                        m_Log.Warn("Could not contact GridUserService", e);
                    }
                }

                try
                {
                    m_PresenceService.Report(new PresenceInfo
                    {
                        UserID = agent.Owner,
                        SessionID = agent.SessionID,
                        SecureSessionID = secureSessionId.AsUUID,
                        RegionID = scene.ID
                    });
                }
                catch (Exception e)
                {
                    m_Log.Warn("Could not contact PresenceService", e);
                }
                circuit.LogIncomingAgent(m_Log, false);
                var useCircuit = new UseCircuitCode
                {
                    AgentID = agentId,
                    SessionID = sessionId.AsUUID,
                    CircuitCode = (uint)circuitCode
                };

                var viewerCircuit = new ViewerCircuit(vc.ClientUDP, (uint)circuitCode, sessionId.AsUUID, agentId, regionEndPoint);
                vc.ClientUDP.AddCircuit(viewerCircuit);
                viewerCircuit.Start();
                viewerCircuit.MessageRouting.Add(MessageType.RegionHandshake, (m) => HandleLogoutReply(m, (uint)circuitCode, vc));
                viewerCircuit.SendMessage(useCircuit);
                viewerCircuit.MessageRouting.Add(MessageType.LogoutReply, (m) => HandleLogoutReply(m, (uint)circuitCode, vc));
                viewerCircuit.MessageRouting.Add(MessageType.TelehubInfo, (m) => TelehubInfoReceivedEvent.ToScriptEvent((TelehubInfo)m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportLocal, (m) => TeleportLocalReceivedEvent.ToScriptEvent((TeleportLocal)m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.EconomyData, (m) => EconomyDataReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportProgress, (m) => TeleportProgressReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportStart, (m) => TeleportStartReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportFailed, (m) => TeleportFailedReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AlertMessage, (m) => AlertMessageReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AgentDataUpdate, (m) => AgentDataUpdateReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AgentDropGroup, (m) => AgentDropGroupReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.CoarseLocationUpdate, (m) => CoarseLocationUpdateReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.HealthMessage, (m) => HealthMessageReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AvatarAnimation, (m) => AvatarAnimationReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AvatarSitResponse, (m) => AvatarSitResponseReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.CameraConstraint, (m) => CameraConstraintReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ClearFollowCamProperties, (m) => ClearFollowCamPropertiesReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.SetFollowCamProperties, (m) => SetFollowCamPropertiesReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ChatFromSimulator, (m) => ChatFromSimulatorReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.EstateCovenantReply, (m) => EstateCovenantReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.LoadURL, (m) => LoadURLReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptTeleportRequest, (m) => ScriptTeleportRequestReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptQuestion, (m) => ScriptQuestionReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptDialog, (m) => ScriptDialogReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptControlChange, (m) => ScriptControlChangeReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.PreloadSound, (m) => PreloadSoundReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AttachedSound, (m) => AttachedSoundReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.SoundTrigger, (m) => SoundTriggerReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AttachedSoundGainChange, (m) => AttachedSoundGainChangeReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.FeatureDisabled, (m) => FeatureDisabledReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.PayPriceReply, (m) => PayPriceReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                vc.ViewerCircuits.Add((uint)circuitCode, viewerCircuit);
                return new ViewerAgentAccessor(agent.ID, (uint)circuitCode, capsPath);
            }
        }

        private void HandleLogoutReply(Message m, uint circuitCode, ViewerConnection vc)
        {
            ViewerCircuit removeCircuit;
            if (vc.ViewerCircuits.TryGetValue(circuitCode, out removeCircuit))
            {
                vc.PostEvent(new LogoutReplyReceivedEvent(m, removeCircuit.CircuitCode));
                removeCircuit.Stop();
                vc.ClientUDP.RemoveCircuit(removeCircuit);
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "Logout")]
        public void LogoutAgent(ScriptInstance instance, ViewerAgentAccessor agent)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var logoutreq = new LogoutRequest
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    viewerCircuit.SendMessage(logoutreq);
                }
            }
        }
    }
}
