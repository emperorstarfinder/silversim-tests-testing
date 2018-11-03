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
using SilverSim.ServiceInterfaces.UserSession;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.UserSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SilverSim.Tests.UserSession
{
    public sealed class ConditionalExpiringUpdateTests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private UserSessionServiceInterface m_UserSessionService;
        private readonly UUID sessionID = new UUID("11223344-1122-1122-1122-112233445566");

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_UserSessionService = loader.GetService<UserSessionServiceInterface>(config.GetString("UserSessionService"));
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {
            m_UserSessionService.Remove(sessionID);
        }

        public bool Run()
        {
            UUID secureSessionID = new UUID("11223344-1122-1122-1133-112233445566");
            UGUIWithName uguiname = new UGUIWithName("11223344-1111-1122-1122-112233445566", "http://example.com/;Example Com");
            UGUI ugui = new UGUI(uguiname);
            const string clientipaddress = "127.0.0.1";

            m_Log.Info("Creating session");
            UserSessionInfo createdSession = m_UserSessionService.CreateSession(uguiname, clientipaddress, sessionID, secureSessionID);
            UserSessionInfo.Entry refData1;
            UserSessionInfo.Entry refData2;

            m_Log.Info("Creating expiring value 1");
            m_UserSessionService.SetExpiringValue(sessionID, "testassoc1", "testvarname1", "testvalue1", TimeSpan.FromMinutes(1));
            m_UserSessionService.TryGetValue(sessionID, "testassoc1", "testvarname1", out refData1);

            m_Log.Info("Creating expiring value 2");
            m_UserSessionService.SetExpiringValue(sessionID, "testassoc2", "testvarname2", "testvalue2", TimeSpan.FromMinutes(1));
            m_UserSessionService.TryGetValue(sessionID, "testassoc2", "testvarname2", out refData2);

            m_Log.Info("Wait ~2s for seconds change");
            Thread.Sleep(2000);

            UserSessionInfo.Entry e;

            m_Log.Info("Try non-matching value 1");
            if(m_UserSessionService.TryCompareValueExtendLifetime(sessionID, "testassoc1", "testvarname1", "nomatch", TimeSpan.FromMinutes(1), out e))
            {
                return false;
            }
            if("testvalue1" != m_UserSessionService[sessionID, "testassoc1", "testvarname1"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }
            if ("testvalue2" != m_UserSessionService[sessionID, "testassoc2", "testvarname2"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }

            m_Log.Info("Try non-matching value 2");
            if (m_UserSessionService.TryCompareAndChangeValueExtendLifetime(sessionID, "testassoc2", "testvarname2", "nomatch", "nochange", TimeSpan.FromMinutes(1), out e))
            {
                return false;
            }
            if ("testvalue1" != m_UserSessionService[sessionID, "testassoc1", "testvarname1"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }
            if ("testvalue2" != m_UserSessionService[sessionID, "testassoc2", "testvarname2"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }

            m_Log.Info("Try matching value 1");
            if (!m_UserSessionService.TryCompareValueExtendLifetime(sessionID, "testassoc1", "testvarname1", "testvalue1", TimeSpan.FromMinutes(1), out e))
            {
                return false;
            }
            m_Log.Info("Check change");
            m_UserSessionService.TryGetValue(sessionID, "testassoc1", "testvarname1", out e);
            if(e.ExpiryDate.AsULong == refData1.ExpiryDate.AsULong)
            {
                return false;
            }
            if ("testvalue1" != m_UserSessionService[sessionID, "testassoc1", "testvarname1"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }
            if ("testvalue2" != m_UserSessionService[sessionID, "testassoc2", "testvarname2"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }

            m_Log.Info("Try matching value 2");
            if (!m_UserSessionService.TryCompareAndChangeValueExtendLifetime(sessionID, "testassoc2", "testvarname2", "testvalue2", "change", TimeSpan.FromMinutes(1), out e))
            {
                return false;
            }
            m_Log.Info("Check change");
            m_UserSessionService.TryGetValue(sessionID, "testassoc2", "testvarname2", out e);
            if (e.ExpiryDate.AsULong == refData2.ExpiryDate.AsULong)
            {
                return false;
            }
            if ("testvalue1" != m_UserSessionService[sessionID, "testassoc1", "testvarname1"])
            {
                m_Log.Info("Unexpected change");
                return false;
            }
            if ("change" != m_UserSessionService[sessionID, "testassoc2", "testvarname2"])
            {
                m_Log.Info("Unexpected no-change");
                return false;
            }

            m_Log.Info("Deleting session");
            if (!m_UserSessionService.Remove(sessionID))
            {
                return false;
            }

            return true;
        }
    }
}
