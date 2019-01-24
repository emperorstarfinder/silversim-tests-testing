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
using SilverSim.Types.Agent;
using SilverSim.Viewer.Messages.Agent;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendAgentPause(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int serialNo)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendAgentResume(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int serialNo)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendSetAlwaysRun(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int alwaysRun)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new SetAlwaysRun
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        AlwaysRun = alwaysRun != 0
                    });
                }
            }
        }

        [APIExtension(ExtensionName)]
        public const int VC_AGENT_UPDATE_FLAGS_HIDE_TITLE = 1;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_UPDATE_FLAGS_CLIENT_AUTOPILOT = 2;

        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_AT_POS = 1 << 0;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_AT_NEG = 1 << 1;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_LEFT_POS = 1 << 2;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_LEFT_NEG = 1 << 3;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_UP_POS = 1 << 4;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_UP_NEG = 1 << 5;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_PITCH_POS = 1 << 6;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_PITCH_NEG = 1 << 7;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_YAW_POS = 1 << 8;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_YAW_NEG = 1 << 9;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_FAST_AT = 1 << 10;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_FAST_LEFT = 1 << 11;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_FAST_UP = 1 << 12;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_FLY = 1 << 13;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_STOP = 1 << 14;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_FINISH_ANIM = 1 << 15;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_STAND_UP = 1 << 16;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_SIT_ON_GROUND = 1 << 17;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_MOUSE_LOOK = 1 << 18;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_NUDGE_AT_POS = 1 << 19;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_NUDGE_AT_NEG = 1 << 20;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_NUDGE_LEFT_POS = 1 << 21;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_NUDGE_LEFT_NEG = 1 << 22;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_NUDGE_UP_POS = 1 << 23;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_NUDGE_UP_NEG = 1 << 24;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_TURN_LEFT = 1 << 25;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_TURN_RIGHT = 1 << 26;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_AWAY = 1 << 27;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_LBUTTON_DOWN = 1 << 28;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_LBUTTON_UP = 1 << 29;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_MOUSE_LOOK_LBUTTON_DOWN = 1 << 30;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_CONTROL_FLAGS_MOUSE_LOOK_LBUTTON_UP = 1 << 31;

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendAgentUpdate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
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
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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
                        ControlFlags = (ControlFlags)controlFlags,
                        Flags = (AgentUpdateFlags)flags
                    });
                }
            }
        }
    }
}
