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
using SilverSim.Scene.Types.Object;
using SilverSim.Scene.Types.Script;
using SilverSim.Scene.Types.Script.Events;
using SilverSim.Scripting.Common;
using SilverSim.Scripting.Lsl;
using SilverSim.Tests.Extensions;
using SilverSim.Tests.Scripting;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilverSim.Tests.Lsl
{
    [LSLImplementation]
    [ScriptApiName("TestExtensions")]
    [PluginName("Testing")]
    public class TestExtensionsApi : IScriptApi, IPlugin
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

        [APIExtension("Testing", "_test_setserverparam")]
        public void TestSetServerParam(ScriptInstance instance, string paraname, string paravalue)
        {
            lock(instance)
            {
                m_Loader.GetServerParamStorage()[UUID.Zero, paraname] = paravalue;
            }
        }

        [APIExtension("Testing", "_test_scriptresetevent")]
        public void TestScriptReset(ScriptInstance instance)
        {
            lock(instance)
            {
                instance.PostEvent(new ResetScriptEvent());
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
                    m_Log.Error(message);
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

        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_NONE = 0;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_NUISANCE = 1;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_VERYLOW = 2;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_LOW = 3;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_MODERATE = 4;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_HIGH = 5;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_VERYHIGH = 6;
        [APIExtension("Testing")]
        public const int OSSL_THREAT_LEVEL_SEVERE = 7;

        [APIExtension("Testing", "_test_ossl_perms")]
        public int TestOsslPerms(ScriptInstance instance, int level, string functionname)
        {
            lock (instance)
            {
                try
                {
                    switch (level)
                    {
                        case OSSL_THREAT_LEVEL_NONE:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.None);
                            break;
                        case OSSL_THREAT_LEVEL_NUISANCE:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.Nuisance);
                            break;
                        case OSSL_THREAT_LEVEL_VERYLOW:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.VeryLow);
                            break;
                        case OSSL_THREAT_LEVEL_LOW:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.Low);
                            break;
                        case OSSL_THREAT_LEVEL_MODERATE:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.Moderate);
                            break;
                        case OSSL_THREAT_LEVEL_HIGH:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.High);
                            break;
                        case OSSL_THREAT_LEVEL_VERYHIGH:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.VeryHigh);
                            break;
                        case OSSL_THREAT_LEVEL_SEVERE:
                            ((Script)instance).CheckThreatLevel(functionname, ThreatLevel.Severe);
                            break;

                        default:
                            return 0;
                    }
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
        }

        [APIExtension("Testing", "_test_InjectScript")]
        public int InjectScript(ScriptInstance instance, string name, string filename, int startparameter)
        {
            lock(instance)
            {
                ObjectPartInventoryItem item = new ObjectPartInventoryItem(instance.Item)
                {
                    Name = name
                };
                UUID assetid = UUID.Random;
                item.AssetID = assetid;

                IScriptAssembly scriptAssembly = null;
                try
                {
                    using (var reader = new StreamReader(filename, new UTF8Encoding(false)))
                    {
                        scriptAssembly = CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, assetid, reader, includeOpen: instance.Part.OpenScriptInclude);
                    }
                    m_Log.InfoFormat("Compilation of injected {1} ({0}) successful", assetid, name);
                }
                catch (CompilerException e)
                {
                    m_Log.ErrorFormat("Compilation of injected {1} ({0}) failed: {2}", assetid, name, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                    return 0;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Compilation of injected {1} ({0}) failed: {2}", assetid, name, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                    return 0;
                }

                ScriptInstance scriptInstance = scriptAssembly.Instantiate(instance.Part, item);
                instance.Part.Inventory.Add(item);
                item.ScriptInstance = scriptInstance;
                item.ScriptInstance.Start(startparameter);

                return 1;
            }
        }
    }
}
