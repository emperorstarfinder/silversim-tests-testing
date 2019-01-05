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
using SilverSim.Viewer.Messages.Generic;
using SilverSim.Viewer.Messages.Land;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverSim.Tests.Viewer
{
    public sealed partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "modifylandparceldata")]
        [APIDisplayName("modifylandparceldata")]
        [APIIsVariableType]
        [APICloneOnAssignment]
        [APIAccessibleMembers]
        public sealed class ModifyLandParcelData
        {
            public int LocalID;
            public double West;
            public double South;
            public double East;
            public double North;
            public double BrushSize;
        }

        [APIExtension(ExtensionName, "modifylandparceldatalist")]
        [APIDisplayName("modifylandparceldatalist")]
        [APIIsVariableType]
        [APICloneOnAssignment]
        [APIAccessibleMembers]
        [Serializable]
        public sealed class ModifyLandParcelDataList : List<ModifyLandParcelData>
        {
            public int Length => Count;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "Add")]
        public void AddModifyLandParcelData(ModifyLandParcelDataList list, ModifyLandParcelData data) => list.Add(data);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendModifyLand")]
        public void SendModifyLand(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int action,
            int brushSize,
            double seconds,
            double height,
            ModifyLandParcelDataList list)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ModifyLand
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Action = (byte)action,
                        Size = (byte)brushSize,
                        Seconds = seconds,
                        Height = height
                    };
                    foreach(ModifyLandParcelData entry in list)
                    {
                        m.ParcelData.Add(new ModifyLand.Data
                        {
                            LocalID = entry.LocalID,
                            West = entry.West,
                            South = entry.South,
                            East = entry.East,
                            North = entry.North,
                            BrushSize = entry.BrushSize
                        });
                    }

                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendTerrainBake")]
        public void SendTerrainBake(
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
                    var msg = new GodlikeMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "terrain"
                    };
                    msg.ParamList.Add("bake".ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendTerrainRevert")]
        public void SendTerrainRevert(
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
                    var msg = new GodlikeMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "terrain"
                    };
                    msg.ParamList.Add("revert".ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendTerrainSwap")]
        public void SendTerrainSwap(
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
                    var msg = new GodlikeMessage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Method = "terrain"
                    };
                    msg.ParamList.Add("swap".ToUTF8Bytes());
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateTerrainDetail(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray list)
        {
            if(list.Count % 2 != 0)
            {
                return;
            }
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
                        SessionID = viewerCircuit.SessionID,
                        Method = "texturedetail"
                    };
                    for(int i = 0; i < list.Count; i += 2)
                    {
                        msg.ParamList.Add($"{list[i].AsInt} {list[i + 1].AsUUID}".ToUTF8Bytes());
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendEstateTerrainHeights(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray list)
        {
            if (list.Count % 3 != 0)
            {
                return;
            }
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
                        SessionID = viewerCircuit.SessionID,
                        Method = "textureheights"
                    };
                    for (int i = 0; i < list.Count; i += 3)
                    {
                        double lowVal = list[i + 1].AsReal;
                        double highVal = list[i + 2].AsReal;
                        msg.ParamList.Add(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", list[i].AsInt, lowVal, highVal).ToUTF8Bytes());
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }
    }
}
