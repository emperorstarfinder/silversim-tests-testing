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
using SilverSim.Types.Inventory;
using SilverSim.Types.Primitive;
using SilverSim.Viewer.Messages.Object;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
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

        [APIExtension("ViewerControl", APIUseAsEnum.MemberFunction, "SendObjectImage")]
        public void SendObjectShape(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int localID,
            string mediaURL,
            TextureEntryContainer textureEntry)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue(agent.CircuitCode, out viewerCircuit))
                {
                    var d = new ObjectImage.ObjectDataEntry
                    {
                        ObjectLocalID = (uint)localID,
                        MediaURL = mediaURL,
                        TextureEntry = textureEntry.GetBytes()
                    };
                    var m = new ObjectImage
                    {
                        AgentID = agent.AgentID,
                        SessionID = viewerCircuit.SessionID
                    };
                    m.ObjectData.Add(d);
                    viewerCircuit.SendMessage(m);
                }
            }
        }
    }
}
