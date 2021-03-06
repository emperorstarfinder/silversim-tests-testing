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

using SilverSim.Scene.Types.Object.Localization;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.Inventory;
using SilverSim.Types.Primitive;
using SilverSim.Viewer.Messages.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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
                        RayTargetID = rayTargetID,
                        Scale = scale,
                        Rotation = rotation,
                        State = (byte)state});
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_SAVE_INTO_AGENT_INVENTORY = 0;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_TAKE_COPY = 1;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_SAVE_INTO_TASK_INVENTORY = 2;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_ATTACHMENT = 3;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_TAKE = 4;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_GOD_TAKE_COPY = 5;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_DELETE_TO_TRASH = 6;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_ATTACHMENT_TO_INV = 7;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_ATTACHMENT_EXISTS = 8;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_RETURN_TO_OWNER = 9;
        [APIExtension(ExtensionName)]
        public const int DEREZ_ACTION_RETURN_TO_LAST_OWNER = 10;

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    viewerCircuit.SendMessage(new DetachAttachmentIntoInv
                    {
                        AgentID = agent.AgentID,
                        ItemID = itemID.AsUUID
                    });
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void SendRequestMultipleObjects(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            AnArray objectData)
        {
            if(objectData.Count % 2 != 0)
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
                    var m = new RequestMultipleObjects
                    {
                        AgentID = agent.AgentID,
                        SessionID = agent.SessionId
                    };
                    for(int i = 0; i < objectData.Count; i += 2)
                    {
                        m.ObjectData.Add(new RequestMultipleObjects.ObjectDataEntry
                        {
                            CacheMissType = (RequestMultipleObjects.CacheMissType)objectData[i].AsInt,
                            LocalID = objectData[i + 1].AsUInt
                        });
                    }
                    viewerCircuit.SendMessage(m);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, "objectimagedata")]
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

        [APIExtension(ExtensionName, "objectimagedatalist")]
        [APIDisplayName("objectimagedatalist")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [APICloneOnAssignment]
        public sealed class ObjectImageDataList : List<ObjectImageData>
        {
            public int Length => Count;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "Add")]
        public void AddObjectImageData(ObjectImageDataList list, ObjectImageData data) => list.Add(data);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    objectData.Count % 4 == 0)
                {
                    var m = new MultipleObjectUpdate
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID,
                    };
                    for (int i = 0; i < objectData.Count; i += 4)
                    {
                        var d = new MultipleObjectUpdate.ObjectDataEntry
                        {
                            ObjectLocalID = objectData[i].AsUInt
                        };
                        IValue position = objectData[i + 1];
                        IValue rotation = objectData[i + 2];
                        IValue scale = objectData[i + 3];
                        int dataSize = 0;
                        if(position is Vector3)
                        {
                            dataSize += 12;
                            d.Flags |= MultipleObjectUpdate.UpdateFlags.UpdatePosition;
                        }
                        if (rotation is Quaternion)
                        {
                            dataSize += 12;
                            d.Flags |= MultipleObjectUpdate.UpdateFlags.UpdateRotation;
                        }
                        if (scale is Vector3)
                        {
                            dataSize += 12;
                            d.Flags |= MultipleObjectUpdate.UpdateFlags.UpdateScale;
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
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

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName, "objectshape")]
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

        [APIExtension(ExtensionName, "objectshapelist")]
        [APIDisplayName("objectshapelist")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        [Serializable]
        public class VcObjectShapeList : List<VcObjectShape>
        {
            public int Length => Count;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction, "Add")]
        public void AddObjectShapeData(VcObjectShapeList list, VcObjectShape data) => list.Add(data);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
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
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
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

        [APIExtension(ExtensionName)]
        public const int VC_UPDATE_TYPE_FULL = 0;
        [APIExtension(ExtensionName)]
        public const int VC_UPDATE_TYPE_COMPRESSED = 1;

        [APIExtension(ExtensionName, "objectdata")]
        [APIDisplayName("objectdata")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public sealed class VcObjectData
        {
            public int LocalID;
            public int UpdateType;

            public int State;
            public LSLKey FullID = new LSLKey();
            public int CRC;
            public int PCode;
            public int Material;
            public int ClickAction;
            public Vector3 Scale;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;
            public Vector3 Acceleration;
            public int ParentID;
            public int UpdateFlags;
            public VcObjectShape ObjectShape = new VcObjectShape();
            public TextureEntryContainer TextureEntry = new TextureEntryContainer();
            public TextureAnimationEntryContainer TextureAnim = new TextureAnimationEntryContainer();
            public string NameValue = string.Empty;
            public ByteArrayApi.ByteArray Data = new ByteArrayApi.ByteArray();
            public string Text = string.Empty;
            public Vector3 TextColor;
            public double TextAlpha;
            public string MediaURL = string.Empty;
            public ParticleSystemContainer ParticleSystem = new ParticleSystemContainer();
            public VcExtraParams ExtraParams = new VcExtraParams();
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
                UpdateType = VC_UPDATE_TYPE_FULL;
                LocalID = (int)d.LocalID;
                State = d.State;
                FullID = d.FullID;
                CRC = (int)d.CRC;
                PCode = (int)d.PCode;
                Material = (int)d.Material;
                ClickAction = (int)d.ClickAction;
                Scale = d.Scale;
                if(d.ObjectData.Length >= 60)
                {
                    Position = new Vector3(d.ObjectData, 0);
                    Velocity = new Vector3(d.ObjectData, 12);
                    Acceleration = new Vector3(d.ObjectData, 24);
                    Rotation = new Quaternion(d.ObjectData, 36, true);
                    AngularVelocity = new Vector3(d.ObjectData, 48);
                }
                ParentID = (int)d.ParentID;
                UpdateFlags = (int)d.UpdateFlags;
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
                TextureAnim = new TextureAnimationEntryContainer(d.TextureAnim);
                NameValue = d.NameValue;
                Data = new ByteArrayApi.ByteArray(d.Data);
                Text = d.Text;
                TextColor = d.TextColor;
                TextAlpha = d.TextColor.A;
                MediaURL = d.MediaURL;
                ParticleSystem = new ParticleSystemContainer(d.PSBlock);
                ExtraParams = new VcExtraParams(d.ExtraParams);
                LoopedSound = d.LoopedSound;
                OwnerID = d.OwnerID;
                Gain = d.Gain;
                Flags = (int)d.Flags;
                Radius = d.Radius;
                JointType = d.JointType;
                JointPivot = d.JointPivot;
                JointAxisOrAnchor = d.JointAxisOrAnchor;
            }

            public VcObjectData(byte[] compressed, uint updateFlags)
            {
                UpdateFlags = (int)updateFlags;
                UpdateType = VC_UPDATE_TYPE_COMPRESSED;
                FullID = new UUID(compressed, 0);
                LocalID = (int)LEToUInt32(compressed, 16);
                PCode = compressed[20];
                State = compressed[21];
                CRC = (int)LEToUInt32(compressed, 22);
                Material = compressed[26];
                ClickAction = compressed[27];
                Scale = new Vector3(compressed, 28);
                Position = new Vector3(compressed, 40);
                Rotation = new Quaternion(compressed, 52, true);
                OwnerID = new UUID(compressed, 68); /* 68 + 16 */
                int offset = 84;
                var compressedFlags = (ObjectUpdateCompressed.CompressedFlags)LEToUInt32(compressed, 64);

                if ((compressedFlags & ObjectUpdateCompressed.CompressedFlags.HasAngularVelocity) != 0)
                {
                    AngularVelocity = new Vector3(compressed, offset);
                    offset += 12;
                }

                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.HasParent) != 0)
                {
                    Array.Reverse(compressed, offset, 4);
                    ParentID = (int)BitConverter.ToUInt32(compressed, offset);
                    offset += 4;
                }

                if ((compressedFlags & ObjectUpdateCompressed.CompressedFlags.Tree) != 0)
                {
                    ++offset;
                }

                if ((compressedFlags & ObjectUpdateCompressed.CompressedFlags.ScratchPad) != 0)
                {
                    byte len = compressed[offset++];
                    offset += len;
                }

                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.HasText) != 0)
                {
                    int endpos;
                    for(endpos = offset; compressed[endpos] != 0; ++endpos)
                    {
                    }
                    Text = compressed.FromUTF8Bytes(offset, endpos - offset);
                    offset = endpos + 1;
                    ColorAlpha textcolor = new ColorAlpha
                    {
                        R_AsByte = compressed[offset],
                        G_AsByte = compressed[offset + 1],
                        B_AsByte = compressed[offset + 2],
                        A_AsByte = (byte)(255 - compressed[offset + 3])
                    };
                    TextColor = textcolor;
                    TextAlpha = textcolor.A;
                    offset += 4;
                }

                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.MediaURL) != 0)
                {
                    int endpos;
                    for (endpos = offset; compressed[endpos] != 0; ++endpos)
                    {
                    }
                    MediaURL = compressed.FromUTF8Bytes(offset, endpos - offset);
                    offset = endpos + 1;
                }

                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.HasParticles) != 0)
                {
                    byte[] particledata = new byte[86];
                    Buffer.BlockCopy(compressed, offset, particledata, 0, 86);
                    offset += 86;
                    ParticleSystem = new ParticleSystemContainer(particledata);
                }

                int extrastart = offset;
                uint elemcount = compressed[offset++];
                while(elemcount-- != 0)
                {
                    int blocklen = (int)LEToUInt32(compressed, offset + 2);
                    offset += 6 + blocklen;
                }
                byte[] extraparam = new byte[offset - extrastart];
                Buffer.BlockCopy(compressed, extrastart, extraparam, 0, offset - extrastart);
                ExtraParams = new VcExtraParams(extraparam);

                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.HasSound) != 0)
                {
                    LoopedSound = new UUID(compressed, offset);
                    offset += 16;
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(compressed, offset, 4);
                    }
                    Gain = BitConverter.ToSingle(compressed, offset);
                    offset += 4;
                    Flags = compressed[offset++];
                    if (!BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(compressed, offset, 4);
                    }
                    Radius = BitConverter.ToSingle(compressed, offset);
                    offset += 4;
                }

                /* NameValue */
                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.HasNameValues) != 0)
                {
                    int endpos;
                    for (endpos = offset; compressed[endpos] != 0; ++endpos)
                    {
                    }
                    NameValue = compressed.FromUTF8Bytes(offset, endpos - offset);
                    offset = endpos + 1;
                }

                /* Shape */
                ObjectShape = new VcObjectShape
                {
                    PathCurve = compressed[offset],
                    PathBegin = LEToUInt16(compressed, offset + 1),
                    PathEnd = LEToUInt16(compressed, offset + 3),
                    PathScaleX = compressed[offset + 5],
                    PathScaleY = compressed[offset + 6],
                    PathShearX = compressed[offset + 7],
                    PathShearY = compressed[offset + 8],
                    PathTwist = (sbyte)compressed[offset + 9],
                    PathTwistBegin = (sbyte)compressed[offset + 10],
                    PathRadiusOffset = (sbyte)compressed[offset + 11],
                    PathTaperX = (sbyte)compressed[offset + 12],
                    PathTaperY = (sbyte)compressed[offset + 13],
                    PathRevolutions = compressed[offset + 14],
                    PathSkew = (sbyte)compressed[offset + 15],
                    ProfileCurve = compressed[offset + 16],
                    ProfileBegin = LEToUInt16(compressed, offset + 17),
                    ProfileEnd = LEToUInt16(compressed, offset + 19),
                    ProfileHollow = LEToUInt16(compressed, offset + 21)
                };
                offset += 23;

                int textureEntrySize = (int)LEToUInt32(compressed, offset);
                offset += 4;
                TextureEntry = new TextureEntryContainer(new TextureEntry(compressed, offset, textureEntrySize));
                offset += textureEntrySize;

                if((compressedFlags & ObjectUpdateCompressed.CompressedFlags.TextureAnimation) != 0)
                {
                    int textureAnimEntrySize = (int)LEToUInt32(compressed, offset);
                    offset += 4;
                    byte[] texAnimData = new byte[textureAnimEntrySize];
                    Buffer.BlockCopy(compressed, offset, texAnimData, 0, textureAnimEntrySize);
                    TextureAnim = new TextureAnimationEntryContainer(texAnimData);
                }
            }
        }

        private static uint LEToUInt32(byte[] data, int offset)
        {
            if(!BitConverter.IsLittleEndian)
            {
                byte[] nd = new byte[4];
                Buffer.BlockCopy(data, offset, nd, 0, 4);
                Array.Reverse(nd);
                data = nd;
                offset = 0;
            }
            return BitConverter.ToUInt32(data, offset);
        }

        private static ushort LEToUInt16(byte[] data, int offset)
        {
            if (!BitConverter.IsLittleEndian)
            {
                byte[] nd = new byte[2];
                Buffer.BlockCopy(data, offset, nd, 0, 2);
                Array.Reverse(nd);
                data = nd;
                offset = 0;
            }
            return BitConverter.ToUInt16(data, offset);
        }

        [APIExtension(ExtensionName, "objectdatalist")]
        [APIDisplayName("objectdatalist")]
        [APIAccessibleMembers("Count", "Length")]
        [APIIsVariableType]
        public class VcObjectDataList : List<VcObjectData>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<VcObjectData>
            {
                private readonly VcObjectDataList Src;
                private int Position = -1;

                public LSLEnumerator(VcObjectDataList src)
                {
                    Src = src;
                }

                public VcObjectData Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }

        [APIExtension(ExtensionName, "objectpropertiesdata")]
        [APIDisplayName("objectpropertiesdata")]
        [APIAccessibleMembers]
        [APIIsVariableType]
        public sealed class VcObjectPropertiesData
        {
            public LSLKey ObjectID = new LSLKey();
            public LSLKey CreatorID = new LSLKey();
            public LSLKey OwnerID = new LSLKey();
            public LSLKey GroupID = new LSLKey();
            public long CreationDate;
            public int BaseMask;
            public int OwnerMask;
            public int GroupMask;
            public int EveryoneMask;
            public int NextOwnerMask;
            public int OwnershipCost;
            public int SaleType;
            public int SalePrice;
            public int AggregatePerms;
            public int AggregatePermTextures;
            public int AggregatePermTexturesOwner;
            public int Category;
            public int InventorySerial;
            public LSLKey ItemID = new LSLKey();
            public LSLKey FolderID = new LSLKey();
            public LSLKey FromTaskID = new LSLKey();
            public LSLKey LastOwnerID = new LSLKey();
            public string Name = string.Empty;
            public string Description = string.Empty;
            public string TouchText = string.Empty;
            public string SitText = string.Empty;

            public VcObjectPropertiesData()
            {
            }

            public VcObjectPropertiesData(byte[] data)
            {
                if(data.Length < (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.BlockLength)
                {
                    return;
                }
                ObjectID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.ObjectID);
                CreatorID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.CreatorID);
                OwnerID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.OwnerID);
                GroupID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.GroupID);
                CreationDate = BytesToUInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.CreationDate);
                BaseMask = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.BaseMask);
                OwnerMask = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.OwnerMask);
                GroupMask = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.GroupMask);
                EveryoneMask = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.EveryoneMask);
                NextOwnerMask = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.NextOwnerMask);
                OwnershipCost = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.OwnershipCost);
                SaleType = data[(int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.SaleType];
                SalePrice = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.SalePrice);
                AggregatePerms = data[(int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.AggregatePerms];
                AggregatePermTextures = data[(int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.AggregatePermTextures];
                AggregatePermTexturesOwner = data[(int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.AggregatePermTexturesOwner];
                Category = BytesToInt32(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.Category);
                InventorySerial = BytesToUInt16(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.InventorySerial);
                ItemID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.ItemID);
                FolderID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.FolderID);
                FromTaskID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.FromTaskID);
                LastOwnerID = new UUID(data, (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.LastOwnerID);
                int pos = (int)ObjectPartLocalizedInfo.PropertiesFixedBlockOffset.BlockLength;
                if(pos < data.Length)
                {
                    int strLen = data[pos++];
                    int remLen = Math.Min(strLen, data.Length - pos);
                    if (remLen > 0)
                    {
                        Name = data.FromUTF8Bytes(pos, remLen);
                    }
                    pos += remLen;
                }
                if (pos < data.Length)
                {
                    int strLen = data[pos++];
                    int remLen = Math.Min(strLen, data.Length - pos);
                    if (remLen > 0)
                    {
                        Description = data.FromUTF8Bytes(pos, remLen);
                    }
                    pos += remLen;
                }
                if (pos < data.Length)
                {
                    int strLen = data[pos++];
                    int remLen = Math.Min(strLen, data.Length - pos);
                    if (remLen > 0)
                    {
                        TouchText = data.FromUTF8Bytes(pos, remLen);
                    }
                    pos += remLen;
                }
                if (pos < data.Length)
                {
                    int strLen = data[pos++];
                    int remLen = Math.Min(strLen, data.Length - pos);
                    if (remLen > 0)
                    {
                        SitText = data.FromUTF8Bytes(pos, remLen);
                    }
                    pos += remLen;
                }
            }

            private static uint BytesToUInt32(byte[] data, int pos)
            {
                byte[] b = data;
                if(!BitConverter.IsLittleEndian)
                {
                    b = new byte[4];
                    Buffer.BlockCopy(data, pos, b, 0, 4);
                    data = b;
                    pos = 0;
                }
                return BitConverter.ToUInt32(b, pos);
            }

            private static ushort BytesToUInt16(byte[] data, int pos)
            {
                byte[] b = data;
                if (!BitConverter.IsLittleEndian)
                {
                    b = new byte[2];
                    Buffer.BlockCopy(data, pos, b, 0, 2);
                    data = b;
                    pos = 0;
                }
                return BitConverter.ToUInt16(b, pos);
            }

            private static int BytesToInt32(byte[] data, int pos)
            {
                byte[] b = data;
                if (!BitConverter.IsLittleEndian)
                {
                    b = new byte[4];
                    Buffer.BlockCopy(data, pos, b, 0, 4);
                    data = b;
                    pos = 0;
                }
                return BitConverter.ToInt32(b, pos);
            }
        }

        [APIExtension(ExtensionName, "objectpropertieslist")]
        [APIDisplayName("objectpropertieslist")]
        [APIAccessibleMembers("Count", "Length")]
        [APIIsVariableType]
        public class VcObjectPropertiesDataList : List<VcObjectPropertiesData>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<VcObjectPropertiesData>
            {
                private readonly VcObjectPropertiesDataList Src;
                private int Position = -1;

                public LSLEnumerator(VcObjectPropertiesDataList src)
                {
                    Src = src;
                }

                public VcObjectPropertiesData Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }
    }
}
