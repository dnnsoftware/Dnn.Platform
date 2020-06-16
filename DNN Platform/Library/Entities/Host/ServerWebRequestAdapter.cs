// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host
{
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

    public class ServerWebRequestAdapter : IServerWebRequestAdapter
    {
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
            return string.Empty;
        }

        /// <summary>
        /// Process Request before the request send to server.
        /// </summary>
        /// <param name="request">The Http Request Object.</param>
        /// <param name="server">The Server Info Object.</param>
        public virtual void ProcessRequest(HttpWebRequest request, ServerInfo server)
        {
        }

        /// <summary>
        /// Check whether response is return from correct server.
        /// </summary>
        /// <param name="response">The Http Response Object.</param>
        /// <param name="statusCode">Out status code if you think the status need change.</param>
        public virtual void CheckResponse(HttpWebResponse response, ServerInfo server, ref HttpStatusCode statusCode)
        {
        }
    }
}
