// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Asset;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Asset;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Database.Memory.Asset
{
    #region Service Implementation
    [SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
    [Description("Memory Asset Backend")]
    public class MemoryAssetService : AssetServiceInterface, IPlugin
    {
        readonly MemoryAssetMetadataService m_MetadataService;
        readonly DefaultAssetReferencesService m_ReferencesService;
        readonly MemoryAssetDataService m_DataService;
        readonly RwLockedDictionary<UUID, AssetData> m_Assets = new RwLockedDictionary<UUID, AssetData>();

        #region Constructor
        public MemoryAssetService()
        {
            m_MetadataService = new MemoryAssetMetadataService(m_Assets);
            m_DataService = new MemoryAssetDataService(m_Assets);
            m_ReferencesService = new DefaultAssetReferencesService(this);
        }

        public void Startup(ConfigurationLoader loader)
        {

        }
        #endregion

        #region Exists methods
        public override bool Exists(UUID key)
        {
            return m_Assets.ContainsKey(key);
        }

        public override Dictionary<UUID, bool> Exists(List<UUID> assets)
        {
            Dictionary<UUID,bool> res = new Dictionary<UUID,bool>();
            foreach(UUID id in assets)
            {
                res[id] = m_Assets.ContainsKey(id);
            }

            return res;
        }

        #endregion

        public override bool IsSameServer(AssetServiceInterface other)
        {
            return other.GetType() == typeof(MemoryAssetService) && other == this;
        }

        #region Accessors
        public override bool TryGetValue(UUID key, out AssetData asset)
        {
            AssetData internalAsset;
            if(m_Assets.TryGetValue(key, out internalAsset))
            {
                internalAsset.CreateTime = Date.Now;
                asset = new AssetData();
                asset.ID = internalAsset.ID;
                asset.Data = new byte[internalAsset.Data.Length];
                Buffer.BlockCopy(internalAsset.Data, 0, asset.Data, 0, internalAsset.Data.Length);
                asset.Type = internalAsset.Type;
                asset.Name = internalAsset.Name;
                asset.CreateTime = internalAsset.CreateTime;
                asset.AccessTime = internalAsset.AccessTime;
                asset.Creator = internalAsset.Creator;
                asset.Flags = internalAsset.Flags;
                asset.Temporary = internalAsset.Temporary;
                return true;
            }
            asset = null;
            return false;
        }

        public override AssetData this[UUID key]
        {
            get
            {
                AssetData asset;
                if(!TryGetValue(key, out asset))
                {
                    throw new AssetNotFoundException(key);
                }
                return asset;
            }
        }

        #endregion

        #region Metadata interface
        public override AssetMetadataServiceInterface Metadata
        {
            get
            {
                return m_MetadataService;
            }
        }
        #endregion

        #region References interface
        public override AssetReferencesServiceInterface References
        {
            get
            {
                return m_ReferencesService;
            }
        }
        #endregion

        #region Data interface
        public override AssetDataServiceInterface Data
        {
            get
            {
                return m_DataService;
            }
        }
        #endregion

        #region Store asset method
        [SuppressMessage("Gendarme.Rules.Exceptions", "DoNotSwallowErrorsCatchingNonSpecificExceptionsRule")]
        public override void Store(AssetData asset)
        {
            AssetData internalAsset;
            if(m_Assets.TryGetValue(asset.ID, out internalAsset))
            {
                if(internalAsset.Flags != AssetFlags.Normal)
                {
                    internalAsset = new AssetData();
                    internalAsset.ID = asset.ID;
                    internalAsset.Data = new byte[asset.Data.Length];
                    Buffer.BlockCopy(asset.Data, 0, internalAsset.Data, 0, asset.Data.Length);
                    internalAsset.Type = asset.Type;
                    internalAsset.Name = asset.Name;
                    internalAsset.CreateTime = asset.CreateTime;
                    internalAsset.AccessTime = asset.AccessTime;
                    internalAsset.Creator = asset.Creator;
                    internalAsset.Flags = asset.Flags;
                    internalAsset.Temporary = asset.Temporary;

                    m_Assets[asset.ID] = internalAsset;
                }
            }
            else
            {
                internalAsset = new AssetData();
                internalAsset.ID = asset.ID;
                internalAsset.Data = new byte[asset.Data.Length];
                Buffer.BlockCopy(asset.Data, 0, internalAsset.Data, 0, asset.Data.Length);
                internalAsset.Type = asset.Type;
                internalAsset.Name = asset.Name;
                internalAsset.CreateTime = asset.CreateTime;
                internalAsset.AccessTime = asset.AccessTime;
                internalAsset.Creator = asset.Creator;
                internalAsset.Flags = asset.Flags;
                internalAsset.Temporary = asset.Temporary;

                m_Assets.Add(asset.ID, asset);
            }
        }
        #endregion

        #region Delete asset method
        public override void Delete(UUID id)
        {
            m_Assets.RemoveIf(id, delegate (AssetData d) { return d.Flags != AssetFlags.Normal; });
        }
        #endregion

        private const int MAX_ASSET_NAME = 64;
    }
    #endregion

    #region Factory
    [PluginName("Assets")]
    public class MemoryAssetServiceFactory : IPluginFactory
    {
        public MemoryAssetServiceFactory()
        {

        }

        public IPlugin Initialize(ConfigurationLoader loader, IConfig ownSection)
        {
            return new MemoryAssetService();
        }
    }
    #endregion
}
