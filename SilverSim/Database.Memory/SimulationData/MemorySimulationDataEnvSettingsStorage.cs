// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Scene.Types.WindLight;
using SilverSim.Threading;
using SilverSim.Types;
using System.Collections.Generic;
using System.IO;

namespace SilverSim.Database.Memory.SimulationData
{
    public class MemorySimulationDataEnvSettingsStorage : SimulationDataEnvSettingsStorageInterface
    {
        readonly RwLockedDictionary<UUID, byte[]> m_Data = new RwLockedDictionary<UUID, byte[]>();

        public MemorySimulationDataEnvSettingsStorage()
        {
        }

        public override bool TryGetValue(UUID regionID, out EnvironmentSettings settings)
        {
            byte[] data;
            if(m_Data.TryGetValue(regionID, out data))
            { 
                using (MemoryStream ms = new MemoryStream(data))
                {
                    settings = EnvironmentSettings.Deserialize(ms);
                    return true;
                }
            }
            settings = null;
            return false;
        }

        /* setting value to null will delete the entry */
        public override EnvironmentSettings this[UUID regionID]
        {
            get
            {
                EnvironmentSettings settings;
                if (!TryGetValue(regionID, out settings))
                {
                    throw new KeyNotFoundException();
                }
                return settings;
            }
            set
            {
                if(value == null)
                {
                    m_Data.Remove(regionID);
                }

                else
                {
                    using(MemoryStream ms = new MemoryStream())
                    {
                        value.Serialize(ms, regionID);
                        m_Data[regionID] = ms.GetBuffer();
                    }
                }
            }
        }

        public override bool Remove(UUID regionID)
        {
            return m_Data.Remove(regionID);
        }
    }
}
