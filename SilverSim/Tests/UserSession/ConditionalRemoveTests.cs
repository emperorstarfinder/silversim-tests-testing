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
using System.Reflection;

namespace SilverSim.Tests.UserSession
{
    public sealed class ConditionalRemoveTests : ITest
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

            m_Log.Info("Creating value 1");
            m_UserSessionService[sessionID, "testassoc1", "testvarname1"] = "testvalue1";

            m_Log.Info("Creating value 2");
            m_UserSessionService[sessionID, "testassoc2", "testvarname2"] = "testvalue2";

            m_Log.Info("Remove matching value 1");
            if(!m_UserSessionService.CompareAndRemove(sessionID, "testassoc1", "testvarname1", "testvalue1"))
            {
                return false;
            }

            m_Log.Info("Check removal matching value 1");
            if(m_UserSessionService.ContainsKey(sessionID, "testassoc1", "testvarname1"))
            {
                return false;
            }
            m_Log.Info("Check existence value 2");
            if (!m_UserSessionService.ContainsKey(sessionID, "testassoc2", "testvarname2"))
            {
                return false;
            }

            m_Log.Info("Try remove non-matching value 2");
            if (m_UserSessionService.CompareAndRemove(sessionID, "testassoc2", "testvarname2", "testvalue1"))
            {
                return false;
            }

            m_Log.Info("Check existence value 2");
            if (!m_UserSessionService.ContainsKey(sessionID, "testassoc2", "testvarname2"))
            {
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
