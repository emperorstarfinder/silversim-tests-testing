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
using SilverSim.Scripting.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SilverSim.Tests.Scripting
{
    public sealed class CompileError : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string m_ScriptFile;
        private readonly Dictionary<int, string> m_ExpectedErrors = new Dictionary<int, string>();
        private TestRunner m_Runner;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            m_ScriptFile = config.Get("ScriptFile", string.Empty);
            foreach (string key in config.GetKeys())
            {
                int line;
                if(int.TryParse(key, out line))
                {
                    m_ExpectedErrors.Add(line, config.GetString(key));
                }
            }
            CompilerRegistry.ScriptCompilers.DefaultCompilerName = config.GetString("DefaultCompiler");
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Runner.ExcludeSummaryCount = true;
        }

        public void Setup()
        {

        }

        public void Cleanup()
        {
            m_Runner = null;
        }

        private TextReader OpenFile(string name, string scriptname)
        {
            return new StreamReader(Path.Combine(Path.GetDirectoryName(scriptname), name));
        }

        public bool Run()
        {
            bool success = true;
            m_Log.InfoFormat("Testing compilation of {0}", m_ScriptFile);
            try
            {
                using (TextReader reader = new StreamReader(m_ScriptFile, new UTF8Encoding(false)))
                {
                    CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UGUI.Unknown, UUID.Zero, reader, includeOpen: (name) => OpenFile(name, m_ScriptFile));
                }
                m_Log.InfoFormat("Compilation of {0} should not be successful", m_ScriptFile);
                success = false;
            }
            catch (CompilerException e)
            {
                m_Log.ErrorFormat("Compilation of {0} failed expectedly: {1}", m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                success = true;
                m_Log.InfoFormat("***** Comparing expected error messages ({0}) *****", m_ExpectedErrors.Count);
                foreach (KeyValuePair<int, string> kvp in m_ExpectedErrors)
                {
                    m_Log.InfoFormat("Checking expected line {0}", kvp.Key);
                    string msg;
                    if (e.Messages.TryGetValue(kvp.Key, out msg))
                    {
                        if (msg != kvp.Value)
                        {
                            m_Log.ErrorFormat("Message for line {0} different: {1}", kvp.Key, msg);
                            success = false;
                        }
                        else
                        {
                            m_Log.InfoFormat("good => {0}", kvp.Value);
                        }
                    }
                    else
                    {
                        m_Log.ErrorFormat("Message for line {0} missing", kvp.Key);
                        success = false;
                    }
                }
                m_Log.Info("***** Detecting unexpected error messages *****");
                foreach (KeyValuePair<int, string> kvp in e.Messages)
                {
                    if (!m_ExpectedErrors.ContainsKey(kvp.Key))
                    {
                        m_Log.ErrorFormat("Message for line {0} not expected: {1}", kvp.Key, kvp.Value);
                        success = false;
                    }
                }
            }
            catch (Exception e)
            {
                m_Log.ErrorFormat("Compilation of ({0}) failed: {1}", m_ScriptFile, e.Message);
                m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                success = false;
            }
            return success;
        }
    }
}
