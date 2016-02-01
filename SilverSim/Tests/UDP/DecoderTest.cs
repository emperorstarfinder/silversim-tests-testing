// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3

using log4net;
using Nini.Config;
using SilverSim.Main.Common;
using SilverSim.Tests.Extensions;
using SilverSim.Types;
using SilverSim.Viewer.Messages;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SilverSim.Tests.UDP
{
    public class DecoderTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        List<KeyValuePair<string, byte[]>> DataBlocks = new List<KeyValuePair<string, byte[]>>();
        TestRunner m_Runner;

        public void Startup(ConfigurationLoader loader)
        {
            IConfig config = loader.Config.Configs[GetType().FullName];
            foreach (string key in config.GetKeys())
            {
                DataBlocks.Add(new KeyValuePair<string, byte[]>(key, config.GetString(key).FromHexStringToByteArray()));
            }
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

        public bool Run()
        {
            bool success = true;
            int count = 0;
            int successcnt = 0;
            UDPPacketDecoder decoder = new UDPPacketDecoder(true);
            foreach (KeyValuePair<string, byte[]> data in DataBlocks)
            {
                ++count;
                TestRunner.TestResult tr = new TestRunner.TestResult();
                tr.Name = "UDP Packet " + data.Key;
                tr.Result = false;
                tr.Message = string.Empty;
                int startTime = Environment.TickCount;
                m_Log.InfoFormat("Testing decoding of {0}", data.Key);
                try
                {
                    UDPPacket p = new UDPPacket();
                    Buffer.BlockCopy(data.Value, 0, p.Data, 0, data.Value.Length);
                    p.DataLength = data.Value.Length;
                    MessageType mType = p.ReadMessageType();
                    Func<UDPPacket, Message> m = decoder.PacketTypes[mType];
                    Message res = m(p);
                    m_Log.InfoFormat("Decoding of {0} successful: got {1}", data.Key, res.GetType().FullName);
                    ++successcnt;
                    tr.Result = true;
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Decoding of {0} failed: {1}", data.Key, e.Message);
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
    }
}
