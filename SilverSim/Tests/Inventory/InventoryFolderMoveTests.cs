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
    public class InventoryFolderMoveTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        InventoryServiceInterface m_InventoryService;
        InventoryServiceInterface m_BackendInventoryService;
        UGUI m_UserID;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            string inventoryServiceName = config.GetString("InventoryService");
            m_InventoryService = loader.GetService<InventoryServiceInterface>(inventoryServiceName);
            m_BackendInventoryService = loader.GetService<InventoryServiceInterface>(config.GetString("BackendInventoryService", inventoryServiceName));
            m_UserID = new UGUI(config.GetString("User"));
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

            InventoryFolder folder;

            InventoryFolder rootFolder = m_InventoryService.Folder[m_UserID.ID, AssetType.RootFolder];
            InventoryFolder notecardFolder = m_InventoryService.Folder[m_UserID.ID, AssetType.Notecard];
            UUID inventoryId = UUID.Random;

            var testFolder = new InventoryFolder(inventoryId)
            {
                Name = "Test Name",
                ParentFolderID = rootFolder.ID,
                DefaultType = AssetType.Unknown,
                Owner = m_UserID,
                Version = 1
            };
            m_InventoryService.Folder.Add(testFolder);

            m_Log.Info("Move folder to notecard folder");
            m_InventoryService.Folder.Move(m_UserID.ID, testFolder.ID, notecardFolder.ID);

            m_Log.Info("Check for parentfolder change");
            folder = m_InventoryService.Folder[m_UserID.ID, testFolder.ID];
            if (folder.ParentFolderID != notecardFolder.ID)
            {
                return false;
            }

            m_Log.Info("Move folder to root folder");
            m_InventoryService.Folder.Move(m_UserID.ID, testFolder.ID, rootFolder.ID);

            m_Log.Info("Check for parentfolder change");
            folder = m_InventoryService.Folder[m_UserID.ID, testFolder.ID];
            if (folder.ParentFolderID != rootFolder.ID)
            {
                return false;
            }

            m_Log.Info("Move folder to unknown parentfolder");
            UUID unknownFolderID;
            do
            {
                unknownFolderID = UUID.Random;
            } while (m_BackendInventoryService.Folder.ContainsKey(m_UserID.ID, unknownFolderID));
            try
            {
                m_InventoryService.Folder.Move(m_UserID.ID, testFolder.ID, unknownFolderID);
                return false;
            }
            catch (InvalidParentFolderIdException)
            {
                /* this is the expected case */
            }

            m_Log.Info("Check for parentfolder not being changed");
            folder = m_InventoryService.Folder[m_UserID.ID, testFolder.ID];
            if (folder.ParentFolderID != rootFolder.ID)
            {
                return false;
            }

            m_Log.Info("Deleting folder");
            m_InventoryService.Folder.Delete(m_UserID.ID, testFolder.ID);

            return true;
        }
    }
}
