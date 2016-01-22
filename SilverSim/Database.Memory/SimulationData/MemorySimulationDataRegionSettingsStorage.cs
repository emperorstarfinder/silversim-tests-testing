// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Scene.Types.Scene;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataRegionSettingsStorage : SimulationDataRegionSettingsStorageInterface
    {
        readonly RwLockedDictionary<UUID, RegionSettings> m_Data = new RwLockedDictionary<UUID, RegionSettings>();

        public MemorySimulationDataRegionSettingsStorage()
        {
        }

        public override RegionSettings this[UUID regionID]
        {
            get
            {
                RegionSettings settings;
                if (!TryGetValue(regionID, out settings))
                {
                    throw new KeyNotFoundException();
                }
                return settings;
            }
            set
            {
                m_Data[regionID] = new RegionSettings(value);
            }
        }

        public override bool TryGetValue(UUID regionID, out RegionSettings settings)
        {
            if(m_Data.TryGetValue(regionID, out settings))
            {
                settings = new RegionSettings(settings);
                return true;
            }
            settings = null;
            return false;
        }

        public override bool ContainsKey(UUID regionID)
        {
            return m_Data.ContainsKey(regionID);
        }

        public override bool Remove(UUID regionID)
        {
            return m_Data.Remove(regionID);
        }
    }
}
