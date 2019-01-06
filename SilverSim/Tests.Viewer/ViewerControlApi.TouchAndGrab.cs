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
using SilverSim.Viewer.Messages.Object;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendObjectGrab")]
        public void SendObjectGrab(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int objectLocalID,
            Vector3 grabOffset,
            AnArray objectData)
        {
            lock(instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 6 == 0)
                {
                    var msg = new ObjectGrab
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ObjectLocalID = (uint)objectLocalID,
                        GrabOffset = grabOffset
                    };
                    for(int i = 0;  i < objectData.Count; i+=6)
                    {
                        msg.ObjectData.Add(new ObjectGrab.Data
                        {
                            UVCoord = objectData[i + 0].AsVector3,
                            STCoord = objectData[i + 1].AsVector3,
                            FaceIndex = objectData[i + 2].AsInt,
                            Position = objectData[i + 3].AsVector3,
                            Normal = objectData[i + 4].AsVector3,
                            Binormal = objectData[i + 5].AsVector3
                        });
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendObjectGrabUpdate")]
        public void SendObjectGrabUpdate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey objectID,
            Vector3 grabOffsetInitial,
            Vector3 grabPosition,
            long timeSinceLast,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 6 == 0)
                {
                    var msg = new ObjectGrabUpdate
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ObjectID = objectID.AsUUID,
                        GrabOffsetInitial = grabOffsetInitial,
                        GrabPosition = grabPosition,
                        TimeSinceLast = (uint)timeSinceLast
                    };
                    for (int i = 0; i < objectData.Count; i += 6)
                    {
                        msg.ObjectData.Add(new ObjectGrabUpdate.Data
                        {
                            UVCoord = objectData[i + 0].AsVector3,
                            STCoord = objectData[i + 1].AsVector3,
                            FaceIndex = objectData[i + 2].AsInt,
                            Position = objectData[i + 3].AsVector3,
                            Normal = objectData[i + 4].AsVector3,
                            Binormal = objectData[i + 5].AsVector3
                        });
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendObjectDeGrab")]
        public void SendObjectDeGrab(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int objectLocalID,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 6 == 0)
                {
                    var msg = new ObjectDeGrab
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ObjectLocalID = (uint)objectLocalID
                    };
                    for (int i = 0; i < objectData.Count; i += 6)
                    {
                        msg.ObjectData.Add(new ObjectDeGrab.Data
                        {
                            UVCoord = objectData[i + 0].AsVector3,
                            STCoord = objectData[i + 1].AsVector3,
                            FaceIndex = objectData[i + 2].AsInt,
                            Position = objectData[i + 3].AsVector3,
                            Normal = objectData[i + 4].AsVector3,
                            Binormal = objectData[i + 5].AsVector3
                        });
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendObjectSelect")]
        public void SendObjectSelect(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectlocalids)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new ObjectSelect
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    foreach(IValue iv in objectlocalids)
                    {
                        msg.ObjectData.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendObjectDeselect")]
        public void SendObjectDeselect(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectlocalids)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new ObjectDeselect
                    {
                        AgentID = viewerCircuit.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    foreach (IValue iv in objectlocalids)
                    {
                        msg.ObjectData.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "SendRequestPayPrice")]
        public void SendRequestPayPrice(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey objectID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new RequestPayPrice
                    {
                        ObjectID = objectID.AsUUID
                    });
                }
            }
        }
    }
}
