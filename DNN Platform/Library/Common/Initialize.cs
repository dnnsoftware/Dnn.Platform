// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.UI.Modules;
    using Microsoft.Win32;

    /// <summary>
    /// The Object to initialize application.
    /// </summary>
    public class Initialize
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Initialize));
        private static readonly object InitializeLock = new object();
        private static bool InitializedAlready;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Inits the app.
        /// </summary>
        /// <param name="app">The app.</param>
        /// -----------------------------------------------------------------------------
        public static void Init(HttpApplication app)
        {
            string redirect;

            // Check if app is initialised
            if (InitializedAlready && Globals.Status == Globals.UpgradeStatus.None)
            {
                return;
            }

            lock (InitializeLock)
            {
                // Double-Check if app was initialised by another request
                if (InitializedAlready && Globals.Status == Globals.UpgradeStatus.None)
                {
                    return;
                }

                // Initialize ...
                redirect = InitializeApp(app, ref InitializedAlready);
            }

            if (!string.IsNullOrEmpty(redirect))
            {
                app.Response.Redirect(redirect, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LogStart logs the Application Start Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static void LogStart()
        {
            var log = new LogInfo
            {
                BypassBuffering = true,
                LogTypeKey = EventLogController.EventLogType.APPLICATION_START.ToString(),
            };
            LogController.Instance.AddLog(log);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LogEnd logs the Application Start Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static void LogEnd()
        {
            try
            {
                ApplicationShutdownReason shutdownReason = HostingEnvironment.ShutdownReason;
                string shutdownDetail;
                switch (shutdownReason)
                {
                    case ApplicationShutdownReason.BinDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the Bin folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the App_Browsers folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.ChangeInGlobalAsax:
                        shutdownDetail = "The AppDomain shut down because of a change to Global.asax.";
                        break;
                    case ApplicationShutdownReason.ChangeInSecurityPolicyFile:
                        shutdownDetail = "The AppDomain shut down because of a change in the code access security policy file.";
                        break;
                    case ApplicationShutdownReason.CodeDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the App_Code folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.ConfigurationChange:
                        shutdownDetail = "The AppDomain shut down because of a change to the application level configuration.";
                        break;
                    case ApplicationShutdownReason.HostingEnvironment:
                        shutdownDetail = "The AppDomain shut down because of the hosting environment.";
                        break;
                    case ApplicationShutdownReason.HttpRuntimeClose:
                        shutdownDetail = "The AppDomain shut down because of a call to Close.";
                        break;
                    case ApplicationShutdownReason.IdleTimeout:
                        shutdownDetail = "The AppDomain shut down because of the maximum allowed idle time limit.";
                        break;
                    case ApplicationShutdownReason.InitializationError:
                        shutdownDetail = "The AppDomain shut down because of an AppDomain initialization error.";
                        break;
                    case ApplicationShutdownReason.MaxRecompilationsReached:
                        shutdownDetail = "The AppDomain shut down because of the maximum number of dynamic recompiles of resources limit.";
                        break;
                    case ApplicationShutdownReason.PhysicalApplicationPathChanged:
                        shutdownDetail = "The AppDomain shut down because of a change to the physical path for the application.";
                        break;
                    case ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename:
                        shutdownDetail = "The AppDomain shut down because of a change to the App_GlobalResources folder or files contained in it.";
                        break;
                    case ApplicationShutdownReason.UnloadAppDomainCalled:
                        shutdownDetail = "The AppDomain shut down because of a call to UnloadAppDomain.";
                        break;
                    default:
                        shutdownDetail = "Shutdown reason: " + shutdownReason;
                        break;
                }

                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.APPLICATION_SHUTTING_DOWN.ToString(),
                };
                log.AddProperty("Shutdown Details", shutdownDetail);
                LogController.Instance.AddLog(log);

                // enhanced shutdown logging
                var runtime = typeof(HttpRuntime).InvokeMember(
                    "_theRuntime",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                    null, null, null) as HttpRuntime;

                if (runtime == null)
                {
                    Logger.InfoFormat("Application shutting down. Reason: {0}", shutdownDetail);
                }
                else
                {
                    var shutDownMessage = runtime.GetType().InvokeMember(
                        "_shutDownMessage",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                        null, runtime, null) as string;

                    var shutDownStack = runtime.GetType().InvokeMember(
                        "_shutDownStack",
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                        null, runtime, null) as string;

                    Logger.Info("Application shutting down. Reason: " + shutdownDetail
                                + Environment.NewLine + "ASP.NET Shutdown Info: " + shutDownMessage
                                + Environment.NewLine + shutDownStack);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            if (Globals.Status != Globals.UpgradeStatus.Install)
            {
                // purge log buffer
                LoggingProvider.Instance().PurgeLogBuffer();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Tests whether this request should be processed in an HttpModule.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool ProcessHttpModule(HttpRequest request, bool allowUnknownExtensions, bool checkOmitFromRewriteProcessing)
        {
            var toLowerLocalPath = request.Url.LocalPath.ToLowerInvariant();

            if (toLowerLocalPath.EndsWith("webresource.axd")
                    || toLowerLocalPath.EndsWith("scriptresource.axd")
                    || toLowerLocalPath.EndsWith("captcha.aspx")
                    || toLowerLocalPath.Contains("upgradewizard.aspx")
                    || toLowerLocalPath.Contains("installwizard.aspx")
                    || toLowerLocalPath.EndsWith("install.aspx"))
            {
                return false;
            }

            if (allowUnknownExtensions == false
                    && toLowerLocalPath.EndsWith(".aspx") == false
                    && toLowerLocalPath.EndsWith(".asmx") == false
                    && toLowerLocalPath.EndsWith(".ashx") == false
                    && toLowerLocalPath.EndsWith(".svc") == false)
            {
                return false;
            }

            return !checkOmitFromRewriteProcessing || !RewriterUtils.OmitFromRewriteProcessing(request.Url.LocalPath);
        }

        public static void RunSchedule(HttpRequest request)
        {
            if (!IsUpgradeOrInstallRequest(request))
            {
                try
                {
                    if (SchedulingProvider.SchedulerMode == SchedulerMode.REQUEST_METHOD && SchedulingProvider.ReadyForPoll)
                    {
                        Logger.Trace("Running Schedule " + SchedulingProvider.SchedulerMode);
                        var scheduler = SchedulingProvider.Instance();
                        var requestScheduleThread = new Thread(scheduler.ExecuteTasks) { IsBackground = true };
                        requestScheduleThread.Start();
                        SchedulingProvider.ScheduleLastPolled = DateTime.Now;
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.LogException(exc);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StartScheduler starts the Scheduler.
        /// </summary>
        /// <param name="resetAppStartElapseTime">Whether reset app start elapse time before running schedule tasks.</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void StartScheduler(bool resetAppStartElapseTime = false)
        {
            if (resetAppStartElapseTime)
            {
                Globals.ResetAppStartElapseTime();
            }

            var scheduler = SchedulingProvider.Instance();
            scheduler.RunEventSchedule(EventName.APPLICATION_START);

            // instantiate APPLICATION_START scheduled jobs
            if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
            {
                Logger.Trace("Running Schedule " + SchedulingProvider.SchedulerMode);
                var newThread = new Thread(scheduler.Start)
                {
                    IsBackground = true,
                    Name = "Scheduler Thread",
                };
                newThread.Start();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// StopScheduler stops the Scheduler.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void StopScheduler()
        {
            // stop scheduled jobs
            SchedulingProvider.Instance().Halt("Stopped by Application_End");
        }

        private static string CheckVersion(HttpApplication app)
        {
            HttpServerUtility Server = app.Server;

            bool autoUpgrade = Config.GetSetting("AutoUpgrade") == null || bool.Parse(Config.GetSetting("AutoUpgrade"));
            bool useWizard = Config.GetSetting("UseInstallWizard") == null || bool.Parse(Config.GetSetting("UseInstallWizard"));

            // Determine the Upgrade status and redirect as neccessary to InstallWizard.aspx
            string retValue = Null.NullString;
            switch (Globals.Status)
            {
                case Globals.UpgradeStatus.Install:
                    if (autoUpgrade || useWizard)
                    {
                        retValue = useWizard ? "~/Install/InstallWizard.aspx" : "~/Install/Install.aspx?mode=install";
                    }
                    else
                    {
                        CreateUnderConstructionPage(Server);
                        retValue = "~/Install/UnderConstruction.htm";
                        Logger.Info("UnderConstruction page was shown because application needs to be installed, and both the AutoUpgrade and UseWizard AppSettings in web.config are false. Use /install/install.aspx?mode=install to install application. ");
                    }

                    break;
                case Globals.UpgradeStatus.Upgrade:
                    if (autoUpgrade || useWizard)
                    {
                        retValue = useWizard ? "~/Install/UpgradeWizard.aspx" : "~/Install/Install.aspx?mode=upgrade";
                    }
                    else
                    {
                        CreateUnderConstructionPage(Server);
                        retValue = "~/Install/UnderConstruction.htm";
                        Logger.Info("UnderConstruction page was shown because application needs to be upgraded, and both the AutoUpgrade and UseInstallWizard AppSettings in web.config are false. Use /install/install.aspx?mode=upgrade to upgrade application. ");
                    }

                    break;
                case Globals.UpgradeStatus.Error:
                    // here we need to check if the application is already installed
                    // or is in pre-install state.
                    // see http://support.dotnetnuke.com/project/DNN/2/item/26053 for scenarios
                    bool isInstalled = Globals.IsInstalled();

                    if (!isInstalled && (useWizard || autoUpgrade))
                    {
                        // app has never been installed, and either Wizard or Autoupgrade is configured
                        CreateUnderConstructionPage(Server);
                        retValue = "~/Install/UnderConstruction.htm";
                        Logger.Error("UnderConstruction page was shown because we cannot ascertain the application was ever installed, and there is no working database connection. Check database connectivity before continuing. ");
                    }
                    else
                    {
                        // 500 Error - Redirect to ErrorPage
                        if (HttpContext.Current != null)
                        {
                            if (!isInstalled)
                            {
                                Logger.Error("The connection to the database has failed, the application is not installed yet, and both AutoUpgrade and UseInstallWizard are not set in web.config, a 500 error page will be shown to visitors");
                            }
                            else
                            {
                                Logger.Error("The connection to the database has failed, however, the application is already completely installed, a 500 error page will be shown to visitors");
                            }

                            string url = "~/ErrorPage.aspx?status=500&error=Site Unavailable&error2=Connection To The Database Failed";
                            HttpContext.Current.Response.Clear();
                            HttpContext.Current.Server.Transfer(url);
                        }
                    }

                    break;
            }

            return retValue;
        }

        private static void CreateUnderConstructionPage(HttpServerUtility server)
        {
            // create an UnderConstruction page if it does not exist already
            if (!File.Exists(server.MapPath("~/Install/UnderConstruction.htm")))
            {
                if (File.Exists(server.MapPath("~/Install/UnderConstruction.template.htm")))
                {
                    File.Copy(server.MapPath("~/Install/UnderConstruction.template.htm"), server.MapPath("~/Install/UnderConstruction.htm"));
                }
            }
        }

        private static Version GetDatabaseEngineVersion()
        {
            return DataProvider.Instance().GetDatabaseEngineVersion();
        }

        private static Version GetNETFrameworkVersion()
        {
            // Obtain the .NET Framework version, 9.4.0 and later requires .NET 4.7.2 or later
            var version = Environment.Version.ToString(2);

            // If unknown version return as is.
            if (version != "4.0")
            {
                return new Version(version);
            }

            // Otherwise utilize release DWORD from registry to determine version
            // Reference List: https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/versions-and-dependencies
            var release = GetDotNet4ReleaseNumberFromRegistry();
            if (release >= 528040)
            {
                version = "4.8";
            }
            else
            {
                version = "4.7.2";
            }

            return new Version(version);
        }

        private static int GetDotNet4ReleaseNumberFromRegistry()
        {
            try
            {
                using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                    .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                {
                    if (ndpKey?.GetValue("Release") != null)
                    {
                        return (int)ndpKey.GetValue("Release");
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return -1;
        }

        private static string InitializeApp(HttpApplication app, ref bool initialized)
        {
            var request = app.Request;
            var redirect = Null.NullString;

            Logger.Trace("Request " + request.Url.LocalPath);

            // Don't process some of the AppStart methods if we are installing
            if (!IsUpgradeOrInstallRequest(app.Request))
            {
                // Check whether the current App Version is the same as the DB Version
                redirect = CheckVersion(app);
                if (string.IsNullOrEmpty(redirect) && !InstallBlocker.Instance.IsInstallInProgress())
                {
                    Logger.Info("Application Initializing");

                    // Set globals
                    Globals.IISAppName = request.ServerVariables["APPL_MD_PATH"];
                    Globals.OperatingSystemVersion = Environment.OSVersion.Version;
                    Globals.NETFrameworkVersion = GetNETFrameworkVersion();
                    Globals.DatabaseEngineVersion = GetDatabaseEngineVersion();

                    // Try and Upgrade to Current Framewok
                    Upgrade.TryUpgradeNETFramework();
                    Upgrade.CheckFipsCompilanceAssemblies();

                    // Log Server information
                    ServerController.UpdateServerActivity(new ServerInfo());

                    // Start Scheduler
                    StartScheduler();

                    // Log Application Start
                    LogStart();

                    // Process any messages in the EventQueue for the Application_Start event
                    EventQueueController.ProcessMessages("Application_Start");

                    ServicesRoutingManager.RegisterServiceRoutes();

                    ModuleInjectionManager.RegisterInjectionFilters();

                    ConnectionsManager.Instance.RegisterConnections();

                    // Set Flag so we can determine the first Page Request after Application Start
                    app.Context.Items.Add("FirstRequest", true);

                    Logger.Info("Application Initialized");

                    initialized = true;
                }
            }
            else
            {
                // NET Framework version is neeed by Upgrade
                Globals.NETFrameworkVersion = GetNETFrameworkVersion();
                Globals.IISAppName = request.ServerVariables["APPL_MD_PATH"];
                Globals.OperatingSystemVersion = Environment.OSVersion.Version;
            }

            return redirect;
        }

        private static bool IsUpgradeOrInstallRequest(HttpRequest request)
        {
            var url = request.Url.LocalPath.ToLowerInvariant();

            return url.EndsWith("/install.aspx")
                || url.Contains("/upgradewizard.aspx")
                || url.Contains("/installwizard.aspx");
        }
    }
}
