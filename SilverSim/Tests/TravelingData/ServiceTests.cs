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
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.ServiceInterfaces.Traveling;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.TravelingData;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.TravelingData
{
    public sealed class ServiceTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private TravelingDataServiceInterface m_TravelingDataService;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_TravelingDataService = loader.GetService<TravelingDataServiceInterface>(config.GetString("TravelingDataService"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            var info = new TravelingDataInfo
            {
                SessionID = UUID.Random,
                UserID = UUID.Random,
                GridExternalName = "http://example.com/",
                ServiceToken = UUID.Random.ToString(),
                ClientIPAddress = "127.0.0.1",
                Timestamp = Date.Now
            };

            m_Log.Info("---- Testing precondition ----");
            if(!CheckNonExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Creating entry 1 ----");
            m_TravelingDataService.Store(info);

            if(!CheckExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Removing entry 1 ----");
            m_TravelingDataService.Remove(info.SessionID);

            if(!CheckNonExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Creating entry 2 ----");
            m_TravelingDataService.Store(info);

            if (!CheckExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Removing entry 2 ----");
            m_TravelingDataService.RemoveByAgentUUID(info.UserID);

            if (!CheckNonExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Creating entry 3 ----");
            m_TravelingDataService.Store(info);

            if (!CheckExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Removing entry 3 ----");
            TravelingDataInfo result;
            m_TravelingDataService.Remove(info.SessionID, out result);

            if (!CheckNonExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Verifying equality with removed entry 1 ----");
            if(!IsEqual(info, result))
            {
            }

            m_Log.Info("---- Creating entry 4 ----");
            m_TravelingDataService.Store(info);

            if (!CheckExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Removing entry 4 ----");
            m_TravelingDataService.RemoveByAgentUUID(info.UserID, out result);

            if (!CheckNonExistence(info))
            {
                return false;
            }

            m_Log.Info("---- Verifying equality with removed entry 2 ----");
            if (!IsEqual(info, result))
            {
                return false;
            }

            return true;
        }

        private bool CheckNonExistence(TravelingDataInfo info)
        {
            m_Log.Info("Testing non-existence 1");
            try
            {
                m_TravelingDataService.GetTravelingData(info.SessionID);
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* expected exception */
            }

            m_Log.Info("Testing non-existence 2");
            try
            {
                m_TravelingDataService.GetTravelingDataByAgentUUIDAndIPAddress(info.UserID, info.ClientIPAddress);
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* expected exception */
            }

            m_Log.Info("Testing non-existence 3");
            if(m_TravelingDataService.GetTravelingDatasByAgentUUID(info.UserID).Count != 0)
            {
                return false;
            }

            return true;
        }

        private bool CheckExistence(TravelingDataInfo info)
        {
            TravelingDataInfo result;
            List<TravelingDataInfo> reslist;
            m_Log.Info("Testing existence 1");
            result = m_TravelingDataService.GetTravelingData(info.SessionID);

            m_Log.Info("Verify equality");
            if(!IsEqual(info, result))
            {
                return false;
            }

            m_Log.Info("Testing existence 2");
            result = m_TravelingDataService.GetTravelingDataByAgentUUIDAndIPAddress(info.UserID, info.ClientIPAddress);

            m_Log.Info("Verify equality");
            if (!IsEqual(info, result))
            {
                return false;
            }

            m_Log.Info("Testing non-existence 3");
            reslist = m_TravelingDataService.GetTravelingDatasByAgentUUID(info.UserID);
            if (reslist.Count != 1)
            {
                return false;
            }

            m_Log.Info("Verify equality");
            if (!IsEqual(info, reslist[0]))
            {
                return false;
            }

            return true;
        }

        private bool IsEqual(TravelingDataInfo t1, TravelingDataInfo t2)
        {
            var mismatches = new List<string>();

            if(t1.SessionID != t2.SessionID)
            {
                mismatches.Add("SessionID");
            }

            if(t1.UserID != t2.UserID)
            {
                mismatches.Add("UserID");
            }

            if(t1.GridExternalName != t2.GridExternalName)
            {
                mismatches.Add("GridExternalName");
            }

            if(t1.ServiceToken != t2.ServiceToken)
            {
                mismatches.Add("ServiceToken");
            }

            if(t1.ClientIPAddress != t2.ClientIPAddress)
            {
                mismatches.Add("ClientIPAddress");
            }

            if(t1.Timestamp.AsULong != t2.Timestamp.AsULong)
            {
                mismatches.Add("Timestamp");
            }

            if(mismatches.Count != 0)
            {
                m_Log.InfoFormat("Mismatches: {0}", string.Join(" ", mismatches));
            }

            return mismatches.Count == 0;
        }
    }
}
