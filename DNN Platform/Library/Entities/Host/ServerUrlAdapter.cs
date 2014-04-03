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
            if (HttpContext.Current != null)
            {
                return Globals.GetDomainName(HttpContext.Current.Request);
            }

            return string.Empty;
        }
    }
}
