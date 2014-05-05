using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Entities.Host
{
    public class ServerWebRequestAdapter : IServerWebRequestAdapter
    {
        #region Private Properties

        private string UniqueIdHeaderName
        {
            get { return HostController.Instance.GetString("WebServer_UniqueIdHeaderName", string.Empty); }
        }

        #endregion

        /// <summary>
        /// Get the server's endpoint which can access the server directly.
        /// </summary>
        /// <returns></returns>
        public virtual string GetServerUrl()
        {
            var domainName = string.Empty;
            if (HttpContext.Current != null)
            {
                domainName = Globals.GetDomainName(HttpContext.Current.Request);
                
                if (domainName.Contains("/"))
                {
                    domainName = domainName.Substring(0, domainName.IndexOf("/"));

                    if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                    {
                        domainName = string.Format("{0}{1}", domainName, Globals.ApplicationPath);
                    }
                }
            }

            return domainName;
        }

        /// <summary>
        /// Get the server's unique id when server is behind affinity tool.
        /// </summary>
        /// <returns></returns>
        public virtual string GetServerUniqueId()
        {
            return Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
        }

        /// <summary>
        /// Process Request before the request send to server.
        /// </summary>
        /// <param name="request">The Http Request Object.</param>
        /// <param name="server">The Server Info Object.</param>
        public void PorcessRequest(HttpWebRequest request, ServerInfo server)
        {
            if (!string.IsNullOrEmpty(server.UniqueId) && !string.IsNullOrEmpty(UniqueIdHeaderName))
            {
                if (request.CookieContainer == null)
                {
                    request.CookieContainer = new CookieContainer();
                }

                request.CookieContainer.Add(new Cookie(UniqueIdHeaderName, server.UniqueId){Domain = request.Host});
            }
        }

        /// <summary>
        /// Check whether response is return from correct server.
        /// </summary>
        /// <param name="response">The Http Response Object.</param>
        /// <param name="statusCode">Out status code if you think the status need change.</param>
        public void CheckResponse(HttpWebResponse response, ServerInfo server, ref HttpStatusCode statusCode)
        {
            if ((response.Headers.AllKeys.Contains(UniqueIdHeaderName) && response.Headers[UniqueIdHeaderName] != server.UniqueId)
                            || (response.Headers["Set-Cookie"].Contains(UniqueIdHeaderName) && !response.Headers["Set-Cookie"].Contains(UniqueIdHeaderName + "=" + server.UniqueId)))
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
            }
        }
    }
}
