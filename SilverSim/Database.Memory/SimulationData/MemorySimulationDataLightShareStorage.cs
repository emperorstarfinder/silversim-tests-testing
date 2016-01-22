// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Scene.Types.SceneEnvironment;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataLightShareStorage : SimulationDataLightShareStorageInterface
    {
        readonly RwLockedDictionary<UUID, KeyValuePair<EnvironmentController.WindlightSkyData, EnvironmentController.WindlightWaterData>> m_Data = new RwLockedDictionary<UUID, KeyValuePair<EnvironmentController.WindlightSkyData, EnvironmentController.WindlightWaterData>>();

        public MemorySimulationDataLightShareStorage()
        {
        }

        public override bool TryGetValue(UUID regionID, out EnvironmentController.WindlightSkyData skyData, out EnvironmentController.WindlightWaterData waterData)
        {
            KeyValuePair<EnvironmentController.WindlightSkyData, EnvironmentController.WindlightWaterData> kvp;
            if(m_Data.TryGetValue(regionID, out kvp))
            {
                skyData = kvp.Key;
                waterData = kvp.Value;
                return true;
            }
            else
            {
                skyData = EnvironmentController.WindlightSkyData.Defaults;
                waterData = EnvironmentController.WindlightWaterData.Defaults;
                return false;
            }
        }

        public override void Store(UUID regionID, EnvironmentController.WindlightSkyData skyData, EnvironmentController.WindlightWaterData waterData)
        {
            m_Data[regionID] = new KeyValuePair<EnvironmentController.WindlightSkyData, EnvironmentController.WindlightWaterData>(skyData, waterData);
        }

        public override bool Remove(UUID regionID)
        {
            return m_Data.Remove(regionID);
        }
    }
}
