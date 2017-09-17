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
using SilverSim.ServiceInterfaces.Account;
using SilverSim.ServiceInterfaces.Presence;
using SilverSim.ServiceInterfaces.Traveling;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.Account;
using SilverSim.Types.Presence;
using SilverSim.Types.TravelingData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SilverSim.Tests.PresenceInit
{
    public class PresenceInit : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        PresenceServiceInterface m_PresenceService;
        TravelingDataServiceInterface m_TravelingDataService;
        UserAccountServiceInterface m_UserAccountService;
        UUI m_UserID;
        UUID m_SessionID;
        UUID m_SecureSessionID;
        string m_ClientIPAddress;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_PresenceService = loader.GetService<PresenceServiceInterface>(config.GetString("PresenceService", "PresenceService"));
            m_TravelingDataService = loader.GetService<TravelingDataServiceInterface>(config.GetString("TravelingDataService", "TravelingDataService"));
            m_UserAccountService = loader.GetService<UserAccountServiceInterface>(config.GetString("UserAccountService", "UserAccountService"));
            m_UserID = new UUI(config.GetString("User"));
            m_SessionID = new UUID(config.GetString("SessionID"));
            m_SecureSessionID = new UUID(config.GetString("SecureSessionID"));
            m_ClientIPAddress = config.GetString("ClientIPAddress");
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            UUID sessionId = UUID.Random;
            m_Log.Info("Create User");
            try
            {
                var account = new UserAccount
                {
                    Principal = m_UserID,
                };
                m_UserAccountService.Add(account);
            }
            catch(Exception e)
            {
                m_Log.Error("UserAccount failed", e);
                return false;
            }

            m_Log.Info("Create Presence");
            try
            {
                var presence = new PresenceInfo
                {
                    UserID = m_UserID,
                    SessionID = m_SessionID,
                    SecureSessionID = m_SecureSessionID
                };
                m_PresenceService.Login(presence);
            }
            catch(Exception e)
            {
                m_Log.Error("Presence failed", e);
                return false;
            }

            m_Log.Info("Create TravelingInfo");
            try
            {
                var travelinginfo = new TravelingDataInfo
                {
                    UserID = m_UserID.ID,
                    SessionID = m_SessionID,
                    ClientIPAddress = m_ClientIPAddress,
                    GridExternalName = "http://127.0.0.1:9300/",
                    ServiceToken = UUID.Random.ToString()
                };
                m_TravelingDataService.Store(travelinginfo);
            }
            catch(Exception e)
            {
                m_Log.Error("TravelingInfo failed", e);
                return false;
            }

            return true;
        }
    }
}
