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
using SilverSim.Types.Parcel;
using SilverSim.Viewer.Messages.Parcel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelInfoRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey parcelID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelInfoRequest
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ParcelID = new ParcelID(parcelID.AsUUID.GetBytes(), 0)
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelObjectOwnersRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int parcelLocalID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelObjectOwnersRequest
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ParcelLocalID = parcelLocalID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelAccessListRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int sequenceId,
            int flags,
            int localID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelAccessListRequest
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        SequenceID = sequenceId,
                        Flags = (ParcelAccessList)flags,
                        LocalID = localID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelAccessListUpdate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int flags,
            int localID,
            LSLKey transactionID,
            int sequenceId,
            int sections,
            AnArray list)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    list.Count % 3 == 0)
                {
                    var msg = new ParcelAccessListUpdate
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Flags = (ParcelAccessList)flags,
                        LocalID = localID,
                        TransactionID = transactionID,
                        SequenceID = sequenceId,
                        Sections = sections
                    };
                    for(int i = 0; i < list.Count; i += 3)
                    {
                        msg.AccessList.Add(new ParcelAccessListUpdate.Data
                        {
                            ID = list[i].AsUUID,
                            Time = list[i + 1].AsUInt,
                            Flags = (ParcelAccessList)list[i + 2].AsUInt
                        });
                    }
                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelBuy(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            LSLKey groupID,
            int isGroupOwned,
            int removeContribution,
            int isFinal,
            int price,
            int area)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelBuy
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID,
                        IsGroupOwned = isGroupOwned != 0,
                        RemoveContribution = removeContribution != 0,
                        LocalID = localID,
                        IsFinal = isFinal != 0,
                        Price = price,
                        Area = area
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelBuyPass(
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
                    viewerCircuit.SendMessage(new ParcelBuyPass
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelDeedToGroup(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            LSLKey groupID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelDeedToGroup
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                        GroupID = groupID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelDisableObjects(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            int returnType,
            AnArray objectIds,
            AnArray ownerIds)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new ParcelDisableObjects
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                        ReturnType = (uint)returnType
                    };

                    foreach(IValue iv in objectIds)
                    {
                        msg.TaskIDs.Add(iv.AsUUID);
                    }

                    foreach (IValue iv in ownerIds)
                    {
                        msg.OwnerIDs.Add(iv.AsUUID);
                    }

                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelReturnObjects(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            int returnType,
            AnArray objectIds,
            AnArray ownerIds)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new ParcelReturnObjects
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                        ReturnType = (ObjectReturnType)returnType
                    };

                    foreach (IValue iv in objectIds)
                    {
                        msg.TaskIDs.Add(iv.AsUUID);
                    }

                    foreach (IValue iv in ownerIds)
                    {
                        msg.OwnerIDs.Add(iv.AsUUID);
                    }

                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelSelectObjects(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            int returnType,
            AnArray returnIds)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var msg = new ParcelSelectObjects
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                        ReturnType = (ObjectReturnType)returnType
                    };

                    foreach (IValue iv in returnIds)
                    {
                        msg.ReturnIDs.Add(iv.AsUUID);
                    }

                    viewerCircuit.SendMessage(msg);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelDivide(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            double west,
            double south,
            double east,
            double north)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {

                    viewerCircuit.SendMessage(new ParcelDivide
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        West = west,
                        South = south,
                        East = east,
                        North = north
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelJoin(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            double west,
            double south,
            double east,
            double north)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {

                    viewerCircuit.SendMessage(new ParcelJoin
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        West = west,
                        South = south,
                        East = east,
                        North = north
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelDwellRequest(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            LSLKey parcelID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelDwellRequest
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                        ParcelID = new ParcelID(parcelID.AsUUID.GetBytes(), 0)
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelGodForceOwner(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey ownerID,
            int localID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelGodForceOwner
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        OwnerID = ownerID,
                        LocalID = localID
                    });
                }
            }
        }
        
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelGodMarkAsContent(
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
                    viewerCircuit.SendMessage(new ParcelGodMarkAsContent
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelReclaim(
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
                    viewerCircuit.SendMessage(new ParcelReclaim
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelRelease(
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
                    viewerCircuit.SendMessage(new ParcelRelease
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendParcelSetOtherCleanTime(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            int otherCleanTime)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ParcelSetOtherCleanTime
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        LocalID = localID,
                        OtherCleanTime = otherCleanTime
                    });
                }
            }
        }
    }
}
