// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.ServiceInterfaces.Estate;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;

namespace SilverSim.Database.Memory.Estate
{
    public sealed class MemoryEstateRegionMapInterface : IEstateRegionMapServiceInterface
    {
        readonly RwLockedDictionaryAutoAdd<uint, RwLockedList<UUID>> m_Data = new RwLockedDictionaryAutoAdd<uint, RwLockedList<UUID>>(delegate () { return new RwLockedList<UUID>(); });

        public MemoryEstateRegionMapInterface()
        {
        }

        public List<UUID> this[uint estateID]
        {
            get 
            {
                RwLockedList<UUID> regions;
                return (m_Data.TryGetValue(estateID, out regions)) ? new List<UUID>(regions) : new List<UUID>();
            }
        }

        public bool TryGetValue(UUID regionID, out uint estateID)
        {
            foreach(KeyValuePair<uint, RwLockedList<UUID>> kvp in m_Data)
            {
                if(kvp.Value.Contains(regionID))
                {
                    estateID = kvp.Key;
                    return true;
                }
            }
            estateID = 0;
            return false;
        }

        public bool Remove(UUID regionID)
        {
            bool found = false;
            foreach (KeyValuePair<uint, RwLockedList<UUID>> kvp in m_Data)
            {
                kvp.Value.Remove(regionID);
                found = true;
            }
            return found;
        }

        public uint this[UUID regionID]
        {
            get
            {
                uint estateID;
                if(!TryGetValue(regionID, out estateID))
                {
                    throw new KeyNotFoundException();
                }
                return estateID;
            }
            set
            {
                if(!m_Data[value].Contains(regionID))
                {
                    m_Data[value].Add(regionID);
                }
            }
        }
    }
}
