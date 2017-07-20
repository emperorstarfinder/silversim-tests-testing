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
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.ServiceInterfaces.Avatar;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Asset.Format;
using SilverSim.Types.Inventory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SilverSim.Tests.Avatar
{
    public class BakeTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        AssetServiceInterface m_AssetService;
        InventoryServiceInterface m_InventoryService;
        UUI m_AgentOwner;
        UUID m_RootFolderID;
        readonly Dictionary<UUID, string> m_InventoryFolders = new Dictionary<UUID, string>();
        readonly Dictionary<UUID, UUID> m_InventoryFolderParents = new Dictionary<UUID, UUID>();
        readonly Dictionary<UUID, InventoryType> m_InventoryFolderTypes = new Dictionary<UUID, InventoryType>();
        readonly Dictionary<UUID, string> m_InventoryFiles = new Dictionary<UUID, string>();
        readonly Dictionary<UUID, UUID> m_InventoryItemParents = new Dictionary<UUID, UUID>();
        readonly Dictionary<UUID, UUID> m_InventoryItemAssetIDs = new Dictionary<UUID, UUID>();

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AssetService = loader.GetService<AssetServiceInterface>(config.GetString("AssetService"));
            m_InventoryService = loader.GetService<InventoryServiceInterface>(config.GetString("InventoryService"));
            m_RootFolderID = new UUID(config.GetString("RootFolderID"));
            m_AgentOwner = new UUI(config.GetString("Owner"));
            foreach(string key in config.GetKeys())
            {
                UUID id;
                if(key.StartsWith("Folder-") && UUID.TryParse(key.Substring(7), out id))
                {
                    m_InventoryFolders[id] = config.GetString(key);
                }
                else if (key.StartsWith("FolderParent-") && UUID.TryParse(key.Substring(13), out id))
                {
                    m_InventoryFolderParents[id] = new UUID(config.GetString(key));
                }
                else if(key.StartsWith("FolderType-") && UUID.TryParse(key.Substring(13), out id))
                {
                    m_InventoryFolderTypes[id] = (InventoryType)int.Parse(config.GetString(key));
                }
                else if (key.StartsWith("Item-") && UUID.TryParse(key.Substring(5), out id))
                {
                    m_InventoryFiles[id] = config.GetString(key);
                }
                else if (key.StartsWith("ItemParent-") && UUID.TryParse(key.Substring(11), out id))
                {
                    m_InventoryItemParents[id] = new UUID(config.GetString(key));
                }
                else if (key.StartsWith("ItemAssetID-") && UUID.TryParse(key.Substring(11), out id))
                {
                    m_InventoryItemAssetIDs[id] = new UUID(config.GetString(key));
                }
            }
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            InventoryFolder rootFolder;
            if(!m_InventoryService.Folder.TryGetValue(m_AgentOwner.ID, AssetType.RootFolder, out rootFolder))
            {
                rootFolder = new InventoryFolder(m_RootFolderID);
                rootFolder.Name = "My Inventory";
                rootFolder.InventoryType = InventoryType.Folder;
                rootFolder.ParentFolderID = UUID.Zero;
                rootFolder.Version = 1;
                m_InventoryService.Folder.Add(rootFolder);
            }
            m_InventoryService.CheckInventory(m_AgentOwner.ID);

            UUID rootFolderID = m_InventoryService.Folder[m_AgentOwner.ID, AssetType.RootFolder].ID;

            foreach(KeyValuePair<UUID, string> kvp in m_InventoryFolders)
            {
                InventoryFolder folder = new InventoryFolder(kvp.Key);
                InventoryType folderType;
                if(m_InventoryFolderTypes.TryGetValue(kvp.Key, out folderType))
                {
                    folder.InventoryType = folderType;
                }

                UUID parentFolderID;
                if(m_InventoryFolderParents.TryGetValue(kvp.Key, out parentFolderID))
                {
                    folder.ParentFolderID = parentFolderID;
                }
                else
                {
                    folder.ParentFolderID = rootFolderID;
                }

                folder.Owner = m_AgentOwner;
                folder.Name = kvp.Value;
                m_InventoryService.Folder.Add(folder);
            }

            foreach(KeyValuePair<UUID, string> kvp in m_InventoryFiles)
            {
                using (FileStream fs = new FileStream(kvp.Value, FileMode.Open, FileAccess.Read))
                {
                    InventoryItem item = LoadInventoryItem(fs);
                    UUID parentFolderID;
                    if (m_InventoryItemParents.TryGetValue(kvp.Key, out parentFolderID))
                    {
                        item.ParentFolderID = parentFolderID;
                    }
                    else
                    {
                        item.ParentFolderID = rootFolderID;
                    }
                }

            }
            return true;
        }

        private InventoryItem LoadInventoryItem(
            Stream s)
        {
            using (XmlTextReader reader = new XmlTextReader(new ObjectXmlStreamFilter(s)))
            {
                for (;;)
                {
                    if (!reader.Read())
                    {
                        throw new InvalidDataException();
                    }

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "InventoryItem")
                            {
                                return LoadInventoryItemData(reader);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private InventoryItem LoadInventoryItemData(
            XmlTextReader reader)
        {
            var item = new InventoryItem()
            {
                Owner = m_AgentOwner
            };
            for (;;)
            {
                if (!reader.Read())
                {
                    throw new InvalidDataException();
                }

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "Name":
                                item.Name = reader.ReadElementValueAsString();
                                break;

                            case "InvType":
                                item.InventoryType = (InventoryType)reader.ReadElementValueAsInt();
                                break;

                            case "CreatorUUID":
                                item.Creator.ID = new UUID(reader.ReadElementValueAsString());
                                break;

                            case "CreatorData":
                                {
                                    string creatorData = reader.ReadElementValueAsString();
                                    try
                                    {
                                        item.Creator.CreatorData = creatorData;
                                    }
                                    catch
                                    {
                                        /* ignore misformatted creator data */
                                    }
                                }
                                break;

                            case "CreationDate":
                                item.CreationDate = Date.UnixTimeToDateTime(reader.ReadElementValueAsULong());
                                break;

                            case "Description":
                                item.Description = reader.ReadElementValueAsString();
                                break;

                            case "AssetType":
                                item.AssetType = (AssetType)reader.ReadElementValueAsInt();
                                break;

                            case "SalePrice":
                                item.SaleInfo.Price = reader.ReadElementValueAsInt();
                                break;

                            case "SaleType":
                                item.SaleInfo.Type = (InventoryItem.SaleInfoData.SaleType)reader.ReadElementValueAsUInt();
                                break;

                            case "BasePermissions":
                                item.Permissions.Base = (InventoryPermissionsMask)reader.ReadElementValueAsUInt();
                                break;

                            case "CurrentPermissions":
                                item.Permissions.Current = (InventoryPermissionsMask)reader.ReadElementValueAsUInt();
                                break;

                            case "EveryOnePermissions":
                                item.Permissions.EveryOne = (InventoryPermissionsMask)reader.ReadElementValueAsUInt();
                                break;

                            case "NextPermissions":
                                item.Permissions.NextOwner = (InventoryPermissionsMask)reader.ReadElementValueAsUInt();
                                break;

                            case "Flags":
                                item.Flags = (InventoryFlags)reader.ReadElementValueAsUInt();
                                break;

                            case "GroupID":
                                item.Group.ID = reader.ReadElementValueAsString();
                                break;

                            case "GroupOwned":
                                item.IsGroupOwned = reader.ReadElementValueAsBoolean();
                                break;

                            case "ID":
                                item.SetNewID(UUID.Parse(reader.ReadElementValueAsString()));
                                break;

                            case "Owner":
                            default:
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Skip();
                                }
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name != "InventoryItem")
                        {
                            throw new InvalidDataException();
                        }
                        return item;

                    default:
                        break;
                }
            }
        }
    }
}
