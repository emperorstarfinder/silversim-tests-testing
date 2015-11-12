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

namespace SilverSim.Tests.Scripting
{
    public class RunScript : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        UUID Uuid;
        string ScriptFile;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];

            
            foreach (string key in config.GetKeys())
            {
                if (UUID.TryParse(key, out Uuid))
                {
                    ScriptFile = config.GetString(key);
                    break;
                }
            }

            if(string.IsNullOrEmpty(ScriptFile))
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

            m_Log.InfoFormat("Testing Execution of {1} ({0})", Uuid, ScriptFile);
            try
            {
                using (TextReader reader = new StreamReader(ScriptFile))
                {
                    CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UUI.Unknown, Uuid, reader);
                }
                m_Log.InfoFormat("Compilation of {1} ({0}) successful", Uuid, ScriptFile);
                ++successcnt;
            }
            catch (CompilerException e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", Uuid, ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                success = false;
            }
            catch (Exception e)
            {
                m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", Uuid, ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace.ToString());
                success = false;
            }

            if(success)
            {

            }
            return success;
        }
    }
}
