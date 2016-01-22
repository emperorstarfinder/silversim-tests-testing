// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Types.Parcel;
using System.Collections.Generic;
using System.Linq;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataParcelAccessListStorage : SimulationDataParcelAccessListStorageInterface
    {
        RwLockedDictionaryAutoAdd<string, RwLockedDictionary<UUI, ParcelAccessEntry>> m_Data = new RwLockedDictionaryAutoAdd<string, RwLockedDictionary<UUI, ParcelAccessEntry>>(delegate() { return new RwLockedDictionary<UUI, ParcelAccessEntry>(); });
        public MemorySimulationDataParcelAccessListStorage()
        {
        }

        string GenKey(UUID regionID, UUID parcelID)
        {
            return regionID.ToString() + ":" + parcelID.ToString();
        }

        public override bool this[UUID regionID, UUID parcelID, UUI accessor]
        {
            get
            {
                RwLockedDictionary<UUI, ParcelAccessEntry> list;
                if(m_Data.TryGetValue(GenKey(regionID, parcelID), out list))
                {
                    IEnumerable<ParcelAccessEntry> en = from entry in list.Values where entry.Accessor.EqualsGrid(accessor) select entry;
                    return en.GetEnumerator().MoveNext();
                }
                return false;
            }
        }

        public override List<ParcelAccessEntry> this[UUID regionID, UUID parcelID]
        {
            get
            {
                RwLockedDictionary<UUI, ParcelAccessEntry> list;
                return (m_Data.TryGetValue(GenKey(regionID, parcelID), out list)) ?
                    new List<ParcelAccessEntry>(from entry in list.Values where true select new ParcelAccessEntry(entry)) :
                    new List<ParcelAccessEntry>();
            }
        }

        public override void Store(ParcelAccessEntry entry)
        {
            string key = GenKey(entry.RegionID, entry.ParcelID);
            m_Data[key][entry.Accessor] = new ParcelAccessEntry(entry);
        }

        public override bool RemoveAllFromRegion(UUID regionID)
        {
            bool found = false;
            List<string> keys = new List<string>(from key in m_Data.Keys where key.StartsWith(regionID.ToString()) select key);
            foreach(string key in keys)
            {
                found = m_Data.Remove(key) || found;
            }
            return found;
        }

        public override bool Remove(UUID regionID, UUID parcelID)
        {
            return m_Data.Remove(GenKey(regionID, parcelID));
        }

        public override bool Remove(UUID regionID, UUID parcelID, UUI accessor)
        {
            RwLockedDictionary<UUI, ParcelAccessEntry> list;
            return m_Data.TryGetValue(GenKey(regionID, parcelID), out list) && list.Remove(accessor);
        }
    }
}
