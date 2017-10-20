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
using SilverSim.Types.Parcel;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.SimulationData
{
    public sealed class ParcelExperienceTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool CompareEntry(ParcelExperienceEntry a, ParcelExperienceEntry b)
        {
            var items = new List<string>();

            if(a.RegionID != b.RegionID)
            {
                items.Add("RegionID");
            }
            if(a.ExperienceID != b.ExperienceID)
            {
                items.Add("ExperienceID");
            }
            if(a.ParcelID != b.ParcelID)
            {
                items.Add("ParcelID");
            }
            if(a.IsAllowed != b.IsAllowed)
            {
                items.Add("IsAllowed");
            }

            if(items.Count != 0)
            {
                m_Log.InfoFormat("Mismatched items: {0}", string.Join(" ", items));
            }
            return items.Count == 0;
        }

        public override bool Run()
        {
            var regionID = new UUID("12345678-1234-1234-1234-123456789012");
            var experienceID = new UUID("11223344-1122-1122-1122-112233445566");
            var experienceID2 = new UUID("11223344-1122-1122-1122-112233445567");
            var parcelID = new UUID("11223344-1122-1122-1122-665544332211");

            var testentry = new ParcelExperienceEntry
            {
                RegionID = regionID,
                ExperienceID = experienceID,
                ParcelID = parcelID,
                IsAllowed = true
            };

            ParcelExperienceEntry entry;

            m_Log.Info("Testing non-existence 1A");
            if(SimulationData.Parcels.Experiences.TryGetValue(regionID, parcelID, experienceID, out entry))
            {
                return false;
            }

            m_Log.Info("Testing non-existence 2A");
            try
            {
                entry = SimulationData.Parcels.Experiences[regionID, parcelID, experienceID];
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* this is the expected */
            }

            m_Log.Info("Testing non-existence 3A");
            if(SimulationData.Parcels.Experiences[regionID, parcelID].Count != 0)
            {
                return false;
            }

            SimulationData.Parcels.Experiences.Store(testentry);

            m_Log.Info("Testing existence 1A");
            if(!SimulationData.Parcels.Experiences.TryGetValue(regionID, parcelID, experienceID, out entry))
            {
                return false;
            }
            if(!CompareEntry(entry, testentry))
            {
                return false;
            }

            m_Log.Info("Testing existence 2A");
            entry = SimulationData.Parcels.Experiences[regionID, parcelID, experienceID];

            if(!CompareEntry(entry, testentry))
            {
                return false;
            }

            m_Log.Info("Testing existence 3A");
            List<ParcelExperienceEntry> entries = SimulationData.Parcels.Experiences[regionID, parcelID];
            if(entries.Count != 1)
            {
                return false;
            }

            if(!CompareEntry(entries[0], testentry))
            {
                return false;
            }

            m_Log.Info("Removing entry");
            if(!SimulationData.Parcels.Experiences.Remove(regionID, parcelID, experienceID))
            {
                return false;
            }

            m_Log.Info("Testing that a non-existing entry on removal is false");
            if (SimulationData.Parcels.Experiences.Remove(regionID, parcelID, experienceID))
            {
                return false;
            }


            testentry.IsAllowed = false;

            m_Log.Info("Testing non-existence 1B");
            if (SimulationData.Parcels.Experiences.TryGetValue(regionID, parcelID, experienceID, out entry))
            {
                return false;
            }

            m_Log.Info("Testing non-existence 2B");
            try
            {
                entry = SimulationData.Parcels.Experiences[regionID, parcelID, experienceID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is the expected */
            }

            m_Log.Info("Testing non-existence 3B");
            if (SimulationData.Parcels.Experiences[regionID, parcelID].Count != 0)
            {
                return false;
            }

            SimulationData.Parcels.Experiences.Store(testentry);

            m_Log.Info("Testing existence 1B");
            if (!SimulationData.Parcels.Experiences.TryGetValue(regionID, parcelID, experienceID, out entry))
            {
                return false;
            }
            if (!CompareEntry(entry, testentry))
            {
                return false;
            }

            m_Log.Info("Testing existence 2B");
            entry = SimulationData.Parcels.Experiences[regionID, parcelID, experienceID];

            if (!CompareEntry(entry, testentry))
            {
                return false;
            }

            m_Log.Info("Testing existence 3B");
            entries = SimulationData.Parcels.Experiences[regionID, parcelID];
            if (entries.Count != 1)
            {
                return false;
            }

            if (!CompareEntry(entries[0], testentry))
            {
                return false;
            }

            m_Log.Info("Removing entry");
            if (!SimulationData.Parcels.Experiences.Remove(regionID, parcelID, experienceID))
            {
                return false;
            }

            m_Log.Info("Testing that a non-existing entry on removal is false");
            if (SimulationData.Parcels.Experiences.Remove(regionID, parcelID, experienceID))
            {
                return false;
            }

            var testentry2 = new ParcelExperienceEntry
            {
                RegionID = regionID,
                ExperienceID = experienceID2,
                ParcelID = parcelID,
                IsAllowed = true
            };

            m_Log.Info("Store two entries");
            SimulationData.Parcels.Experiences.Store(testentry);
            SimulationData.Parcels.Experiences.Store(testentry2);

            m_Log.Info("Check that two entries exist");
            if(SimulationData.Parcels.Experiences[regionID, parcelID].Count != 2)
            {
                return false;
            }

            m_Log.Info("Remove first entry");
            if(!SimulationData.Parcels.Experiences.Remove(regionID, parcelID, experienceID))
            {
                return false;
            }

            m_Log.Info("Check existence of entry 2");
            if(!SimulationData.Parcels.Experiences.TryGetValue(regionID, parcelID, experienceID2, out entry))
            {
                return false;
            }

            m_Log.Info("Re-add first entry");
            SimulationData.Parcels.Experiences.Store(testentry);

            m_Log.Info("Remove all experience entries");
            SimulationData.Parcels.Experiences.Remove(regionID, parcelID);

            m_Log.Info("Check that no entries exist");
            if(SimulationData.Parcels.Experiences[regionID, parcelID].Count != 0)
            {
                return false;
            }
            return true;
        }
    }
}
