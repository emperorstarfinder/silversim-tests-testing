// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Threading;
using SilverSim.Types;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataEnvControllerStorage : SimulationDataEnvControllerStorageInterface
    {
        readonly RwLockedDictionary<UUID, byte[]> m_Data = new RwLockedDictionary<UUID, byte[]>();

        public MemorySimulationDataEnvControllerStorage()
        {

        }

        public override byte[] this[UUID regionID]
        {
            get
            {
                return m_Data[regionID];
            }

            set
            {
                if (value != null)
                {
                    m_Data[regionID] = value;
                }
                else
                {
                    m_Data.Remove(regionID);
                }
            }
        }

        public override bool Remove(UUID regionID)
        {
            return m_Data.Remove(regionID);
        }

        public override bool TryGetValue(UUID regionID, out byte[] settings)
        {
            return m_Data.TryGetValue(regionID, out settings);
        }
    }
}
