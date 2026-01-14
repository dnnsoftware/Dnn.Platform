//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace DotNetNuke.Tests.UI.WatiN.Utilities;

using System;
using System.Diagnostics;
using Microsoft.Web.Administration;

public static class IISManager
{
    public static void CreateIISApplication(string appName, string physicalPath)
    {
        ServerManager iisManager = new ServerManager();
        Site defaultSite = iisManager.Sites["Default Web Site"];
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

        // Remove application if it already exists
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

        // Remove application if it already exists
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
        ////ServerManager iisManager = new ServerManager();
        ////Site defaultSite = iisManager.Sites["Default Web Site"];
        ////Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

        ////if (app != null)
        ////{
        ////    string appPool = app.ApplicationPoolName;
        ////    ApplicationPool pool = iisManager.ApplicationPools[appPool];
        ////    pool.Recycle();
        ////}
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
