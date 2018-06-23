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
using SilverSim.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace SilverSim.Tests.Http.Post.AfterRequestBody
{
    public sealed class Error3XXTest : ITest
    {
        private static readonly ILog m_Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private BaseHttpServer m_HttpServer;
        private HttpStatusCode m_SendStatusCode;

        public void Startup(ConfigurationLoader loader)
        {
            m_HttpServer = loader.HttpServer;
        }

        public void Setup()
        {
        }

        public bool Run()
        {
            m_HttpServer.UriHandlers.Add("/test", HttpHandler);
            m_Log.InfoFormat("Testing HTTP POST error responses");
            for (m_SendStatusCode = HttpStatusCode.MultipleChoices; m_SendStatusCode < HttpStatusCode.HttpVersionNotSupported; ++m_SendStatusCode)
            {
                var headers = new Dictionary<string, string>();
                try
                {
                    HttpStatusCode statuscode = new HttpClient.Post(m_HttpServer.ServerURI + "test", "text/plain", string.Empty)
                    {
                        TimeoutMs = 60000,
                        ConnectionMode = m_SendStatusCode == HttpStatusCode.GatewayTimeout ? HttpClient.ConnectionModeEnum.Close : HttpClient.ConnectionModeEnum.Keepalive,
                        Headers = headers,
                        DisableExceptions = HttpClient.Request.DisableExceptionFlags.Disable3XX
                    }.ExecuteStatusRequest();
                    if(statuscode != m_SendStatusCode)
                    {
                        m_Log.ErrorFormat("Got wrong status at connection {0}: {1}", m_SendStatusCode, statuscode);
                        return false;
                    }
                }
                catch(System.Web.HttpException e)
                {
                    if(e.GetHttpCode() == (int)HttpStatusCode.NotFound ||( e.GetHttpCode() >= 300 && e.GetHttpCode() <= 399))
                    {
                        m_Log.ErrorFormat("Got exception at connection {0}: {1}: {2}: {3}", m_SendStatusCode, e.GetType().FullName, e.Message, e.StackTrace);
                        return false;
                    }
                    else if(e.GetHttpCode() != (int)m_SendStatusCode)
                    {
                        m_Log.ErrorFormat("Got wrong status at connection {0}: {1}: {2}: {3}", m_SendStatusCode, e.GetType().FullName, e.Message, e.StackTrace);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    m_Log.ErrorFormat("Failed at connection {0}: {1}: {2}: {3}", m_SendStatusCode, e.GetType().FullName, e.Message, e.StackTrace);
                    return false;
                }
            }
            return true;
        }

        private void HttpHandler(HttpRequest req)
        {
            using (Stream s = req.Body)
            {
                s.ReadToStreamEnd();
            }
            req.ErrorResponse(m_SendStatusCode);
        }

        public void Cleanup()
        {
            m_HttpServer.UriHandlers.Remove("/test");
        }
    }
}
