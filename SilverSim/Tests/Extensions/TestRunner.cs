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
using SilverSim.Main.Common.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Xml;

namespace SilverSim.Tests.Extensions
{
    [PluginName("TestRunner")]
    public class TestRunner : IPlugin, IPostLoadStep, IPluginSubFactory, IPluginShutdown
    {
        private static readonly ILog m_Log = LogManager.GetLogger("TEST RUNNER");
        List<ITest> m_Tests = new List<ITest>();
        string m_TestName = string.Empty;
        string m_XUnitResultsFileName = string.Empty;
        string m_NUnitResultsFileName = string.Empty;
        ConfigurationLoader m_Loader;
        StringBuilder m_LogOutput = new StringBuilder();
        readonly object m_LogOutputLock = new object();

        private string GetLog()
        {
            string output;
            lock(m_LogOutputLock)
            {
                output = m_LogOutput.ToString();
                m_LogOutput.Clear();
            }
            return output;
        }

        private void ClearLog()
        {
            lock (m_LogOutputLock)
            {
                m_LogOutput.Clear();
            }
        }

        public void AddLog(DateTime date, string levelname, string loggername, string renderedmessage)
        {
            lock(m_LogOutputLock)
            {
                m_LogOutput.AppendFormat("{0} {1}  {2}: {3}\n", date.ToShortDateString(), levelname, loggername, renderedmessage);
            }
        }

        public struct TestResult
        {
            public string Name;
            public bool Result;
            public int RunTime;
            public string Message;
            public string StackTrace;
        }

        public List<TestResult> TestResults = new List<TestResult>();
        public bool OtherThreadResult = true; /* storage for tests running over multiple threads */
        public bool ExcludeSummaryCount;

        public ShutdownOrder ShutdownOrder => ShutdownOrder.Any;

        public TestRunner(IConfig ownSection)
        {
            m_TestName = ownSection.GetString("Name", "");
            m_XUnitResultsFileName = ownSection.GetString("XUnitResultsFile", string.Empty);
            m_NUnitResultsFileName = ownSection.GetString("NUnitResultsFile", string.Empty);
        }

        public void Startup(ConfigurationLoader loader)
        {
            m_Loader = loader;
            if(loader.GetServicesByValue<TestRunner>().Count != 1)
            {
                throw new Exception("Too many TestRunner instances");
            }
            m_Tests = loader.GetServicesByValue<ITest>();
        }

