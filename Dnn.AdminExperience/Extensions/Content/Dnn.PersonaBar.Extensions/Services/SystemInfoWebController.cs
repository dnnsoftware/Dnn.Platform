using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Servers.Components.WebServer;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Servers.Services
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class SystemInfoWebController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemInfoWebController));
        
        [HttpGet]
        public HttpResponseMessage GetWebServerInfo()
        {
            try
            {
                var serverInfo = new ServerInfo();
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    osVersion = serverInfo.OSVersion,
                    iisVersion = serverInfo.IISVersion,
                    framework = serverInfo.Framework,
                    identity = serverInfo.Identity,
                    hostName = serverInfo.HostName,
                    physicalPath = serverInfo.PhysicalPath,
                    url = serverInfo.Url,
                    relativePath = serverInfo.RelativePath,
                    serverTime = serverInfo.ServerTime
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
