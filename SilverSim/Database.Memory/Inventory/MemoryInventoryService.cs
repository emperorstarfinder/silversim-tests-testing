// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Account;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SilverSim.Database.Memory.Inventory
{
    #region Service Implementation
    [Description("Memory Inventory Backend")]
    public sealed class MemoryInventoryService : InventoryServiceInterface, IPlugin, IUserAccountDeleteServiceInterface
    {
        readonly MemoryInventoryItemService m_InventoryItemService;
        readonly MemoryInventoryFolderService m_InventoryFolderService;

        readonly RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<UUID, InventoryFolder>> m_Folders = new RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<UUID, InventoryFolder>>(delegate () { return new RwLockedDictionary<UUID, InventoryFolder>(); });
        readonly RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<UUID, InventoryItem>> m_Items = new RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<UUID, InventoryItem>>(delegate () { return new RwLockedDictionary<UUID, InventoryItem>(); });

        public MemoryInventoryService()
        {
            m_InventoryItemService = new MemoryInventoryItemService(m_Items, m_Folders);
            m_InventoryFolderService = new MemoryInventoryFolderService(m_Items, m_Folders);
        }

        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override InventoryFolderServiceInterface Folder
        {
            get
            {
                return m_InventoryFolderService;
            }
        }

        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override InventoryItemServiceInterface Item
        {
            get 
            {
                return m_InventoryItemService;
            }
        }

        public override List<InventoryItem> GetActiveGestures(UUID principalID)
        {
            RwLockedDictionary<UUID, InventoryItem> agentitems;
            return (m_Items.TryGetValue(principalID, out agentitems)) ?
                new List<InventoryItem>(from item in agentitems.Values where item.AssetType == AssetType.Gesture && (item.Flags & InventoryFlags.GestureActive) != 0 select new InventoryItem(item)) :
                new List<InventoryItem>();
        }

        public void Startup(ConfigurationLoader loader)
        {
            /* nothing to do */
        }

        public void Remove(UUID scopeID, UUID userAccount)
        {
            m_Items.Remove(userAccount);
            m_Folders.Remove(userAccount);
        }
    }
    #endregion

    #region Factory
    [PluginName("Inventory")]
    public class MemoryInventoryServiceFactory : IPluginFactory
    {
        public MemoryInventoryServiceFactory()
        {

        }

        public IPlugin Initialize(ConfigurationLoader loader, IConfig ownSection)
        {
            return new MemoryInventoryService();
        }
    }
    #endregion
}
