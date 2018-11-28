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
    public sealed class RegionTrustedExperienceTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool Run()
        {
            var regionID = new UUID("12345678-1234-1234-1234-123456789012");
            var experienceID = new UEI("11223344-1122-1122-1122-112233445566");
            var experienceID2 = new UEI("11223344-1122-1122-1122-112233445567");
            bool trusted;
            List<UEI> res;

            m_Log.Info("Testing non-existence 1A");
            if(!SimulationData.TrustedExperiences.TryGetValue(regionID, experienceID, out trusted) || trusted)
            {
                return false;
            }

            m_Log.Info("Testing non-existence 2A");
            if(SimulationData.TrustedExperiences[regionID, experienceID])
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3A");
            if(SimulationData.TrustedExperiences[regionID].Count != 0)
            {
                return false;
            }

            SimulationData.TrustedExperiences[regionID, experienceID] = true;

            m_Log.Info("Testing existence 1A");
            if (!SimulationData.TrustedExperiences.TryGetValue(regionID, experienceID, out trusted) || !trusted)
            {
                return false;
            }

            m_Log.Info("Testing existence 2A");
            if (!SimulationData.TrustedExperiences[regionID, experienceID])
            {
                return false;
            }
            m_Log.Info("Testing existence 3A");
            res = SimulationData.TrustedExperiences[regionID];
            if (res.Count != 1)
            {
                return false;
            }
            if(res[0] != experienceID)
            {
                return false;
            }

            m_Log.Info("Unset trusted experience");
            SimulationData.TrustedExperiences[regionID, experienceID] = false;

            m_Log.Info("Testing non-existence 1B");
            if (!SimulationData.TrustedExperiences.TryGetValue(regionID, experienceID, out trusted) || trusted)
            {
                return false;
            }

            m_Log.Info("Testing non-existence 2B");
            if (SimulationData.TrustedExperiences[regionID, experienceID])
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3B");
            if (SimulationData.TrustedExperiences[regionID].Count != 0)
            {
                return false;
            }

            SimulationData.TrustedExperiences[regionID, experienceID] = true;

            m_Log.Info("Testing existence 1B");
            if (!SimulationData.TrustedExperiences.TryGetValue(regionID, experienceID, out trusted) || !trusted)
            {
                return false;
            }

            m_Log.Info("Testing existence 2B");
            if (!SimulationData.TrustedExperiences[regionID, experienceID])
            {
                return false;
            }
            m_Log.Info("Testing existence 3B");
            res = SimulationData.TrustedExperiences[regionID];
            if (res.Count != 1)
            {
                return false;
            }
            if (res[0] != experienceID)
            {
                return false;
            }

            m_Log.Info("Remove entry");
            if(!SimulationData.TrustedExperiences.Remove(regionID, experienceID))
            {
                return false;
            }
            m_Log.Info("Try to remove removed entry");
            if (SimulationData.TrustedExperiences.Remove(regionID, experienceID))
            {
                return false;
            }

            m_Log.Info("Testing non-existence 1C");
            if (!SimulationData.TrustedExperiences.TryGetValue(regionID, experienceID, out trusted) || trusted)
            {
                return false;
            }

            m_Log.Info("Testing non-existence 2C");
            if (SimulationData.TrustedExperiences[regionID, experienceID])
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3C");
            if (SimulationData.TrustedExperiences[regionID].Count != 0)
            {
                return false;
            }

            m_Log.Info("Storing two entries");
            SimulationData.TrustedExperiences[regionID, experienceID] = true;
            SimulationData.TrustedExperiences[regionID, experienceID2] = true;

            m_Log.Info("Check that two entries exist");
            if(SimulationData.TrustedExperiences[regionID].Count != 2)
            {
                return false;
            }

            m_Log.Info("Remove first entry");
            SimulationData.TrustedExperiences[regionID, experienceID] = false;

            m_Log.Info("Check that entry 2 still exists");
            if(!SimulationData.TrustedExperiences[regionID, experienceID2])
            {
                return false;
            }

            m_Log.Info("Re-add first entry");
            SimulationData.TrustedExperiences[regionID, experienceID] = true;

            m_Log.Info("Remove all entries");
            SimulationData.TrustedExperiences.RemoveRegion(regionID);

            m_Log.Info("Check that no entries exist");
            if(SimulationData.TrustedExperiences[regionID].Count != 0)
            {
                return false;
            }

            return true;
        }
    }
}
