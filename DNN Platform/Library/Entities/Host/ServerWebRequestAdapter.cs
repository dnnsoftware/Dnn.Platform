// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Net;
    using System.Web;

    using DotNetNuke.Common;

    public class ServerWebRequestAdapter : IServerWebRequestAdapter
    {
        /// <inheritdoc />
        public virtual string GetServerUrl()
        {
            var domainName = string.Empty;
            if (HttpContext.Current != null)
            {
                domainName = Globals.GetDomainName(HttpContext.Current.Request);

                if (domainName.Contains("/", StringComparison.Ordinal))
                {
                    domainName = domainName.Substring(0, domainName.IndexOf("/", StringComparison.Ordinal));

                    if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                    {
                        domainName = $"{domainName}{Globals.ApplicationPath}";
                    }
                }
            }

            return domainName;
        }

        /// <inheritdoc />
        public virtual string GetServerUniqueId()
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public virtual void ProcessRequest(HttpWebRequest request, ServerInfo server)
        {
        }

        /// <inheritdoc />
        public virtual void CheckResponse(HttpWebResponse response, ServerInfo server, ref HttpStatusCode statusCode)
        {
        }
    }
}
