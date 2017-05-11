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
using SilverSim.ServiceInterfaces.Presence;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Account;
using SilverSim.Types.Grid;
using SilverSim.Types.Presence;
using SilverSim.Viewer.Core;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Circuit;
using System;
using System.Net;
using System.Net.Sockets;

namespace SilverSim.Tests.Viewer
{
    partial class ViewerControlApi
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
                UserAccount acc = new UserAccount();
                acc.Principal.ID = UUID.Random;
                acc.Principal.FirstName = firstName;
                acc.Principal.LastName = lastName;
                m_UserAccountService.Add(acc);
                return acc.Principal.ID;
            }
        }

        [APIExtension("ViewerControl", "vcLoginAgent")]
        public string LoginAgent(
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
                IPAddress[] addresses;
                addresses = DnsNameCache.GetHostAddresses(externalHostName);

                string clientIP = string.Empty;

                if (addresses.Length == 0)
                {
                    m_Log.InfoFormat("ExternalHostName \"{0}\" does not resolve", externalHostName);
                    return string.Empty;
                }

                foreach(IPAddress addr in addresses)
                {
                    if(addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        clientIP = addr.ToString();
                    }
                }

                if(string.IsNullOrEmpty(clientIP))
                {
                    m_Log.InfoFormat("ExternalHostName \"{0}\" does not resolve", externalHostName);
                    return string.Empty;
                }

                UUID capsId = UUID.Random;
                string capsPath = m_CapsRedirector.ServerURI + "CAPS/" + capsId.ToString() + "0000/";
                SceneInterface scene;
                if (!m_Scenes.TryGetValue(regionId, out scene))
                {
                    return string.Empty;
                }
                ClientInfo clientInfo = new ClientInfo();
                clientInfo.ClientIP = clientIP;
                clientInfo.Channel = viewerChannel;
                clientInfo.ClientVersion = viewerVersion;
                clientInfo.ID0 = id0;
                clientInfo.Mac = mac;

                UserAccount userAccount;
                if (!m_UserAccountService.TryGetValue(UUID.Zero, agentId, out userAccount))
                {
                    m_Log.InfoFormat("User account {0} does not exist", agentId.ToString());
                    return string.Empty;
                }

                PresenceInfo presenceInfo = new PresenceInfo();
                presenceInfo.RegionID = regionId;
                presenceInfo.SecureSessionID = secureSessionId;
                presenceInfo.SessionID = sessionId;
                presenceInfo.UserID = userAccount.Principal;

                AgentServiceList serviceList = new AgentServiceList();
                serviceList.Add(m_AgentAssetService);
                serviceList.Add(m_AgentInventoryService);
                if (m_AgentProfileService != null)
                {
                    serviceList.Add(m_AgentProfileService);
                }
                serviceList.Add(m_AgentFriendsService);
                serviceList.Add(m_AgentUserAgentService);
                serviceList.Add(m_PresenceService);
                serviceList.Add(m_GridUserService);
                serviceList.Add(m_GridService);
                if (m_OfflineIMService != null)
                {
                    serviceList.Add(m_OfflineIMService);
                }

                ViewerAgent agent = new ViewerAgent(
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
                    return string.Empty;
                }

                UDPCircuitsManager udpServer = (UDPCircuitsManager)scene.UDPServer;

                IPAddress ipAddr;
                if (!IPAddress.TryParse(clientInfo.ClientIP, out ipAddr))
                {
                    m_Log.InfoFormat("Invalid IP address for agent {0}", userAccount.Principal.FullName);
                    return string.Empty;
                }
                IPEndPoint ep = new IPEndPoint(ipAddr, 0);
                IPEndPoint regionEndPoint = new IPEndPoint(ipAddr, (int)scene.RegionPort);
                AgentCircuit circuit = new AgentCircuit(
                    m_Commands,
                    agent,
                    udpServer,
                    (uint)circuitCode,
                    m_CapsRedirector,
                    capsId,
                    agent.ServiceURLs,
                    string.Empty,
                    m_PacketHandlerPlugins,
                    ep);
                circuit.LastTeleportFlags = (TeleportFlags)teleportFlags;
                circuit.Agent = agent;
                circuit.AgentID = userAccount.Principal.ID;
                circuit.SessionID = sessionId.AsUUID;
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
                    return string.Empty;
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
                    PresenceInfo pinfo = new PresenceInfo();
                    pinfo.UserID = agent.Owner;
                    pinfo.SessionID = agent.SessionID;
                    pinfo.SecureSessionID = secureSessionId.AsUUID;
                    pinfo.RegionID = scene.ID;
                    m_PresenceService[agent.SessionID, agent.ID, PresenceServiceInterface.SetType.Report] = pinfo;
                }
                catch (Exception e)
                {
                    m_Log.Warn("Could not contact PresenceService", e);
                }
                circuit.LogIncomingAgent(m_Log, false);
                UseCircuitCode useCircuit = new UseCircuitCode();
                useCircuit.AgentID = agentId;
                useCircuit.SessionID = sessionId.AsUUID;
                useCircuit.CircuitCode = (uint)circuitCode;

                ViewerCircuit viewerCircuit = new ViewerCircuit(m_ClientUDP, (uint)circuitCode, sessionId.AsUUID, agentId, regionEndPoint);
                m_ClientUDP.AddCircuit(viewerCircuit);
                viewerCircuit.SendMessage(useCircuit);
                viewerCircuit.MessageRouting.Add(MessageType.LogoutReply, delegate (Message m)
                {
                    ViewerCircuit removeCircuit;
                    if (m_ViewerCircuits.TryGetValue((uint)circuitCode, out removeCircuit))
                    {
                        removeCircuit.Stop();
                        m_ClientUDP.RemoveCircuit(removeCircuit);
                    }
                });
                m_ViewerCircuits.Add((uint)circuitCode, viewerCircuit);
                return capsPath;
            }
        }

        [APIExtension("ViewerControl", "vcLogoutAgent")]
        public void LogoutAgent(ScriptInstance instance, int circuitCode)
        {
            lock (instance)
            {
                ViewerCircuit viewerCircuit;
                if (m_ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    LogoutReply logoutreq = new LogoutReply();
                    logoutreq.AgentID = viewerCircuit.AgentID;
                    logoutreq.SessionID = viewerCircuit.SessionID;
                    viewerCircuit.SendMessage(logoutreq);
                }
            }
        }
    }
}
