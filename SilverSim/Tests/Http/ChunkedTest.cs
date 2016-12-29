using log4net;
using SilverSim.Http;
using SilverSim.Main.Common;
using SilverSim.Tests.Extensions;
using System.IO;
using System.Reflection;

namespace SilverSim.Tests.Http
{
    public class ChunkedTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Cleanup()
        {
            /* intentionally left empty */
        }

        public bool Run()
        {
            byte[] testdata = new byte[131072];
            for(int i = 0; i < testdata.Length; ++i)
            {
                testdata[i] = (byte)(i % 256);
            }
            return RunTestData(testdata);
        }

        bool RunTestData(byte[] data)
        {
            byte[] encoded;
            byte[] decoded = new byte[data.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                using (HttpWriteChunkedBodyStream write = new HttpWriteChunkedBodyStream(ms))
                {
                    write.Write(data, 0, data.Length);
                }
                encoded = ms.ToArray();
            }

            using (MemoryStream ms = new MemoryStream(encoded))
            {
                using (HttpReadChunkedBodyStream read = new HttpReadChunkedBodyStream(ms))
                {
                    int rcvdbytes = 0;
                    rcvdbytes = read.Read(decoded, 0, data.Length);
                    if (data.Length != rcvdbytes)
                    {
                        m_Log.ErrorFormat("chunked stream does not contain all input bytes ({0}!={1})", data.Length, rcvdbytes);
                        return false;
                    }
                    byte[] t = new byte[1];
                    if (read.Read(t, 0, 1) != 0)
                    {
                        m_Log.Error("chunked stream does not end with last byte");
                        return false;
                    }
                }
            }

            int i;
            for(i = 0; i < data.Length; ++i)
            {
                if(data[i] != decoded[i])
                {
                    m_Log.ErrorFormat("Compare error at byte position {0}", i);
                    return false;
                }
            }
            return true;
        }

        public void Setup()
        {
            /* intentionally left empty */
        }

        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left empty */
        }
    }
}
