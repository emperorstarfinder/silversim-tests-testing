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
using SilverSim.Types;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.SimulationData
{
    public sealed class ScriptStateTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            byte[] testData = "Hello, World".ToUTF8Bytes();
            UUID regionID = new UUID("11223344-1122-1122-1122-112233445566");
            UUID primID = new UUID("11223344-1122-1122-1123-112233445566");
            UUID itemID = new UUID("11223344-1122-1122-1124-112233445566");

            byte[] retrieveData;

            m_Log.Info("Testing non-existence of script state via TryGetValue");
            if(SimulationData.ScriptStates.TryGetValue(regionID, primID, itemID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of script state via this");
            try
            {
                retrieveData = SimulationData.ScriptStates[regionID, primID, itemID];
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* expected exception */
            }

            m_Log.Info("Storing script state");
            SimulationData.ScriptStates[regionID, primID, itemID] = testData;

            m_Log.Info("Testing existence of script state via TryGetValue");
            if (!SimulationData.ScriptStates.TryGetValue(regionID, primID, itemID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if(!CompareArrays(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing existence of script state via this");
            retrieveData = SimulationData.ScriptStates[regionID, primID, itemID];

            m_Log.Info("Comparing result");
            if (!CompareArrays(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Removing script state via Remove");
            if(!SimulationData.ScriptStates.Remove(regionID, primID, itemID))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of script state via TryGetValue");
            if (SimulationData.ScriptStates.TryGetValue(regionID, primID, itemID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of script state via this");
            try
            {
                retrieveData = SimulationData.ScriptStates[regionID, primID, itemID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* expected exception */
            }

            m_Log.Info("Storing script state");
            SimulationData.ScriptStates[regionID, primID, itemID] = testData;

            m_Log.Info("Testing existence of script state via TryGetValue");
            if (!SimulationData.ScriptStates.TryGetValue(regionID, primID, itemID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Comparing result");
            if (!CompareArrays(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing existence of script state via this");
            retrieveData = SimulationData.ScriptStates[regionID, primID, itemID];

            m_Log.Info("Comparing result");
            if (!CompareArrays(testData, retrieveData))
            {
                return false;
            }

            m_Log.Info("Removing script state via RemoveRegion");
            SimulationData.RemoveRegion(regionID);

            m_Log.Info("Testing non-existence of script state via TryGetValue");
            if (SimulationData.ScriptStates.TryGetValue(regionID, primID, itemID, out retrieveData))
            {
                return false;
            }

            m_Log.Info("Testing non-existence of script state via this");
            try
            {
                retrieveData = SimulationData.ScriptStates[regionID, primID, itemID];
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
