// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Threading;
using SilverSim.Types;
using SilverSim.Viewer.Messages.LayerData;
using System.Collections.Generic;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataTerrainStorage : SimulationDataTerrainStorageInterface
    {
        internal readonly RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<uint, byte[]>> m_Data = new RwLockedDictionaryAutoAdd<UUID, RwLockedDictionary<uint, byte[]>>(delegate() { return new RwLockedDictionary<uint, byte[]>(); } );

        public MemorySimulationDataTerrainStorage()
        {
        }

        public override List<LayerPatch> this[UUID regionID]
        {
            get
            {
                RwLockedDictionary<uint, byte[]> patchesData;
                List<LayerPatch> patches = new List<LayerPatch>();
                if (m_Data.TryGetValue(regionID, out patchesData))
                {
                    foreach(KeyValuePair<uint, byte[]> kvp in patchesData)
                    {
                        LayerPatch patch = new LayerPatch();
                        patch.ExtendedPatchID = kvp.Key;
                        patch.Serialization = kvp.Value;
                        patches.Add(patch);
                    }
                }
                return patches;
            }
        }

        public void Remove(UUID regionID)
        {
            m_Data.Remove(regionID);
        }
    }
}
