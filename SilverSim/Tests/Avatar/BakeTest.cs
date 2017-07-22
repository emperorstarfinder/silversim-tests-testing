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
using SilverSim.Scene.Types.Agent;
using SilverSim.Scene.Types.Scene;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.ServiceInterfaces.Avatar;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Agent;
using SilverSim.Types.Asset;
using SilverSim.Types.Asset.Format;
using SilverSim.Types.Inventory;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        readonly Dictionary<UUID, string> m_AssetFiles = new Dictionary<UUID, string>();
        readonly Dictionary<UUID, string> m_AssetLinkDescriptions = new Dictionary<UUID, string>();
        string m_BakeEyesFilename;
        string m_BakeHeadFilename;
        string m_BakeUpperFilename;
        string m_BakeLowerFilename;
        string m_BakeHairFilename;

        string m_ReferenceBakeEyesFilename;
        string m_ReferenceBakeHeadFilename;
        string m_ReferenceBakeUpperFilename;
        string m_ReferenceBakeLowerFilename;
        string m_ReferenceBakeHairFilename;

        double m_ReferenceBakeEyesTolerance;
        double m_ReferenceBakeHeadTolerance;
        double m_ReferenceBakeUpperTolerance;
        double m_ReferenceBakeLowerTolerance;
        double m_ReferenceBakeHairTolerance;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AssetService = loader.GetService<AssetServiceInterface>(config.GetString("AssetService"));
            m_InventoryService = loader.GetService<InventoryServiceInterface>(config.GetString("InventoryService"));
            m_RootFolderID = new UUID(config.GetString("RootFolderID"));
            m_AgentOwner = new UUI(config.GetString("Owner"));

            m_BakeEyesFilename = config.GetString("BakeEyeFilename", string.Empty);
            m_BakeHeadFilename = config.GetString("BakeHeadFilename", string.Empty);
            m_BakeUpperFilename = config.GetString("BakeUpperFilename", string.Empty);
            m_BakeLowerFilename = config.GetString("BakeLowerFilename", string.Empty);
            m_BakeHairFilename = config.GetString("BakeHairFilename", string.Empty);

            m_ReferenceBakeEyesFilename = config.GetString("ReferenceBakeEyeFilename", string.Empty);
            m_ReferenceBakeHeadFilename = config.GetString("ReferenceBakeHeadFilename", string.Empty);
            m_ReferenceBakeUpperFilename = config.GetString("ReferenceBakeUpperFilename", string.Empty);
            m_ReferenceBakeLowerFilename = config.GetString("ReferenceBakeLowerFilename", string.Empty);
            m_ReferenceBakeHairFilename = config.GetString("ReferenceBakeHairFilename", string.Empty);

            double tolerance = config.GetDouble("ReferenceBakeTolerance", 0);
            m_ReferenceBakeEyesTolerance = config.GetDouble("ReferenceBakeEyesTolerance", tolerance);
            m_ReferenceBakeHeadTolerance = config.GetDouble("ReferenceBakeHeadTolerance", tolerance);
            m_ReferenceBakeUpperTolerance = config.GetDouble("ReferenceBakeUpperTolerance", tolerance);
            m_ReferenceBakeLowerTolerance = config.GetDouble("ReferenceBakeLowerTolerance", tolerance);
            m_ReferenceBakeHairTolerance = config.GetDouble("ReferenceBakeHairTolerance", tolerance);

            foreach (string key in config.GetKeys())
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
                else if(key.StartsWith("FolderType-") && UUID.TryParse(key.Substring(11), out id))
                {
                    m_InventoryFolderTypes[id] = (InventoryType)config.GetInt(key);
                }
                else if (key.StartsWith("Item-") && UUID.TryParse(key.Substring(5), out id))
                {
                    m_InventoryFiles[id] = config.GetString(key);
                }
                else if (key.StartsWith("ItemParent-") && UUID.TryParse(key.Substring(11), out id))
                {
                    m_InventoryItemParents[id] = new UUID(config.GetString(key));
                }
                else if (key.StartsWith("ItemAssetID-") && UUID.TryParse(key.Substring(12), out id))
                {
                    m_InventoryItemAssetIDs[id] = new UUID(config.GetString(key));
                }
                else if (key.StartsWith("ItemIsLink-") && UUID.TryParse(key.Substring(11), out id))
                {
                    m_AssetLinkDescriptions[id] = config.GetString(key);
                }
                else if (key.StartsWith("Asset-") && UUID.TryParse(key.Substring(6), out id))
                {
                    m_AssetFiles[id] = config.GetString(key);
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
            SceneInterface.ResourceAssetService resourceAssets = new SceneInterface.ResourceAssetService();

            m_Log.Info("Loading all default assets");
            /* copy all default assets */
            foreach(UUID id in resourceAssets.GetKnownAssets())
            {
                m_AssetService.Store(resourceAssets[id]);
            }

            if (!m_InventoryService.Folder.TryGetValue(m_AgentOwner.ID, AssetType.RootFolder, out rootFolder))
            {
                rootFolder = new InventoryFolder(m_RootFolderID);
                rootFolder.Owner = m_AgentOwner;
                rootFolder.Name = "My Inventory";
                rootFolder.InventoryType = InventoryType.Folder;
                rootFolder.ParentFolderID = UUID.Zero;
                rootFolder.Version = 1;
                m_InventoryService.Folder.Add(rootFolder);
            }

            UUID rootFolderID = m_InventoryService.Folder[m_AgentOwner.ID, AssetType.RootFolder].ID;

            foreach(KeyValuePair<UUID, string> kvp in m_AssetFiles)
            {
                AssetData asset = new AssetData();
                asset.ID = kvp.Key;
                asset.FileName = Path.GetFileName(kvp.Value);
                using (FileStream fs = new FileStream(kvp.Value, FileMode.Open, FileAccess.Read))
                {
                    asset.Data = fs.ReadToStreamEnd();
                }
                m_AssetService.Store(asset);
            }

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

            foreach (KeyValuePair<UUID, string> kvp in m_InventoryFiles)
            {
                InventoryItem item;
                using (var fs = new FileStream(kvp.Value, FileMode.Open, FileAccess.Read))
                {
                    item = LoadInventoryItem(fs);
                }
                item.SetNewID(kvp.Key);

                UUID parentFolderID;
                if (m_InventoryItemParents.TryGetValue(kvp.Key, out parentFolderID))
                {
                    item.ParentFolderID = parentFolderID;
                }
                else
                {
                    item.ParentFolderID = rootFolderID;
                }
                UUID assetid;
                if(m_InventoryItemAssetIDs.TryGetValue(kvp.Key, out assetid))
                {
                    item.AssetID = assetid;
                }
                string linkDescription;
                if (m_AssetLinkDescriptions.TryGetValue(kvp.Key, out linkDescription))
                {
                    item.Description = linkDescription;
                    item.AssetType = AssetType.Link;
                }
                item.Owner = m_AgentOwner;
                m_InventoryService.Item.Add(item);
            }

            AgentBakeAppearance.AgentInfo info = new AgentBakeAppearance.AgentInfo(m_AgentOwner, m_InventoryService, m_AssetService, UUID.Zero);
            AppearanceInfo appearance = AgentBakeAppearance.LoadAppearanceFromCurrentOutfit(info, m_AssetService, m_Log.Info);
            UUID bakeEyesId = appearance.AvatarTextures[(int)AvatarTextureIndex.EyesBaked];
            UUID bakeHeadId = appearance.AvatarTextures[(int)AvatarTextureIndex.HeadBaked];
            UUID bakeUpperId = appearance.AvatarTextures[(int)AvatarTextureIndex.UpperBaked];
            UUID bakeLowerId = appearance.AvatarTextures[(int)AvatarTextureIndex.LowerBaked];
            UUID bakeHairId = appearance.AvatarTextures[(int)AvatarTextureIndex.HairBaked];

            if (!string.IsNullOrEmpty(m_BakeEyesFilename))
            {
                using (var fs = new FileStream(m_BakeEyesFilename, FileMode.Create, FileAccess.Write))
                {
                    m_AssetService.Data[bakeEyesId].CopyTo(fs);
                }
            }
            if (!string.IsNullOrEmpty(m_BakeHeadFilename))
            {
                using (var fs = new FileStream(m_BakeHeadFilename, FileMode.Create, FileAccess.Write))
                {
                    m_AssetService.Data[bakeHeadId].CopyTo(fs);
                }
            }
            if (!string.IsNullOrEmpty(m_BakeUpperFilename))
            {
                using (var fs = new FileStream(m_BakeUpperFilename, FileMode.Create, FileAccess.Write))
                {
                    m_AssetService.Data[bakeUpperId].CopyTo(fs);
                }
            }
            if (!string.IsNullOrEmpty(m_BakeLowerFilename))
            {
                using (var fs = new FileStream(m_BakeLowerFilename, FileMode.Create, FileAccess.Write))
                {
                    m_AssetService.Data[bakeLowerId].CopyTo(fs);
                }
            }
            if (!string.IsNullOrEmpty(m_BakeHairFilename))
            {
                using (var fs = new FileStream(m_BakeHairFilename, FileMode.Create, FileAccess.Write))
                {
                    m_AssetService.Data[bakeHairId].CopyTo(fs);
                }
            }

            bool referenceok = true;
            if(!string.IsNullOrEmpty(m_ReferenceBakeEyesFilename))
            {
                double actTolerance = CompareImages(m_AssetService[bakeEyesId], m_ReferenceBakeHairFilename);
                if(actTolerance > m_ReferenceBakeEyesTolerance)
                {
                    referenceok = false;
                    m_Log.ErrorFormat("Eyes reference bake tolerance exceeded {0} > {1}", actTolerance, m_ReferenceBakeHairTolerance);
                }
            }
            if (!string.IsNullOrEmpty(m_ReferenceBakeHeadFilename))
            {
                double actTolerance = CompareImages(m_AssetService[bakeHeadId], m_ReferenceBakeHeadFilename);
                if (actTolerance > m_ReferenceBakeHeadTolerance)
                {
                    referenceok = false;
                    m_Log.ErrorFormat("Head reference bake tolerance exceeded {0} > {1}", actTolerance, m_ReferenceBakeHeadTolerance);
                }
            }
            if (!string.IsNullOrEmpty(m_ReferenceBakeUpperFilename))
            {
                double actTolerance = CompareImages(m_AssetService[bakeEyesId], m_ReferenceBakeUpperFilename);
                if (actTolerance > m_ReferenceBakeUpperTolerance)
                {
                    referenceok = false;
                    m_Log.ErrorFormat("Upper reference bake tolerance exceeded {0} > {1}", actTolerance, m_ReferenceBakeUpperTolerance);
                }
            }
            if (!string.IsNullOrEmpty(m_ReferenceBakeLowerFilename))
            {
                double actTolerance = CompareImages(m_AssetService[bakeEyesId], m_ReferenceBakeLowerFilename);
                if (actTolerance > m_ReferenceBakeLowerTolerance)
                {
                    referenceok = false;
                    m_Log.ErrorFormat("Lower reference bake tolerance exceeded {0} > {1}", actTolerance, m_ReferenceBakeLowerTolerance);
                }
            }
            if (!string.IsNullOrEmpty(m_ReferenceBakeHairFilename))
            {
                double actTolerance = CompareImages(m_AssetService[bakeEyesId], m_ReferenceBakeHairFilename);
                if (actTolerance > m_ReferenceBakeHairTolerance)
                {
                    referenceok = false;
                    m_Log.ErrorFormat("Hair reference bake tolerance exceeded {0} > {1}", actTolerance, m_ReferenceBakeHairTolerance);
                }
            }

            return referenceok;
        }

        private double CompareImages(AssetData asset, string imagefilename)
        {
            using (Stream img1input = asset.InputStream)
            using (Image img1 = Image.FromStream(asset.InputStream))
            using (Image img2 = Image.FromFile(imagefilename))
            using (var bmp1 = new Bitmap(img1))
            using (var bmp2 = new Bitmap(img2))
            {
                if (img1.Width != img2.Width || img1.Height != img2.Height)
                {
                    return 0;
                }

                long diff = 0;
                for (int y = 0; y < img1.Height; ++y)
                {
                    for (int x = 0; x < img1.Width; ++x)
                    {
                        System.Drawing.Color a = bmp1.GetPixel(x, y);
                        System.Drawing.Color b = bmp2.GetPixel(x, y);
                        diff += Math.Abs(a.R - b.R);
                        diff += Math.Abs(a.G - b.G);
                        diff += Math.Abs(a.B - b.B);
                    }
                }
                return (double)diff / ((long)img1.Width * img1.Height * 3 * 255);
            }
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
