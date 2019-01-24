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
using SilverSim.Viewer.Messages.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilverSim.Types;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendTelehubConnect(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        Method = "telehub"
                    };
                    msg.ParamList.Add("connect".ToUTF8Bytes());
                    msg.ParamList.Add(((uint)localID).ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendTelehubDelete(
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
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        Method = "telehub"
                    };
                    msg.ParamList.Add("delete".ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendTelehubInfoRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey transactionID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        TransactionID = transactionID,
                        Method = "telehub"
                    };
                    msg.ParamList.Add("info ui".ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendTelehubSpawnpointAdd(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        Method = "telehub"
                    };
                    msg.ParamList.Add("spawnpoint add".ToUTF8Bytes());
                    msg.ParamList.Add(((uint)localID).ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendTelehubSpawnpointRemove(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new EstateOwnerMessage
                    {
                        AgentID = agent.AgentID,
                        Method = "telehub"
                    };
                    msg.ParamList.Add("spawnpoint remove".ToUTF8Bytes());
                    msg.ParamList.Add(((uint)localID).ToString().ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }
    }
}
