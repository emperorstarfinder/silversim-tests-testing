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
using System.Collections.Generic;
using System.IO;

namespace SilverSim.Tests.Viewer.Caps
{
    public abstract class FetchInventory2Client : IInventoryItemServiceInterface
    {
        public int TimeoutMs { get; set; }
        private readonly string m_CapabilityUri;

        protected FetchInventory2Client(string uri)
        {
            m_CapabilityUri = uri;
            TimeoutMs = 20000;
        }

        public InventoryItem this[UUID key]
        {
            get
            {
                InventoryItem item;
                if (!TryGetValue(key, out item))
                {
                    throw new InventoryItemNotFoundException(key);
                }
                return item;
            }
        }

        public InventoryItem this[UUID principalID, UUID key] => this[key];

        public List<InventoryItem> this[UUID principalID, List<UUID> itemids] => this[itemids];

        List<InventoryItem> this[List<UUID> itemids]
        {
            get
            {
                var items = new AnArray();
                var reqmap = new Map
                {
                    ["items"] = items
                };
                foreach(UUID itemid in itemids)
                {
                    items.Add(itemid);
                }
                byte[] reqdata;
                using (var ms = new MemoryStream())
                {
                    LlsdXml.Serialize(reqmap, ms);
                    reqdata = ms.ToArray();
                }
                Map resdata;
                using (Stream s = new HttpClient.Post(m_CapabilityUri, "application/llsd+xml", reqdata.Length,
                    (Stream o) => o.Write(reqdata, 0, reqdata.Length))
                {
                    TimeoutMs = TimeoutMs
                }.ExecuteStreamRequest())
                {
                    resdata = LlsdXml.Deserialize(s) as Map;
                }
                if(resdata == null)
                {
                    throw new InvalidDataException();
                }

                AnArray itemres;
                if(!resdata.TryGetValue("items", out itemres))
                {
                    throw new InvalidDataException();
                }
                var result = new List<InventoryItem>();
                foreach(IValue item in itemres)
                {
                    var itemdata = item as Map;
                    if(itemdata == null)
                    {
                        continue;
                    }

                    var permissions = (Map)itemdata["permissions"];
                    var sale_info = (Map)itemdata["sale_info"];
                    var resitem = new InventoryItem(itemdata["item_id"].AsUUID)
                    {
                        AssetID = itemdata["asset_id"].AsUUID,
                        CreationDate = Date.UnixTimeToDateTime(itemdata["created_at"].AsULong),
                        Description = itemdata["desc"].ToString(),
                        Flags = (InventoryFlags)itemdata["flags"].AsInt,
                        InventoryType = (InventoryType)itemdata["inv_type"].AsInt,
                        Name = itemdata["name"].ToString(),
                        ParentFolderID = itemdata["parent_id"].AsUUID,
                        AssetType = (AssetType)itemdata["type"].AsInt,
                        Creator = new UUI(permissions["creator_id"].AsUUID),
                        Group = new UGI(permissions["group_id"].AsUUID),
                        IsGroupOwned = permissions["is_owner_group"].AsBoolean,
                        LastOwner = new UUI(permissions["last_owner_id"].AsUUID),
                        Owner = new UUI(permissions["owner_id"].AsUUID),
                        SaleInfo = new InventoryItem.SaleInfoData
                        {
                            Price = sale_info["sale_price"].AsInt,
                            Type = (InventoryItem.SaleInfoData.SaleType)sale_info["sale_type"].AsInt
                        }
                    };
                    resitem.Permissions.Base = (InventoryPermissionsMask)permissions["base_mask"].AsInt;
                    resitem.Permissions.EveryOne = (InventoryPermissionsMask)permissions["everyone_mask"].AsInt;
                    resitem.Permissions.Group = (InventoryPermissionsMask)permissions["group_mask"].AsInt;
                    resitem.Permissions.NextOwner = (InventoryPermissionsMask)permissions["next_owner_mask"].AsInt;
                    resitem.Permissions.Current = (InventoryPermissionsMask)permissions["owner_mask"].AsInt;
                }
                return result;
            }
        }

        public bool ContainsKey(UUID key)
        {
            InventoryItem item;
            return TryGetValue(key, out item);
        }

        public bool ContainsKey(UUID principalID, UUID key) =>
            ContainsKey(key);

        public bool TryGetValue(UUID key, out InventoryItem item)
        {
            item = default(InventoryItem);
            List<InventoryItem> items = this[UUID.Zero, new List<UUID> { key }];
            if(items.Count != 1)
            {
                return false;
            }
            item = items[0];
            return true;
        }

        public bool TryGetValue(UUID principalID, UUID key, out InventoryItem item) => 
            TryGetValue(key, out item);

        public abstract void Add(InventoryItem item);

        public abstract void Update(InventoryItem item);

        public abstract void Delete(UUID principalID, UUID id);

        public abstract void Move(UUID principalID, UUID id, UUID newFolder);

        public abstract UUID Copy(UUID principalID, UUID id, UUID newFolder);

        public abstract List<UUID> Delete(UUID principalID, List<UUID> ids);
    }
}
