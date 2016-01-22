// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SilverSim.Database.Memory.Asset
{
    public class MemoryAssetDataService : AssetDataServiceInterface
    {
        readonly RwLockedDictionary<UUID, AssetData> m_Assets;

        public MemoryAssetDataService(RwLockedDictionary<UUID, AssetData> assets)
        {
            m_Assets = assets;
        }

        #region Accessor
        [SuppressMessage("Gendarme.Rules.Correctness", "EnsureLocalDisposalRule")]
        public override Stream this[UUID key]
        {
            get
            {
                Stream s;
                if(!TryGetValue(key, out s))
                {
                    throw new AssetNotFoundException(key);
                }
                return s;
            }
        }

        public override bool TryGetValue(UUID key, out Stream s)
        {
            AssetData data;
            if(m_Assets.TryGetValue(key, out data))
            {
                s = data.InputStream;
                return true;
            }
            else
            {
                s = null;
                return false;
            }
        }
        #endregion
    }
}
