// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Types;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace SilverSim.Database.Memory.SimulationData
{
    #region Service Implementation
    [SuppressMessage("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule")]
    [Description("Memory Simulation Data Backend")]
    public sealed partial class MemorySimulationDataStorage : SimulationDataStorageInterface, IPlugin
    {
        private static readonly ILog m_Log = LogManager.GetLogger("MEMORY SIMULATION STORAGE");

        readonly MemorySimulationDataObjectStorage m_ObjectStorage;
        readonly MemorySimulationDataParcelStorage m_ParcelStorage;
        readonly MemorySimulationDataScriptStateStorage m_ScriptStateStorage;
        readonly MemorySimulationDataTerrainStorage m_TerrainStorage;
        readonly MemorySimulationDataEnvSettingsStorage m_EnvironmentStorage;
        readonly MemorySimulationDataRegionSettingsStorage m_RegionSettingsStorage;
        readonly MemorySimulationDataSpawnPointStorage m_SpawnPointStorage;
        readonly MemorySimulationDataLightShareStorage m_LightShareStorage;

        #region Constructor
        public MemorySimulationDataStorage()
        {
            m_ObjectStorage = new MemorySimulationDataObjectStorage();
            m_ParcelStorage = new MemorySimulationDataParcelStorage();
            m_TerrainStorage = new MemorySimulationDataTerrainStorage();
            m_ScriptStateStorage = new MemorySimulationDataScriptStateStorage();
            m_EnvironmentStorage = new MemorySimulationDataEnvSettingsStorage();
            m_RegionSettingsStorage = new MemorySimulationDataRegionSettingsStorage();
            m_SpawnPointStorage = new MemorySimulationDataSpawnPointStorage();
            m_LightShareStorage = new MemorySimulationDataLightShareStorage();
        }

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }
        #endregion

        #region Properties
        public override SimulationDataLightShareStorageInterface LightShare
        {
            get
            {
                return m_LightShareStorage;
            }
        }

        public override SimulationDataSpawnPointStorageInterface Spawnpoints
        {
            get
            {
                return m_SpawnPointStorage;
            }
        }

        public override SimulationDataEnvSettingsStorageInterface EnvironmentSettings
        {
            get 
            {
                return m_EnvironmentStorage;
            }
        }

        public override SimulationDataObjectStorageInterface Objects
        {
            get
            {
                return m_ObjectStorage;
            }
        }

        public override SimulationDataParcelStorageInterface Parcels
        {
            get
            {
                return m_ParcelStorage;
            }
        }
        public override SimulationDataScriptStateStorageInterface ScriptStates
        {
            get 
            {
                return m_ScriptStateStorage;
            }
        }

        public override SimulationDataTerrainStorageInterface Terrains
        {
            get 
            {
                return m_TerrainStorage;
            }
        }

        public override SimulationDataRegionSettingsStorageInterface RegionSettings
        {
            get
            {
                return m_RegionSettingsStorage;
            }
        }
        #endregion

        static readonly string[] Tables = new string[]
        {
            "primitems",
            "prims",
            "objects",
        };

        public override void RemoveRegion(UUID regionID)
        {
            m_ScriptStateStorage.RemoveAllInRegion(regionID);
            m_RegionSettingsStorage.Remove(regionID);
            m_TerrainStorage.Remove(regionID);
            m_ParcelStorage.RemoveAllInRegion(regionID);
            m_EnvironmentStorage.Remove(regionID);
            m_LightShareStorage.Remove(regionID);
            m_SpawnPointStorage.Remove(regionID);
        }
    }
    #endregion

    #region Factory
    [PluginName("SimulationData")]
    public class MemorySimulationDataServiceFactory : IPluginFactory
    {
        public MemorySimulationDataServiceFactory()
        {

        }

        public IPlugin Initialize(ConfigurationLoader loader, IConfig ownSection)
        {
            return new MemorySimulationDataStorage();
        }
    }
    #endregion
}
