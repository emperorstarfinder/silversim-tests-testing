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
    public sealed class ParcelTests : CommonSimDataTest
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

        private bool CompareParcels(ParcelInfo a, ParcelInfo b)
        {
            List<string> mismatches = new List<string>();
            if(!CompareArrays(a.LandBitmap.Data, b.LandBitmap.Data))
            {
                mismatches.Add("LandBitmap");
            }

            if(a.ActualArea != b.ActualArea)
            {
                mismatches.Add("ActualArea");
            }

            if(a.AnyAvatarSounds != b.AnyAvatarSounds)
            {
                mismatches.Add("AnyAvatarSounds");
            }

            if(a.Area != b.Area)
            {
                mismatches.Add("Area");
            }

            if(a.AuctionID != b.AuctionID)
            {
                mismatches.Add("AuctionID");
            }

            if(!a.AuthBuyer.EqualsGrid(b.AuthBuyer))
            {
                mismatches.Add("AuthBuyer");
            }

            if(a.BillableArea != b.BillableArea)
            {
                mismatches.Add("BillableArea");
            }

            if(a.Category != b.Category)
            {
                mismatches.Add("Category");
            }

            if(a.ClaimDate.AsULong != b.ClaimDate.AsULong)
            {
                mismatches.Add("ClaimDate");
            }

            if(a.ClaimPrice != b.ClaimPrice)
            {
                mismatches.Add("ClaimPrice");
            }

            if(a.Description != b.Description)
            {
                mismatches.Add("Description");
            }

            if(a.Dwell != b.Dwell)
            {
                mismatches.Add("Dwell");
            }

            if(a.Flags != b.Flags)
            {
                mismatches.Add("Flags");
            }

            if(!a.Group.Equals(b.Group))
            {
                mismatches.Add("Group");
            }

            if(a.GroupAvatarSounds != b.GroupAvatarSounds)
            {
                mismatches.Add("GroupAvatarSounds");
            }

            if(a.GroupOwned != b.GroupOwned)
            {
                mismatches.Add("GroupOwned");
            }

            if(a.ID != b.ID)
            {
                mismatches.Add("ID");
            }

            if(a.IsPrivate != b.IsPrivate)
            {
                mismatches.Add("IsPrivate");
            }

            if(a.LandingLookAt != b.LandingLookAt)
            {
                mismatches.Add("LandingLookAt");
            }

            if(a.LandingPosition != b.LandingPosition)
            {
                mismatches.Add("LandingPosition");
            }

            if(a.LandingType != b.LandingType)
            {
                mismatches.Add("LandingType");
            }

            if(a.MediaAutoScale != b.MediaAutoScale)
            {
                mismatches.Add("MediaAutoScale");
            }

            if(a.MediaDescription != b.MediaDescription)
            {
                mismatches.Add("MediaDescription");
            }

            if(a.MediaHeight != b.MediaHeight)
            {
                mismatches.Add("MediaHeight");
            }

            if(a.MediaID != b.MediaID)
            {
                mismatches.Add("MediaID");
            }

            if(a.MediaLoop != b.MediaLoop)
            {
                mismatches.Add("MediaLoop");
            }

            if(a.MediaType != b.MediaType)
            {
                mismatches.Add("MediaType");
            }

            if(!CompareURI(a.MediaURI, b.MediaURI))
            {
                mismatches.Add("MediaURI");
            }

            if(a.MediaWidth != b.MediaWidth)
            {
                mismatches.Add("MediaWidth");
            }

            if(!CompareURI(a.MusicURI, b.MusicURI))
            {
                mismatches.Add("MusicURI");
            }

            if(a.Name != b.Name)
            {
                mismatches.Add("Name");
            }

            if(a.ObscureMedia != b.ObscureMedia)
            {
                mismatches.Add("ObscureMedia");
            }

            if(a.ObscureMusic != b.ObscureMusic)
            {
                mismatches.Add("ObscureMusic");
            }

            if(a.OtherCleanTime != b.OtherCleanTime)
            {
                mismatches.Add("OtherCleanTime");
            }

            if(!a.Owner.EqualsGrid(b.Owner))
            {
                mismatches.Add("Owner");
            }

            if(a.ParcelPrimBonus != b.ParcelPrimBonus)
            {
                mismatches.Add("ParcelPrimBonus");
            }

            if(a.PassHours != b.PassHours)
            {
                mismatches.Add("PassHours");
            }

            if(a.PassPrice != b.PassPrice)
            {
                mismatches.Add("PassPrice");
            }

            if(a.RentPrice != b.RentPrice)
            {
                mismatches.Add("RentPrice");
            }

            if(a.SalePrice != b.SalePrice)
            {
                mismatches.Add("SalePrice");
            }

            if(a.SeeAvatars != b.SeeAvatars)
            {
                mismatches.Add("SeeAvatars");
            }

            if(a.SnapshotID != b.SnapshotID)
            {
                mismatches.Add("SnapshotID");
            }

            if(a.Status != b.Status)
            {
                mismatches.Add("Status");
            }

            if(mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches {0}", string.Join(" ", mismatches));
            }

            return mismatches.Count == 0;
        }

        private bool CompareURI(URI a, URI b)
        {
            if(a == null && b == null)
            {
                return true;
            }
            if(a == null || b == null)
            {
                return false;
            }
            return a.ToString() == b.ToString();
        }

        public override bool Run()
        {
            ParcelInfo testData1 = new ParcelInfo(256, 256)
            {
                Name = "Parcel 1",
                ID = new UUID("11223344-1122-1122-0001-112233445566"),
                Owner = new UGUI("11223344-1122-1123-0001-112233445566")
            };
            UUID testRegion1 = new UUID("11223344-1122-1124-0001-112233445566");
            testData1.LandBitmap.SetAllBits();
            ParcelInfo testData2 = new ParcelInfo(512, 512)
            {
                Name = "Parcel 2",
                ID = new UUID("11223344-1122-1122-0002-112233445566"),
                Owner = new UGUI("11223344-1122-1123-0002-112233445566")
            };
            UUID testRegion2 = new UUID("11223344-1122-1124-0002-112233445566");
            testData2.LandBitmap.SetAllBits();
            ParcelInfo testData3 = new ParcelInfo(512, 256)
            {
                Name = "Parcel 3",
                ID = new UUID("11223344-1122-1122-0003-112233445566"),
                Owner = new UGUI("11223344-1122-1123-0003-112233445566")
            };
            UUID testRegion3 = new UUID("11223344-1122-1124-0003-112233445566");
            testData3.LandBitmap.SetAllBits();
            ParcelInfo testData4 = new ParcelInfo(256, 512)
            {
                Name = "Parcel 4",
                ID = new UUID("11223344-1122-1122-0004-112233445566"),
                Owner = new UGUI("11223344-1122-1123-0004-112233445566")
            };
            UUID testRegion4 = new UUID("11223344-1122-1124-0004-112233445566");
            testData4.LandBitmap.SetAllBits();

            ParcelInfo retrieveInfo;

            m_Log.Info("Testing non-existence of parcel 1");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion1, testData1.ID];
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 1 via ParcelsInRegion");
            if(SimulationData.Parcels.ParcelsInRegion(testRegion1).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 2");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion2, testData2.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 2 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion2).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 3");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion3, testData3.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 3 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion3).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 4");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion4, testData4.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 4 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion4).Count != 0)
            {
                return false;
            }

            m_Log.Info("Adding parcel 1");
            SimulationData.Parcels.Store(testRegion1, testData1);

            m_Log.Info("Adding parcel 2");
            SimulationData.Parcels.Store(testRegion2, testData2);

            m_Log.Info("Adding parcel 3");
            SimulationData.Parcels.Store(testRegion3, testData3);

            m_Log.Info("Adding parcel 4");
            SimulationData.Parcels.Store(testRegion4, testData4);

            m_Log.Info("Testing existence of parcel 1");
            retrieveInfo = SimulationData.Parcels[testRegion1, testData1.ID];

            if(!CompareParcels(retrieveInfo, testData1))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 1 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion1).Count != 1)
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 2");
            retrieveInfo = SimulationData.Parcels[testRegion2, testData2.ID];

            if (!CompareParcels(retrieveInfo, testData2))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 2 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion2).Count != 1)
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 3");
            retrieveInfo = SimulationData.Parcels[testRegion3, testData3.ID];

            if (!CompareParcels(retrieveInfo, testData3))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 3 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion3).Count != 1)
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 4");
            retrieveInfo = SimulationData.Parcels[testRegion4, testData4.ID];

            if (!CompareParcels(retrieveInfo, testData4))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 4 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion4).Count != 1)
            {
                return false;
            }

            m_Log.Info("Deleting parcel 1");
            SimulationData.Parcels.Remove(testRegion1, testData1.ID);

            m_Log.Info("Deleting parcel 2");
            SimulationData.Parcels.Remove(testRegion2, testData2.ID);

            m_Log.Info("Deleting parcel 3");
            SimulationData.Parcels.Remove(testRegion3, testData3.ID);

            m_Log.Info("Deleting parcel 4");
            SimulationData.Parcels.Remove(testRegion4, testData4.ID);

            m_Log.Info("Testing non-existence of parcel 1");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion1, testData1.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 1 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion1).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 2");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion2, testData2.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 2 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion2).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 3");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion3, testData3.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 3 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion3).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 4");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion4, testData4.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 4 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion4).Count != 0)
            {
                return false;
            }

            m_Log.Info("Adding parcel 1");
            SimulationData.Parcels.Store(testRegion1, testData1);

            m_Log.Info("Adding parcel 2");
            SimulationData.Parcels.Store(testRegion2, testData2);

            m_Log.Info("Adding parcel 3");
            SimulationData.Parcels.Store(testRegion3, testData3);

            m_Log.Info("Adding parcel 4");
            SimulationData.Parcels.Store(testRegion4, testData4);

            m_Log.Info("Testing existence of parcel 1");
            retrieveInfo = SimulationData.Parcels[testRegion1, testData1.ID];

            if (!CompareParcels(retrieveInfo, testData1))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 1 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion1).Count != 1)
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 2");
            retrieveInfo = SimulationData.Parcels[testRegion2, testData2.ID];

            if (!CompareParcels(retrieveInfo, testData2))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 2 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion2).Count != 1)
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 3");
            retrieveInfo = SimulationData.Parcels[testRegion3, testData3.ID];

            if (!CompareParcels(retrieveInfo, testData3))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 3 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion3).Count != 1)
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 4");
            retrieveInfo = SimulationData.Parcels[testRegion4, testData4.ID];

            if (!CompareParcels(retrieveInfo, testData4))
            {
                return false;
            }

            m_Log.Info("Testing existence of parcel 4 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion4).Count != 1)
            {
                return false;
            }

            m_Log.Info("Deleting parcel 1 via RemoveRegion");
            SimulationData.RemoveRegion(testRegion1);

            m_Log.Info("Deleting parcel 2 via RemoveRegion");
            SimulationData.RemoveRegion(testRegion2);

            m_Log.Info("Deleting parcel 3 via RemoveRegion");
            SimulationData.RemoveRegion(testRegion3);

            m_Log.Info("Deleting parcel 4 via RemoveRegion");
            SimulationData.RemoveRegion(testRegion4);

            m_Log.Info("Testing non-existence of parcel 1");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion1, testData1.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 1 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion1).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 2");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion2, testData2.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 2 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion2).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 3");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion3, testData3.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 3 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion3).Count != 0)
            {
                return false;
            }

            m_Log.Info("Testing non-existence of parcel 4");
            try
            {
                retrieveInfo = SimulationData.Parcels[testRegion4, testData4.ID];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is expected here */
            }

            m_Log.Info("Testing non-existence of parcel 4 via ParcelsInRegion");
            if (SimulationData.Parcels.ParcelsInRegion(testRegion4).Count != 0)
            {
                return false;
            }

            return true;
        }
    }
}
