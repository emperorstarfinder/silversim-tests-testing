// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace SilverSim.Tests.Scripting
{
    public class RunScript : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        UUID m_AssetID;
        string m_ScriptFile;
        int m_TimeoutMs;
        ManualResetEvent m_RunTimeoutEvent = new ManualResetEvent(false);

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];

            m_AssetID = UUID.Parse(config.GetString("AssetID"));
            m_ScriptFile = config.GetString("Filename");
            m_TimeoutMs = config.GetInt("RunTimeout", 1000);

            if(string.IsNullOrEmpty(m_ScriptFile))
            {
                throw new ArgumentException("Script filename and UUID missing");
            }
            CompilerRegistry.ScriptCompilers.DefaultCompilerName = config.GetString("DefaultCompiler");
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            bool success = true;
            int successcnt = 0;

            m_Log.InfoFormat("Testing Execution of {1} ({0})", m_AssetID, m_ScriptFile);
            try
            {
                using (TextReader reader = new StreamReader(m_ScriptFile))
                {
                    CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UUI.Unknown, m_AssetID, reader);
                }
                m_Log.InfoFormat("Compilation of {1} ({0}) successful", m_AssetID, m_ScriptFile);
                ++successcnt;
            }
            catch (CompilerException e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                success = false;
            }
            catch (Exception e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", m_AssetID, m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                success = false;
            }

            if(success)
            {
                m_RunTimeoutEvent.WaitOne(m_TimeoutMs);
            }
            return success;
        }
    }
}
