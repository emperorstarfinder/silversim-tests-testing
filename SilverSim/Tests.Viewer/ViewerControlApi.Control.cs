﻿// SilverSim is distributed under the terms of the
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
using SilverSim.Types.Agent;
using SilverSim.Viewer.Messages.Agent;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", "vcSendAgentPause")]
        public void SendAgentPause(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            int serialNo)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agentId.AsUUID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new AgentPause
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        SerialNum = (uint)serialNo
                    });
                }
            }
        }

        [APIExtension("ViewerControl", "vcSendAgentResume")]
        public void SendAgentResume(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            int serialNo)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agentId.AsUUID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new AgentResume
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        SerialNum = (uint)serialNo
                    });
                }
            }
        }

        [APIExtension("ViewerControl", "vcSendAgentUpdate")]
        public void SendAgentUpdate(
            ScriptInstance instance,
            LSLKey agentId,
            int circuitCode,
            Quaternion bodyRotation,
            Quaternion headRotation,
            int state,
            Vector3 cameraCenter,
            Vector3 cameraAtAxis,
            Vector3 cameraLeftAxis,
            Vector3 cameraUpAxis,
            double far,
            int controlFlags,
            int flags)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agentId.AsUUID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new AgentUpdate
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        BodyRotation = bodyRotation,
                        HeadRotation = headRotation,
                        State = (AgentState)state,
                        CameraCenter = cameraCenter,
                        CameraAtAxis = cameraAtAxis,
                        CameraLeftAxis = cameraLeftAxis,
                        CameraUpAxis = cameraUpAxis,
                        Far = far,
                        ControlFlags = (Types.Agent.ControlFlags)controlFlags,
                        Flags = (byte)flags
                    });
                }
            }
        }
    }
}
