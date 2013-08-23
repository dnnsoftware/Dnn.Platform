using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;

namespace DotNetNukeSetup
{
    public static class IISManager
    {
        public static void CreateIISWebSite(string siteName, string siteAddress, string physicalPath)
        {
            var iisManager = new ServerManager();
            var site = iisManager.Sites[siteName];
            if (site != null)
            {
                iisManager.Sites.Remove(iisManager.Sites[siteName]);
                iisManager.CommitChanges();
            }

            var newSite = iisManager.Sites.Add(siteName, "http", string.Concat("*:80:", siteAddress), physicalPath);
            //var binding = newSite.Bindings.CreateElement("binding");
            //binding["protocol"] = "http";
            //binding["bindingInformation"] = string.Format(@"127.0.0.1:80:{0}", siteAddress);
            //newSite.Bindings.Add(binding);
            newSite.Applications[0].ApplicationPoolName = ".Net v4.5";
            iisManager.CommitChanges(); 
            iisManager.Dispose();
        }

    }
}
