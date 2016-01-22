// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("TestExtensions")]
    public class TestExtensions_Api : IScriptApi, IPlugin
    {
        private static readonly ILog m_Log = LogManager.GetLogger("SCRIPT");
        ConfigurationLoader m_Loader;
        TestRunner m_TestRunner;

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
        }

        [APIExtension("Testing", "_test_Shutdown")]
        public void TestShutdown(ScriptInstance instance)
        {
            lock (instance)
            {
                m_Log.Info("Shutdown triggered by script");
                m_Loader.TriggerShutdown();
                m_TestRunner.Shutdown();
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
