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
using SilverSim.Main.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types.AuthInfo;
using System;
using System.Reflection;

namespace SilverSim.Tests.AuthInfo
{
    public class PasswordCheckTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Cleanup()
        {
        }

        public bool Run()
        {
            // password: Hello (in $1$8b1a9953c4611296a827abf8c47804d7)
            // salt: World
            UserAuthInfo authInfo = new UserAuthInfo()
            {
                PasswordHash = "fd6224f938c9c333dfbeb2336c6640e7",
                PasswordSalt = "f5a7924e621e84c9280a9a27e1bcb7f6"
            };

            m_Log.Info("Testing predefined password data");
            try
            {
                m_Log.Info("- Old way");
                authInfo.CheckPassword("Hello");
                m_Log.Info("- Current way");
                authInfo.CheckPassword("$1$8b1a9953c4611296a827abf8c47804d7");
            }
            catch
            {
                return false;
            }

            m_Log.Info("Testing predefined password data - wrong");
            try
            {
                m_Log.Info("- Old way");
                authInfo.CheckPassword("Hello2");
                return false;
            }
            catch
            {
                /* only successful on error */
            }
            try
            { 
                m_Log.Info("- Current way");
                authInfo.CheckPassword("$1$b83099b8ce596f31f2f60c8fd4d72826");
                return false;
            }
            catch
            {
                /* only successful on error */
            }

            m_Log.Info("Setting password through property");
            authInfo.Password = "Hello";
            try
            {
                m_Log.Info("- Old way");
                authInfo.CheckPassword("Hello");
                m_Log.Info("- Current way");
                authInfo.CheckPassword("$1$8b1a9953c4611296a827abf8c47804d7");
            }
            catch
            {
                return false;
            }

            m_Log.Info("Testing property set password data - wrong");
            try
            {
                m_Log.Info("- Old way");
                authInfo.CheckPassword("Hello2");
                return false;
            }
            catch
            {
                /* only successful on error */
            }
            try
            {
                m_Log.Info("- Current way");
                authInfo.CheckPassword("$1$b83099b8ce596f31f2f60c8fd4d72826");
                return false;
            }
            catch
            {
                /* only successful on error */
            }

            return true;
        }

        public void Setup()
        {
        }

        public void Startup(ConfigurationLoader loader)
        {
        }
    }
}
