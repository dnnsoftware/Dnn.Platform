using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Entities.Host
{
    public class ServerUrlAdapter : IServerUrlAdapter
    {
        public string GetServerUrl()
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

        public string GetServerUniqueId()
        {
            return Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
        }
    }
}
