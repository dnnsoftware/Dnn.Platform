#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Diagnostics;
using Microsoft.Web.Administration;
using System;

namespace DotNetNuke.Tests.UI.WatiN.Utilities
{
    public static class IISManager
    {
        public static void CreateIISApplication(string appName, string physicalPath)
        {
            ServerManager iisManager = new ServerManager();
            Site defaultSite = iisManager.Sites["Default Web Site"];
            Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

            //Remove application if it already exists
            if (app != null)
            {
                defaultSite.Applications.Remove(app);
                iisManager.CommitChanges();
            }

            defaultSite = iisManager.Sites["Default Web Site"];
            defaultSite.Applications.Add(String.Format("/{0}", appName), physicalPath);
            iisManager.CommitChanges();
            iisManager.Dispose();
        }

        public static void CreateIISApplication(string appName, string physicalPath, string appPool)
        {
            ServerManager iisManager = new ServerManager();
            Site defaultSite = iisManager.Sites["Default Web Site"];
            Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

            //Remove application if it already exists
            if (app != null)
            {
                defaultSite.Applications.Remove(app);
                iisManager.CommitChanges();
            }

            defaultSite.Applications.Add(String.Format("/{0}", appName), physicalPath);
            SetApplicationPool(appName, appPool);
            iisManager.CommitChanges();
            iisManager.Dispose();
        }

        public static void CreateIISWebSite(string siteName, string siteAddress, string physicalPath)
        {
            ServerManager iisManager = new ServerManager();
            Site site = iisManager.Sites[siteName];
            if (site != null)
            {
                iisManager.Sites.Remove(iisManager.Sites[siteName]);
                iisManager.CommitChanges();
            }

            iisManager.Sites.Add(siteName, "http", string.Concat("*:80:", siteAddress), physicalPath);
            iisManager.CommitChanges();
            iisManager.Dispose();
        }

        public static void RecycleApplicationPool(string appName)
        {
            //ServerManager iisManager = new ServerManager();
            //Site defaultSite = iisManager.Sites["Default Web Site"];
            //Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

            //if (app != null)
            //{
            //    string appPool = app.ApplicationPoolName;
            //    ApplicationPool pool = iisManager.ApplicationPools[appPool];
            //    pool.Recycle();
            //}
        }

        public static void SetApplicationPool(string appName, string appPool)
        {
            ServerManager iisManager = new ServerManager();
            Site defaultSite = iisManager.Sites["Default Web Site"];
            Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

            if (app != null)
            {
                app.ApplicationPoolName = appPool;
                iisManager.CommitChanges();
                iisManager.Dispose();
            }
        }
        public static void KillW3WP()
        {
            foreach (Process p in Process.GetProcessesByName("w3wp"))
            {
                p.Kill();
            }
        }
    }
}
