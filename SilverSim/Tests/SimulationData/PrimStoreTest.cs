// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3 with
// the following clarification and special exception.

// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU Affero General Public License cover the whole
// combination.

// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

using log4net;
using SilverSim.Scene.Types.Agent;
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Scene;
using SilverSim.Scene.Types.Script;
using SilverSim.Types;
using SilverSim.Types.Asset;
using SilverSim.Types.Inventory;
using SilverSim.Types.Parcel;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using SceneListener = SilverSim.Scene.ServiceInterfaces.SimulationData.SimulationDataStorageInterface.SceneListener;

namespace SilverSim.Tests.SimulationData
{
    public sealed class PrimStoreTest : CommonSimDataTest
    {
        private class DummyScene : SceneInterface
        {
            public DummyScene(uint sizeX, uint sizeY, UUID id) 
                : base(sizeX, sizeY)
            {
                ID = id;
            }

            public override uint ServerHttpPort
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override string ServerURI
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override ISceneObjects Objects
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override ISceneObjectGroups ObjectGroups
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override ISceneObjectParts Primitives
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override ISceneAgents Agents
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override ISceneAgents RootAgents
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override ISceneParcels Parcels
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override IRegionExperienceList RegionExperiences
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override IRegionTrustedExperienceList RegionTrustedExperiences
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override List<ObjectUpdateInfo> UpdateInfos
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override IUDPCircuitsManager UDPServer
            {
                get
                {
                    throw new System.NotImplementedException();
                }
            }

            public override void AbortRegionRestart()
            {
                throw new System.NotImplementedException();
            }

            public override void Add(IObject obj)
            {
                throw new System.NotImplementedException();
            }

            public override void AddObjectGroupOnly(IObject obj)
            {
                throw new System.NotImplementedException();
            }

            public override void ClearObjects()
            {
                throw new System.NotImplementedException();
            }

            public override void LoadScene()
            {
                throw new System.NotImplementedException();
            }

            public override void LoadSceneSync()
            {
                throw new System.NotImplementedException();
            }

            public override void RelocateRegion(GridVector location)
            {
                throw new System.NotImplementedException();
            }

            public override bool Remove(IObject obj, ScriptInstance instance = null)
            {
                throw new System.NotImplementedException();
            }

            public override bool RemoveObjectGroupOnly(UUID objID)
            {
                throw new System.NotImplementedException();
            }

            public override bool RemoveParcel(ParcelInfo p, UUID mergeTo)
            {
                throw new System.NotImplementedException();
            }

            public override void RequestRegionRestart(int seconds)
            {
                throw new System.NotImplementedException();
            }

            public override void ReregisterRegion()
            {
                throw new System.NotImplementedException();
            }

            public override void ResetParcels()
            {
                throw new System.NotImplementedException();
            }

            public override void RevertTerrainToDefault(IAgent agent)
            {
                throw new System.NotImplementedException();
            }

            public override void RezScriptsForObject(ObjectGroup group)
            {
                throw new System.NotImplementedException();
            }

            public override void SendRegionInfo(IAgent agent)
            {
                throw new System.NotImplementedException();
            }

            public override void StartStorage()
            {
                throw new System.NotImplementedException();
            }

            public override void StoreTerrainAsDefault(IAgent agent)
            {
                throw new System.NotImplementedException();
            }

            public override void SwapTerrainWithDefault(IAgent agent)
            {
                throw new System.NotImplementedException();
            }

            public override void TriggerEstateUpdate()
            {
                throw new System.NotImplementedException();
            }

            public override void TriggerLightShareSettingsChanged()
            {
                throw new System.NotImplementedException();
            }

            public override void TriggerParcelUpdate(ParcelInfo pInfo)
            {
                throw new System.NotImplementedException();
            }

            public override void TriggerRegionDataChanged()
            {
                throw new System.NotImplementedException();
            }

            public override void TriggerRegionSettingsChanged()
            {
                throw new System.NotImplementedException();
            }

            public override void TriggerStoreOfEnvironmentSettings()
            {
                throw new System.NotImplementedException();
            }

