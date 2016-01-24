// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Main.Common.CmdIO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;

namespace SilverSim.Tests.Extensions
{
    #region Implementation
    public class TestRunner : IPlugin, IPostLoadStep, IPluginSubFactory, IPluginShutdown
    {
        private static readonly ILog m_Log = LogManager.GetLogger("TEST RUNNER");
        List<ITest> m_Tests = new List<ITest>();
        string m_TestName = string.Empty;
        string m_XmlResultFileName = string.Empty;
        ConfigurationLoader m_Loader;
        public struct TestResult
        {
            public string Name;
            public bool Result;
            public int RunTime;
            public string Message;
        }

        public List<TestResult> TestResults = new List<TestResult>();
        public bool OtherThreadResult = true; /* storage for tests running over multiple threads */
        public bool ExcludeSummaryCount;

        public ShutdownOrder ShutdownOrder
        {
            get
            {
                return ShutdownOrder.Any;
            }
        }

        public TestRunner(string testName, string xmlResultFileName)
        {
            m_TestName = testName;
            m_XmlResultFileName = xmlResultFileName;
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
            foreach(ITest test in m_Tests)
            {
                TestResult tr = new TestResult();
                tr.Name = test.GetType().FullName;
                tr.RunTime = Environment.TickCount;
                m_Log.Info("********************************************************************************");
                m_Log.InfoFormat("Executing test {0}", test.GetType().FullName);
                m_Log.Info("********************************************************************************");
                try
                {
                    test.Setup();

                    if (test.Run())
                    {
                        m_Log.Info("********************************************************************************");
                        m_Log.InfoFormat("Executed test {0} with SUCCESS", test.GetType().FullName);
                        m_Log.Info("********************************************************************************");
                        tr.Result = true;
                        tr.Message = "Success";
                    }
                    else
                    {
                        m_Log.Info("********************************************************************************");
                        m_Log.ErrorFormat("Executed test {0} with FAILURE", test.GetType().FullName);
                        m_Log.Info("********************************************************************************");
                        failed = true;
                        tr.Message = "Failure";
                        tr.Result = false;
                    }
                }
                catch(Exception e)
                {
                    m_Log.Info("********************************************************************************");
                    m_Log.InfoFormat("Executed test {0} with FAILURE", test.GetType().FullName);
                    m_Log.ErrorFormat("Exception {0}: {1}\n{2}", e.GetType().FullName, e.ToString(), e.StackTrace.ToString());
                    m_Log.Info("********************************************************************************");
                    failed = true;
                    tr.Message = string.Format("Exception {0}: {1}\n{2}", e.GetType().FullName, e.ToString(), e.StackTrace.ToString());
                    tr.Result = false;
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
                    tr.Message = string.Format("Exception {0}: {1}\n{2}", e.GetType().FullName, e.ToString(), e.StackTrace.ToString());
                    tr.Result = false;
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

            if (!string.IsNullOrEmpty(m_XmlResultFileName))
            {
                XmlTextWriter xmlTestResults = new XmlTextWriter(m_XmlResultFileName, new UTF8Encoding(false));
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
                xmlTestResults.WriteAttributeString("tests", num_tests.ToString());
                xmlTestResults.WriteAttributeString("errors", "0");
                xmlTestResults.WriteAttributeString("failures", num_failures.ToString());
                xmlTestResults.WriteAttributeString("skip", "0");
                foreach (TestResult re in TestResults)
                {
                    xmlTestResults.WriteStartElement("testcase");
                    xmlTestResults.WriteAttributeString("name", re.Name);
                    xmlTestResults.WriteAttributeString("time", (re.RunTime / 1000f).ToString());
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
                xmlTestResults.Close();
            }

            if (failed)
            {
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
    #endregion

    #region Factory
    [PluginName("TestRunner")]
    class TestRunnerFactory : IPluginFactory
    {
        public TestRunnerFactory()
        {

        }

        public IPlugin Initialize(ConfigurationLoader loader, IConfig ownSection)
        {
            return new TestRunner(ownSection.GetString("Name", ""), ownSection.GetString("XUnitResultsFile", ""));
        }
    }
    #endregion
}
