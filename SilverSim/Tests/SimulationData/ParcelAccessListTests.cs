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
using SilverSim.Scene.ServiceInterfaces.SimulationData;
using SilverSim.Types;
using SilverSim.Types.Parcel;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.SimulationData
{
    public class ParcelAccessListTests : CommonSimDataTest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool Run()
        {
            return RunSection(SimulationData.Parcels.WhiteList, "WhiteList") && 
                RunSection(SimulationData.Parcels.BlackList, "BlackList") && 
                RunSection(SimulationData.Parcels.LandpassList, "LandpassList");
        }

        private bool CompareEntry(ParcelAccessEntry a, ParcelAccessEntry b)
        {
            var mismatches = new List<string>();
            if(!a.Accessor.EqualsGrid(b.Accessor))
            {
                mismatches.Add("Accessor");
            }
            if(a.RegionID != b.RegionID)
            {
                m_Log.InfoFormat("Mismatching RegionID {0} != {1}", a.RegionID, b.RegionID);
                mismatches.Add("RegionID");
            }
            if (a.ParcelID != b.ParcelID)
            {
                m_Log.InfoFormat("Mismatching ParcelID {0} != {1}", a.ParcelID, b.ParcelID);
                mismatches.Add("ParcelID");
            }

            if(a.ExpiresAt == null && b.ExpiresAt == null)
            {
                return true;
            }
            else if (a.ExpiresAt == null || b.ExpiresAt == null ||
                a.ExpiresAt.AsULong != b.ExpiresAt.AsULong)
            {
                mismatches.Add("ExpiresAt");
            }
            if (mismatches.Count != 0)
            {
                m_Log.InfoFormat("Detected mismatches {0}", string.Join(" ", mismatches));
            }
            return mismatches.Count == 0;
        }

        private bool RunSection(ISimulationDataParcelAccessListStorageInterface lut, string listInfo)
        {
            m_Log.InfoFormat("Testing {0}", listInfo);

            var regionId = new UUID("11223344-1122-1122-1122-112233445566");
            var parcelId = new UUID("11223344-1122-1122-1122-112233445577");
            var user = new UUI("11223344-1122-1122-1122-112233445588;http://example.com/;Example Com");
            ParcelAccessEntry entry;
            ParcelAccessEntry testentry;
            List<ParcelAccessEntry> res;

            m_Log.InfoFormat("{0}: A: Testing non-existence 1", listInfo);
            if(lut[regionId, parcelId, user])
            {
                return false;
            }
            m_Log.InfoFormat("{0}: A: Testing non-existence 2", listInfo);
            if (lut.TryGetValue(regionId, parcelId, user, out entry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: A: Testing non-existence 3", listInfo);
            if (lut[regionId, parcelId].Count != 0)
            {
                return false;
            }

            m_Log.InfoFormat("{0}: Storing unlimited entry", listInfo);
            testentry = new ParcelAccessEntry
            {
                RegionID = regionId,
                ParcelID = parcelId,
                Accessor = user
            };

            lut.Store(testentry);

            m_Log.InfoFormat("{0}: A: Testing existence 1", listInfo);
            if (!lut[regionId, parcelId, user])
            {
                return false;
            }
            m_Log.InfoFormat("{0}: A: Testing existence 2", listInfo);
            if (!lut.TryGetValue(regionId, parcelId, user, out entry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: A: Comparing entry", listInfo);
            if (!CompareEntry(entry, testentry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: A: Testing existence 3", listInfo);
            res = lut[regionId, parcelId];
            if (res.Count != 1)
            {
                m_Log.InfoFormat("{0}: A: Expecting=1 Got={1}", listInfo, res.Count);
                return false;
            }

            m_Log.InfoFormat("{0}: A: Comparing entry", listInfo);
            if (!CompareEntry(res[0], testentry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: A: Remove entry", listInfo);
            if(!lut.Remove(regionId, parcelId, user))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: A: Check if entry is gone", listInfo);
            if (lut.Remove(regionId, parcelId, user))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: B: Testing non-existence 1", listInfo);
            if (lut[regionId, parcelId, user])
            {
                return false;
            }
            m_Log.InfoFormat("{0}: B: Testing non-existence 2", listInfo);
            if (lut.TryGetValue(regionId, parcelId, user, out entry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: B: Testing non-existence 3", listInfo);
            if (lut[regionId, parcelId].Count != 0)
            {
                return false;
            }


            m_Log.InfoFormat("{0}: B: Storing limited entry", listInfo);
            testentry = new ParcelAccessEntry
            {
                RegionID = regionId,
                ParcelID = parcelId,
                Accessor = user,
                ExpiresAt = Date.UnixTimeToDateTime(Date.Now.AsULong + 1000000)
            };

            lut.Store(testentry);

            m_Log.InfoFormat("{0}: B: Testing existence 1", listInfo);
            if (!lut[regionId, parcelId, user])
            {
                return false;
            }
            m_Log.InfoFormat("{0}: B: Testing existence 2", listInfo);
            if (!lut.TryGetValue(regionId, parcelId, user, out entry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: B: Comparing entry", listInfo);
            if (!CompareEntry(entry, testentry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: B: Testing existence 3", listInfo);
            res = lut[regionId, parcelId];
            if (res.Count != 1)
            {
                m_Log.InfoFormat("{0}: B: Expecting=1 Got={1}", listInfo, res.Count);
                return false;
            }

            m_Log.InfoFormat("{0}: B: Comparing entry", listInfo);
            if (!CompareEntry(res[0], testentry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: B: Remove entry", listInfo);
            if (!lut.Remove(regionId, parcelId, user))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: B: Check if entry is gone", listInfo);
            if (lut.Remove(regionId, parcelId, user))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: C: Testing non-existence 1", listInfo);
            if (lut[regionId, parcelId, user])
            {
                return false;
            }
            m_Log.InfoFormat("{0}: C: Testing non-existence 2", listInfo);
            if (lut.TryGetValue(regionId, parcelId, user, out entry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: C: Testing non-existence 3", listInfo);
            if (lut[regionId, parcelId].Count != 0)
            {
                return false;
            }

            m_Log.InfoFormat("{0}: D: Storing limited entry into past 1", listInfo);
            testentry = new ParcelAccessEntry
            {
                RegionID = regionId,
                ParcelID = parcelId,
                Accessor = user,
                ExpiresAt = Date.UnixTimeToDateTime(Date.Now.AsULong - 1000000)
            };

            lut.Store(testentry);

            m_Log.InfoFormat("{0}: D: Testing non-existence 1", listInfo);
            if (lut[regionId, parcelId, user])
            {
                return false;
            }

            m_Log.InfoFormat("{0}: E: Storing limited entry into past 2", listInfo);
            lut.Store(testentry);

            m_Log.InfoFormat("{0}: E: Testing non-existence 2", listInfo);
            if (lut.TryGetValue(regionId, parcelId, user, out entry))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: F: Storing limited entry into past 3", listInfo);
            lut.Store(testentry);

            m_Log.InfoFormat("{0}: F: Testing non-existence 3", listInfo);
            if (lut[regionId, parcelId].Count != 0)
            {
                return false;
            }

            testentry.ExpiresAt = null;
            m_Log.InfoFormat("{0}: G: Storing entry 1", listInfo);
            lut.Store(testentry);
            m_Log.InfoFormat("{0}: G: Storing entry 2", listInfo);
            testentry.Accessor = new UUI("22334455-1122-1122-1122-112233445566;http://example.com/;Com Example");
            lut.Store(testentry);

            m_Log.InfoFormat("{0}: G: Removing entry 2", listInfo);
            if(!lut.Remove(regionId, parcelId, testentry.Accessor))
            {
                return false;
            }

            m_Log.InfoFormat("{0}: G: Check entry 1", listInfo);
            if (!lut[regionId, parcelId, user])
            {
                m_Log.InfoFormat("{0}: G: Entry 1 should not have been deleted when deleting entry 2", listInfo);
                return false;
            }

            m_Log.InfoFormat("{0}: G: Removing entry 1", listInfo);
            if (!lut.Remove(regionId, parcelId, user))
            {
                return false;
            }

            return true;
        }
    }
}