        public void PostLoad()
        {
            bool failed = false;
            var failedtests = new List<string>();
            LogController.AddLogAction(AddLog);
            foreach (ITest test in m_Tests)
            {
                var tr = new TestResult
                {
                    Name = test.GetType().FullName,
                    RunTime = Environment.TickCount
                };
                m_Log.Info("********************************************************************************");
                m_Log.InfoFormat("Executing test {0}", test.GetType().FullName);
                m_Log.Info("********************************************************************************");
                try
                {
                    ClearLog();
                    test.Setup();

                    if (test.Run())
                    {
                        tr.Message = "Success\n\nLog:\n" + GetLog();
                        tr.Result = true;

                        m_Log.Info("********************************************************************************");
                        m_Log.InfoFormat("Executed test {0} with SUCCESS", test.GetType().FullName);
                        m_Log.Info("********************************************************************************");
                    }
                    else
                    {
                        failed = true;
                        tr.Message = "Failure\n\nLog:\n" + GetLog();
                        tr.Result = false;
                        failedtests.Add(test.GetType().FullName);

                        m_Log.Info("********************************************************************************");
                        m_Log.ErrorFormat("Executed test {0} with FAILURE", test.GetType().FullName);
                        m_Log.Info("********************************************************************************");
                    }
                }
                catch(Exception e)
                {
                    failed = true;
                    tr.Message = string.Format("Exception {0}: {1}\n{2}\n\nLog:\n", e.GetType().FullName, e.ToString(), e.StackTrace.ToString()) + GetLog();
                    tr.Result = false;
                    tr.StackTrace = e.StackTrace;
                    if (!failedtests.Contains(test.GetType().FullName))
                    {
                        failedtests.Add(test.GetType().FullName);
                    }

                    m_Log.Info("********************************************************************************");
                    m_Log.InfoFormat("Executed test {0} with FAILURE", test.GetType().FullName);
                    m_Log.ErrorFormat("Exception {0}: {1}\n{2}", e.GetType().FullName, e.ToString(), e.StackTrace.ToString());
                    m_Log.Info("********************************************************************************");
                }

                try
                {
                    test.Cleanup();
                }
                catch(Exception e)
                {
                    m_Log.Info("********************************************************************************");
                    m_Log.InfoFormat("Executed test {0} with FAILURE (Cleanup)", test.GetType().FullName);
                    m_Log.ErrorFormat("Exception {0}: {1}\n{2}", e.GetType().FullName, e.ToString(), e.StackTrace.ToString());
                    m_Log.Info("********************************************************************************");
                    failed = true;
                    tr.Message = string.Format("Exception {0}: {1}\n{2}\n\nLog:\n", e.GetType().FullName, e.ToString(), e.StackTrace.ToString()) + GetLog();
                    tr.Result = false;
                    tr.StackTrace = e.StackTrace;
                    if (!failedtests.Contains(test.GetType().FullName))
                    {
                        failedtests.Add(test.GetType().FullName);
                    }
                }

                if (ExcludeSummaryCount)
                {
                    tr.RunTime = 0;
                }
                else
                {
                    tr.RunTime = Environment.TickCount - tr.RunTime;
                }

                TestResults.Add(tr);
            }

            if (m_XUnitResultsFileName?.Length != 0)
            {
                WriteXUnitResults(m_XUnitResultsFileName);
            }

            if(m_NUnitResultsFileName?.Length != 0)
            {
                WriteNUnit2Results(m_NUnitResultsFileName);
            }

            if (failed)
            {
                m_Log.InfoFormat("Failed tests ({0}): {1}", failedtests.Count, string.Join(" ", failedtests));
                Thread.Sleep(100);
                throw new ConfigurationLoader.TestingErrorException();
            }
            else
            {
                ConfigurationLoader loader = m_Loader;
                if (null != loader)
                {
                    loader.TriggerShutdown();
                }
            }
        }

        private void WriteXUnitResults(string filename)
        {
            using (var xmlTestResults = new XmlTextWriter(filename, new UTF8Encoding(false)))
            {
                xmlTestResults.WriteStartElement("testsuite");
                xmlTestResults.WriteAttributeString("name", m_TestName);
                int num_failures = 0;
                int num_tests = 0;
                foreach (TestResult tr in TestResults)
                {
                    if (!tr.Result)
                    {
                        ++num_failures;
                    }
                    ++num_tests;
                }
                xmlTestResults.WriteAttributeString("tests", num_tests.ToString(CultureInfo.InvariantCulture));
                xmlTestResults.WriteAttributeString("errors", "0");
                xmlTestResults.WriteAttributeString("failures", num_failures.ToString(CultureInfo.InvariantCulture));
                xmlTestResults.WriteAttributeString("skip", "0");
                foreach (TestResult re in TestResults)
                {
                    xmlTestResults.WriteStartElement("testcase");
                    xmlTestResults.WriteAttributeString("name", re.Name);
                    xmlTestResults.WriteAttributeString("time", (re.RunTime / 1000f).ToString(CultureInfo.InvariantCulture));
                    {
                        if (re.Result)
                        {
                            xmlTestResults.WriteStartElement("success");
                        }
                        else
                        {
                            xmlTestResults.WriteStartElement("failure");
                        }
                        xmlTestResults.WriteAttributeString("classname", re.Name);
                        xmlTestResults.WriteAttributeString("name", re.Name);
                        xmlTestResults.WriteValue(re.Message);
                        xmlTestResults.WriteEndElement();
                    }
                    xmlTestResults.WriteEndElement();
                }
                xmlTestResults.WriteEndElement();
            }
        }

