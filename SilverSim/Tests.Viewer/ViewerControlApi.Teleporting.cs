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

using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.Grid;
using SilverSim.Viewer.Messages.Circuit;
using SilverSim.Viewer.Messages.Teleport;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendTeleportRequest")]
        public void SendTeleportRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey regionId,
            Vector3 position,
            Vector3 lookAt)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new TeleportRequest
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        RegionID = regionId.AsUUID,
                        Position = position,
                        LookAt = lookAt
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendTeleportLureRequest")]
        public void SendTeleportLureRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey lureID,
            int teleportFlags)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new TeleportLureRequest
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LureID = lureID,
                        TeleportFlags = (TeleportFlags)teleportFlags
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendTeleportLocationRequest")]
        public void SendTeleportLocationRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            Vector3 gridLocation,
            Vector3 position,
            Vector3 lookAt
            )
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new TeleportLocationRequest
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GridPosition = new GridVector((uint)gridLocation.X, (uint)gridLocation.Y),
                        Position = position,
                        LookAt = lookAt
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendTeleportLocal")]
        public void SendTeleportLandmarkRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey landmarkId)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new TeleportLandmarkRequest
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LandmarkID = landmarkId.AsUUID
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendCompleteAgentMovement")]
        public void SendCompleteAgentMovement(
            ScriptInstance instance,
            ViewerAgentAccessor agent)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new CompleteAgentMovement
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        CircuitCode = viewerCircuit.CircuitCode
                    });
                }
            }
        }
    }
}
