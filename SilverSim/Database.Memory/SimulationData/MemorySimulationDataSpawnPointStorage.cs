// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataSpawnPointStorage : SimulationDataSpawnPointStorageInterface
    {
        readonly RwLockedDictionary<UUID, RwLockedList<Vector3>> m_Data = new RwLockedDictionary<UUID, RwLockedList<Vector3>>();

        public MemorySimulationDataSpawnPointStorage()
        {
        }

        public override List<Vector3> this[UUID regionID]
        {
            get
            {
                RwLockedList<Vector3> data;
                return (m_Data.TryGetValue(regionID, out data)) ?
                    new List<Vector3>(data) :
                    new List<Vector3>();
            }
            set
            {
                m_Data[regionID] = new RwLockedList<Vector3>(value);
            }
        }

        public override bool Remove(UUID regionID)
        {
            return m_Data.Remove(regionID);
        }
    }
}
