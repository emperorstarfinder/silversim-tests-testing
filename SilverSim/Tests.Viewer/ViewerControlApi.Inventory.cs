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

using SilverSim.AISv3.Client;
using SilverSim.Scene.Types.Agent;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Scripting.Lsl.Api.Agents.Properties;
using SilverSim.Scripting.Lsl.Api.Hashtable;
using SilverSim.Tests.Viewer.Inventory;
using SilverSim.Tests.Viewer.UDP;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using SilverSim.Viewer.Messages.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SilverSim.Tests.Viewer
{
    public partial class ViewerControlApi
    {
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_INVENTORY_UDP_ONLY = 0;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_INVENTORY_FETCH_CAPS = 1;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_INVENTORY_FETCH_AND_CREATE_CAPS = 2;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_INVENTORY_MIXED_AISV3 = 3;
        [APIExtension(ExtensionName)]
        public const int VC_AGENT_INVENTORY_FULL_AISV3 = 4;

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public AgentInventoryApi.AgentInventory GetAgentInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse) =>
            GetAgentInventory(instance, agent, rootFolderID, seedResponse, VC_AGENT_INVENTORY_FULL_AISV3);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public AgentInventoryApi.AgentInventory GetAgentInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse,
            int inventoryOption)
        {
            lock(instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit))
                {
                    IValue value;
                    string aisv3_agent_uri = seedResponse.TryGetValue("InventoryAPIv3", out value) ? value.ToString() : string.Empty;
                    string fetchinventory2_agent_uri = seedResponse.TryGetValue("FetchInventory2", out value) ? value.ToString() : string.Empty;
                    string fetchinventorydescendents2_agent_uri = seedResponse.TryGetValue("FetchInventoryDescendents2", out value) ? value.ToString() : string.Empty;
                    string createinventorycategory_agent_uri = seedResponse.TryGetValue("CreateInventoryCategory", out value) ? value.ToString() : string.Empty;

                    if (inventoryOption >= VC_AGENT_INVENTORY_FULL_AISV3 && !string.IsNullOrEmpty(aisv3_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new AISv3ClientConnector(aisv3_agent_uri), new UGUI(viewerCircuit.AgentID), false);
                    }

                    if(inventoryOption >= VC_AGENT_INVENTORY_FETCH_CAPS && !string.IsNullOrEmpty(fetchinventory2_agent_uri) &&
                        !string.IsNullOrEmpty(fetchinventorydescendents2_agent_uri))
                    {
                        if (inventoryOption >= VC_AGENT_INVENTORY_FETCH_AND_CREATE_CAPS && !string.IsNullOrEmpty(createinventorycategory_agent_uri))
                        {
                            return new AgentInventoryApi.AgentInventory(instance, new InventoryV2Client(viewerCircuit, fetchinventorydescendents2_agent_uri, fetchinventory2_agent_uri, createinventorycategory_agent_uri, rootFolderID), new UGUI(viewerCircuit.AgentID), false);
                        }

                        return new AgentInventoryApi.AgentInventory(instance, new InventoryV2Client(viewerCircuit, fetchinventorydescendents2_agent_uri, fetchinventory2_agent_uri, rootFolderID), new UGUI(viewerCircuit.AgentID), false);
                    }

                    return new AgentInventoryApi.AgentInventory(instance, new LLUDPInventoryClient(viewerCircuit, rootFolderID), new UGUI(viewerCircuit.AgentID), false);
                }

                return new AgentInventoryApi.AgentInventory();
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public AgentInventoryApi.AgentInventory GetLocalAgentInventory(
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
                    return new AgentInventoryApi.AgentInventory(instance, m_AgentInventoryService, new UGUI(viewerCircuit.AgentID), false);
                }
                return new AgentInventoryApi.AgentInventory();
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public AgentInventoryApi.AgentInventory GetLibraryInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int circuitCode,
            LSLKey libraryAgentId,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse) =>
            GetLibraryInventory(instance, agent, circuitCode, libraryAgentId, rootFolderID, seedResponse, VC_AGENT_INVENTORY_FULL_AISV3);

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public AgentInventoryApi.AgentInventory GetLibraryInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int circuitCode,
            LSLKey libraryAgentId,
            LSLKey rootFolderID,
            HashtableApi.Hashtable seedResponse,
            int inventoryOption)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)circuitCode, out viewerCircuit))
                {
                    IValue value;
                    string aisv3_agent_uri = seedResponse.TryGetValue("LibraryAPIv3", out value) ? value.ToString() : string.Empty;
                    string fetchlib2_agent_uri = seedResponse.TryGetValue("FetchLib2", out value) ? value.ToString() : string.Empty;
                    string fetchlibdescendents2_agent_uri = seedResponse.TryGetValue("FetchLibDescendents2", out value) ? value.ToString() : string.Empty;

                    if (inventoryOption >= VC_AGENT_INVENTORY_FULL_AISV3 && !string.IsNullOrEmpty(aisv3_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new AISv3ClientConnector(aisv3_agent_uri), new UGUI(libraryAgentId), false);
                    }

                    if (inventoryOption >= VC_AGENT_INVENTORY_FETCH_CAPS && !string.IsNullOrEmpty(fetchlib2_agent_uri) &&
                        !string.IsNullOrEmpty(fetchlibdescendents2_agent_uri))
                    {
                        return new AgentInventoryApi.AgentInventory(instance, new InventoryV2Client(viewerCircuit, fetchlibdescendents2_agent_uri, fetchlib2_agent_uri, rootFolderID), new UGUI(libraryAgentId), false);
                    }

                    return new AgentInventoryApi.AgentInventory(instance, new LLUDPInventoryClient(viewerCircuit, rootFolderID), new UGUI(libraryAgentId), false);
                }

                return new AgentInventoryApi.AgentInventory();
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public void CheckInventory(
            ScriptInstance instance,
            ViewerAgentAccessor agent)
        {
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent))
                {
                    actagent.InventoryService.CheckInventory(agent.AgentID);
                }
            }
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public LSLKey GetFolderForType(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            int assetType)
        {
            UUID folderID = UUID.Zero;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                InventoryFolder folder;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent) &&
                    actagent.InventoryService.Folder.TryGetValue(agent.AgentID, (AssetType)assetType, out folder))
                {
                    folderID = folder.ID;
                }
            }
            return folderID;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public LSLKey AddInventoryItem(
            ScriptInstance instance, 
            ViewerAgentAccessor agent,
            LSLKey parentFolderID,
            string name,
            string description,
            int inventoryType,
            int assetType,
            LSLKey assetID,
            int invFlags,
            int basePerm,
            int ownerPerm,
            int groupPerm,
            int everyOnePerm,
            int nextOwnerPerm)
        {
            UUID itemID = UUID.Zero;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent))
                {
                    itemID = UUID.Random;
                    var item = new InventoryItem(itemID)
                    {
                        Name = name,
                        Description = description,
                        InventoryType = (InventoryType)inventoryType,
                        AssetType = (AssetType)assetType,
                        AssetID = assetID,
                        Creator = UGUI.Unknown,
                        Owner = actagent.Owner,
                        CreationDate = Date.Now,
                        LastOwner = actagent.Owner,
                        Flags = (InventoryFlags)invFlags,
                        ParentFolderID = parentFolderID.AsUUID
                    };
                    item.Permissions.Base = (InventoryPermissionsMask)basePerm;
                    item.Permissions.Current = (InventoryPermissionsMask)ownerPerm;
                    item.Permissions.Group = (InventoryPermissionsMask)groupPerm;
                    item.Permissions.EveryOne = (InventoryPermissionsMask)everyOnePerm;
                    item.Permissions.NextOwner = (InventoryPermissionsMask)nextOwnerPerm;
                    actagent.InventoryService.Item.Add(item);
                }
            }
            return itemID;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int GetInventoryItemBasePerm(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            int perm = 0;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                InventoryItem item;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent) &&
                    actagent.InventoryService.Item.TryGetValue(agent.AgentID, itemID.AsUUID, out item))
                {
                    perm = (int)item.Permissions.Base;
                }
            }
            return perm;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int GetInventoryItemOwnerPerm(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            int perm = 0;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                InventoryItem item;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent) &&
                    actagent.InventoryService.Item.TryGetValue(agent.AgentID, itemID.AsUUID, out item))
                {
                    perm = (int)item.Permissions.Current;
                }
            }
            return perm;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int GetInventoryItemGroupPerm(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            int perm = 0;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                InventoryItem item;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent) &&
                    actagent.InventoryService.Item.TryGetValue(agent.AgentID, itemID.AsUUID, out item))
                {
                    perm = (int)item.Permissions.Group;
                }
            }
            return perm;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int GetInventoryItemEveryOnePerm(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            int perm = 0;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                InventoryItem item;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent) &&
                    actagent.InventoryService.Item.TryGetValue(agent.AgentID, itemID.AsUUID, out item))
                {
                    perm = (int)item.Permissions.EveryOne;
                }
            }
            return perm;
        }

        [APIExtension(ExtensionName, APIUseAsEnum.MemberFunction)]
        public int GetInventoryItemNextOwnerPerm(
            ScriptInstance instance,
            ViewerAgentAccessor agent,
            LSLKey itemID)
        {
            int perm = 0;
            lock (instance)
            {
                ViewerConnection vc;
                ViewerCircuit viewerCircuit;
                IAgent actagent;
                InventoryItem item;
                if (m_Clients.TryGetValue(agent.AgentID, out vc) &&
                    vc.ViewerCircuits.TryGetValue((uint)agent.CircuitCode, out viewerCircuit) &&
                    instance.Part.ObjectGroup.Scene.RootAgents.TryGetValue(agent.AgentID, out actagent) &&
                    actagent.InventoryService.Item.TryGetValue(agent.AgentID, itemID.AsUUID, out item))
                {
                    perm = (int)item.Permissions.NextOwner;
                }
            }
            return perm;
        }

        [APIExtension(ExtensionName, "updateinventoryfolder")]
        [APIDisplayName("updateinventoryfolder")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public sealed class AgentUpdateInventoryFolder
        {
            public LSLKey ID = UUID.Zero;
            public LSLKey ParentFolderID = UUID.Zero;
            public int DefaultType;
            public string Name = string.Empty;

            public AgentUpdateInventoryFolder()
            {
            }

            public AgentUpdateInventoryFolder(BulkUpdateInventory.FolderDataEntry e)
            {
                ID = e.FolderID;
                ParentFolderID = e.ParentID;
                DefaultType = (int)e.DefaultType;
                Name = e.Name;
            }
        }

        [APIExtension(ExtensionName, "updateinventoryfolderlist")]
        [APIDisplayName("updateinventoryfolderlist")]
        [APIAccessibleMembers("Count", "Length")]
        [APIIsVariableType]
        public class AgentUpdateInventoryFolderList : List<AgentUpdateInventoryFolder>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<AgentUpdateInventoryFolder>
            {
                private readonly AgentUpdateInventoryFolderList Src;
                private int Position = -1;

                public LSLEnumerator(AgentUpdateInventoryFolderList src)
                {
                    Src = src;
                }

                public AgentUpdateInventoryFolder Current => Src[Position];

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }

                public bool MoveNext() => ++Position < Src.Count;

                public void Reset() => Position = -1;
            }

            public LSLEnumerator GetLslForeachEnumerator() => new LSLEnumerator(this);
        }

        [APIExtension(ExtensionName, "updateinventoryitem")]
        [APIDisplayName("updateinventoryitem")]
        [APIIsVariableType]
        [APIAccessibleMembers]
        public sealed class AgentUpdateInventoryItem
        {
            public int CallbackID;
            public int BaseMask;
            public int OwnerMask;
            public int GroupMask;
            public int EveryoneMask;
            public int NextOwnerMask;

            public LSLKey ParentFolderID = UUID.Zero;
            public LSLKey AssetID = UUID.Zero;
            public int IsGroupOwned;
            public LSLKey GroupID = UUID.Zero;
            public int SalePrice;
            public int SaleType;
            public int AssetType;
            public LSLKey CreatorID = UUID.Zero;
            public LSLKey OwnerID = UUID.Zero;
            public int Flags;
            public int InventoryType;
            public long CreationDate;
            public LSLKey ID = UUID.Zero;

            public string Name = string.Empty;
            public string Description = string.Empty;

            public AgentUpdateInventoryItem()
            {
            }

            public AgentUpdateInventoryItem(BulkUpdateInventory.ItemDataEntry e)
            {
                CallbackID = (int)e.CallbackID;
                BaseMask = (int)e.BaseMask;
                OwnerMask = (int)e.OwnerMask;
                GroupMask = (int)e.GroupMask;
                EveryoneMask = (int)e.EveryoneMask;
                NextOwnerMask = (int)e.NextOwnerMask;

                ParentFolderID = e.FolderID;
                AssetID = e.AssetID;
                IsGroupOwned = e.IsGroupOwned.ToLSLBoolean();
                GroupID = e.GroupID;
                SalePrice = e.SalePrice;
                SaleType = (int)e.SaleType;
                AssetType = (int)e.Type;
                CreatorID = e.CreatorID;
                OwnerID = e.OwnerID;
                Flags = (int)e.Flags;
                InventoryType = (int)e.InvType;
                CreationDate = e.CreationDate;
                ID = e.ItemID;
                Name = e.Name;
                Description = e.Description;
            }

            public AgentUpdateInventoryItem(UpdateCreateInventoryItem.ItemDataEntry e)
            {
                CallbackID = (int)e.CallbackID;
                BaseMask = (int)e.BaseMask;
                OwnerMask = (int)e.OwnerMask;
                GroupMask = (int)e.GroupMask;
                EveryoneMask = (int)e.EveryoneMask;
                NextOwnerMask = (int)e.NextOwnerMask;

                ParentFolderID = e.FolderID;
                AssetID = e.AssetID;
                IsGroupOwned = e.IsGroupOwned.ToLSLBoolean();
                GroupID = e.GroupID;
                SalePrice = e.SalePrice;
                SaleType = (int)e.SaleType;
                AssetType = (int)e.Type;
                CreatorID = e.CreatorID;
                OwnerID = e.OwnerID;
                Flags = (int)e.Flags;
                InventoryType = (int)e.InvType;
                CreationDate = e.CreationDate;
                ID = e.ItemID;
                Name = e.Name;
                Description = e.Description;
            }
        }

        [APIExtension(ExtensionName, "updateinventoryitemlist")]
        [APIDisplayName("updateinventoryitemlist")]
        [APIAccessibleMembers("Count", "Length")]
        [APIIsVariableType]
        public class AgentUpdateInventoryItemList : List<AgentUpdateInventoryItem>
        {
            public int Length => Count;

            public sealed class LSLEnumerator : IEnumerator<AgentUpdateInventoryItem>
            {
                private readonly AgentUpdateInventoryItemList Src;
                private int Position = -1;

                public LSLEnumerator(AgentUpdateInventoryItemList src)
                {
                    Src = src;
                }

                public AgentUpdateInventoryItem Current => Src[Position];

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
