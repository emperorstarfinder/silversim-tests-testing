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
using SilverSim.ServiceInterfaces.UserSession;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Account;
using SilverSim.Types.Agent;
using SilverSim.Types.Grid;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Circuit;
using SilverSim.Viewer.Messages.God;
using SilverSim.Viewer.Messages.Telehub;
using SilverSim.Viewer.Messages.Teleport;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_SETHOMETOTARGET = 1;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_SETLASTTOTARGET = 2;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIALURE = 4;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIALANDMARK = 8;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIALOCATION = 16;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIAHOME = 32;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIATELEHUB = 64;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIALOGIN = 128;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIAGODLIKELURE = 256;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_GODLIKE = 512;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_NINEONEONE = 1024;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_DISABLECANCEL = 2048;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIAREGIONID = 4096;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_ISFLYING = 8192;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_RESETHOME = 16384;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_FORCEREDIRECT = 32768;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_FINISHEDVIALURE = 67108864;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_FINISHEDVIANEWSIM = 268435456;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_FINISHEDVIASAMESIM = 536870912;
        [APIExtension(ExtensionName)]
        public const int TELEPORT_FLAGS_VIAHGLOGIN = 1073741824;

        [APIExtension(ExtensionName, "vcCreateAccount")]
        public LSLKey CreateAccount(
            ScriptInstance instance,
            string firstName,
            string lastName)
        {
            lock(instance)
            {
                var acc = new UserAccount
                {
                    Principal = new UGUIWithName { ID = UUID.Random, FirstName = firstName, LastName = lastName, HomeURI = new Uri(HomeURI) }
                };
                m_UserAccountService.Add(acc);
                return acc.Principal.ID;
            }
        }

        [APIExtension(ExtensionName, "vcSetAgentLimit")]
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

        [APIExtension(ExtensionName, "vieweragent")]
        [APIDisplayName("vieweragent")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        [Serializable]
        [ImplementsCustomTypecasts]
        public class ViewerAgentAccessor
        {
            public LSLKey AgentID { get; }
            public int CircuitCode { get; }
            public string CapsPath { get; }
            public LSLKey SessionId { get; }
            public LSLKey SecureSessionId { get; }

            public ViewerAgentAccessor()
            {
                AgentID = new LSLKey();
                SessionId = new LSLKey();
                SecureSessionId = new LSLKey();
            }

            public ViewerAgentAccessor(UUID agentID, int circuitCode, string capsPath, LSLKey sessionId, LSLKey secureSessionId)
            {
                AgentID = agentID;
                CircuitCode = circuitCode;
                CapsPath = capsPath;
                SessionId = sessionId;
                SecureSessionId = secureSessionId;
            }

            [APIExtension("ViewerControl", "vieweragent")]
            public static implicit operator bool(ViewerAgentAccessor vc) => (bool)vc.AgentID;
        }

        [APIExtension(ExtensionName, "vcLoginAgent")]
        public ViewerAgentAccessor LoginAgent(
            ScriptInstance instance,
            int circuitCode,
            LSLKey regionId,
            LSLKey agentId,
            LSLKey sessionId,
            LSLKey secureSessionId,
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
                if (!m_UserAccountService.TryGetValue(agentId, out userAccount))
                {
                    m_Log.InfoFormat("User account {0} does not exist", agentId.ToString());
                    return new ViewerAgentAccessor();
                }

                var presenceInfo = m_UserSessionService.CreateSession(userAccount.Principal, clientIP, sessionId.AsUUID, secureSessionId.AsUUID);
                var serviceList = new AgentServiceList
                {
                    m_AgentAssetService,
                    m_AgentInventoryService,
                    m_AgentFriendsService,
                    m_AgentUserAgentService,
                    new StandalonePresenceService(m_UserAccountService, userAccount.Principal, m_UserSessionService, presenceInfo.SessionID, new List<IUserSessionStatusHandler>()),
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
                if(m_AgentExperienceService != null)
                {
                    serviceList.Add(m_AgentExperienceService);
                }
                if(m_AgentGroupsService != null)
                {
                    serviceList.Add(m_AgentGroupsService);
                }

                var agent = new ViewerAgent(
                    m_Scenes,
                    agentId,
                    userAccount.Principal.FirstName,
                    userAccount.Principal.LastName,
                    userAccount.Principal.HomeURI,
                    presenceInfo.SessionID,
                    presenceInfo.SecureSessionID,
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
                try
                {
                    agent.UserAgentService.SetLastRegion(agent.Owner, new UserRegionData
                    {
                        RegionID = scene.ID,
                        Position = agent.GlobalPosition,
                        LookAt = agent.LookAt,
                        GatekeeperURI = new URI(scene.GatekeeperURI)
                    });
                }
                catch (Exception e)
                {
                    m_Log.Warn("Could not contact UserAgentService", e);
                }

                try
                {
                    m_UserAccountService.SetPosition(agent.Owner.ID, new UserRegionData
                    {
                        RegionID = scene.ID,
                        Position = agent.GlobalPosition,
                        LookAt = agent.LookAt,
                        GatekeeperURI = new URI(scene.GatekeeperURI)
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
                viewerCircuit.MessageRouting.Add(MessageType.AgentDataUpdate, (m) => AgentDataUpdateReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AgentDropGroup, (m) => AgentDropGroupReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AgentMovementComplete, (m) => AgentMovementCompleteReceivedEvent.HandleAgentMovementComplete(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AlertMessage, (m) => AlertMessageReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AttachedSound, (m) => AttachedSoundReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AttachedSoundGainChange, (m) => AttachedSoundGainChangeReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AvatarAnimation, (m) => AvatarAnimationReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.AvatarSitResponse, (m) => AvatarSitResponseReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.CameraConstraint, (m) => CameraConstraintReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ChatFromSimulator, (m) => ChatFromSimulatorReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ClearFollowCamProperties, (m) => ClearFollowCamPropertiesReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.CoarseLocationUpdate, (m) => CoarseLocationUpdateReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.DeRezAck, (m) => DeRezAckReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.EconomyData, (m) => EconomyDataReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.EstateCovenantReply, (m) => EstateCovenantReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.FeatureDisabled, (m) => FeatureDisabledReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ForceObjectSelect, (m) => ForceObjectSelectReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.GrantGodlikePowers, (m) => GrantGodlikePowersReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.HealthMessage, (m) => HealthMessageReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ImageData, (m) => HandleImageData(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ImageNotInDatabase, (m) => HandleImageNotInDatabase(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ImagePacket, (m) => HandleImagePacket(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ImprovedInstantMessage, (m) => ImprovedInstantMessageReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.KillObject, (m) => KillObjectReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.LoadURL, (m) => LoadURLReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.LogoutReply, (m) => HandleLogoutReply(m, (uint)circuitCode, vc));
                viewerCircuit.MessageRouting.Add(MessageType.MoneyBalanceReply, (m) => MoneyBalanceReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ObjectAnimation, (m) => ObjectAnimationReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ObjectUpdate, (m) => ObjectUpdateReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ObjectUpdateCompressed, (m) => ObjectUpdateReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.OfflineNotification, (m) => OfflineNotificationReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.OnlineNotification, (m) => OnlineNotificationReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ParcelInfoReply, (m) => ParcelInfoReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ParcelObjectOwnersReply, (m) => ParcelObjectOwnersReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.PayPriceReply, (m) => PayPriceReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.PreloadSound, (m) => PreloadSoundReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.RegionHandshake, (m) => RegionHandshakeReceivedEvent.HandleRegionHandshake(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.RegionInfo, (m) => RegionInfoReceivedEvent.HandleRegionInfo(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptControlChange, (m) => ScriptControlChangeReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptDialog, (m) => ScriptDialogReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptQuestion, (m) => ScriptQuestionReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptRunningReply, (m) => ScriptRunningReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.ScriptTeleportRequest, (m) => ScriptTeleportRequestReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.SetFollowCamProperties, (m) => SetFollowCamPropertiesReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.SoundTrigger, (m) => SoundTriggerReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TelehubInfo, (m) => TelehubInfoReceivedEvent.ToScriptEvent((TelehubInfo)m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportFailed, (m) => TeleportFailedReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportLocal, (m) => TeleportLocalReceivedEvent.ToScriptEvent((TeleportLocal)m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportProgress, (m) => TeleportProgressReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.TeleportStart, (m) => TeleportStartReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.UUIDGroupNameReply, (m) => UUIDGroupNameReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                viewerCircuit.MessageRouting.Add(MessageType.UUIDNameReply, (m) => UUIDNameReplyReceivedEvent.ToScriptEvent(m, vc, (uint)circuitCode));
                vc.ViewerCircuits.Add((uint)circuitCode, viewerCircuit);
                viewerCircuit.SendMessage(useCircuit);
                return new ViewerAgentAccessor(agent.ID, circuitCode, capsPath, sessionId.AsUUID, secureSessionId.AsUUID);
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "Logout")]
        public void LogoutAgent(ScriptInstance instance, ViewerAgentAccessor agent)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new LogoutRequest
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendRequestGodlikePowers")]
        public void SendRequestGodlikePowers(ScriptInstance instance, ViewerAgentAccessor agent, int isgodlike, LSLKey token)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new RequestGodlikePowers
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        IsGodlike = isgodlike != 0,
                        Token = token
                    });
                }
            }
        }
    }
}
