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
using SilverSim.Main.Common.Tar;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace SilverSim.Tests.Scripting
{
    public sealed class CompileFromArchive : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        List<string> Archives = new List<string>();
        TestRunner m_Runner;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            foreach (string key in config.GetKeys())
            {
                if (key.StartsWith("Archive"))
                {
                    Archives.Add(config.GetString(key));
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

        private TextReader OpenFile(string name)
        {
            throw new NotSupportedException();
        }

        public bool Run()
        {
            bool success = true;
            int count = 0;
            int successcnt = 0;
            foreach (string file in Archives)
            {
                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        using (var tarreader = new TarArchiveReader(gz))
                        {
                            for (; ; )
                            {
                                TarArchiveReader.Header h;

                                try
                                {
                                    h = tarreader.ReadHeader();
                                }
                                catch (TarArchiveReader.EndOfTarException)
                                {
                                    break;
                                }
                                if (h.FileType != TarFileType.File || !h.FileName.StartsWith("assets/") || !h.FileName.EndsWith(".lsl"))
                                {
                                    continue;
                                }
                                ++count;
                                UUID id = UUID.Parse(h.FileName.Substring(7, 36));
                                var tr = new TestRunner.TestResult
                                {
                                    Name = "Script " + id + "(" + file + ")",
                                    Result = false,
                                    Message = string.Empty
                                };
                                int startTime = Environment.TickCount;
                                m_Log.InfoFormat("Testing compilation of {1} ({0})", id, file);
                                try
                                {
                                    using (TextReader reader = new StreamReader(tarreader, new UTF8Encoding(false)))
                                    {
                                        CompilerRegistry.ScriptCompilers.Compile(AppDomain.CurrentDomain, UUI.Unknown, id, reader, includeOpen: (name) => OpenFile(name));
                                    }
                                    m_Log.InfoFormat("Compilation of {1} ({0}) successful", id, file);
                                    ++successcnt;
                                    tr.Result = true;
                                }
                                catch (CompilerException e)
                                {
                                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", id, file, e.Message);
                                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                                    tr.Message = e.Message + "\n" + e.StackTrace;
                                    success = false;
                                }
                                catch (Exception e)
                                {
                                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", id, file, e.Message);
                                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                                    tr.Message = e.Message + "\n" + e.StackTrace;
                                    success = false;
                                }
                                tr.RunTime = Environment.TickCount - startTime;
                                m_Runner.TestResults.Add(tr);
                            }
                        }
                    }
                }
            }
            m_Log.InfoFormat("{0} of {1} compilations successful", successcnt, count);
            return success;
        }
    }
}
