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
    public sealed class SpawnPointTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool Run()
        {
            UUID testRegion = new UUID("11223344-1122-1122-1122-112233445566");
            List<Vector3> testData = new List<Vector3>
            {
                new Vector3(1,1,1),
                new Vector3(-1,-1,1),
                new Vector3(2,2,2),
                new Vector3(2,2,-2)
            };

            List<Vector3> retrieveData;

            m_Log.Info("Verify empty spawnpoint list");
            if(SimulationData.Spawnpoints[testRegion].Count != 0)
            {
                return false;
            }

            m_Log.Info("Store spawnpoint list");
            SimulationData.Spawnpoints[testRegion] = testData;

            m_Log.Info("Verify set spawnpoint list");
            retrieveData = SimulationData.Spawnpoints[testRegion];

            m_Log.Info("Check number of spawnpoints");
            if(retrieveData.Count != testData.Count)
            {
                return false;
            }

            m_Log.Info("Comparing result");
            for (int i = 0; i < testData.Count; ++i)
            {
                if (testData[i] != retrieveData[i])
                {
                    return false;
                }
            }

            m_Log.Info("Testing remove via Remove");
            if(!SimulationData.Spawnpoints.Remove(testRegion))
            {
                return false;
            }

            m_Log.Info("Verify empty spawnpoint list");
            if (SimulationData.Spawnpoints[testRegion].Count != 0)
            {
                return false;
            }

            m_Log.Info("Store spawnpoint list");
            SimulationData.Spawnpoints[testRegion] = testData;

            m_Log.Info("Verify set spawnpoint list");
            retrieveData = SimulationData.Spawnpoints[testRegion];

            m_Log.Info("Check number of spawnpoints");
            if (retrieveData.Count != testData.Count)
            {
                return false;
            }

            m_Log.Info("Comparing result");
            for (int i = 0; i < testData.Count; ++i)
            {
                if (testData[i] != retrieveData[i])
                {
                    return false;
                }
            }

            m_Log.Info("Testing remove via RemoveRegion");
            SimulationData.RemoveRegion(testRegion);

            m_Log.Info("Verify empty spawnpoint list");
            if (SimulationData.Spawnpoints[testRegion].Count != 0)
            {
                return false;
            }

            return true;
        }
    }
}
