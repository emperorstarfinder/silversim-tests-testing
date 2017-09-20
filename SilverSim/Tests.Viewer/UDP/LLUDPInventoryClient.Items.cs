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

using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset.Format;
using SilverSim.Types.Inventory;
using SilverSim.Viewer.Messages;
using SilverSim.Viewer.Messages.Inventory;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SilverSim.Tests.Viewer.UDP
{
    public partial class LLUDPInventoryClient : IInventoryItemServiceInterface
    {
        private class FetchInventoryListener
        {
            public AutoResetEvent Event = new AutoResetEvent(false);
            public FetchInventoryReply Reply;
        };

        private readonly RwLockedDictionary<UUID, FetchInventoryListener> m_FetchInventoryReply = new RwLockedDictionary<UUID, FetchInventoryListener>();
        private void HandleUpdateInventoryItem(Message m)
        {

        }

        private void HandleUpdateCreateInventoryItem(Message m)
        {

        }

        private void HandleFetchInventoryReply(Message m)
        {
            var reply = (FetchInventoryReply)m;
            FetchInventoryListener listener;
            if (m_FetchInventoryReply.TryGetValue(reply.ItemData[0].ItemID, out listener))
            {
                listener.Reply = reply;
                listener.Event.Set();
            }
        }

        InventoryItem IInventoryItemServiceInterface.this[UUID key]
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        InventoryItem IInventoryItemServiceInterface.this[UUID principalID, UUID key]
        {
            get
            {
                InventoryItem item;
                if(!Item.TryGetValue(principalID, key, out item))
                {
                    throw new InventoryItemNotFoundException(key);
                }
                return item;
            }
        }

        List<InventoryItem> IInventoryItemServiceInterface.this[UUID principalID, List<UUID> itemids]
        {
            get
            {
                var result = new List<InventoryItem>();
                foreach(UUID itemid in itemids)
                {
                    InventoryItem item;
                    if(Item.TryGetValue(principalID, itemid, out item))
                    {
                        result.Add(item);
                    }
                }
                return result;
            }
        }

        void IInventoryItemServiceInterface.Add(InventoryItem item)
        {
            var req = new CreateInventoryItem
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
                CallbackID = 0,
                FolderID = item.ParentFolderID,
                TransactionID = UUID.Zero,
                NextOwnerMask = item.Permissions.NextOwner,
                AssetType = item.AssetType,
                InvType = item.InventoryType,
                WearableType = (WearableType)(item.Flags & InventoryFlags.WearablesTypeMask),
                Name = item.Name,
                Description = item.Description
            };
            m_ViewerCircuit.SendMessage(req);
        }

        bool IInventoryItemServiceInterface.ContainsKey(UUID key)
        {
            throw new NotSupportedException();
        }

        bool IInventoryItemServiceInterface.ContainsKey(UUID principalID, UUID key)
        {
            InventoryItem item;
            return Item.TryGetValue(principalID, key, out item);
        }

        void IInventoryItemServiceInterface.Delete(UUID principalID, UUID id)
        {
            var req = new RemoveInventoryItem
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.InventoryData.Add(id);
            m_ViewerCircuit.SendMessage(req);
        }

        List<UUID> IInventoryItemServiceInterface.Delete(UUID principalID, List<UUID> ids)
        {
            var req = new RemoveInventoryItem
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.InventoryData.AddRange(ids);
            m_ViewerCircuit.SendMessage(req);
            return new List<UUID>(ids);
        }

        void IInventoryItemServiceInterface.Move(UUID principalID, UUID id, UUID newFolder)
        {
            var req = new MoveInventoryItem
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.InventoryData.Add(new MoveInventoryItem.InventoryDataEntry
            {
                FolderID = newFolder,
                ItemID = id,
                NewName = string.Empty
            });
            throw new NotImplementedException();
        }

        bool IInventoryItemServiceInterface.TryGetValue(UUID key, out InventoryItem item)
        {
            throw new NotSupportedException();
        }

        bool IInventoryItemServiceInterface.TryGetValue(UUID principalID, UUID key, out InventoryItem item)
        {
            var req = new FetchInventory
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
            };
            req.InventoryData.Add(new FetchInventory.InventoryDataEntry
            {
                ItemID = key,
                OwnerID = principalID
            });

            var listener = new FetchInventoryListener();
            m_FetchInventoryReply[key] = listener;
            m_ViewerCircuit.SendMessage(req);

            if(!listener.Event.WaitOne(5000))
            {
                throw new InventoryItemNotFoundException(key);
            }
            m_FetchInventoryReply.Remove(key);
            if(listener.Reply == null)
            {
                throw new InventoryItemNotFoundException(key);
            }

            FetchInventoryReply.ItemDataEntry d = listener.Reply.ItemData[0];

            item = new InventoryItem(d.ItemID)
            {
                Name = d.Name,
                SaleInfo = new InventoryItem.SaleInfoData
                {
                    Price = d.SalePrice,
                    Type = d.SaleType
                },
                Flags = d.Flags,
                InventoryType = d.InvType,
                AssetType = d.Type,
                AssetID = d.AssetID,
                IsGroupOwned = d.IsGroupOwned,
                Permissions = new InventoryPermissionsData
                {
                    NextOwner = d.NextOwnerMask,
                    EveryOne = d.EveryoneMask,
                    Group = d.GroupMask,
                    Current = d.OwnerMask,
                    Base = d.BaseMask
                },
                Group = new UGI(d.GroupID),
                Owner = new UUI(d.OwnerID),
                Creator = new UUI(d.CreatorID),
                ParentFolderID = d.FolderID,
                Description = d.Description
            };
            return true;
        }

        void IInventoryItemServiceInterface.Update(InventoryItem item)
        {
            var req = new UpdateInventoryItem
            {
                AgentID = m_ViewerCircuit.AgentID,
                SessionID = m_ViewerCircuit.SessionID,
                TransactionID = UUID.Zero,
            };
            req.InventoryData.Add(new UpdateInventoryItem.InventoryDataEntry
            {
                ItemID = item.ID,
                Description = item.Description,
                Name = item.Name,
                SalePrice = item.SaleInfo.Price,
                SaleType = item.SaleInfo.Type,
                Flags = item.Flags,
                InvType = item.InventoryType,
                Type = item.AssetType,
                TransactionID = UUID.Zero,
                IsGroupOwned = item.IsGroupOwned,
                NextOwnerMask = item.Permissions.NextOwner,
                EveryoneMask = item.Permissions.EveryOne,
                GroupMask = item.Permissions.Group,
                OwnerMask = item.Permissions.Current,
                BaseMask = item.Permissions.Base,
                GroupID = item.Group.ID,
                OwnerID = item.Owner.ID,
                CreatorID = item.Creator.ID,
                CallbackID = 0,
                FolderID = item.ParentFolderID,
                CreationDate = item.CreationDate.AsUInt,
            });
            m_ViewerCircuit.SendMessage(req);
        }
    }
}
