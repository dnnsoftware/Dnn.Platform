#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
#region Usings

using System;
using System.Linq;
using System.Net;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using DotNetNuke.Modules.HTMLEditorProvider;
using DotNetNuke.Modules.NavigationProvider;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Profile;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.ClientCapability;
using DotNetNuke.Services.Cryptography;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.ModuleCache;
using DotNetNuke.Services.OutputCache;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Services.Sitemap;
using DotNetNuke.Services.Url.FriendlyUrl;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Blocker;

#endregion

namespace DotNetNuke.Web.Common.Internal
{
    /// <summary>
    /// DotNetNuke Http Application. It will handle Start, End, BeginRequest, Error event for whole application.
    /// </summary>
    public class DotNetNukeHttpApplication : HttpApplication
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (DotNetNukeHttpApplication));

        void Application_Error(object sender, EventArgs eventArgs)
        {
            // Code that runs when an unhandled error occurs
            if (HttpContext.Current != null)
            {
                // Get the exception object.
                Logger.Trace("Dumping all Application Errors");
                foreach (Exception exc in HttpContext.Current.AllErrors) Logger.Fatal(exc);
                Logger.Trace("End Dumping all Application Errors");
            }
        }

        private void Application_Start(object sender, EventArgs eventArgs)
        {
            Logger.InfoFormat("Application Starting ({0})", Globals.ElapsedSinceAppStart); // just to start the timer

            var name = Config.GetSetting("ServerName");
            Globals.ServerName = String.IsNullOrEmpty(name) ? Dns.GetHostName() : name;

            ComponentFactory.Container = new SimpleContainer();

            ComponentFactory.InstallComponents(new ProviderInstaller("data", typeof(DataProvider), typeof(SqlDataProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider), typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("logging", typeof(LoggingProvider), typeof(DBLoggingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("scheduling", typeof(SchedulingProvider), typeof(DNNScheduler)));
            ComponentFactory.InstallComponents(new ProviderInstaller("searchIndex", typeof(IndexingProvider), typeof(ModuleIndexer)));
            #pragma warning disable 0618
            ComponentFactory.InstallComponents(new ProviderInstaller("searchDataStore", typeof(SearchDataStoreProvider), typeof(SearchDataStore)));
            #pragma warning restore 0618
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(MembershipProvider), typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("roles", typeof(RoleProvider), typeof(DNNRoleProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider), typeof(DNNProfileProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("permissions", typeof(PermissionProvider), typeof(CorePermissionProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("outputCaching", typeof(OutputCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("moduleCaching", typeof(ModuleCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("sitemap", typeof(SitemapProvider), typeof(CoreSitemapProvider)));

            ComponentFactory.InstallComponents(new ProviderInstaller("friendlyUrl", typeof(FriendlyUrlProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("folder", typeof(FolderProvider)));
            RegisterIfNotAlreadyRegistered<FolderProvider, StandardFolderProvider>("StandardFolderProvider");
            RegisterIfNotAlreadyRegistered<FolderProvider, SecureFolderProvider>("SecureFolderProvider");
            RegisterIfNotAlreadyRegistered<FolderProvider, DatabaseFolderProvider>("DatabaseFolderProvider");
            RegisterIfNotAlreadyRegistered<PermissionProvider>();
            ComponentFactory.InstallComponents(new ProviderInstaller("htmlEditor", typeof(HtmlEditorProvider), ComponentLifeStyleType.Transient));
            ComponentFactory.InstallComponents(new ProviderInstaller("navigationControl", typeof(NavigationProvider), ComponentLifeStyleType.Transient));
            ComponentFactory.InstallComponents(new ProviderInstaller("clientcapability", typeof(ClientCapabilityProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider),typeof(FipsCompilanceCryptographyProvider)));

            Logger.InfoFormat("Application Started ({0})", Globals.ElapsedSinceAppStart); // just to start the timer
            DotNetNukeShutdownOverload.InitializeFcnSettings();
            //DotNetNukeSecurity.Initialize();
        }
        
        private static void RegisterIfNotAlreadyRegistered<TConcrete>() where TConcrete : class, new()
        {
            RegisterIfNotAlreadyRegistered<TConcrete, TConcrete>("");
        }

        private static void RegisterIfNotAlreadyRegistered<TAbstract, TConcrete>(string name)
            where TAbstract : class
            where TConcrete : class, new()
        {
            var provider = ComponentFactory.GetComponent<TAbstract>();
            if (provider == null)
            {
                if (String.IsNullOrEmpty(name))
                {
                    ComponentFactory.RegisterComponentInstance<TAbstract>(new TConcrete());
                }
                else
                {
                    ComponentFactory.RegisterComponentInstance<TAbstract>(name, new TConcrete());
                }
            }
        }

        private void Application_End(object sender, EventArgs eventArgs)
        {
            Logger.Info("Application Ending");

            try
            {
                Initialize.LogEnd();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            try
            {
                Initialize.StopScheduler();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            //Shutdown Lucene, but not when we are installing
            if (Globals.Status != Globals.UpgradeStatus.Install)
            {
                Logger.Trace("Disposing Lucene");
                var lucene = LuceneController.Instance as IDisposable;
                if (lucene != null) lucene.Dispose();
            }
            Logger.Trace("Dumping all Application Errors");
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.AllErrors != null)
                {
                    foreach (Exception exc in HttpContext.Current.AllErrors) Logger.Fatal(exc);
                }
            }
            Logger.Trace("End Dumping all Application Errors");
            Logger.Info("Application Ended");
        }

        private static readonly string[] Endings =
            {
                ".css", ".gif", ".jpeg", ".jpg", ".js", ".png", "scriptresource.axd", "webresource.axd"
            };

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var requestUrl = app.Request.Url.LocalPath.ToLower();
            if (!requestUrl.EndsWith(".aspx") && !requestUrl.EndsWith("/") &&  Endings.Any(requestUrl.EndsWith))
            {
                return;
            }

            if (IsInstallInProgress(app))
            {
                return;
            }

            Initialize.Init(app);
            Initialize.RunSchedule(app.Request);
        }

		private void Application_PreSendRequestHeaders(object sender, EventArgs e)
		{
			if (HttpContext.Current != null && HttpContext.Current.Handler is PageBase)
			{
				var page = HttpContext.Current.Handler as PageBase;
				page.HeaderIsWritten = true;
			}
		}

        private bool IsInstallInProgress(HttpApplication app)
        {
            return InstallBlocker.Instance.IsInstallInProgress();
        }

    }
}