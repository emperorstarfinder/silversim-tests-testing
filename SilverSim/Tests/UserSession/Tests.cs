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
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.UserSession
{
    public sealed class Tests : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private UserSessionServiceInterface m_UserSessionService;

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

        }

        public bool Run()
        {
            UUID sessionID = new UUID("11223344-1122-1122-1122-112233445566");
            UUID secureSessionID = new UUID("11223344-1122-1122-1133-112233445566");
            UGUIWithName uguiname = new UGUIWithName("11223344-1111-1122-1122-112233445566", "http://example.com/;Example Com");
            UGUI ugui = new UGUI(uguiname);
            string clientipaddress = "127.0.0.1";

            m_Log.Info("Testing with UGUI");
            if (!TestNonExistence(sessionID, secureSessionID, ugui))
            {
                return false;
            }
            m_Log.Info("Testing with UGUIWithName");
            if (!TestNonExistence(sessionID, secureSessionID, uguiname))
            {
                return false;
            }

            m_Log.Info("Creating session");
            UserSessionInfo createdSession = m_UserSessionService.CreateSession(uguiname, clientipaddress, sessionID, secureSessionID);

            List<string> failingEntries = new List<string>();
            if(createdSession.SessionID != sessionID)
            {
                failingEntries.Add("SessionID");
            }
            if (createdSession.SecureSessionID != secureSessionID)
            {
                failingEntries.Add("SecureSessionID");
            }
            if (createdSession.ClientIPAddress != clientipaddress)
            {
                failingEntries.Add("ClientIPAddress");
            }
            if (createdSession.User != ugui)
            {
                failingEntries.Add("User");
            }
            if (createdSession.DynamicData.Count != 0)
            {
                failingEntries.Add("DynamicData");
            }

            if(failingEntries.Count != 0)
            {
                m_Log.Info("Data mismatch: " + string.Join(" ", failingEntries));
                return false;
            }

            m_Log.Info("Creating same session should fail");
            try
            {
                UserSessionInfo failingSession = m_UserSessionService.CreateSession(ugui, clientipaddress, sessionID, secureSessionID);
                return false;
            }
            catch
            {
                /* the good case */
            }

            m_Log.Info("Testing with UGUI");
            if (!TestExistence(ugui, createdSession))
            {
                return false;
            }

            m_Log.Info("Testing with UGUIWithName");
            if (!TestExistence(uguiname, createdSession))
            {
                return false;
            }

            m_Log.Info("Testing remove by SessionID");
            if(!m_UserSessionService.Remove(sessionID))
            {
                return false;
            }

            m_Log.Info("Testing removed with UGUI");
            if(!TestNonExistence(sessionID, secureSessionID, ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed with UGUIWithName");
            if (!TestNonExistence(sessionID, secureSessionID, uguiname))
            {
                return false;
            }

            m_Log.Info("Testing removed by SessionID");
            if (m_UserSessionService.Remove(sessionID))
            {
                return false;
            }

            m_Log.Info("Testing removed by UGUI");
            if (m_UserSessionService.Remove(ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed by UGUIWithName");
            if (m_UserSessionService.Remove(uguiname))
            {
                return false;
            }

            m_Log.Info("Creating session");
            createdSession = m_UserSessionService.CreateSession(uguiname, clientipaddress, sessionID, secureSessionID);

            m_Log.Info("Testing remove by UGUI");
            if (!m_UserSessionService.Remove(ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed with UGUI");
            if (!TestNonExistence(sessionID, secureSessionID, ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed with UGUIWithName");
            if (!TestNonExistence(sessionID, secureSessionID, uguiname))
            {
                return false;
            }

            m_Log.Info("Testing removed by SessionID");
            if (m_UserSessionService.Remove(sessionID))
            {
                return false;
            }

            m_Log.Info("Testing removed by UGUI");
            if (m_UserSessionService.Remove(ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed by UGUIWithName");
            if (m_UserSessionService.Remove(uguiname))
            {
                return false;
            }

            m_Log.Info("Creating session");
            createdSession = m_UserSessionService.CreateSession(ugui, clientipaddress, sessionID, secureSessionID);

            m_Log.Info("Testing remove by UGUIWithName");
            if (!m_UserSessionService.Remove(uguiname))
            {
                return false;
            }

            m_Log.Info("Testing removed with UGUI");
            if (!TestNonExistence(sessionID, secureSessionID, ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed with UGUIWithName");
            if (!TestNonExistence(sessionID, secureSessionID, uguiname))
            {
                return false;
            }

            m_Log.Info("Testing removed by SessionID");
            if (m_UserSessionService.Remove(sessionID))
            {
                return false;
            }

            m_Log.Info("Testing removed by UGUI");
            if (m_UserSessionService.Remove(ugui))
            {
                return false;
            }

            m_Log.Info("Testing removed by UGUIWithName");
            if (m_UserSessionService.Remove(uguiname))
            {
                return false;
            }

            return true;
        }

        private bool TestForEquality(UserSessionInfo infoA, UserSessionInfo infoB)
        {
            m_Log.Info("Testing for equality");
            List<string> failingEntries = new List<string>();
            if (infoA.SessionID != infoB.SessionID)
            {
                failingEntries.Add("SessionID");
            }
            if (infoA.SecureSessionID != infoB.SecureSessionID)
            {
                failingEntries.Add("SecureSessionID");
            }
            if (infoA.ClientIPAddress != infoB.ClientIPAddress)
            {
                failingEntries.Add("ClientIPAddress");
            }
            if (infoA.User != infoB.User)
            {
                failingEntries.Add("User");
            }
            if (infoA.DynamicData.Count != infoB.DynamicData.Count)
            {
                failingEntries.Add("DynamicData");
            }

            if (failingEntries.Count != 0)
            {
                m_Log.Info("Data mismatch: " + string.Join(" ", failingEntries));
                return false;
            }
            return true;
        }

        private bool TestExistence(UGUI user, UserSessionInfo userSessionInfo)
        {
            UserSessionInfo sessionInfo;
            m_Log.Info("Testing existence 1");
            if (!m_UserSessionService.ContainsKey(userSessionInfo.SessionID))
            {
                return false;
            }
            m_Log.Info("Testing existence 2");
            if (!m_UserSessionService.ContainsKey(user))
            {
                return false;
            }
            m_Log.Info("Testing existence 3");
            try
            {
                sessionInfo = m_UserSessionService[userSessionInfo.SessionID];
            }
            catch(KeyNotFoundException)
            {
                return false;
            }
            if(!TestForEquality(userSessionInfo, sessionInfo))
            {
                return false;
            }
            m_Log.Info("Testing existence 4");
            List<UserSessionInfo> infos = m_UserSessionService[user];
            if (infos.Count != 1)
            {
                return false;
            }
            if (!TestForEquality(userSessionInfo, infos[0]))
            {
                return false;
            }
            m_Log.Info("Testing existence 5");
            if (!m_UserSessionService.TryGetValue(userSessionInfo.SessionID, out sessionInfo))
            {
                return false;
            }
            if (!TestForEquality(userSessionInfo, sessionInfo))
            {
                return false;
            }
            m_Log.Info("Testing existence 6");
            if (!m_UserSessionService.TryGetSecureValue(userSessionInfo.SecureSessionID, out sessionInfo))
            {
                return false;
            }
            if (!TestForEquality(userSessionInfo, sessionInfo))
            {
                return false;
            }
            return true;
        }

        private bool TestNonExistence(UUID sessionID, UUID secureSessionID, UGUI ugui)
        {
            UserSessionInfo sessionInfo;
            m_Log.Info("Testing non-existence 1");
            if(m_UserSessionService.ContainsKey(sessionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 2");
            if(m_UserSessionService.ContainsKey(ugui))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 3");
            try
            {
                sessionInfo = m_UserSessionService[sessionID];
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* this is the OK case */
            }
            m_Log.Info("Testing non-existence 4");
            if(m_UserSessionService[ugui].Count != 0)
            {
                return false;
            }
            m_Log.Info("Testing non-existence 5");
            if(m_UserSessionService.TryGetValue(sessionID, out sessionInfo))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 6");
            if (m_UserSessionService.TryGetSecureValue(secureSessionID, out sessionInfo))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 7");
            if(m_UserSessionService.Remove(sessionID))
            {
                return false;
            }
            m_Log.Info("Testing non-existence 8");
            if (m_UserSessionService.Remove(ugui))
            {
                return false;
            }
            return true;
        }
    }
}
