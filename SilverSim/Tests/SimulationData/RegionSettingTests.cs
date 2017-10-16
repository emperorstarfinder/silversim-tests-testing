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
using SilverSim.Scene.Types.Scene;
using SilverSim.Types;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.SimulationData
{
    public class RegionSettingTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool CompareSettings(RegionSettings a, RegionSettings b)
        {
            var mismatches = new List<string>();

            if(a.AgentLimit != b.AgentLimit)
            {
                mismatches.Add("AgentLimit");
            }

            if(a.AllowDamage != b.AllowDamage)
            {
                mismatches.Add("AllowDamage");
            }

            if(a.AllowDirectTeleport != b.AllowDirectTeleport)
            {
                mismatches.Add("AllowDirectTeleport");
            }

            if(a.AllowLandJoinDivide != b.AllowLandJoinDivide)
            {
                mismatches.Add("AllowLandJoinDivide");
            }

            if(a.AllowLandmark != b.AllowLandmark)
            {
                mismatches.Add("AllowLandmark");
            }

            if(a.AllowLandResell != b.AllowLandResell)
            {
                mismatches.Add("AllowLandResell");
            }

            if(a.BlockDwell != b.BlockDwell)
            {
                mismatches.Add("BlockDwell");
            }

            if(a.BlockFly != b.BlockFly)
            {
                mismatches.Add("BlockFly");
            }

            if(a.BlockFlyOver != b.BlockFlyOver)
            {
                mismatches.Add("BlockFlyOver");
            }

            if(a.BlockShowInSearch != b.BlockShowInSearch)
            {
                mismatches.Add("BlockShowInSearch");
            }

            if(a.BlockTerraform != b.BlockTerraform)
            {
                mismatches.Add("BlockTerraform");
            }

            if(a.DisableCollisions != b.DisableCollisions)
            {
                mismatches.Add("DisableCollisions");
            }

            if(a.DisablePhysics != b.DisablePhysics)
            {
                mismatches.Add("DisablePhysics");
            }

            if(a.DisableScripts != b.DisableScripts)
            {
                mismatches.Add("DisableScripts");
            }

            if(a.Elevation1NE != b.Elevation1NE)
            {
                mismatches.Add("Elevation1NE");
            }

            if(a.Elevation1NW != b.Elevation1NW)
            {
                mismatches.Add("Elevation1NW");
            }

            if(a.Elevation1SE != b.Elevation1SE)
            {
                mismatches.Add("Elevation1SE");
            }

            if(a.Elevation1SW != b.Elevation1SW)
            {
                mismatches.Add("Elevation1SW");
            }

            if(a.Elevation2NE != b.Elevation2NE)
            {
                mismatches.Add("Elevation2NE");
            }

            if(a.Elevation2NW != b.Elevation2NW)
            {
                mismatches.Add("Elevation2NW");
            }

            if(a.Elevation2SE != b.Elevation2SE)
            {
                mismatches.Add("Elevation2SE");
            }

            if(a.Elevation2SW != b.Elevation2SW)
            {
                mismatches.Add("Elevation2SW");
            }

            if(a.IsSunFixed != b.IsSunFixed)
            {
                mismatches.Add("IsSunFixed");
            }

            if(a.ObjectBonus != b.ObjectBonus)
            {
                mismatches.Add("ObjectBonus");
            }

            if(a.ResetHomeOnTeleport != b.ResetHomeOnTeleport)
            {
                mismatches.Add("ResetHomeOnTeleport");
            }

            if(a.RestrictPushing != b.RestrictPushing)
            {
                mismatches.Add("RestrictPushing");
            }

            if(a.Sandbox != b.Sandbox)
            {
                mismatches.Add("Sandbox");
            }

            if(a.SunPosition != b.SunPosition)
            {
                mismatches.Add("SunPosition");
            }

            if(a.TelehubObject != b.TelehubObject)
            {
                mismatches.Add("TelehubObject");
            }

            if(a.TerrainLowerLimit != b.TerrainLowerLimit)
            {
                mismatches.Add("TerrainLowerLimit");
            }

            if(a.TerrainRaiseLimit != b.TerrainRaiseLimit)
            {
                mismatches.Add("TerrainRaiseLimit");
            }

            if(a.TerrainTexture1 != b.TerrainTexture1)
            {
                mismatches.Add("TerrainTexture1");
            }

            if(a.TerrainTexture2 != b.TerrainTexture2)
            {
                mismatches.Add("TerrainTexture2");
            }

            if (a.TerrainTexture3 != b.TerrainTexture3)
            {
                mismatches.Add("TerrainTexture3");
            }

            if (a.TerrainTexture4 != b.TerrainTexture4)
            {
                mismatches.Add("TerrainTexture4");
            }

            if(a.UseEstateSun != b.UseEstateSun)
            {
                mismatches.Add("UseEstateSun");
            }

            if(a.WaterHeight != b.WaterHeight)
            {
                mismatches.Add("WaterHeight");
            }

            if (mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }
            return mismatches.Count == 0;
        }

        public override bool Run()
        {
            var testData = new RegionSettings
            {
                AgentLimit = 100,
                AllowDamage = true,
                AllowDirectTeleport = true,
                AllowLandJoinDivide = true,
                AllowLandmark = true,
                AllowLandResell = true,
                BlockTerraform = true,
                ObjectBonus = 1,
                ResetHomeOnTeleport = true,
                RestrictPushing = true,
                TelehubObject = new UUID("11223344-1122-1122-1100-112233445566"),
                UseEstateSun = true
            };
            RegionSettings retrieveData;
            var testRegion = new UUID("11223344-1122-1122-1122-112233445566");

            m_Log.Info("Testing non-existence via ContainsKey");
            if(SimulationData.RegionSettings.ContainsKey(testRegion))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via TryGetValue");
            if (SimulationData.RegionSettings.TryGetValue(testRegion, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via this");
            try
            {
                retrieveData = SimulationData.RegionSettings[testRegion];
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* expected exception */
            }

            m_Log.Info("Storing region settings");
            SimulationData.RegionSettings[testRegion] = testData;

            m_Log.Info("Testing existence via ContainsKey");
            if (!SimulationData.RegionSettings.ContainsKey(testRegion))
            {
                return false;
            }

            m_Log.Info("Testing existence via TryGetValue");
            if (!SimulationData.RegionSettings.TryGetValue(testRegion, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if(!CompareSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing existence via this");
            retrieveData = SimulationData.RegionSettings[testRegion];

            m_Log.Info("Comparing result");
            if (!CompareSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Removing RegionSettings via Remove");
            if(!SimulationData.RegionSettings.Remove(testRegion))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via ContainsKey");
            if (SimulationData.RegionSettings.ContainsKey(testRegion))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via TryGetValue");
            if (SimulationData.RegionSettings.TryGetValue(testRegion, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via this");
            try
            {
                retrieveData = SimulationData.RegionSettings[testRegion];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* expected exception */
            }

            m_Log.Info("Storing region settings");
            SimulationData.RegionSettings[testRegion] = testData;

            m_Log.Info("Testing existence via ContainsKey");
            if (!SimulationData.RegionSettings.ContainsKey(testRegion))
            {
                return false;
            }

            m_Log.Info("Testing existence via TryGetValue");
            if (!SimulationData.RegionSettings.TryGetValue(testRegion, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing existence via this");
            retrieveData = SimulationData.RegionSettings[testRegion];

            m_Log.Info("Comparing result");
            if (!CompareSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Removing RegionSettings via RemoveRegion");
            SimulationData.RemoveRegion(testRegion);

            m_Log.Info("Testing non-existence via ContainsKey");
            if (SimulationData.RegionSettings.ContainsKey(testRegion))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via TryGetValue");
            if (SimulationData.RegionSettings.TryGetValue(testRegion, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence via this");
            try
            {
                retrieveData = SimulationData.RegionSettings[testRegion];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* expected exception */
            }

            return true;
        }
    }
}
