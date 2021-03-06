﻿// SilverSim is distributed under the terms of the
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
using System.Reflection;

namespace SilverSim.Tests.UserSession
{
    public sealed class DataTests : ITest
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

            if(!TestNonExistence(sessionID, "testassoc1", "testvarname1"))
            {
                return false;
            }

            if (!TestNonExistence(sessionID, "testassoc2", "testvarname2"))
            {
                return false;
            }

            m_Log.Info("Creating value 1");
            m_UserSessionService[sessionID, "testassoc1", "testvarname1"] = "testvalue1";

            if(!TestExistence(sessionID, "testassoc1", "testvarname1", "testvalue1"))
            {
                return false;
            }
            if(!TestNonExistence(sessionID, "testassoc2", "testvarname2"))
            {
                return false;
            }

            m_Log.Info("Creating value 2");
            m_UserSessionService[sessionID, "testassoc2", "testvarname2"] = "testvalue2";

            if (!TestExistence(sessionID, "testassoc1", "testvarname1", "testvalue1"))
            {
                return false;
            }
            if (!TestExistence(sessionID, "testassoc2", "testvarname2", "testvalue2"))
            {
                return false;
            }

            m_Log.Info("Updating value 1");
            m_UserSessionService[sessionID, "testassoc1", "testvarname1"] = "testvalue3";

            if (!TestExistence(sessionID, "testassoc1", "testvarname1", "testvalue3"))
            {
                return false;
            }
            if (!TestExistence(sessionID, "testassoc2", "testvarname2", "testvalue2"))
            {
                return false;
            }

            m_Log.Info("Removing value 1");
            if(!m_UserSessionService.Remove(sessionID, "testassoc1", "testvarname1"))
            {
                return false;
            }

            if (!TestNonExistence(sessionID, "testassoc1", "testvarname1"))
            {
                return false;
            }
            if (!TestExistence(sessionID, "testassoc2", "testvarname2", "testvalue2"))
            {
                return false;
            }

            m_Log.Info("Removing value 2");
            if (!m_UserSessionService.Remove(sessionID, "testassoc2", "testvarname2"))
            {
                return false;
            }

            if (!TestNonExistence(sessionID, "testassoc1", "testvarname1"))
            {
                return false;
            }
            if (!TestNonExistence(sessionID, "testassoc2", "testvarname2"))
            {
                return false;
            }

            m_Log.Info("Creating value");
            m_UserSessionService[sessionID, "testassoc", "testvarname"] = "testvalue";

            m_Log.Info("Deleting session");
            if(!m_UserSessionService.Remove(sessionID))
            {
                return false;
            }
            if (!TestNonExistence(sessionID, "testassoc", "testvarname"))
            {
                return false;
            }

            m_Log.Info("Creating session");
            createdSession = m_UserSessionService.CreateSession(uguiname, clientipaddress, sessionID, secureSessionID);

            m_Log.Info("Creating value");
            m_UserSessionService[sessionID, "testassoc", "testvarname"] = "testvalue";

            m_Log.Info("Deleting session");
            if (!m_UserSessionService.Remove(sessionID))
            {
                return false;
            }
            if (!TestNonExistence(sessionID, "testassoc", "testvarname"))
            {
                return false;
            }

            return true;
        }

        private bool TestExistence(UUID sessionID, string assoc, string varname, string value)
        {
            string t;
            UserSessionInfo.Entry e;
            m_Log.InfoFormat("Testing existence 1 {0}/{1}", assoc, varname);
            try
            {
                t = m_UserSessionService[sessionID, assoc, varname];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            m_Log.Info("Testing equality");
            if (t != value)
            {
                return false;
            }

            m_Log.InfoFormat("Testing existence 2 {0}/{1}", assoc, varname);
            if (!m_UserSessionService.TryGetValueExtendLifetime(sessionID, assoc, varname, TimeSpan.FromHours(1), out e))
            {
                return false;
            }
            m_Log.Info("Testing equality");
            if (t != e.Value)
            {
                return false;
            }
            if(e.ExpiryDate != null)
            {
                return false;
            }

            m_Log.InfoFormat("Testing existence 3 {0}/{1}", assoc, varname);
            if (!m_UserSessionService.TryGetValue(sessionID, assoc, varname, out t))
            {
                return false;
            }
            m_Log.Info("Testing equality");
            if (t != value)
            {
                return false;
            }

            m_Log.InfoFormat("Testing existence 4 {0}/{1}", assoc, varname);
            if (!m_UserSessionService.TryGetValue(sessionID, assoc, varname, out e))
            {
                return false;
            }
            m_Log.Info("Testing equality");
            if (t != e.Value)
            {
                return false;
            }
            if(e.ExpiryDate != null)
            {
                return false;
            }

            m_Log.InfoFormat("Testing existence 5 {0}/{1}", assoc, varname);
            if (!m_UserSessionService.ContainsKey(sessionID, assoc, varname))
            {
                return false;
            }

            return true;
        }

        private bool TestNonExistence(UUID sessionID, string assoc, string varname)
        {
            string t;
            UserSessionInfo.Entry e;
            m_Log.InfoFormat("Testing non-existence 1 {0}/{1}", assoc, varname);
            try
            {
                t = m_UserSessionService[sessionID, assoc, varname];
                return false;
            }
            catch (KeyNotFoundException)
            {
                /* this is the good case */
            }

            m_Log.InfoFormat("Testing non-existence 2 {0}/{1}", assoc, varname);
            if(m_UserSessionService.TryGetValueExtendLifetime(sessionID, assoc, varname, TimeSpan.FromHours(1), out e))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 3 {0}/{1}", assoc, varname);
            if (m_UserSessionService.TryGetValue(sessionID, assoc, varname, out t))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 4 {0}/{1}", assoc, varname);
            if (m_UserSessionService.TryGetValue(sessionID, assoc, varname, out e))
            {
                return false;
            }

            m_Log.InfoFormat("Testing non-existence 5 {0}/{1}", assoc, varname);
            if (m_UserSessionService.ContainsKey(sessionID, assoc, varname))
            {
                return false;
            }

            return true;
        }
    }
}
