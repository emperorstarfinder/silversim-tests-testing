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
using SilverSim.Tests.Extensions;
using SilverSim.Types.StructuredData.Llsd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SilverSim.Tests.StructuredData
{
    public class LLSDBinaryDecoderTest : ITest
    {
        TestRunner m_Runner;

        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        List<string> Filenames = new List<string>();
        public void Setup()
        {

        }

        public void Cleanup()
        {

        }

        public bool Run()
        {
            bool success = true;
            int count = 0;
            int successcnt = 0;
            foreach (string filename in Filenames)
            {
                ++count;
                var tr = new TestRunner.TestResult()
                {
                    Name = "LLSD Binary Data " + filename,
                    Result = false,
                    Message = string.Empty
                };
                int startTime = Environment.TickCount;
                m_Log.InfoFormat("Testing decoding of {0}", filename);
                try
                {
                    using (var stream = new FileStream(filename, FileMode.Open))
                    {
                        LlsdBinary.Deserialize(stream);
                    }
                    m_Log.InfoFormat("Decoding of {0} successful", filename);
                    ++successcnt;
                    tr.Result = true;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Decoding of {0} failed: {1}", filename, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                    tr.Message = e.Message + "\n" + e.StackTrace;
                    success = false;
                }
                tr.RunTime = Environment.TickCount - startTime;
                m_Runner.TestResults.Add(tr);
            }
            m_Log.InfoFormat("{0} of {1} decodings successful", successcnt, count);
            return success;
        }

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            foreach (string key in config.GetKeys())
            {
                Filenames.Add(config.GetString(key));
            }
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Runner.ExcludeSummaryCount = true;
        }

        static UTF8Encoding UTF8NoBOM = new UTF8Encoding(false);
    }
}
