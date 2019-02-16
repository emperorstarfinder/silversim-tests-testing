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
using SilverSim.Types.Inventory;
using SilverSim.Viewer.Messages.Object;
using System.Collections.Generic;

namespace SilverSim.Tests.Viewer
{
    public sealed partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, "rezobjectdata")]
        [APIDisplayName("rezobjectdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        public class RezObjectData
        {
            public LSLKey ItemID = UUID.Zero;
            public int AttachmentPoint;
            public int ItemFlags;
            public int GroupMask;
            public int EveryoneMask;
            public int NextOwnerMask;
            public string Name = string.Empty;
            public string Description = string.Empty;

            public RezObjectData()
            {
            }

            public RezObjectData(RezObjectData src)
            {
                ItemID = new LSLKey(src.ItemID);
                AttachmentPoint = src.AttachmentPoint;
                ItemFlags = src.ItemFlags;
                GroupMask = src.GroupMask;
                EveryoneMask = src.EveryoneMask;
                NextOwnerMask = src.NextOwnerMask;
                Name = src.Name;
                Description = src.Description;
            }
        }

        [APIExtension(ExtensionName, "rayrezobjectdata")]
        [APIDisplayName("rayrezobjectdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public class RayRezObjectData : RezObjectData
        {
            public LSLKey FromTaskID = UUID.Zero;
            public int BypassRaycast;
            public Vector3 RayStart = Vector3.Zero;
            public Vector3 RayEnd = Vector3.Zero;
            public LSLKey RayTargetID = UUID.Zero;
            public int RayEndIsIntersection;
            public int RezSelected;
            public int RemoveItem;

            public RayRezObjectData()
            {
            }

            public RayRezObjectData(RayRezObjectData src)
                : base(src)
            {
                FromTaskID = src.FromTaskID;
                BypassRaycast = src.BypassRaycast;
                RayStart = src.RayStart;
                RayEnd = src.RayEnd;
                RayTargetID = new LSLKey(src.RayTargetID);
                RayEndIsIntersection = src.RayEndIsIntersection;
                RezSelected = src.RezSelected;
                RemoveItem = src.RemoveItem;
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRezSingleAttachmentFromInv(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            RezObjectData rezdata)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new RezSingleAttachmentFromInv
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ItemID = rezdata.ItemID.AsUUID,
                        OwnerID = agent.AgentID,
                        AttachmentPoint = (AttachmentPoint)rezdata.AttachmentPoint,
                        ItemFlags = (uint)rezdata.ItemFlags,
                        GroupMask = (InventoryPermissionsMask)rezdata.GroupMask,
                        EveryoneMask = (InventoryPermissionsMask)rezdata.EveryoneMask,
                        NextOwnerMask = (InventoryPermissionsMask)rezdata.NextOwnerMask,
                        Name = rezdata.Name,
                        Description = rezdata.Description
                    });
                }
            }
        }

        [APIExtension(ExtensionName, "rezobjectdatalist")]
        [APIDisplayName("rezobjectdatalist")]
        [APIIsVariableType]
        [APIAccessibleMembers("Length")]
        public sealed class RezObjectDataList : List<RezObjectData>
        {
            public int Length => Count;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void Add(RezObjectDataList list, RezObjectData data) => list.Add(new RezObjectData(data));

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRezMultipleAttachmentsFromInv(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey compoundMsgID,
            int totalObjects,
            int firstDetachAll,
            RezObjectDataList rezdatalist)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var list = new List<RezMultipleAttachmentsFromInv.ObjectDataS>();
                    foreach(RezObjectData rezdata in rezdatalist)
                    {
                        list.Add(new RezMultipleAttachmentsFromInv.ObjectDataS
                        {
                            ItemID = rezdata.ItemID,
                            OwnerID = agent.AgentID,
                            AttachmentPoint = (AttachmentPoint)rezdata.AttachmentPoint,
                            ItemFlags = (uint)rezdata.ItemFlags,
                            GroupMask = (InventoryPermissionsMask)rezdata.GroupMask,
                            EveryoneMask = (InventoryPermissionsMask)rezdata.EveryoneMask,
                            NextOwnerMask = (InventoryPermissionsMask)rezdata.NextOwnerMask,
                            Name = rezdata.Name,
                            Description = rezdata.Description
                        });
                    }

                    viewerCircuit.SendMessage(new RezMultipleAttachmentsFromInv
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        CompoundMsgID = compoundMsgID,
                        TotalObjects = (byte)totalObjects,
                        FirstDetachAll = firstDetachAll != 0,
                        ObjectData = list
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRezObject(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            RayRezObjectData rezdata,
            LSLKey itemID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new RezObject
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID,
                        RezData = new RezObject.RezDataS
                        {
                            FromTaskID = rezdata.FromTaskID,
                            BypassRaycast = rezdata.BypassRaycast != 0,
                            RayStart = rezdata.RayStart,
                            RayEnd = rezdata.RayEnd,
                            RayTargetID = rezdata.RayTargetID,
                            RayEndIsIntersection = rezdata.RayEndIsIntersection != 0,
                            RezSelected = rezdata.RezSelected != 0,
                        },
                        InventoryData = new RezObject.InventoryDataS
                        {
                            ItemID = itemID,
                            Name = string.Empty,
                            Description = string.Empty
                        }
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRezObjectFromNotecard(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            RayRezObjectData rezdata,
            LSLKey objectID,
            LSLKey notecardItemID,
            AnArray inventorylist)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    var inventoryList = new List<UUID>();
                    foreach(IValue id in inventorylist)
                    {
                        inventoryList.Add(id.AsUUID);
                    }
                    viewerCircuit.SendMessage(new RezObjectFromNotecard
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID,
                        RezData = new RezObjectFromNotecard.RezDataS
                        {
                            FromTaskID = rezdata.FromTaskID,
                            BypassRaycast = rezdata.BypassRaycast != 0,
                            RayStart = rezdata.RayStart,
                            RayEnd = rezdata.RayEnd,
                            RayTargetID = rezdata.RayTargetID,
                            RayEndIsIntersection = rezdata.RayEndIsIntersection != 0,
                            RezSelected = rezdata.RezSelected != 0,
                        },
                        NotecardData = new RezObjectFromNotecard.NotecardDataS
                        {
                            NotecardItemID = notecardItemID,
                            ObjectID = objectID
                        },
                        InventoryData = inventoryList
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRezRestoreToWorld(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            LSLKey itemID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new RezRestoreToWorld
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID,
                        ItemID = itemID
                    });
                }
            }
        }
    }
}
