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
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.Inventory;
using SilverSim.Types.Primitive;
using SilverSim.Viewer.Messages.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectAdd")]
        public void SendObjectAdd(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            int pcode,
            int material,
            int addFlags,
            VcObjectShape shapeData,
            int bypassRaycast,
            Vector3 rayStart,
            Vector3 rayEnd,
            LSLKey rayTargetID,
            int rayEndIsIntersection,
            Vector3 scale,
            Quaternion rotation,
            int state)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new ObjectAdd
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID.AsUUID,
                        PCode = (PrimitiveCode)pcode,
                        Material = (PrimitiveMaterial)material,
                        AddFlags = (uint)addFlags,
                        PathCurve = (byte)shapeData.PathCurve,
                        ProfileCurve = (byte)shapeData.ProfileCurve,
                        PathBegin = (ushort)shapeData.PathBegin,
                        PathEnd = (ushort)shapeData.PathEnd,
                        PathScaleX = (byte)shapeData.PathScaleX,
                        PathScaleY = (byte)shapeData.PathScaleY,
                        PathShearX = (byte)shapeData.PathShearX,
                        PathShearY = (byte)shapeData.PathShearY,
                        PathTwist = (sbyte)shapeData.PathTwist,
                        PathTwistBegin = (sbyte)shapeData.PathTwistBegin,
                        PathRadiusOffset = (sbyte)shapeData.PathRadiusOffset,
                        PathTaperX = (sbyte)shapeData.PathTaperX,
                        PathTaperY = (sbyte)shapeData.PathTaperY,
                        PathRevolutions = (byte)shapeData.PathRevolutions,
                        PathSkew = (sbyte)shapeData.PathSkew,
                        ProfileBegin = (ushort)shapeData.ProfileBegin,
                        ProfileEnd = (ushort)shapeData.ProfileEnd,
                        ProfileHollow = (ushort)shapeData.ProfileHollow,
                        BypassRaycast = bypassRaycast != 0,
                        RayStart = rayStart,
                        RayEnd = rayEnd,
                        RayEndIsIntersection = rayEndIsIntersection != 0,
                        Scale = scale,
                        Rotation = rotation,
                        State = (byte)state});
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendBuyObjectInventory")]
        public void SendBuyObjectInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey objectID,
            LSLKey itemID,
            LSLKey folderID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new BuyObjectInventory
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        ObjectID = objectID.AsUUID,
                        ItemID = itemID.AsUUID,
                        FolderID = folderID.AsUUID
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectBuy")]
        public void SendObjectBuy(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            LSLKey categoryID,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 3 == 0)
                {
                    var ev = new ObjectBuy
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID,
                        CategoryID = categoryID
                    };
                    for(int i = 0; i < objectData.Count; i+= 3)
                    {
                        ev.ObjectData.Add(new ObjectBuy.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            SaleType = (InventoryItem.SaleInfoData.SaleType)objectData[i + 1].AsInt,
                            SalePrice = objectData[i + 2].AsInt
                        });
                    }
                }
            }
        }

        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_SAVE_INTO_AGENT_INVENTORY = 0;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_TAKE_COPY = 1;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_SAVE_INTO_TASK_INVENTORY = 2;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_ATTACHMENT = 3;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_TAKE = 4;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_GOD_TAKE_COPY = 5;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_DELETE_TO_TRASH = 6;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_ATTACHMENT_TO_INV = 7;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_ATTACHMENT_EXISTS = 8;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_RETURN_TO_OWNER = 9;
        [APIExtension("ViewerControl")]
        public const int DEREZ_ACTION_RETURN_TO_LAST_OWNER = 10;

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendDeRezObject")]
        public void SendDeRezObject(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            int derezAction,
            LSLKey destinationID,
            LSLKey transactionID,
            int packetCount,
            int packetNumber)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new DeRezObject
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID.AsUUID,
                        Destination = (DeRezObject.DeRezAction)derezAction,
                        DestinationID = destinationID.AsUUID,
                        TransactionID = transactionID.AsUUID,
                        PacketCount = (byte)packetCount,
                        PacketNumber = (byte)packetNumber
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendDetachAttachmentIntoInv")]
        public void SendDetachAttachmentIntoInv(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new DetachAttachmentIntoInv
                    {
                        AgentID = agent.AgentID,
                        ItemID = itemID.AsUUID
                    });
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectCategory")]
        public void SendObjectCategory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    (objectData.Count % 2) == 0)
                {
                    var m = new ObjectCategory
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    for(int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new ObjectCategory.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Category = objectData[i + 1].AsUInt
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectClickAction")]
        public void SendObjectClickAction(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    (objectData.Count % 2) == 0)
                {
                    var m = new ObjectClickAction
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new ObjectClickAction.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            ClickAction = (ClickActionType)objectData[i + 1].AsUInt
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDelete")]
        public void SendObjectDelete(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int force,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectDelete
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        Force = force != 0
                    };
                    foreach(IValue iv in objectData)
                    {
                        m.ObjectLocalIDs.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDelink")]
        public void SendObjectDelink(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectDelink
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectList.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDescription")]
        public void SendObjectDescription(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectDescription
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new ObjectDescription.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Description = objectData[i + 1].ToString()
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDetach")]
        public void SendObjectDetach(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectDetach
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectList.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDrop")]
        public void SendObjectDrop(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectDetach
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectList.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDuplicate")]
        public void SendObjectDuplicate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey group,
            Vector3 offset,
            int duplicateFlags,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectDuplicate
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = group,
                        Offset = offset,
                        DuplicateFlags = (PrimitiveFlags)duplicateFlags
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectLocalIDs.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectDuplicateOnRay")]
        public void SendObjectDuplicateOnRay(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey group,
            Vector3 rayStart,
            Vector3 rayEnd,
            int bypassRayCast,
            int rayEndIsIntersection,
            int copyCenters,
            int copyRotates,
            LSLKey rayTargetID,
            int duplicateFlags,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectDuplicateOnRay
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = group,
                        RayStart = rayStart,
                        RayEnd = rayEnd,
                        BypassRayCast = bypassRayCast != 0,
                        RayEndIsIntersection = rayEndIsIntersection != 0,
                        CopyCenters = copyCenters != 0,
                        CopyRotates = copyRotates != 0,
                        RayTargetID = rayTargetID,
                        DuplicateFlags = (PrimitiveFlags)duplicateFlags
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectLocalIDs.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectExportSelected")]
        public void SendObjectExportSelected(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey requestId,
            int volumeDetail,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectExportSelected
                    {
                        AgentID = agent.AgentID,
                        RequestID = requestId,
                        VolumeDetail = (short)volumeDetail
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectIDs.Add(iv.AsUUID);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectFlagUpdate")]
        public void SendObjectFlagUpdate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            int usePhysics,
            int isTemporary,
            int isPhantom)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectFlagUpdate
                    {
                        AgentID = agent.AgentID,
                        CircuitSessionID = viewerCircuit.SessionID,
                        ObjectLocalID = (uint)localID,
                        UsePhysics = usePhysics != 0,
                        IsTemporary = isTemporary != 0,
                        IsPhantom = isPhantom != 0
                    };
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectFlagUpdate")]
        public void SendObjectFlagUpdate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            int usePhysics,
            int isTemporary,
            int isPhantom,
            int physicsShapeType,
            double density,
            double friction,
            double restitution,
            double gravityMultiplier)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectFlagUpdate
                    {
                        AgentID = agent.AgentID,
                        CircuitSessionID = viewerCircuit.SessionID,
                        ObjectLocalID = (uint)localID,
                        UsePhysics = usePhysics != 0,
                        IsTemporary = isTemporary != 0,
                        IsPhantom = isPhantom != 0
                    };
                    m.ExtraPhysics.Add(new ObjectFlagUpdate.ExtraPhysicsData
                    {
                        PhysicsShapeType = (PrimitivePhysicsShapeType)physicsShapeType,
                        Density = density,
                        Friction = friction,
                        Restitution = restitution,
                        GravityMultiplier = gravityMultiplier
                    });
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectGroup")]
        public void SendObjectGroup(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectGroup
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        GroupID = groupID
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectList.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", "objectimagedata")]
        [APIDisplayName("objectimagedata")]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        [APIIsVariableType]
        [Serializable]
        public sealed class ObjectImageData
        {
            public int LocalID;
            public string MediaURL = string.Empty;
            public TextureEntryContainer TextureEntry = new TextureEntryContainer();

            public ObjectImageData()
            {
            }

            public ObjectImageData(ObjectImageData src)
            {
                LocalID = src.LocalID;
                MediaURL = src.MediaURL;
                TextureEntry = new TextureEntryContainer(src.TextureEntry);
            }
        }

        [APIExtension("ViewerControl", "objectimagedatalist")]
        [APIDisplayName("objectimagedatalist")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        public sealed class ObjectImageDataList : List<ObjectImageData>
        {
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "Add")]
        public void AddObjectImageData(ObjectImageDataList list, ObjectImageData data) => list.Add(data);

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectImage")]
        public void SendObjectImage(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            ObjectImageDataList objectImageList)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectImageList.Count <= 255)
                {
                    var m = new ObjectImage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    foreach(ObjectImageData data in objectImageList)
                    {
                        m.ObjectData.Add(new ObjectImage.ObjectDataEntry
                        {
                            ObjectLocalID = (uint)data.LocalID,
                            MediaURL = data.MediaURL,
                            TextureEntry = data.TextureEntry.GetBytes()
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectIncludeInSearch")]
        public void SendObjectIncludeInSearch(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey groupID,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectIncludeInSearch
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for(int i = 0; i < objectData.Count; i+= 2)
                    {
                        m.ObjectData.Add(new ObjectIncludeInSearch.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            IncludeInSearch = objectData[i + 1].AsBoolean
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectLink")]
        public void SendObjectLink(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectLink
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectList.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectMaterial")]
        public void SendObjectMaterial(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectMaterial
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {

                        m.ObjectData.Add(new ObjectMaterial.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Material = (PrimitiveMaterial)objectData[i + 1].AsInt
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectName")]
        public void SendObjectName(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectName
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {

                        m.ObjectData.Add(new ObjectName.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Name = objectData[i + 1].ToString()
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectOwner")]
        public void SendObjectOwner(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int hasGodBit,
            LSLKey owner,
            LSLKey group,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectOwner
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        HasGodBit = hasGodBit != 0,
                        OwnerID = owner,
                        GroupID = group
                    };
                    foreach (IValue iv in objectData)
                    {
                        m.ObjectList.Add(iv.AsUInt);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectPermissions")]
        public void SendObjectPermissions(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int hasGodBit,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 4 == 0)
                {
                    var m = new ObjectPermissions
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        HasGodBit = hasGodBit != 0,
                    };
                    for(int i = 0; i < objectData.Count; i += 4)
                    {
                        m.ObjectData.Add(new ObjectPermissions.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Field = (ObjectPermissions.ChangeFieldMask)objectData[i + 1].AsInt,
                            ChangeType = (ObjectPermissions.ChangeType)objectData[i + 2].AsInt,
                            Mask = (InventoryPermissionsMask)objectData[i + 3].AsUInt
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectPhysicsProperties")]
        public void SendObjectPhysicsProperties(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 6 == 0)
                {
                    var m = new ObjectPhysicsProperties();
                    for (int i = 0; i < objectData.Count; i += 6)
                    {
                        m.ObjectData.Add(new ObjectPhysicsProperties.ObjectDataEntry
                        {
                            LocalID = objectData[i].AsUInt,
                            PhysicsShapeType = (PrimitivePhysicsShapeType)objectData[i + 1].AsInt,
                            Density = objectData[i + 2].AsReal,
                            Friction = objectData[i + 3].AsReal,
                            Restitution = objectData[i + 4].AsReal,
                            GravityMultiplier = objectData[i + 5].AsReal
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendMultipleObjectUpdate")]
        public void SendMultipleObjectUpdate(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            [Description("4 elements stride (localid, position, rotation, scale) if one of the last three is not set to a vector/rotation it is disabled")]
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 4 == 0)
                {
                    var m = new MultipleObjectUpdate
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 4)
                    {
                        var d = new MultipleObjectUpdate.ObjectDataEntry();
                        IValue position = objectData[i + 1];
                        IValue rotation = objectData[i + 2];
                        IValue scale = objectData[i + 3];
                        int dataSize = 0;
                        if(position is Vector3)
                        {
                            dataSize += 12;
                        }
                        if (rotation is Quaternion)
                        {
                            dataSize += 12;
                        }
                        if (scale is Vector3)
                        {
                            dataSize += 12;
                        }

                        byte[] datablock = new byte[dataSize];
                        int dataPos = 0;
                        if(position is Vector3)
                        {
                            ((Vector3)position).ToBytes(datablock, dataPos);
                            dataPos += 12;
                        }
                        if (rotation is Quaternion)
                        {
                            ((Quaternion)rotation).ToBytes(datablock, dataPos);
                            dataPos += 12;
                        }
                        if (scale is Vector3)
                        {
                            ((Vector3)scale).ToBytes(datablock, dataPos);
                        }
                        d.Data = datablock;
                        m.ObjectData.Add(d);
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectPosition")]
        public void SendObjectPosition(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectPosition
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new ObjectPosition.ObjectDataEntry
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Position = objectData[i + 1].AsVector3
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectRotation")]
        public void SendObjectRotation(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectRotation
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new ObjectRotation.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Rotation = objectData[i + 1].AsQuaternion
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectSaleInfo")]
        public void SendObjectSaleInfo(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 3 == 0)
                {
                    var m = new ObjectSaleInfo
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 3)
                    {
                        m.ObjectData.Add(new ObjectSaleInfo.Data
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            SaleType = (InventoryItem.SaleInfoData.SaleType)objectData[i + 1].AsInt,
                            SalePrice = objectData[i + 2].AsInt
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectScale")]
        public void SendObjectScale(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 2 == 0)
                {
                    var m = new ObjectScale
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new ObjectScale.ObjectDataEntry
                        {
                            ObjectLocalID = objectData[i].AsUInt,
                            Size = objectData[i + 1].AsVector3
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendRequestObjectPropertiesFamily")]
        public void SendRequestObjectPropertiesFamily(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int requestFlags,
            LSLKey objectID)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new RequestObjectPropertiesFamily
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                        RequestFlags = (uint)requestFlags,
                        ObjectID = objectID
                    };
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", "objectshape")]
        [APIDisplayName("objectshape")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public sealed class VcObjectShape
        {
            public int LocalID;

            public int PathCurve = 16;
            public int ProfileCurve = 1;
            public int PathBegin;// 0 to 1, quanta = 0.01
            public int PathEnd; // 0 to 1, quanta = 0.01
            public int PathScaleX = 100; // 0 to 1, quanta = 0.01
            public int PathScaleY = 100; // 0 to 1, quanta = 0.01
            public int PathShearX; // -.5 to .5, quanta = 0.01
            public int PathShearY; // -.5 to .5, quanta = 0.01
            public int PathTwist;  // -1 to 1, quanta = 0.01
            public int PathTwistBegin; // -1 to 1, quanta = 0.01
            public int PathRadiusOffset; // -1 to 1, quanta = 0.01
            public int PathTaperX; // -1 to 1, quanta = 0.01
            public int PathTaperY; // -1 to 1, quanta = 0.01
            public int PathRevolutions; // 0 to 3, quanta = 0.015
            public int PathSkew; // -1 to 1, quanta = 0.01
            public int ProfileBegin; // 0 to 1, quanta = 0.01
            public int ProfileEnd; // 0 to 1, quanta = 0.01
            public int ProfileHollow; // 0 to 1, quanta = 0.01
        }

        [APIExtension("ViewerControl", "objectshapelist")]
        [APIDisplayName("objectshapelist")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class VcObjectShapeList : List<VcObjectShape>
        {
        }

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "Add")]
        public void AddObjectShapeData(VcObjectShapeList list, VcObjectShape data) => list.Add(data);

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectShape")]
        public void SendObjectShape(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            VcObjectShapeList shapelist)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var m = new ObjectShape
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    foreach(VcObjectShape shape in shapelist)
                    {
                        m.ObjectData.Add(new ObjectShape.Data
                        {
                            ObjectLocalID = (ushort)shape.LocalID,
                            PathCurve = (byte)shape.PathCurve,
                            ProfileCurve = (byte)shape.ProfileCurve,
                            PathBegin = (ushort)shape.PathBegin,
                            PathEnd = (ushort)shape.PathEnd,
                            PathScaleX = (byte)shape.PathScaleX,
                            PathScaleY = (byte)shape.PathScaleY,
                            PathShearX = (byte)shape.PathShearX,
                            PathShearY = (byte)shape.PathShearY,
                            PathTwist = (sbyte)shape.PathTwist,
                            PathTwistBegin = (sbyte)shape.PathTwistBegin,
                            PathRadiusOffset = (sbyte)shape.PathRadiusOffset,
                            PathTaperX = (sbyte)shape.PathTaperX,
                            PathTaperY = (sbyte)shape.PathTaperY,
                            PathRevolutions = (byte)shape.PathRevolutions,
                            PathSkew = (sbyte)shape.PathSkew,
                            ProfileBegin = (ushort)shape.ProfileBegin,
                            ProfileEnd = (ushort)shape.ProfileEnd,
                            ProfileHollow = (ushort)shape.ProfileHollow
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension("ViewerControl", "objectdata")]
        [APIDisplayName("objectdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public sealed class VcObjectData
        {
            public int LocalID;

            public int State;
            public LSLKey FullID = new LSLKey();
            public int CRC;
            public int PCode;
            public int Material;
            public int ClickAction;
            public Vector3 Scale;
            public ByteArrayApi.ByteArray ObjectData = new ByteArrayApi.ByteArray();
            public int ParentID;
            public PrimitiveFlags UpdateFlags;
            public VcObjectShape ObjectShape = new VcObjectShape();
            public TextureEntryContainer TextureEntry = new TextureEntryContainer();
            public ByteArrayApi.ByteArray TextureAnim = new ByteArrayApi.ByteArray();
            public string NameValue = string.Empty;
            public ByteArrayApi.ByteArray Data = new ByteArrayApi.ByteArray();
            public string Text = string.Empty;
            public Vector3 TextColor;
            public double TextAlpha;
            public string MediaURL = string.Empty;
            public ByteArrayApi.ByteArray PSBlock = new ByteArrayApi.ByteArray();
            public ByteArrayApi.ByteArray ExtraParams = new ByteArrayApi.ByteArray();
            public LSLKey LoopedSound = new LSLKey();
            public LSLKey OwnerID = new LSLKey();
            public double Gain;
            public int Flags;
            public double Radius;
            public int JointType;
            public Vector3 JointPivot;
            public Vector3 JointAxisOrAnchor;

            public VcObjectData()
            {

            }

            public VcObjectData(UnreliableObjectUpdate.ObjData d)
            {
                LocalID = (int)d.LocalID;
                State = d.State;
                FullID = d.FullID;
                CRC = (int)d.CRC;
                PCode = (int)d.PCode;
                Material = (int)d.Material;
                ClickAction = (int)d.ClickAction;
                Scale = d.Scale;
                ObjectData = new ByteArrayApi.ByteArray(d.ObjectData);
                ParentID = (int)d.ParentID;
                UpdateFlags = d.UpdateFlags;
                ObjectShape = new VcObjectShape
                {
                    LocalID = (int)d.LocalID,

                    PathCurve = d.PathCurve,
                    ProfileCurve = d.ProfileCurve,
                    PathBegin = d.PathBegin,
                    PathEnd = d.PathEnd,
                    PathScaleX = d.PathScaleX,
                    PathScaleY = d.PathScaleY,
                    PathShearX = d.PathShearX,
                    PathShearY = d.PathShearY,
                    PathTwist = d.PathTwist,
                    PathTwistBegin = d.PathTwistBegin,
                    PathRadiusOffset = d.PathRadiusOffset,
                    PathTaperX = d.PathTaperX,
                    PathTaperY = d.PathTaperY,
                    PathRevolutions = d.PathRevolutions,
                    PathSkew = d.PathSkew,
                    ProfileBegin = d.ProfileBegin,
                    ProfileEnd = d.ProfileEnd,
                    ProfileHollow = d.ProfileHollow
                };
                TextureEntry = new TextureEntryContainer(new TextureEntry(d.TextureEntry));
                TextureAnim = new ByteArrayApi.ByteArray(d.TextureAnim);
                NameValue = d.NameValue;
                Data = new ByteArrayApi.ByteArray(d.Data);
                Text = d.Text;
                TextColor = d.TextColor;
                TextAlpha = d.TextColor.A;
                MediaURL = d.MediaURL;
                PSBlock = new ByteArrayApi.ByteArray(d.PSBlock);
                ExtraParams = new ByteArrayApi.ByteArray(d.ExtraParams);
                LoopedSound = d.LoopedSound;
                OwnerID = d.OwnerID;
                Gain = d.Gain;
                Flags = (int)d.Flags;
                Radius = d.Radius;
                JointType = d.JointType;
                JointPivot = d.JointPivot;
                JointAxisOrAnchor = d.JointAxisOrAnchor;
            }
        }

        [APIExtension("ViewerControl", "objectdatalist")]
        [APIDisplayName("objectdatalist")]
        [APIAccessibleMembers("Count")]
        [APIIsVariableType]
        public class VcObjectDataList : List<VcObjectData>
        {
        }
    }
}
