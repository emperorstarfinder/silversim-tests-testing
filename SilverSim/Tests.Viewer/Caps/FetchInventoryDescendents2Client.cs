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

using SilverSim.Http.Client;
using SilverSim.ServiceInterfaces.Inventory;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using SilverSim.Types.StructuredData.Llsd;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilverSim.Tests.Viewer.Caps
{
    public abstract class FetchInventoryDescendents2Client : IInventoryFolderServiceInterface, IInventoryFolderContentServiceInterface
    {
        public int TimeoutMs { get; set; }
        private readonly string m_CapabilityUri;
        private readonly UUID m_RootFolderID;

        protected FetchInventoryDescendents2Client(UUID rootfolderid, string uri)
        {
            m_RootFolderID = rootfolderid;
            m_CapabilityUri = uri;
            TimeoutMs = 20000;
        }

        InventoryFolder IInventoryFolderServiceInterface.this[UUID key]
        {
            get
            {
                InventoryFolder folder;
                if(!((IInventoryFolderServiceInterface)this).TryGetValue(key, out folder))
                {
                    throw new InventoryFolderNotFoundException(key);
                }
                return folder;
            }
        }

        InventoryFolder IInventoryFolderServiceInterface.this[UUID principalID, UUID key]
        {
            get
            {
                InventoryFolder folder;
                if (!((IInventoryFolderServiceInterface)this).TryGetValue(principalID, key, out folder))
                {
                    throw new InventoryFolderNotFoundException(key);
                }
                return folder;
            }
        }

        InventoryFolder IInventoryFolderServiceInterface.this[UUID principalID, AssetType type]
        {
            get
            {
                InventoryFolder folder;
                if (!((IInventoryFolderServiceInterface)this).TryGetValue(principalID, type, out folder))
                {
                    throw new InventoryFolderNotFoundException();
                }
                return folder;
            }
        }

        InventoryFolderContent IInventoryFolderContentServiceInterface.this[UUID principalID, UUID folderID]
        {
            get
            {
                InventoryFolderContent folder;
                if (!((IInventoryFolderContentServiceInterface)this).TryGetValue(principalID, folderID, out folder))
                {
                    throw new InventoryFolderNotFoundException(folderID);
                }
                return folder;
            }
        }

        List<InventoryFolderContent> IInventoryFolderContentServiceInterface.this[UUID principalID, UUID[] folderIDs]
        {
            get
            {
                var result = new List<InventoryFolderContent>();
                foreach(KeyValuePair<InventoryFolder, InventoryFolderContent> kvp in GetFolderContents(folderIDs, true).Values)
                {
                    result.Add(kvp.Value);
                }
                return result;
            }
        }

        IInventoryFolderContentServiceInterface IInventoryFolderServiceInterface.Content => this;

        public abstract void Add(InventoryFolder folder);

        bool IInventoryFolderServiceInterface.ContainsKey(UUID key)
        {
            return GetFolderContents(new UUID[] { key }, false).Count == 1;
        }

        bool IInventoryFolderServiceInterface.ContainsKey(UUID principalID, UUID key)
        {
            return GetFolderContents(new UUID[] { key }, false).Count == 1;
        }

        bool IInventoryFolderServiceInterface.ContainsKey(UUID principalID, AssetType type)
        {
            InventoryFolder folder;
            return ((IInventoryFolderServiceInterface)this).TryGetValue(principalID, type, out folder);
        }

        bool IInventoryFolderContentServiceInterface.ContainsKey(UUID principalID, UUID folderID)
        {
            return GetFolderContents(new UUID[] { folderID }, false).Count == 1;
        }

        public abstract void Delete(UUID principalID, UUID folderID);

        public abstract List<UUID> Delete(UUID principalID, List<UUID> folderIDs);

        List<InventoryFolder> IInventoryFolderServiceInterface.GetFolders(UUID principalID, UUID key)
        {
            InventoryFolderContent content;
            if (((IInventoryFolderContentServiceInterface)this).TryGetValue(principalID, key, out content))
            {
                return content.Folders;
            }
            throw new InventoryFolderNotFoundException(key);
        }

        List<InventoryItem> IInventoryFolderServiceInterface.GetItems(UUID principalID, UUID key)
        {
            InventoryFolderContent content;
            if(((IInventoryFolderContentServiceInterface)this).TryGetValue(principalID, key, out content))
            {
                return content.Items;
            }
            throw new InventoryFolderNotFoundException(key);
        }

        void IInventoryFolderServiceInterface.IncrementVersion(UUID principalID, UUID folderID)
        {
            /* not implemented */
        }

        public abstract void Move(UUID principalID, UUID folderID, UUID toFolderID);

        public abstract void Purge(UUID folderID);

        public abstract void Purge(UUID principalID, UUID folderID);

        bool IInventoryFolderServiceInterface.TryGetValue(UUID key, out InventoryFolder folder)
        {
            InventoryFolderContent content;
            if(!TryGetValue(key, out folder, out content))
            {
                return false;
            }
            if(folder.ParentFolderID == UUID.Zero)
            {
                return true;
            }
            InventoryFolder ignfolder;
            if (!TryGetValue(folder.ParentFolderID, out ignfolder, out content))
            {
                return true;
            }
            foreach (InventoryFolder realfolder in content.Folders)
            {
                if(realfolder.ID == key)
                {
                    folder = realfolder;
                }
            }
            return true;
        }

        bool IInventoryFolderServiceInterface.TryGetValue(UUID principalID, UUID key, out InventoryFolder folder) =>
            ((IInventoryFolderServiceInterface)this).TryGetValue(key, out folder);

        bool IInventoryFolderServiceInterface.TryGetValue(UUID principalID, AssetType type, out InventoryFolder folder)
        {
            InventoryFolderContent content;
            folder = default(InventoryFolder);
            if(!((IInventoryFolderContentServiceInterface)this).TryGetValue(principalID, m_RootFolderID, out content))
            {
                return false;
            }
            foreach(InventoryFolder f in content.Folders)
            {
                if(f.InventoryType == (InventoryType)(int)type)
                {
                    folder = f;
                    return true;
                }
            }
            return false;
        }

        bool IInventoryFolderContentServiceInterface.TryGetValue(UUID principalID, UUID folderID, out InventoryFolderContent inventoryFolderContent)
        {
            InventoryFolder folder;
            return TryGetValue(folderID, out folder, out inventoryFolderContent);
        }

        public abstract void Update(InventoryFolder folder);

        private bool TryGetValue(UUID folderID, out InventoryFolder folder, out InventoryFolderContent inventoryFolderContent)
        {
            KeyValuePair<InventoryFolder, InventoryFolderContent> result;
            inventoryFolderContent = default(InventoryFolderContent);
            folder = default(InventoryFolder);
            if (!GetFolderContents(new UUID[] { folderID }, true).TryGetValue(folderID, out result))
            {
                return false;
            }
            folder = result.Key;
            inventoryFolderContent = result.Value;
            return true;
        }

        private Dictionary<UUID, KeyValuePair<InventoryFolder, InventoryFolderContent>> GetFolderContents(UUID[] folderids, bool fetch_children)
        {
            var folderidsreq = new AnArray();
            var reqmap = new Map
            {
                ["folder_ids"] = folderidsreq
            };
            foreach (UUID folderid in folderids)
            {
                folderidsreq.Add(new Map { { "folder_id", folderid }, { "fetch_folders", fetch_children }, { "fetch_items", fetch_children } });
            }
            byte[] reqdata;
            using (var ms = new MemoryStream())
            {
                LlsdXml.Serialize(reqmap, ms);
                reqdata = ms.ToArray();
            }
            Map resdata;
            using (Stream s = HttpClient.DoStreamRequest("POST", m_CapabilityUri, null, "application/llsd+xml", reqdata.Length,
                (Stream o) => o.Write(reqdata, 0, reqdata.Length), false, TimeoutMs))
            {
                resdata = LlsdXml.Deserialize(s) as Map;
            }
            if (resdata == null)
            {
                throw new InvalidDataException();
            }
            AnArray foldersres;
            var resultset = new Dictionary<UUID, KeyValuePair<InventoryFolder, InventoryFolderContent>>();
            if (!resdata.TryGetValue("folders", out foldersres))
            {
                return resultset;
            }
            foreach(IValue folderiv in foldersres)
            {
                var folderdata = folderiv as Map;
                if(folderdata == null)
                {
                    continue;
                }
                var folder = new InventoryFolder(folderdata["folder_id"].AsUUID)
                {
                    Owner = new UUI(folderdata["owner_id"].AsUUID),
                    Version = folderdata["version"].AsInt,
                    ParentFolderID = folderdata["parent_id"].AsUUID,
                };
                var content = new InventoryFolderContent
                {
                    Version = folderdata["version"].AsInt,
                    FolderID = folderdata["folder_id"].AsUUID,
                    Owner = new UUI(folderdata["owner_id"].AsUUID)
                };
                AnArray categories;
                if (folderdata.TryGetValue("categories", out categories))
                {
                    foreach (IValue categoryiv in categories)
                    {
                        var category = categoryiv as Map;
                        if (category == null)
                        {
                            continue;
                        }
                        content.Folders.Add(new InventoryFolder(category["folder_id"].AsUUID)
                        {
                            Name = category["name"].ToString(),
                            ParentFolderID = category["parent_id"].AsUUID,
                            InventoryType = (InventoryType)category["type"].AsInt,
                            Version = category["version"].AsInt
                        });
                    }
                }

                AnArray childitems;
                if(folderdata.TryGetValue("items", out childitems))
                {
                    foreach (IValue childiv in childitems)
                    {
                        var childdata = childiv as Map;
                        if (childdata == null)
                        {
                            continue;
                        }
                        if(childdata["parent_id"].AsUUID != folder.ID)
                        {
                            continue;
                        }
                        var permissions = (Map)childdata["permissions"];
                        var sale_info = (Map)childdata["sale_info"];
                        content.Items.Add(new InventoryItem(childdata["item_id"].AsUUID)
                        {
                            AssetID = childdata["asset_id"].AsUUID,
                            CreationDate = Date.UnixTimeToDateTime(childdata["created_at"].AsULong),
                            Description = childdata["desc"].ToString(),
                            Flags = (InventoryFlags)childdata["flags"].AsInt,
                            InventoryType = (InventoryType)childdata["inv_type"].AsInt,
                            Name = childdata["name"].ToString(),
                            ParentFolderID = childdata["parent_id"].AsUUID,
                            AssetType = (AssetType)childdata["type"].AsInt,
                            Creator = new UUI(permissions["creator_id"].AsUUID),
                            Group = new UGI(permissions["group_id"].AsUUID),
                            IsGroupOwned = permissions["is_owner_group"].AsBoolean,
                            LastOwner = new UUI(permissions["last_owner_id"].AsUUID),
                            Owner = new UUI(permissions["owner_id"].AsUUID),
                            Permissions = new InventoryPermissionsData
                            {
                                Base = (InventoryPermissionsMask)permissions["base_mask"].AsInt,
                                EveryOne = (InventoryPermissionsMask)permissions["everyone_mask"].AsInt,
                                Group = (InventoryPermissionsMask)permissions["group_mask"].AsInt,
                                NextOwner = (InventoryPermissionsMask)permissions["next_owner_mask"].AsInt,
                                Current = (InventoryPermissionsMask)permissions["owner_mask"].AsInt
                            },
                            SaleInfo = new InventoryItem.SaleInfoData
                            {
                                Price = sale_info["sale_price"].AsInt,
                                Type = (InventoryItem.SaleInfoData.SaleType)sale_info["sale_type"].AsInt
                            }
                        });
                    }
                }

                resultset.Add(folder.ID, new KeyValuePair<InventoryFolder, InventoryFolderContent>(folder, content));
            }

            return resultset;
        }
    }
}
