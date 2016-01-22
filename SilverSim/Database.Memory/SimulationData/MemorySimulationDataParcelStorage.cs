// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Parcel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataParcelStorage : SimulationDataParcelStorageInterface
    {
        readonly RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<UUID, ParcelInfo>> m_Data = new RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<UUID, ParcelInfo>>(delegate () { return new RwLockedDictionary<UUID, ParcelInfo>(); });
        readonly MemorySimulationDataParcelAccessListStorage m_WhiteListStorage;
        readonly MemorySimulationDataParcelAccessListStorage m_BlackListStorage;

        public MemorySimulationDataParcelStorage()
        {
            m_WhiteListStorage = new MemorySimulationDataParcelAccessListStorage();
            m_BlackListStorage = new MemorySimulationDataParcelAccessListStorage();
        }

        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override ParcelInfo this[UUID regionID, UUID parcelID]
        {
            get
            {
                RwLockedDictionary<UUID, ParcelInfo> parcels;
                if(m_Data.TryGetValue(regionID, out parcels))
                {
                    return new ParcelInfo(parcels[parcelID]);
                }
                throw new KeyNotFoundException();
            }
        }

        public override bool Remove(UUID regionID, UUID parcelID)
        {
            RwLockedDictionary<UUID, ParcelInfo> parcels;
            return m_Data.TryGetValue(regionID, out parcels) && parcels.Remove(parcelID);
        }

        public void RemoveAllInRegion(UUID regionID)
        {
            m_Data.Remove(regionID);
        }

        public override List<UUID> ParcelsInRegion(UUID key)
        {
            RwLockedDictionary<UUID, ParcelInfo> parcels;
            if (m_Data.TryGetValue(key, out parcels))
            {
                return new List<UUID>(parcels.Keys);
            }
            else
            {
                return new List<UUID>();
            }
        }

        public override void Store(UUID regionID, ParcelInfo parcel)
        {
            m_Data[regionID][parcel.ID] = new ParcelInfo(parcel);
        }

        public override SimulationDataParcelAccessListStorageInterface WhiteList
        {
            get
            {
                return m_WhiteListStorage;
            }
        }

        public override SimulationDataParcelAccessListStorageInterface BlackList
        {
            get
            {
                return m_BlackListStorage;
            }
        }
    }
}
