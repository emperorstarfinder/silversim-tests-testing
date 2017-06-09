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
using SilverSim.Scene.Types.WindLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SilverSim.Types;
using System.Threading.Tasks;
using System.Xml;

namespace SilverSim.Tests.SimulationData
{
    public sealed class EnvSettingsTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private byte[] Serialize(EnvironmentSettings env)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                env.Serialize(ms, UUID.Zero);
                return ms.ToArray();
            }
        }

        private bool CompareEnvSettings(EnvironmentSettings a, EnvironmentSettings b) =>
            CompareArrays(Serialize(a), Serialize(b));

        private bool CompareArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
        public override bool Run()
        {
            EnvironmentSettings testData = new EnvironmentSettings();
            testData.SkySettings.Add("0", new SkyEntry());
            testData.SkySettings.Add("12", new SkyEntry());
            EnvironmentSettings retrieveData;

            UUID regionID = new UUID("11223344-1122-1122-1122-112233445566");

            m_Log.Info("Testing non-existence of environment settings data via TryGetValue");
            if (SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* we expect this exception here */
            }

            m_Log.Info("Storing environment settings test data");
            SimulationData.EnvironmentSettings[regionID] = testData;

            m_Log.Info("Testing existence of environment settings data via TryGetValue");
            if (!SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareEnvSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareEnvSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Unsetting environment settings data via this");
            SimulationData.EnvironmentSettings[regionID] = null;

            m_Log.Info("Testing non-existence of settings controller data via TryGetValue");
            if (SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* we expect this exception here */
            }

            m_Log.Info("Storing environment settings test data");
            SimulationData.EnvironmentSettings[regionID] = testData;

            m_Log.Info("Testing existence of environment settings data via TryGetValue");
            if (!SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareEnvSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareEnvSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Unsetting environment settings data via Remove");
            if (!SimulationData.EnvironmentSettings.Remove(regionID))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via TryGetValue");
            if (SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* we expect this exception here */
            }

            m_Log.Info("Storing environment settings test data");
            SimulationData.EnvironmentSettings[regionID] = testData;

            m_Log.Info("Testing existence of environment settings data via TryGetValue");
            if (!SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareEnvSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareEnvSettings(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Unsetting environment settings data via RemoveRegion");
            SimulationData.RemoveRegion(regionID);

            m_Log.Info("Testing non-existence of environment settings data via TryGetValue");
            if (SimulationData.EnvironmentSettings.TryGetValue(regionID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of environment settings data via this");
            try
            {
                retrieveData = SimulationData.EnvironmentSettings[regionID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* we expect this exception here */
            }

            return true;
        }
    }
}
