// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Install
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Xml.XPath;

    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.Services.UserRequest;

    using Globals = DotNetNuke.Common.Globals;
    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallWizard class provides the Installation Wizard for DotNetNuke.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class UpgradeWizard : PageBase
    {
        protected static readonly string StatusFilename = "upgradestat.log.resources.txt";
        protected static new string LocalResourceFile = "~/Install/App_LocalResources/UpgradeWizard.aspx.resx";
        private const string LocalesFile = "/Install/App_LocalResources/Locales.xml";
        private static string _culture;
        private static string[] _supportedLanguages;

        private static IInstallationStep _currentStep;
        private static bool _upgradeRunning;
        private static int _upgradeProgress;

        // steps shown in UI
        private static IInstallationStep upgradeDatabase = new InstallDatabaseStep();
        private static IInstallationStep upgradeExtensions = new InstallExtensionsStep();
        private static IInstallationStep iisVerification = new IISVerificationStep();

        // Ordered List of Steps (and weight in percentage) to be executed
        private static IDictionary<IInstallationStep, int> _steps = new Dictionary<IInstallationStep, int>
            {
            // {new AddFcnModeStep(), 1},
                { iisVerification, 1 },
                { upgradeDatabase, 49 },
                { upgradeExtensions, 49 },
                { new InstallVersionStep(), 1 },
            };

        static UpgradeWizard()
        {
            IsAuthenticated = false;
        }

        protected Version ApplicationVersion
        {
            get
            {
                return DotNetNukeContext.Current.Application.Version;
            }
        }

        protected Version CurrentVersion
        {
            get
            {
                return DotNetNukeContext.Current.Application.CurrentVersion;
            }
        }

        protected bool NeedAcceptTerms
        {
            get { return File.Exists(Path.Combine(Globals.ApplicationMapPath, "Licenses\\Dnn_Corp_License.pdf")); }
        }

        private static string StatusFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", StatusFilename);
            }
        }

        private static bool IsAuthenticated { get; set; }

        [System.Web.Services.WebMethod]
        public static Tuple<bool, string> ValidateInput(Dictionary<string, string> accountInfo)
        {
            string errorMsg;
            var result = VerifyHostUser(accountInfo, out errorMsg);

            return new Tuple<bool, string>(result, errorMsg);
        }

        [System.Web.Services.WebMethod]
        public static void RunUpgrade(Dictionary<string, string> accountInfo)
        {
            string errorMsg;
            var result = VerifyHostUser(accountInfo, out errorMsg);

            if (result == true)
            {
                _upgradeRunning = false;
                LaunchUpgrade();

                // DNN-8833: Must run this after all other upgrade steps are done; sequence is important.
                HostController.Instance.Update("DnnImprovementProgram", accountInfo["dnnImprovementProgram"], false);

                // DNN-9355: reset the installer files check flag after each upgrade, to make sure the installer files removed.
                HostController.Instance.Update("InstallerFilesRemoved", "False", true);
            }
        }

        [System.Web.Services.WebMethod]
        public static object GetInstallationLog(int startRow)
        {
            if (IsAuthenticated == false)
            {
                return string.Empty;
            }

            var data = string.Empty;
            string logFile = "InstallerLog" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".resources";
            try
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
                var errorLogged = false;
                if (lines.Length > startRow)
                {
                    var count = lines.Length - startRow > 500 ? 500 : lines.Length - startRow;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (var i = startRow; i < startRow + count; i++)
                    {
                        if (lines[i].Contains("[ERROR]"))
                        {
                            sb.Append(lines[i]);
                            sb.Append("<br/>");
                            errorLogged = true;
                        }
                    }

                    data = sb.ToString();
                }

                if (errorLogged == false)
                {
                    Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/InstallWizard.aspx.resx");
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return data;
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile, _culture);
        }

        protected override void OnError(EventArgs e)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the Page is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Upgrade.Upgrade.UpdateNewtonsoftVersion())
            {
                this.Response.Redirect(this.Request.RawUrl, true);
            }

            this.SslRequiredCheck();
            GetInstallerLocales();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the Page loads.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                this.Response.Redirect("Install.aspx", true);
            }

            base.OnLoad(e);

            this.pnlAcceptTerms.Visible = this.NeedAcceptTerms;
            this.LocalizePage();

            if (this.Request.RawUrl.EndsWith("?complete"))
            {
                this.CompleteUpgrade();
            }

            // Create Status Files
            if (!this.Page.IsPostBack)
            {
                // Reset the accept terms flag
                HostController.Instance.Update("AcceptDnnTerms", "N");
                if (!File.Exists(StatusFile))
                {
                    File.CreateText(StatusFile).Close();
                }

                Upgrade.Upgrade.RemoveInvalidAntiForgeryCookie();
            }
        }

        private static void GetInstallerLocales()
        {
            var filePath = Globals.ApplicationMapPath + LocalesFile.Replace("/", "\\");

            if (File.Exists(filePath))
            {
                var doc = new XPathDocument(filePath);
                var languages = doc.CreateNavigator().Select("root/language");

                if (languages.Count > 0)
                {
                    _supportedLanguages = new string[languages.Count];
                    var i = 0;
                    foreach (XPathNavigator nav in languages)
                    {
                        if (nav.NodeType != XPathNodeType.Comment)
                        {
                            _supportedLanguages.SetValue(nav.GetAttribute("key", string.Empty), i);
                        }

                        i++;
                    }
                }
                else
                {
                    _supportedLanguages = new string[1];
                    _supportedLanguages.SetValue("en-US", 0);
                }
            }
            else
            {
                _supportedLanguages = new string[1];
                _supportedLanguages.SetValue("en-US", 0);
            }
        }

        private static string LocalizeStringStatic(string key)
        {
            return Localization.GetString(key, LocalResourceFile, _culture);
        }

        private static void LaunchUpgrade()
        {
            // Get current Script time-out
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            // Set Script timeout to MAX value
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

            if (_culture != null)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(_culture);
            }

            // bail out early if upgrade is in progress
            if (_upgradeRunning)
            {
                return;
            }

            var percentForEachStep = 100 / _steps.Count;
            var useGenericPercent = false;
            var totalPercent = _steps.Sum(step => step.Value);
            if (totalPercent != 100)
            {
                useGenericPercent = true;
            }

            _upgradeRunning = true;
            _upgradeProgress = 0;

            // Output the current time for the user
            CurrentStepActivity(string.Concat(
                Localization.GetString("UpgradeStarted", LocalResourceFile),
                ":", DateTime.Now.ToString()));

            foreach (var step in _steps)
            {
                _currentStep = step.Key;

                try
                {
                    _currentStep.Activity += CurrentStepActivity;
                    _currentStep.Execute();
                }
                catch (Exception ex)
                {
                    CurrentStepActivity(Localization.GetString("ErrorInStep", LocalResourceFile) + ": " + ex.Message);
                    _upgradeRunning = false;
                    return;
                }

                switch (_currentStep.Status)
                {
                    case StepStatus.AppRestart:
                        _upgradeRunning = false;
                        HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                        break;
                    default:
                        if (_currentStep.Status != StepStatus.Done)
                        {
                            CurrentStepActivity(string.Format(
                                Localization.GetString("ErrorInStep", LocalResourceFile),
                                _currentStep.Errors.Count > 0 ? string.Join(",", _currentStep.Errors.ToArray()) : _currentStep.Details));
                            _upgradeRunning = false;
                            return;
                        }

                        break;
                }

                if (useGenericPercent)
                {
                    _upgradeProgress += percentForEachStep;
                }
                else
                {
                    _upgradeProgress += step.Value;
                }
            }

            _currentStep = null;
            _upgradeProgress = 100;
            CurrentStepActivity(Localization.GetString("UpgradeDone", LocalResourceFile));

            // indicate we are done
            _upgradeRunning = false;

            // restore Script timeout
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
        }

        private static void CurrentStepActivity(string status)
        {
            var percentage = (_currentStep == null) ? _upgradeProgress : _upgradeProgress + (_currentStep.Percentage / _steps.Count);
            var obj = new
            {
                progress = percentage,
                details = status,
                check0 = upgradeDatabase.Status.ToString() + (upgradeDatabase.Errors.Count == 0 ? string.Empty : " Errors " + upgradeDatabase.Errors.Count),
                check1 = upgradeExtensions.Status.ToString() + (upgradeExtensions.Errors.Count == 0 ? string.Empty : " Errors " + upgradeExtensions.Errors.Count),
            };

            try
            {
                if (!File.Exists(StatusFile))
                {
                    File.CreateText(StatusFile);
                }

                using (var sw = new StreamWriter(StatusFile, true))
                {
                    sw.WriteLine(obj.ToJson());
                    sw.Close();
                }
            }
            catch (Exception)
            {
                // TODO - do something
            }
        }

        private static bool VerifyHostUser(Dictionary<string, string> accountInfo, out string errorMsg)
        {
            var result = true;
            errorMsg = string.Empty;

            UserLoginStatus loginStatus = UserLoginStatus.LOGIN_FAILURE;
            var userRequestIpAddressController = UserRequestIPAddressController.Instance;
            var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(HttpContext.Current.Request));
            UserInfo hostUser = UserController.ValidateUser(-1, accountInfo["username"], accountInfo["password"], "DNN", string.Empty, string.Empty, ipAddress, ref loginStatus);

            if (loginStatus == UserLoginStatus.LOGIN_FAILURE || !hostUser.IsSuperUser)
            {
                result = false;
                errorMsg = LocalizeStringStatic("InvalidCredentials");
            }
            else
            {
                IsAuthenticated = true;
            }

            if (result && (!accountInfo.ContainsKey("acceptTerms") || accountInfo["acceptTerms"] != "Y"))
            {
                result = false;
                errorMsg = LocalizeStringStatic("AcceptTerms.Required");
            }

            return result;
        }

        private void LocalizePage()
        {
            this.SetBrowserLanguage();
            this.Page.Title = this.LocalizeString("Title");
            if (Globals.FormatVersion(this.ApplicationVersion) == Globals.FormatVersion(this.CurrentVersion))
            {
                this.versionLabel.Visible = false;
                this.currentVersionLabel.Visible = false;
                this.versionsMatch.Text = this.LocalizeString("VersionsMatch");
                if (Globals.IncrementalVersionExists(this.CurrentVersion))
                {
                    this.versionsMatch.Text = this.LocalizeString("VersionsMatchButIncrementalExists");
                }
            }
            else
            {
                this.versionLabel.Text = string.Format(this.LocalizeString("Version"), Globals.FormatVersion(this.ApplicationVersion));
                this.currentVersionLabel.Text = string.Format(this.LocalizeString("CurrentVersion"), Globals.FormatVersion(this.CurrentVersion));
            }
        }

        private void SetBrowserLanguage()
        {
            string cultureCode;
            if (string.IsNullOrEmpty(this.PageLocale.Value) && string.IsNullOrEmpty(_culture))
            {
                cultureCode = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(_supportedLanguages);
            }
            else if (string.IsNullOrEmpty(this.PageLocale.Value) && !string.IsNullOrEmpty(_culture))
            {
                cultureCode = _culture;
            }
            else
            {
                cultureCode = this.PageLocale.Value;
            }

            this.PageLocale.Value = cultureCode;
            _culture = cultureCode;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
        }

        private void CompleteUpgrade()
        {
            // Delete the status file.
            try
            {
                File.Delete(StatusFile);
            }
            catch (Exception)
            {
                // Do nothing
            }

            // remove installwizard files added back by upgrade package
            Upgrade.Upgrade.DeleteInstallerFiles();

            Config.Touch();
            this.Response.Redirect("../Default.aspx", true);
        }

        private void SslRequiredCheck()
        {
            if (Entities.Host.Host.UpgradeForceSsl && !this.Request.IsSecureConnection)
            {
                var sslDomain = Entities.Host.Host.SslDomain;
                if (string.IsNullOrEmpty(sslDomain))
                {
                    sslDomain = this.Request.Url.Host;
                }
                else if (sslDomain.Contains("://"))
                {
                    sslDomain = sslDomain.Substring(sslDomain.IndexOf("://") + 3);
                }

                var sslUrl = string.Format(
                    "https://{0}{1}",
                    sslDomain, this.Request.RawUrl);

                this.Response.Redirect(sslUrl, true);
            }
        }
    }
}
