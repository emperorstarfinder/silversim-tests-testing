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

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using System.Reflection;

namespace SilverSim.Tests.Inventory
{
    public class InventoryItemMoveTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        InventoryServiceInterface m_InventoryService;
        InventoryServiceInterface m_BackendInventoryService;
        UUI m_UserID;
        UUI m_CreatorID;
        UUI m_LastOwnerID;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            string inventoryServiceName = config.GetString("InventoryService");
            m_InventoryService = loader.GetService<InventoryServiceInterface>(inventoryServiceName);
            m_BackendInventoryService = loader.GetService<InventoryServiceInterface>(config.GetString("BackendInventoryService", inventoryServiceName));
            m_UserID = new UUI(config.GetString("User"));
            m_CreatorID = new UUI(config.GetString("Creator"));
            m_LastOwnerID = new UUI(config.GetString("LastOwner"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
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
            InventoryFolder notecardFolder = m_InventoryService.Folder[m_UserID.ID, AssetType.Notecard];
            UUID inventoryId = UUID.Random;

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
            m_BackendInventoryService.Item.Add(testItem);

            m_Log.InfoFormat("Move item to notecard folder");
            m_InventoryService.Item.Move(m_UserID.ID, testItem.ID, notecardFolder.ID);

            m_Log.InfoFormat("Check for folder change");
            item = m_InventoryService.Item[m_UserID.ID, testItem.ID];
            if(item.ParentFolderID != notecardFolder.ID)
            {
                return false;
            }

            m_Log.InfoFormat("Move item to root folder");
            m_InventoryService.Item.Move(m_UserID.ID, testItem.ID, rootFolder.ID);

            m_Log.InfoFormat("Check for folder change");
            item = m_InventoryService.Item[m_UserID.ID, testItem.ID];
            if (item.ParentFolderID != rootFolder.ID)
            {
                return false;
            }

            m_Log.InfoFormat("Move item to unknown folder");
            UUID unknownFolderID;
            do
            {
                unknownFolderID = UUID.Random;
            } while (m_BackendInventoryService.Folder.ContainsKey(unknownFolderID));
            try
            {
                m_InventoryService.Item.Move(m_UserID.ID, testItem.ID, unknownFolderID);
                return false;
            }
            catch(InvalidParentFolderIdException)
            {
                /* this is the expected case */
            }

            m_Log.InfoFormat("Check for folder not being changed");
            item = m_InventoryService.Item[m_UserID.ID, testItem.ID];
            if (item.ParentFolderID != rootFolder.ID)
            {
                return false;
            }

            m_Log.InfoFormat("Deleting item");
            m_InventoryService.Item.Delete(m_UserID.ID, testItem.ID);

            return true;
        }
    }
}
