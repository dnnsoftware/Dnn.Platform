// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Application
{
    using System;
    using System.IO;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Upgrade;

    /// <inheritdoc />
    public class ApplicationStatusInfo : IApplicationStatusInfo
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApplicationStatusInfo));

        private readonly IApplicationInfo applicationInfo;

        private UpgradeStatus status = UpgradeStatus.Unknown;
        private string applicationMapPath;

        /// <summary>Initializes a new instance of the <see cref="ApplicationStatusInfo"/> class.</summary>
        /// <param name="applicationInfo">The application info.</param>
        /// <remarks>
        /// This constructor is designed to be used with Dependency Injection.
        /// </remarks>
        public ApplicationStatusInfo(IApplicationInfo applicationInfo)
        {
            this.applicationInfo = applicationInfo;
        }

        /// <inheritdoc />
        public UpgradeStatus Status
        {
            get
            {
                if (this.status != UpgradeStatus.Unknown && this.status != UpgradeStatus.Error)
                {
                    return this.status;
                }

                Logger.Trace("Getting application status");
                var tempStatus = UpgradeStatus.None;

                // first call GetProviderPath - this insures that the Database is Initialised correctly
                // and also generates the appropriate error message if it cannot be initialised correctly
                string strMessage = DataProvider.Instance().GetProviderPath();

                // get current database version from DB
                if (!strMessage.StartsWith("ERROR:"))
                {
                    try
                    {
                        this.DatabaseVersion = DataProvider.Instance().GetVersion();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        strMessage = "ERROR:" + ex.Message;
                    }
                }

                if (strMessage.StartsWith("ERROR"))
                {
                    if (this.IsInstalled())
                    {
                        // Errors connecting to the database after an initial installation should be treated as errors.
                        tempStatus = UpgradeStatus.Error;
                    }
                    else
                    {
                        // An error that occurs before the database has been installed should be treated as a new install
                        tempStatus = UpgradeStatus.Install;
                    }
                }
                else if (this.DatabaseVersion == null)
                {
                    // No Db Version so Install
                    tempStatus = UpgradeStatus.Install;
                }
                else
                {
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (version.Major > this.DatabaseVersion.Major)
                    {
                        // Upgrade Required (Major Version Upgrade)
                        tempStatus = UpgradeStatus.Upgrade;
                    }
                    else if (version.Major == this.DatabaseVersion.Major && version.Minor > this.DatabaseVersion.Minor)
                    {
                        // Upgrade Required (Minor Version Upgrade)
                        tempStatus = UpgradeStatus.Upgrade;
                    }
                    else if (version.Major == this.DatabaseVersion.Major && version.Minor == this.DatabaseVersion.Minor &&
                             version.Build > this.DatabaseVersion.Build)
                    {
                        // Upgrade Required (Build Version Upgrade)
                        tempStatus = UpgradeStatus.Upgrade;
                    }
                    else if (version.Major == this.DatabaseVersion.Major && version.Minor == this.DatabaseVersion.Minor &&
                             version.Build == this.DatabaseVersion.Build && this.IncrementalVersionExists(version))
                    {
                        // Upgrade Required (Build Version Upgrade)
                        tempStatus = UpgradeStatus.Upgrade;
                    }
                }

                this.status = tempStatus;

                Logger.Trace($"result of getting providerpath: {strMessage}");
                Logger.Trace("Application status is " + this.status);

                return this.status;
            }
        }

        /// <inheritdoc />
        public Version DatabaseVersion { get; private set; }

        /// <inheritdoc />
        public string ApplicationMapPath { get => this.applicationMapPath ??= GetCurrentDomainDirectory(); }

        /// <inheritdoc />
        public bool IsInstalled()
        {
            const int c_PassingScore = 4;
            int installationdatefactor = Convert.ToInt32(HasInstallationDate() ? 1 : 0);
            int dataproviderfactor = Convert.ToInt32(HasDataProviderLogFiles() ? 3 : 0);
            int htmlmodulefactor = Convert.ToInt32(this.ModuleDirectoryExists("html") ? 2 : 0);
            int portaldirectoryfactor = Convert.ToInt32(this.HasNonDefaultPortalDirectory() ? 2 : 0);
            int localexecutionfactor = Convert.ToInt32(HttpContext.Current.Request.IsLocal ? c_PassingScore - 1 : 0);

            // This calculation ensures that you have a more than one item that indicates you have already installed DNN.
            // While it is possible that you might not have an installation date or that you have deleted log files
            // it is unlikely that you have removed every trace of an installation and yet still have a working install
            bool isInstalled = (!IsInstallationURL()) && ((installationdatefactor + dataproviderfactor + htmlmodulefactor + portaldirectoryfactor + localexecutionfactor) >= c_PassingScore);

            // we need to tighten this check. We now are enforcing the existence of the InstallVersion value in web.config. If
            // this value exists, then DNN was previously installed, and we should never try to re-install it
            return isInstalled || HasInstallVersion();
        }

        /// <summary>Sets the status.</summary>
        /// <param name="status">The status.</param>
        public void SetStatus(UpgradeStatus status)
        {
            this.status = status;
        }

        /// <inheritdoc />
        public void UpdateDatabaseVersion(Version version)
        {
            // update the version
            DataProvider.Instance().UpdateDatabaseVersion(version.Major, version.Minor, version.Build, this.applicationInfo.Name);
            this.DatabaseVersion = version;
        }

        /// <inheritdoc />
        public void UpdateDatabaseVersionIncrement(Version version, int increment)
        {
            // update the version and increment
            DataProvider.Instance().UpdateDatabaseVersionIncrement(version.Major, version.Minor, version.Build, increment, DotNetNukeContext.Current.Application.Name);
            this.DatabaseVersion = version;
        }

        /// <inheritdoc />
        public bool IncrementalVersionExists(Version version)
        {
            Provider currentdataprovider = Config.GetDefaultProvider("data");
            string providerpath = currentdataprovider.Attributes["providerPath"];

            // If the provider path does not exist, then there can't be any log files
            if (!string.IsNullOrEmpty(providerpath))
            {
                providerpath = HttpRuntime.AppDomainAppPath + providerpath.Replace("~", string.Empty);
                if (Directory.Exists(providerpath))
                {
                    var incrementalcount = Directory.GetFiles(providerpath, Upgrade.GetStringVersion(version) + ".*." + Upgrade.DefaultProvider).Length;

                    if (incrementalcount > this.GetLastAppliedIteration(version))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public int GetLastAppliedIteration(Version version)
        {
            try
            {
                return DataProvider.Instance().GetLastAppliedIteration(version.Major, version.Minor, version.Build);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>Determines whether current request is for install.</summary>
        /// <returns><see langword="true"/> if current request is for install; otherwise, <see langword="false"/>.</returns>
        private static bool IsInstallationURL()
        {
            string requestURL = HttpContext.Current.Request.RawUrl.ToLowerInvariant().Replace("\\", "/");
            return requestURL.Contains("/install.aspx") || requestURL.Contains("/installwizard.aspx");
        }

        /// <summary>Determines whether has installation date.</summary>
        /// <returns><see langword="true"/> if has installation date; otherwise, <see langword="false"/>.</returns>
        private static bool HasInstallationDate()
        {
            return Config.GetSetting("InstallationDate") != null;
        }

        /// <summary>Determines whether has data provider log files.</summary>
        /// <returns><see langword="true"/> if has data provider log files; otherwise, <see langword="false"/>.</returns>
        private static bool HasDataProviderLogFiles()
        {
            Provider currentdataprovider = Config.GetDefaultProvider("data");
            string providerpath = currentdataprovider.Attributes["providerPath"];

            // If the provider path does not exist, then there can't be any log files
            if (!string.IsNullOrEmpty(providerpath))
            {
                providerpath = HttpContext.Current.Server.MapPath(providerpath);
                if (Directory.Exists(providerpath))
                {
                    return Directory.GetFiles(providerpath, "*.log.resources").Length > 0;
                }
            }

            return false;
        }

        /// <summary>Determines whether has InstallVersion set.</summary>
        /// <returns><see langword="true"/> if has installation date; otherwise, <see langword="false"/>.</returns>
        private static bool HasInstallVersion()
        {
            return Config.GetSetting("InstallVersion") != null;
        }

        /// <summary>Get the current domain directory.</summary>
        /// <returns>returns the domain directory.</returns>
        private static string GetCurrentDomainDirectory()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory.Replace("/", "\\");
            if (dir.Length > 3 && dir.EndsWith("\\"))
            {
                dir = dir.Substring(0, dir.Length - 1);
            }

            return dir;
        }

        /// <summary>Check whether the modules directory exists.</summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <returns><see langword="true"/> if the module directory exist, otherwise, <see langword="false"/>.</returns>
        private bool ModuleDirectoryExists(string moduleName)
        {
            string dir = $@"{this.ApplicationMapPath}\desktopmodules\{moduleName}";
            return Directory.Exists(dir);
        }

        /// <summary>Determines whether has portal directory except default portal directory in portal path.</summary>
        /// <returns><see langword="true"/> if has portal directory except default portal directory in portal path; otherwise, <see langword="false"/>.</returns>
        private bool HasNonDefaultPortalDirectory()
        {
            string dir = $@"{this.ApplicationMapPath}\portals";
            if (Directory.Exists(dir))
            {
                return Directory.GetDirectories(dir).Length > 1;
            }

            return false;
        }
    }
}
