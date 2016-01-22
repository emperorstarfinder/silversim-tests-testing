// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataScriptStateStorage : SimulationDataScriptStateStorageInterface
    {
        readonly RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<string, byte[]>> m_Data = new RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<string, byte[]>>(delegate () { return new RwLockedDictionary<string, byte[]>(); });

        public MemorySimulationDataScriptStateStorage()
        {
        }

        string GenKey(UUID primID, UUID itemID)
        {
            return primID.ToString() + ":" + itemID.ToString();
        }

        public override bool TryGetValue(UUID regionID, UUID primID, UUID itemID, out byte[] state)
        {
            RwLockedDictionary<string, byte[]> states;
            state = null;
            return m_Data.TryGetValue(regionID, out states) && states.TryGetValue(GenKey(primID, itemID), out state);
        }

        /* setting value to null will delete the entry */
        [SuppressMessage("Gendarme.Rules.Design", "AvoidMultidimensionalIndexerRule")]
        public override byte[] this[UUID regionID, UUID primID, UUID itemID] 
        {
            get
            {
                byte[] state;
                if(!TryGetValue(regionID, primID, itemID, out state))
                {
                    throw new KeyNotFoundException();
                }

                return state;
            }
            set
            {
                m_Data[regionID][GenKey(primID, itemID)] = value;
            }
        }

        public override bool Remove(UUID regionID, UUID primID, UUID itemID)
        {
            RwLockedDictionary<string, byte[]> states;
            return m_Data.TryGetValue(regionID, out states) && states.Remove(GenKey(primID, itemID));
        }

        public void RemoveAllInRegion(UUID regionID)
        {
            m_Data.Remove(regionID);
        }
    }
}
