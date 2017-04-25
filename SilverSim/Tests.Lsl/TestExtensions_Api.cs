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
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Extensions;
using SilverSim.Tests.Scripting;
using System.Collections.Generic;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("TestExtensions")]
    public class TestExtensions_Api : IScriptApi, IPlugin
    {
        private static readonly ILog m_Log = LogManager.GetLogger("SCRIPT");
        ConfigurationLoader m_Loader;
        TestRunner m_TestRunner;
        RunScript m_ScriptRunner;

        [APIExtension("Testing")]
        public const int LOG_INFO = 0;
        [APIExtension("Testing")]
        public const int LOG_WARN = 1;
        [APIExtension("Testing")]
        public const int LOG_ERROR = 2;
        [APIExtension("Testing")]
        public const int LOG_FATAL = 3;
        [APIExtension("Testing")]
        public const int LOG_DEBUG = 4;

        public TestExtensions_Api()
        {

        }

        public void Startup(ConfigurationLoader loader)
        {
            m_Loader = loader;
            m_TestRunner = m_Loader.GetServicesByValue<TestRunner>()[0];
            List<RunScript> scriptrunners = m_Loader.GetServicesByValue<RunScript>();
            if(scriptrunners.Count > 0)
            {
                m_ScriptRunner = scriptrunners[0];
            }
        }

        [APIExtension("Testing", "_test_Shutdown")]
        public void TestShutdown(ScriptInstance instance)
        {
            lock (instance)
            {
                m_Log.Info("Shutdown triggered by script");
                if (null != m_ScriptRunner)
                {
                    m_ScriptRunner.Shutdown();
                }
                else
                {
                    m_Loader.TriggerShutdown();
                }
            }
        }

        [APIExtension("Testing", "_test_Result")]
        public void TestResult(ScriptInstance instance, int result)
        {
            lock (instance)
            {
                m_TestRunner.OtherThreadResult = (result != 0);
            }
        }

        [APIExtension("Testing", "_test_Log")]
        public void Log(ScriptInstance instance, int logLevel, string message)
        {
            switch(logLevel)
            {
                case LOG_WARN:
                    m_Log.Warn(message);
                    break;

                case LOG_ERROR:
                    m_Log.Warn(message);
                    break;

                case LOG_FATAL:
                    m_Log.Fatal(message);
                    break;

                case LOG_DEBUG:
                    m_Log.Debug(message);
                    break;

                case LOG_INFO:
                default:
                    m_Log.Info(message);
                    break;
            }
        }
    }

    [PluginName("Testing")]
    public class TestExtensionsFactory : IPluginFactory
    {
        public TestExtensionsFactory()
        {

        }

        public IPlugin Initialize(ConfigurationLoader loader, IConfig ownConfig)
        {
            return new TestExtensions_Api();
        }
    }
}
