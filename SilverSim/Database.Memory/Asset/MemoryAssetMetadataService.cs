// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset;
using System;

namespace SilverSim.Database.Memory.Asset
{
    public class MemoryAssetMetadataService : AssetMetadataServiceInterface
    {
        readonly RwLockedDictionary<UUID, AssetData> m_Assets;

        public MemoryAssetMetadataService(RwLockedDictionary<UUID, AssetData> assets)
        {
            m_Assets = assets;
        }

        #region Accessor
        public override AssetMetadata this[UUID key]
        {
            get
            {
                AssetMetadata metadata;
                if(!TryGetValue(key, out metadata))
                {
                    throw new AssetNotFoundException(key);
                }
                return metadata;
            }
        }

        public override bool TryGetValue(UUID key, out AssetMetadata metadata)
        {
            AssetData data;
            if(m_Assets.TryGetValue(key, out data))
            {
                metadata = new AssetMetadata();
                metadata = new AssetData();
                metadata.ID = data.ID;
                metadata.Type = data.Type;
                metadata.Name = data.Name;
                metadata.CreateTime = data.CreateTime;
                metadata.AccessTime = data.AccessTime;
                metadata.Creator = data.Creator;
                metadata.Flags = data.Flags;
                metadata.Temporary = data.Temporary;
                return true;
            }
            else
            {
                metadata = null;
                return false;
            }
        }
        #endregion
    }
}
