// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Inventory;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SilverSim.Database.Memory.Inventory
{
    sealed class MemoryInventoryItemService : InventoryItemServiceInterface
    {
        readonly MemoryInventoryService m_Service;
        public MemoryInventoryItemService(MemoryInventoryService service)
        {
            m_Service = service;
        }

        public override bool ContainsKey(UUID key)
        {
            foreach(RwLockedDictionary<UUID, InventoryItem> dict in m_Service.m_Items.Values)
            {
                if(dict.ContainsKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool TryGetValue(UUID key, out InventoryItem item)
        {
            foreach (RwLockedDictionary<UUID, InventoryItem> dict in m_Service.m_Items.Values)
            {
                if (dict.TryGetValue(key, out item))
                {
                    return true;
                }
            }

            item = default(InventoryItem);
            return false;
        }

        public override InventoryItem this[UUID key]
        {
            get
            {
                InventoryItem item;
                if(!TryGetValue(key, out item))
                {
                    throw new KeyNotFoundException();
                }
                return item;
            }
        }

        public override bool ContainsKey(UUID principalID, UUID key)
        {
            RwLockedDictionary<UUID, InventoryItem> dict;
            return m_Service.m_Items.TryGetValue(principalID, out dict) && dict.ContainsKey(key);
        }

        public override bool TryGetValue(UUID principalID, UUID key, out InventoryItem item)
        {
            RwLockedDictionary<UUID, InventoryItem> dict;
            item = default(InventoryItem);
            if(m_Service.m_Items.TryGetValue(principalID, out dict) && dict.TryGetValue(key, out item))
            {
                item = new InventoryItem(item);
                return true;
            }
            return false;
        }

        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override InventoryItem this[UUID principalID, UUID key]
        {
            get 
            {
                InventoryItem item;
                if(!TryGetValue(principalID, key, out item))
                {
                    throw new KeyNotFoundException();
                }
                return item;
            }
        }

        public override void Add(InventoryItem item)
        {
            m_Service.m_Items[item.Owner.ID].Add(item.ID, new InventoryItem(item));
            IncrementVersion(item.Owner.ID, item.ParentFolderID);
        }

        public override void Update(InventoryItem item)
        {
            RwLockedDictionary<UUID, InventoryItem> itemSet;
            InventoryItem storedItem;
            if(m_Service.m_Items.TryGetValue(item.Owner.ID, out itemSet) &&
                itemSet.TryGetValue(item.ID, out storedItem))
            {
                storedItem.AssetID = item.AssetID;
                storedItem.Name = item.Name;
                storedItem.Description = item.Description;
                storedItem.Permissions.Base = item.Permissions.Base;
                storedItem.Permissions.Current = item.Permissions.Current;
                storedItem.Permissions.EveryOne = item.Permissions.EveryOne;
                storedItem.Permissions.NextOwner = item.Permissions.NextOwner;
                storedItem.Permissions.Group = item.Permissions.Group;
                storedItem.SaleInfo.Price = item.SaleInfo.Price;
                storedItem.SaleInfo.Type = item.SaleInfo.Type;
                IncrementVersion(item.Owner.ID, item.ParentFolderID);
            }
        }

        public override void Delete(UUID principalID, UUID id)
        {
            InventoryItem item;
            RwLockedDictionary<UUID, InventoryItem> itemSet;
            if (m_Service.m_Items.TryGetValue(principalID, out itemSet) &&
                itemSet.Remove(id, out item))
            {
                IncrementVersion(principalID, item.ParentFolderID);
                return;
            }
            throw new InventoryItemNotFoundException(id);
        }

        public override void Move(UUID principalID, UUID id, UUID toFolderID)
        {
            InventoryItem item;
            RwLockedDictionary<UUID, InventoryItem> itemSet;
            if (m_Service.m_Items.TryGetValue(principalID, out itemSet) &&
                itemSet.TryGetValue(id, out item))
            {
                UUID oldFolderID = item.ParentFolderID;
                item.ParentFolderID = toFolderID;
                IncrementVersion(principalID, oldFolderID);
                IncrementVersion(principalID, item.ParentFolderID);
                return;
            }

            throw new InventoryFolderNotStoredException(id);
        }

        [SuppressMessage("Gendarme.Rules.Exceptions", "DoNotSwallowErrorsCatchingNonSpecificExceptionsRule")]
        void IncrementVersion(UUID principalID, UUID folderID)
        {
            RwLockedDictionary<UUID, InventoryFolder> folderSet;
            InventoryFolder folder;
            if(m_Service.m_Folders.TryGetValue(principalID, out folderSet) &&
                folderSet.TryGetValue(folderID, out folder))
            {
                Interlocked.Increment(ref folder.Version);
            }
        }

    }
}
