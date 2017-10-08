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
using SilverSim.ServiceInterfaces.AuthInfo;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Types.AuthInfo;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.AuthInfo
{
    public class AuthInfoLoadStoreTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        AuthInfoServiceInterface m_AuthInfoService;
        AuthInfoServiceInterface m_AuthInfoServiceBackend;
        UUID m_UserID;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_AuthInfoService = loader.GetService<AuthInfoServiceInterface>(config.GetString("AuthInfoService"));
            m_AuthInfoServiceBackend = loader.GetService<AuthInfoServiceInterface>(config.GetString("AuthInfoServiceBackend", config.GetString("AuthInfoService")));
            m_UserID = config.GetString("UserID");
        }

        public void Setup()
        {
        }

        public bool Run()
        {
            m_Log.Info("Testing that we get no data");
            UserAuthInfo authInfo;
            UserAuthInfo checkAuthInfo;
            try
            {
                authInfo = m_AuthInfoServiceBackend[m_UserID];
                return false;
            }
            catch(KeyNotFoundException)
            {
                /* this happens here */
            }

            m_Log.Info("Testing predefined Auth Info");
            authInfo = new UserAuthInfo()
            {
                ID = m_UserID,
                PasswordHash = "fd6224f938c9c333dfbeb2336c6640e7",
                PasswordSalt = "f5a7924e621e84c9280a9a27e1bcb7f6"
            };

            m_Log.InfoFormat("Hash={0} Salt={1}", authInfo.PasswordHash, authInfo.PasswordSalt);

            m_AuthInfoServiceBackend.Store(authInfo);

            m_Log.Info("Retrieving auth info");
            try
            {
                checkAuthInfo = m_AuthInfoServiceBackend[m_UserID];
            }
            catch
            {
                return false;
            }

            m_Log.InfoFormat("Hash={0} Salt={1}", checkAuthInfo.PasswordHash, checkAuthInfo.PasswordSalt);

            if (checkAuthInfo.ID != authInfo.ID ||
                checkAuthInfo.PasswordHash != authInfo.PasswordHash ||
                checkAuthInfo.PasswordSalt != authInfo.PasswordSalt)
            {
                if(checkAuthInfo.ID != authInfo.ID)
                {
                    m_Log.Info("ID not equal");
                }
                if (checkAuthInfo.PasswordHash != authInfo.PasswordHash)
                {
                    m_Log.Info("PasswordHash not equal");
                }
                if (checkAuthInfo.PasswordSalt != authInfo.PasswordSalt)
                {
                    m_Log.Info("PasswordSalt not equal");
                }
                return false;
            }

            m_Log.Info("Testing password check of original data");
            try
            {
                authInfo.CheckPassword("Hello");
            }
            catch
            {
                m_Log.InfoFormat("Hash={0} Salt={1}", authInfo.PasswordHash, authInfo.PasswordSalt);
                return false;
            }

            m_Log.Info("Testing password check of copy data");
            try
            {
                checkAuthInfo.CheckPassword("Hello");
            }
            catch
            {
                m_Log.InfoFormat("Hash={0} Salt={1}", checkAuthInfo.PasswordHash, checkAuthInfo.PasswordSalt);
                return false;
            }

            UUID sessionID = UUID.Random;
            m_Log.Info("Testing authenticate with old way");
            try
            {
                m_AuthInfoService.Authenticate(sessionID, m_UserID, "Hello", 30);
            }
            catch
            {
                return false;
            }

            m_Log.Info("Testing authenticate with new way");
            try
            {
                m_AuthInfoService.Authenticate(sessionID, m_UserID, "$1$8b1a9953c4611296a827abf8c47804d7", 30);
            }
            catch(Exception e)
            {
                m_Log.Debug("Exception", e);
                return false;
            }

            return true;
        }

        public void Cleanup()
        {
        }
    }
}