        private void WriteNUnit2Results(string filename)
        {
            using (var xmlTestResults = new XmlTextWriter(filename, new UTF8Encoding(false)))
            {
                int num_failures = 0;
                int num_tests = 0;
                long runtime = 0;
                foreach (TestResult tr in TestResults)
                {
                    if (!tr.Result)
                    {
                        ++num_failures;
                    }
                    ++num_tests;
                    runtime += tr.RunTime;
                }

                xmlTestResults.WriteStartElement("test-results");
                xmlTestResults.WriteAttributeString("name", m_TestName);
                xmlTestResults.WriteAttributeString("total", num_tests.ToString(CultureInfo.InvariantCulture));
                xmlTestResults.WriteAttributeString("errors", "0");
                xmlTestResults.WriteAttributeString("failures", num_failures.ToString(CultureInfo.InvariantCulture));
                xmlTestResults.WriteAttributeString("not-run", "0");
                xmlTestResults.WriteAttributeString("inconclusive", "0");
                xmlTestResults.WriteAttributeString("ignored", "0");
                xmlTestResults.WriteAttributeString("skipped", "0");
                xmlTestResults.WriteAttributeString("invalid", "0");
                {
                    xmlTestResults.WriteStartElement("test-suite");
                    xmlTestResults.WriteAttributeString("name", m_TestName);
                    xmlTestResults.WriteAttributeString("executed", "True");
                    xmlTestResults.WriteAttributeString("result", num_failures != 0 ? "Failure" : "Success");
                    xmlTestResults.WriteAttributeString("success", num_failures != 0 ? "False" : "True");
                    xmlTestResults.WriteAttributeString("time", (runtime / 1000.0).ToString(CultureInfo.InvariantCulture));
                    xmlTestResults.WriteAttributeString("asserts", "0");
                    xmlTestResults.WriteStartElement("results");
                    {
                        foreach (TestResult re in TestResults)
                        {
                            xmlTestResults.WriteStartElement("test-case");
                            {
                                xmlTestResults.WriteAttributeString("name", re.Name);
                                xmlTestResults.WriteAttributeString("executed", "True");
                                xmlTestResults.WriteAttributeString("result", re.Result ? "Success" : "Failure");
                                xmlTestResults.WriteAttributeString("success", re.Result ? "False" : "True");
                                xmlTestResults.WriteAttributeString("time", (re.RunTime / 1000.0).ToString(CultureInfo.InvariantCulture));
                                xmlTestResults.WriteAttributeString("asserts", "0");
                                xmlTestResults.WriteStartElement("reason");
                                {
                                    xmlTestResults.WriteStartElement("message");
                                    xmlTestResults.WriteValue(re.Message);
                                    xmlTestResults.WriteEndElement();
                                    if (re.StackTrace?.Length > 0)
                                    {
                                        xmlTestResults.WriteStartElement("stack-trace");
                                        xmlTestResults.WriteValue(re.Message);
                                        xmlTestResults.WriteEndElement();
                                    }
                                }
                                xmlTestResults.WriteEndElement();
                            }
                            xmlTestResults.WriteEndElement();
                        }
                        xmlTestResults.WriteEndElement();
                    }
                    xmlTestResults.WriteEndElement();
                }
                xmlTestResults.WriteEndElement();
            }
        }

        public void AddPlugins(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs["Tests"];
            if(config == null)
            {
                m_Log.Fatal("Nothing to test");
                throw new ConfigurationLoader.TestingErrorException();
            }

            foreach(string testname in config.GetKeys())
            {
                Type t = GetType().Assembly.GetType(testname);
                if(t == null)
                {
                    m_Log.FatalFormat("Missing test {0}", testname);
                    throw new ConfigurationLoader.TestingErrorException();
                }
                loader.AddPlugin(testname, (IPlugin)Activator.CreateInstance(t));
            }
        }

        public void Shutdown()
        {
            m_Loader = null;
        }
    }
}
