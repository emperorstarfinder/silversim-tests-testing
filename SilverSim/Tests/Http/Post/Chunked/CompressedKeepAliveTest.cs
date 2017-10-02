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
using SilverSim.Http.Client;
using SilverSim.Main.Common;
using SilverSim.Main.Common.HttpServer;
using SilverSim.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace SilverSim.Tests.Http.Post.Chunked
{
    public class CompressedKeepAliveTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private BaseHttpServer m_HttpServer;

        public void Startup(ConfigurationLoader loader)
        {
            m_HttpServer = loader.HttpServer;
        }

        public void Setup()
        {
        }

        private string GetConnectionValue(Dictionary<string, string> headers)
        {
            foreach (KeyValuePair<string, string> kvp in headers)
            {
                if (string.Compare(kvp.Key, "connection", true) == 0)
                {
                    return kvp.Value;
                }
            }
            return string.Empty;
        }

        private string GetTransferEncodingValue(Dictionary<string, string> headers)
        {
            foreach (KeyValuePair<string, string> kvp in headers)
            {
                if (string.Compare(kvp.Key, "transfer-encoding", true) == 0)
                {
                    return kvp.Value;
                }
            }
            return string.Empty;
        }

        public bool Run()
        {
            m_HttpServer.UriHandlers.Add("/test", HttpHandler);
            int NumberConnections = 1000;
            int numConns = m_HttpServer.AcceptedConnectionsCount;
            m_Log.InfoFormat("Testing 1000 HTTP POST requests (keep-alive)");
            for (int connidx = 0; connidx++ < NumberConnections;)
            {
                string res;
                var headers = new Dictionary<string, string>();
                try
                {
                    res = new HttpClient.Post(m_HttpServer.ServerURI + "test", "text/plain", (instream) =>
                    {
                        using (var gzip = new GZipStream(instream, CompressionMode.Compress))
                        {
                            byte[] connbytes = Encoding.ASCII.GetBytes(connidx.ToString());
                            gzip.Write(connbytes, 0, connbytes.Length);
                        }
                    })
                    {
                        IsCompressed = true,
                        TimeoutMs = 60000,
                        ConnectionMode = connidx == NumberConnections ? HttpClient.ConnectionModeEnum.Close : HttpClient.ConnectionModeEnum.Keepalive,
                        Headers = headers
                    }.ExecuteRequest();
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Failed at connection {0}: {1}: {2}: {3}", connidx, e.GetType().FullName, e.Message, e.StackTrace);
                    return false;
                }
                int resval;
                if (!int.TryParse(res, out resval) || connidx != resval)
                {
                    m_Log.InfoFormat("Wrong text response at connection {0}: {1}", connidx, res);
                    return false;
                }
                string connval = GetConnectionValue(headers).Trim().ToLower();
                if (connidx == NumberConnections)
                {
                    if (connval != "close")
                    {
                        m_Log.ErrorFormat("Connection: field has wrong response on last request: \"{0}\"", connval);
                        return false;
                    }
                }
                else
                {
                    if (connval != "keep-alive")
                    {
                        m_Log.ErrorFormat("Connection: field has wrong response: \"{0}\"", connval);
                        return false;
                    }
                }
                string chunkval = GetTransferEncodingValue(headers).Trim().ToLower();
                if (chunkval != string.Empty)
                {
                    m_Log.ErrorFormat("Transfer-Encoding: field has wrong response: \"{0}\"", chunkval);
                    return false;
                }
            }
            numConns = m_HttpServer.AcceptedConnectionsCount - numConns;
            if (numConns != 1)
            {
                m_Log.InfoFormat("HTTP client did not re-use connections (actual {0})", numConns);
                return false;
            }
            return true;
        }

        private void HttpHandler(HttpRequest req)
        {
            byte[] outdata;
            using (var ms = new MemoryStream())
            {
                req.Body.CopyTo(ms);
                outdata = ms.ToArray();
            }
            string encoding;
            if (!req.TryGetHeader("x-content-encoding", out encoding) || encoding != "gzip")
            {
                outdata = Encoding.ASCII.GetBytes("POST not gzip encoded");
            }
            if (req.MajorVersion != 1)
            {
                outdata = Encoding.ASCII.GetBytes("Not HTTP/1");
            }
            string transferencoding;
            if (!req.TryGetHeader("transfer-encoding", out transferencoding) || transferencoding != "chunked")
            {
                outdata = Encoding.ASCII.GetBytes("Transfer-Encoding: chunked should be use");
            }
            if (req.ContainsHeader("expect"))
            {
                outdata = Encoding.ASCII.GetBytes("Expect: 100-continue should not be used");
            }
            using (HttpResponse res = req.BeginResponse())
            {
                using (Stream s = res.GetOutputStream(outdata.Length))
                {
                    s.Write(outdata, 0, outdata.Length);
                }
            }
        }

        public void Cleanup()
        {
            m_HttpServer.UriHandlers.Remove("/test");
        }
    }
}