            protected override void TriggerSpawnpointUpdate()
            {
                throw new System.NotImplementedException();
            }
        }
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool Run()
        {
            var owner = new UUI("11223344-1122-1122-1122-123456789012");
            var regionID = new UUID("12345678-1234-1234-1234-123456789012");
            var scene = new DummyScene(256, 256, regionID);
            var objgrp = new ObjectGroup();
            var part = new ObjectPart
            {
                Name = "Test Object"
            };
            var item = new ObjectPartInventoryItem
            {
                Name = "Test Item",
                AssetType = AssetType.CallingCard,
                InventoryType = InventoryType.CallingCard,
                Owner = owner
            };
            part.Inventory.Add(item);
            objgrp.Add(1, part);
            objgrp.Owner = owner;
            part.ObjectGroup = objgrp;
            objgrp.Scene = scene;

            SceneListener listener = SimulationData.GetSceneListener(regionID);
            listener.StartStorageThread();
            try
            {
                m_Log.Info("Checking that region object data is empty");
                if (SimulationData.Objects.ObjectsInRegion(regionID).Count != 0)
                {
                    return false;
                }
                m_Log.Info("Checking that region prim data is empty");
                if (SimulationData.Objects.PrimitivesInRegion(regionID).Count != 0)
                {
                    return false;
                }
                m_Log.Info("Checking that actual object does not exist");
                if (SimulationData.Objects[regionID].Count != 0)
                {
                    return false;
                }

                m_Log.Info("Store object");
                listener.ScheduleUpdate(part.UpdateInfo, regionID);
                /* ensure working time for listener */
                m_Log.Info("Wait 2s for processing");
                Thread.Sleep(2000);

                List<UUID> resultList;
                m_Log.Info("Checking that region contains one object");
                resultList = SimulationData.Objects.ObjectsInRegion(regionID);
                if (resultList.Count != 1)
                {
                    return false;
                }

                if (resultList[0] != objgrp.ID)
                {
                    return false;
                }

                m_Log.Info("Checking that region contains one prim");
                resultList = SimulationData.Objects.PrimitivesInRegion(regionID);
                if (resultList.Count != 1)
                {
                    return false;
                }
                m_Log.Info("Checking that actual object exists");
                List<ObjectGroup> objectList = SimulationData.Objects[regionID];
                if (objectList.Count != 1)
                {
                    return false;
                }

                m_Log.Info("Check that actual object contains one prim");
                if (objectList[0].Count != 1)
                {
                    return false;
                }

                ObjectPart resPart;
                m_Log.Info("Try retrieving known prim");
                if (!objectList[0].TryGetValue(part.ID, out resPart))
                {
                    return false;
                }

                m_Log.Info("Check that actual prim contains one item");
                if (resPart.Inventory.Count != 1)
                {
                    return false;
                }

                m_Log.Info("Check that actual item has known ID");
                if (!resPart.Inventory.ContainsKey(item.ID))
                {
                    return false;
                }

                m_Log.Info("Remove inventory item");
                resPart.Inventory.Remove(item.ID);
                item.UpdateInfo.SetRemovedItem();
                listener.ScheduleUpdate(item.UpdateInfo, regionID);
                /* ensure working time for listener */
                m_Log.Info("Wait 2s for processing");
                Thread.Sleep(2000);

                m_Log.Info("check that inventory item got deleted");
                objectList = SimulationData.Objects[regionID];
                if (objectList.Count != 1 || !objectList[0].TryGetValue(part.ID, out resPart) || resPart.Inventory.Count != 0)
                {
                    return false;
                }

                m_Log.Info("Remove prim");
                part.UpdateInfo.KillObject();
                listener.ScheduleUpdate(part.UpdateInfo, regionID);

                /* ensure working time for listener */
                m_Log.Info("Wait 2s for processing");
                Thread.Sleep(2000);

                m_Log.Info("Checking that region object data is empty");
                if (SimulationData.Objects.ObjectsInRegion(regionID).Count != 0)
                {
                    return false;
                }
                m_Log.Info("Checking that region prim data is empty");
                if (SimulationData.Objects.PrimitivesInRegion(regionID).Count != 0)
                {
                    return false;
                }
                m_Log.Info("Checking that actual object does not exist");
                if (SimulationData.Objects[regionID].Count != 0)
                {
                    return false;
                }
            }
            finally
            {
                listener.StopStorageThread();
            }
            return true;
        }
    }
}
