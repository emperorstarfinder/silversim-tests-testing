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

#pragma warning disable CS0618

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.Inventory
{
    public class InventoryItemCreateTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        InventoryServiceInterface m_InventoryService;
        InventoryServiceInterface m_BackendInventoryService;
        UGUI m_UserID;
        UGUI m_CreatorID;
        UGUI m_LastOwnerID;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            string inventoryServiceName = config.GetString("InventoryService");
            m_InventoryService = loader.GetService<InventoryServiceInterface>(inventoryServiceName);
            m_BackendInventoryService = loader.GetService<InventoryServiceInterface>(config.GetString("BackendInventoryService", inventoryServiceName));
            m_UserID = new UGUI(config.GetString("User"));
            m_CreatorID = new UGUI(config.GetString("Creator"));
            m_LastOwnerID = new UGUI(config.GetString("LastOwner"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        private bool IsDataEqual(InventoryItem a, InventoryItem b)
        {
            var mismatches = new List<string>();
            if (a.Name != b.Name)
            {
                mismatches.Add("Name");
            }
            if (a.Description != b.Description)
            {
                mismatches.Add("Description");
            }
            if (a.AssetID != b.AssetID)
            {
                mismatches.Add("AssetID");
            }
            if (a.AssetType != b.AssetType)
            {
                mismatches.Add("AssetType");
            }
            if (a.Creator.ID != b.Creator.ID)
            {
                mismatches.Add("Creator.ID");
            }
            if (a.Owner.ID != b.Owner.ID)
            {
                mismatches.Add("Owner.ID");
            }
            if (a.LastOwner.ID != b.LastOwner.ID)
            {
                mismatches.Add("LastOwner.ID");
            }
            if (a.Group.ID != b.Group.ID)
            {
                mismatches.Add("Group.ID");
                m_Log.InfoFormat("Mismatch Group.ID {0} != {1}", a.Group.ID, b.Group.ID);
            }
            if (a.Flags != b.Flags)
            {
                mismatches.Add("Flags");
            }
            if (a.Permissions.Base != b.Permissions.Base)
            {
                mismatches.Add("Permissions.Base");
            }
            if (a.Permissions.Current != b.Permissions.Current)
            {
                mismatches.Add("Permissions.Current");
            }
            if (a.Permissions.NextOwner != b.Permissions.NextOwner)
            {
                mismatches.Add("Permissions.NextOwner");
            }
            if (a.Permissions.Group != b.Permissions.Group)
            {
                mismatches.Add("Permissions.Group");
            }
            if (a.Permissions.EveryOne != b.Permissions.EveryOne)
            {
                mismatches.Add("Permissions.EveryOne");
            }
            if (a.ParentFolderID != b.ParentFolderID)
            {
                mismatches.Add("ParentFolderID");
            }
            if (a.SaleInfo.Price != b.SaleInfo.Price)
            {
                mismatches.Add("SaleInfo.Price");
            }
            if (a.SaleInfo.Type != b.SaleInfo.Type)
            {
                mismatches.Add("SaleInfo.Type");
            }
            if (a.IsGroupOwned != b.IsGroupOwned)
            {
                mismatches.Add("IsGroupOwned");
            }
            if (a.InventoryType != b.InventoryType)
            {
                mismatches.Add("InventoryType");
            }
            if (a.CreationDate.DateTimeToUnixTime() != b.CreationDate.DateTimeToUnixTime())
            {
                m_Log.WarnFormat("CreationDate A({0} => {1}) B({2} => {3})", a.CreationDate, a.CreationDate.DateTimeToUnixTime(), b.CreationDate, b.CreationDate.DateTimeToUnixTime());
                mismatches.Add("CreationDate");
            }

            if (mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatch at {0}", string.Join(" ", mismatches));
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool Run()
        {
            List<InventoryItem> result;
            m_Log.Info("Create User Inventory");
            try
            {
                m_BackendInventoryService.CheckInventory(m_UserID.ID);
            }
            catch
            {
                return false;
            }

            InventoryItem item;

            InventoryFolder rootFolder = m_InventoryService.Folder[m_UserID.ID, AssetType.RootFolder];
            UUID inventoryId = UUID.Random;

            m_Log.InfoFormat("Testing non-existence 1");
            if (m_InventoryService.Item.ContainsKey(inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 2");
            if (m_InventoryService.Item.ContainsKey(m_UserID.ID, inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 3");
            if (m_InventoryService.Item.TryGetValue(inventoryId, out item))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 4");
            if (m_InventoryService.Item.TryGetValue(m_UserID.ID, inventoryId, out item))
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 5");
            try
            {
                item = m_InventoryService.Item[inventoryId];
                return false;
            }
            catch (InventoryItemNotFoundException)
            {
                /* this is the okay case */
            }
            m_Log.InfoFormat("Testing non-existence 6");
            try
            {
                item = m_InventoryService.Item[m_UserID.ID, inventoryId];
                return false;
            }
            catch (InventoryItemNotFoundException)
            {
                /* this is the okay case */
            }
            m_Log.InfoFormat("Testing non-existence 7");
            result = m_InventoryService.Item[m_UserID.ID, new List<UUID> { inventoryId }];
            if (result.Count != 0)
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 8");
            result = m_InventoryService.Folder.GetItems(m_UserID.ID, rootFolder.ID);
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    return false;
                }
            }
            m_Log.InfoFormat("Testing non-existence 9");
            result = m_InventoryService.Folder.Content[m_UserID.ID, rootFolder.ID].Items;
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    return false;
                }
            }

            var testItem = new InventoryItem(inventoryId)
            {
                Name = "Test Name",
                Description = "Test Description",
                AssetID = UUID.Random,
                AssetType = AssetType.Notecard,
                Creator = m_UserID,
                Flags = InventoryFlags.ObjectPermOverwriteNextOwner,
                LastOwner = m_UserID,
                ParentFolderID = rootFolder.ID,
                SaleInfo = new InventoryItem.SaleInfoData
                {
                    Price = 10,
                    Type = InventoryItem.SaleInfoData.SaleType.Copy
                },
                IsGroupOwned = false,
                Group = new UGI(UUID.Random),
                InventoryType = InventoryType.Notecard,
                Owner = m_UserID,
                CreationDate = Date.Now
            };
            testItem.Permissions.Base = InventoryPermissionsMask.Every;
            testItem.Permissions.Current = InventoryPermissionsMask.All;
            testItem.Permissions.NextOwner = InventoryPermissionsMask.Copy;
            testItem.Permissions.Group = InventoryPermissionsMask.Damage;
            testItem.Permissions.EveryOne = InventoryPermissionsMask.Move;
            m_InventoryService.Item.Add(testItem);
            inventoryId = testItem.ID;

            m_Log.InfoFormat("Testing existence 1");
            if (!m_InventoryService.Item.ContainsKey(inventoryId))
            {
                return false;
            }
            m_Log.InfoFormat("Testing existence 2");
            if (!m_InventoryService.Item.ContainsKey(m_UserID.ID, inventoryId))
            {
                return false;
            }
            m_Log.InfoFormat("Testing existence 3");
            if (!m_InventoryService.Item.TryGetValue(inventoryId, out item))
            {
                return false;
            }
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }

            m_Log.InfoFormat("Testing existence 4");
            if (!m_InventoryService.Item.TryGetValue(m_UserID.ID, inventoryId, out item))
            {
                return false;
            }
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }
            m_Log.InfoFormat("Testing existence 5");
            item = m_InventoryService.Item[inventoryId];
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }
            m_Log.InfoFormat("Testing existence 6");
            item = m_InventoryService.Item[m_UserID.ID, inventoryId];
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }
            m_Log.InfoFormat("Testing existence 7");
            result = m_InventoryService.Item[m_UserID.ID, new List<UUID> { inventoryId }];
            if (result.Count != 1)
            {
                return false;
            }
            if (!IsDataEqual(result[0], testItem))
            {
                return false;
            }

            item = null;
            m_Log.InfoFormat("Testing non-existence 8");
            result = m_InventoryService.Folder.GetItems(m_UserID.ID, rootFolder.ID);
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    item = checkItem;
                }
            }
            if (item == null)
            {
                return false;
            }
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 9");
            result = m_InventoryService.Folder.Content[m_UserID.ID, rootFolder.ID].Items;
            item = null;
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    item = checkItem;
                }
            }
            if (item == null)
            {
                return false;
            }
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }

            m_Log.InfoFormat("Updating item");
            testItem.Name = "Test Name 2";
            testItem.Description = "Test Description 2";
            testItem.Flags = InventoryFlags.None;
            testItem.AssetID = UUID.Random;
            testItem.SaleInfo.Price = 20;
            testItem.SaleInfo.Type = InventoryItem.SaleInfoData.SaleType.Original;
            testItem.Permissions.Current = InventoryPermissionsMask.None;
            testItem.Permissions.EveryOne = InventoryPermissionsMask.None;
            testItem.Permissions.NextOwner = InventoryPermissionsMask.None;
            testItem.Permissions.Group = InventoryPermissionsMask.None;

            m_InventoryService.Item.Update(testItem);

            m_Log.InfoFormat("Testing changes");
            item = m_InventoryService.Item[m_UserID.ID, inventoryId];
            if (!IsDataEqual(item, testItem))
            {
                return false;
            }

            m_Log.InfoFormat("Deleting item");
            m_InventoryService.Item.Delete(m_UserID.ID, inventoryId);

            m_Log.InfoFormat("Testing non-existence 1");
            if (m_InventoryService.Item.ContainsKey(inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 2");
            if (m_InventoryService.Item.ContainsKey(m_UserID.ID, inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 3");
            if (m_InventoryService.Item.TryGetValue(inventoryId, out item))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 4");
            if (m_InventoryService.Item.TryGetValue(m_UserID.ID, inventoryId, out item))
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 5");
            try
            {
                item = m_InventoryService.Item[inventoryId];
                return false;
            }
            catch (InventoryItemNotFoundException)
            {
                /* this is the okay case */
            }
            m_Log.InfoFormat("Testing non-existence 6");
            try
            {
                item = m_InventoryService.Item[m_UserID.ID, inventoryId];
                return false;
            }
            catch (InventoryItemNotFoundException)
            {
                /* this is the okay case */
            }

            m_Log.InfoFormat("Testing non-existence 7");
            result = m_InventoryService.Item[m_UserID.ID, new List<UUID> { inventoryId }];
            if (result.Count != 0)
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 8");
            result = m_InventoryService.Folder.GetItems(m_UserID.ID, rootFolder.ID);
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    return false;
                }
            }
            m_Log.InfoFormat("Testing non-existence 9");
            result = m_InventoryService.Folder.Content[m_UserID.ID, rootFolder.ID].Items;
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    return false;
                }
            }

            m_Log.InfoFormat("Creating the item");
            m_InventoryService.Item.Add(testItem);
            inventoryId = testItem.ID;

            m_Log.InfoFormat("Deleting item");
            List<UUID> deleted = m_InventoryService.Item.Delete(m_UserID.ID, new List<UUID> { inventoryId });
            if (!deleted.Contains(inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 1");
            if (m_InventoryService.Item.ContainsKey(inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 2");
            if (m_InventoryService.Item.ContainsKey(m_UserID.ID, inventoryId))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 3");
            if (m_InventoryService.Item.TryGetValue(inventoryId, out item))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 4");
            if (m_InventoryService.Item.TryGetValue(m_UserID.ID, inventoryId, out item))
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 5");
            try
            {
                item = m_InventoryService.Item[inventoryId];
                return false;
            }
            catch (InventoryItemNotFoundException)
            {
                /* this is the okay case */
            }
            m_Log.InfoFormat("Testing non-existence 6");
            try
            {
                item = m_InventoryService.Item[m_UserID.ID, inventoryId];
                return false;
            }
            catch (InventoryItemNotFoundException)
            {
                /* this is the okay case */
            }
            m_Log.InfoFormat("Testing non-existence 7");
            result = m_InventoryService.Item[m_UserID.ID, new List<UUID> { inventoryId }];
            if (result.Count != 0)
            {
                return false;
            }
            m_Log.InfoFormat("Testing non-existence 8");
            result = m_InventoryService.Folder.GetItems(m_UserID.ID, rootFolder.ID);
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    return false;
                }
            }
            m_Log.InfoFormat("Testing non-existence 9");
            result = m_InventoryService.Folder.Content[m_UserID.ID, rootFolder.ID].Items;
            foreach (InventoryItem checkItem in result)
            {
                if (checkItem.ID == inventoryId)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
