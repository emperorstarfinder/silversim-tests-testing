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
using SilverSim.Http.Client;
using SilverSim.Main.Common;
using SilverSim.Scene.Types.Object;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SilverSim.Tests.Assets.Formats
{
    public class ObjectDeserializationFromUri : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<UUID, string> Assets = new Dictionary<UUID, string>();
        private TestRunner m_Runner;

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
            foreach (KeyValuePair<UUID, string> file in Assets)
            {
                ++count;
                var tr = new TestRunner.TestResult
                {
                    Name = "Asset " + file.Key + "(" + file.Value + ")",
                    Result = false,
                    Message = string.Empty
                };
                int startTime = Environment.TickCount;
                m_Log.InfoFormat("Testing deserialization of {1} ({0})", file.Key, file.Value);

                try
                {
                    using(Stream s = new HttpClient.Get(file.Value).ExecuteStreamRequest())
                    {
                        ObjectXML.FromXml(s, UUI.Unknown, XmlDeserializationOptions.ReadKeyframeMotion);
                    }
                    m_Log.InfoFormat("Deserialization of {1} ({0}) successful", file.Key, file.Value);
                    ++successcnt;
                    tr.Result = true;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Compilation of {1} ({0}) failed: {2}", file.Key, file.Value, e.Message);
                    m_Log.WarnFormat("Stack Trace:\n{0}", e.StackTrace);
                    tr.Message = e.Message + "\n" + e.StackTrace;
                    success = false;
                }
                tr.RunTime = Environment.TickCount - startTime;
                m_Runner.TestResults.Add(tr);
            }
            m_Log.InfoFormat("{0} of {1} compilations successful", successcnt, count);
            return success;
        }

        public void Startup(ConfigurationLoader loader)
        {
            m_Runner = loader.GetServicesByValue<TestRunner>()[0];
            m_Runner.ExcludeSummaryCount = true;

            IConfig config = loader.Config.Configs[GetType().FullName];
            foreach (string key in config.GetKeys())
            {
                UUID uuid;
                if (UUID.TryParse(key, out uuid))
                {
                    Assets[uuid] = config.GetString(key);
                }
            }
        }
    }
}
