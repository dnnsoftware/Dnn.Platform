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
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.Services.UserRequest;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;
    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The InstallWizard class provides the Installation Wizard for DotNetNuke.</summary>
    public partial class UpgradeWizard : PageBase
    {
        /// <summary>Client ID of the hidden input containing the Telerik anti-forgery token.</summary>
        protected static readonly string TelerikAntiForgeryTokenClientID = "telerikAntiForgeryToken";

        /// <summary>Client ID of the Telerik uninstall radio buttons.</summary>
        protected static readonly string TelerikUninstallOptionClientID = DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey;

        /// <summary>Form value when user selects Yes.</summary>
        protected static readonly string OptionYes = "Y";

        /// <summary>Form value when user selects No.</summary>
        protected static readonly string OptionNo = "N";

        /// <summary>The upgrade status filename.</summary>
        protected static readonly string StatusFilename = "upgradestat.log.resources.txt";

        private const string LocalesFile = "/Install/App_LocalResources/Locales.xml";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradeWizard));
        private static string localResourceFile = "~/Install/App_LocalResources/UpgradeWizard.aspx.resx";
        private static string culture;
        private static string[] supportedLanguages;
        private static IInstallationStep currentStep;
        private static bool upgradeRunning;
        private static int upgradeProgress;

        // steps shown in UI
        private static IInstallationStep upgradeDatabase = new InstallDatabaseStep();
        private static IInstallationStep upgradeExtensions = new InstallExtensionsStep();
        private static IInstallationStep iisVerification = new IISVerificationStep();

        // Ordered List of Steps (and weight in percentage) to be executed
        private static IDictionary<IInstallationStep, int> steps = new Dictionary<IInstallationStep, int>
            {
            // {new AddFcnModeStep(), 1},
                { iisVerification, 1 },
                { upgradeDatabase, 49 },
                { upgradeExtensions, 49 },
                { new InstallVersionStep(), 1 },
            };

        private readonly IApplicationStatusInfo applicationStatus;
        private readonly IHostSettings hostSettings;
        private readonly IApplicationInfo application;

        static UpgradeWizard()
        {
            IsAuthenticated = false;
        }

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        public UpgradeWizard()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UpgradeWizard"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="application">The application info.</param>
        public UpgradeWizard(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IApplicationInfo application)
            : base(portalController, appStatus, hostSettings)
        {
            this.applicationStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
        }

        /// <summary>Gets the application version.</summary>
        protected Version ApplicationVersion => this.application.Version;

        /// <summary>Gets the current version.</summary>
        protected Version CurrentVersion => this.application.CurrentVersion;

        private static string StatusFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", StatusFilename);

        private static bool IsAuthenticated { get; set; }

        /// <summary>
        /// Validates the user input.
        /// </summary>
        /// <param name="accountInfo">The account information.</param>
        /// <returns>A tuple with the result and a message.</returns>
        [WebMethod]
        public static Tuple<bool, string> ValidateInput(Dictionary<string, string> accountInfo)
        {
            string errorMsg;
            var result = VerifyHostUser(accountInfo, out errorMsg);

            return new Tuple<bool, string>(result, errorMsg);
        }

        /// <summary>Returns information to render the Security tab.</summary>
        /// <param name="accountInfo">Username and password to validate host user.</param>
        /// <returns>An instance of <see cref="SecurityTabResult"/>.</returns>
        [WebMethod]
        public static Tuple<bool, string, SecurityTabResult> GetSecurityTab(Dictionary<string, string> accountInfo)
        {
            if (!VerifyHostUser(accountInfo, out var errorMsg))
            {
                return Tuple.Create(false, errorMsg, default(SecurityTabResult));
            }

            var telerikUtils = CreateTelerikUtils();

            if (!telerikUtils.TelerikIsInstalled())
            {
                return Tuple.Create(
                    true,
                    default(string),
                    GetTelerikNotInstalledResult());
            }

            var version = telerikUtils.GetTelerikVersion();
            if (!telerikUtils.IsTelerikVersionVulnerable(version))
            {
                return Tuple.Create(
                    true,
                    default(string),
                    GetTelerikInstalledWithVersionNotKnownToBeVulnerable(version));
            }

            var assemblies = telerikUtils.GetAssembliesThatDependOnTelerik()
                .Select(a => Path.GetFileName(a));

            if (!assemblies.Any())
            {
                return Tuple.Create(
                    true,
                    default(string),
                    GetTelerikInstalledButNotUsedResult(version));
            }

            return Tuple.Create(
                true,
                default(string),
                GetTelerikInstalledAndUsedResult(assemblies, version));
        }

        /// <summary>
        /// Runs the upgrade.
        /// </summary>
        /// <param name="accountInfo">The account information.</param>
        [WebMethod]
        public static void RunUpgrade(Dictionary<string, string> accountInfo)
        {
            string errorMsg;
            var result = VerifyHostUser(accountInfo, out errorMsg);

            if (result == true)
            {
                if (!TelerikAntiForgeryTokenIsValid(accountInfo))
                {
                    throw new InvalidOperationException(LocalizeStringStatic("TelerikInvalidAntiForgeryToken"));
                }

                if (!accountInfo.ContainsKey(TelerikUninstallOptionClientID))
                {
                    throw new InvalidOperationException(LocalizeStringStatic("TelerikUninstallOptionMissing"));
                }

                var option = accountInfo[TelerikUninstallOptionClientID];
                SetHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey, option);

                upgradeRunning = false;
                LaunchUpgrade();

                // DNN-9355: reset the installer files check flag after each upgrade, to make sure the installer files removed.
                var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
                hostSettingsService.Update("InstallerFilesRemoved", "False", true);
            }
        }

        /// <summary>
        /// Gets the installation log.
        /// </summary>
        /// <param name="startRow">At which line to start obtaining log lines.</param>
        /// <returns>Log string from the provided line number forward.</returns>
        [WebMethod]
        public static object GetInstallationLog(int startRow)
        {
            var maxLines = 500;
            if (IsAuthenticated == false)
            {
                return string.Empty;
            }

            var logFile = "InstallerLog" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".resources";
            try
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
                var sb = new StringBuilder();
                var errorLogged = false;
                var lineCount = 0;

                // Important to return empty string to stop retries.
                if (startRow > lines.Count())
                {
                    return string.Empty;
                }

                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("[ERROR]"))
                    {
                        errorLogged = true;

                        // Only append if error is in current lines range.
                        if (i > startRow && lineCount < maxLines)
                        {
                            sb.Append(lines[i]).Append("<br />");
                            lineCount++;

                            // If we have reached the max lines, break out of loop.
                            if (lineCount >= maxLines)
                            {
                                break;
                            }
                        }
                    }
                }

                if (!errorLogged)
                {
                    return Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/UpgradeWizard.aspx.resx");
                }

                return sb.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Localizes a string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The localized value for the key.</returns>
        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, localResourceFile, culture);
        }

        /// <inheritdoc/>
        protected override void OnError(EventArgs e)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Upgrade.Upgrade.UpdateNewtonsoftVersion())
            {
                this.Response.Redirect(this.Request.RawUrl, true);
            }

            this.SslRequiredCheck();
            GetInstallerLocales(this.applicationStatus);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                this.Response.Redirect("Install.aspx", true);
            }

            base.OnLoad(e);
            this.LocalizePage();

            if (this.Request.RawUrl.EndsWith("?complete"))
            {
                this.CompleteUpgrade();
            }

            // Create Status Files
            if (!this.Page.IsPostBack)
            {
                if (!File.Exists(StatusFile))
                {
                    File.CreateText(StatusFile).Close();
                }

                Upgrade.Upgrade.RemoveInvalidAntiForgeryCookie();
            }
        }

        private static ITelerikUtils CreateTelerikUtils()
        {
            return Globals.GetCurrentServiceProvider().GetRequiredService<ITelerikUtils>();
        }

        private static SecurityTabResult GetTelerikNotInstalledResult()
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(),
                    CreateHiddenField(TelerikUninstallOptionClientID, OptionNo),
                    CreateHeading("TelerikNotInstalledHeading"),
                    CreateParagraph("TelerikNotInstalledInfo")),
            };
        }

        private static SecurityTabResult GetTelerikInstalledWithVersionNotKnownToBeVulnerable(Version version)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(),
                    CreateHiddenField(TelerikUninstallOptionClientID, DotNetNuke.Maintenance.Constants.TelerikUninstallNoValue),
                    CreateTelerikInstalledHeader(version),
                    CreateParagraph("TelerikVersionNotKnownToBeVulnerableInfo")),
            };
        }

        private static SecurityTabResult GetTelerikInstalledButNotUsedResult(Version version)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(),
                    CreateHiddenField(TelerikUninstallOptionClientID, DotNetNuke.Maintenance.Constants.TelerikUninstallYesValue),
                    CreateTelerikInstalledHeader(version),
                    CreateParagraph("TelerikInstalledBulletin"),
                    CreateParagraph("TelerikInstalledButNotUsedInfo"),
                    CreateParagraph("TelerikUninstallInfo")),
            };
        }

        private static SecurityTabResult GetTelerikInstalledAndUsedResult(
            IEnumerable<string> assemblies, Version version)
        {
            return new SecurityTabResult
            {
                CanProceed = true,
                View = RenderControls(
                    CreateTelerikAntiForgeryTokenField(),
                    CreateHiddenField(TelerikUninstallOptionClientID, DotNetNuke.Maintenance.Constants.TelerikUninstallYesValue),
                    CreateTelerikInstalledHeader(version),
                    CreateParagraph("TelerikInstalledBulletin"),
                    CreateParagraph("TelerikInstalledAndUsedInfo"),
                    CreateTable(assemblies, maxRows: 3, maxColumns: 4),
                    new Literal { Text = "<br />" },
                    CreateParagraph("TelerikInstalledAndUsedWarning")),
            };
        }

        private static Control CreateTelerikAntiForgeryTokenField() =>
            CreateHiddenField(TelerikAntiForgeryTokenClientID, CreateTelerikAntiForgeryToken());

        private static Control CreateTelerikInstalledHeader(Version version)
        {
            return CreateBundle(
                CreateHeading("TelerikInstalledHeading"),
                CreateTelerikInstalledDetectedParagraph(version));
        }

        private static Control CreateTelerikInstalledDetectedParagraph(Version version)
        {
            return new HtmlGenericControl("p")
            {
                Controls =
                {
                    new Label { Text = LocalizeStringStatic("TelerikInstalledDetected") },
                    new Literal { Text = " " },
                    new Label { Text = version.ToString(), CssClass = "telerikVersion" },
                },
            };
        }

        private static Control CreateBundle(params Control[] controls)
        {
            var bundle = new PlaceHolder();

            foreach (var control in controls)
            {
                bundle.Controls.Add(control);
            }

            return bundle;
        }

        private static Control CreateHeading(string localizationKey) => CreateLabel("h4", localizationKey);

        private static Control CreateParagraph(string localizationKey) => CreateLabel("p", localizationKey);

        private static Control CreateLabel(string tag, string localizationKey)
        {
            var control = new HtmlGenericControl(tag);

            control.Controls.Add(new Label
            {
                Text = LocalizeStringStatic(localizationKey),
            });

            return control;
        }

        private static Control CreateHiddenField(string id, string value)
        {
            return new HiddenField
            {
                ID = id,
                Value = value,
            };
        }

        private static Table CreateTable(IEnumerable<string> items, int maxRows, int maxColumns)
        {
            var capacity = maxRows * maxColumns;

            var row = new TableRow();
            var list = StartNewColumn(row);

            foreach (var extension in items.Take(capacity - 1))
            {
                list.Items.Add(new ListItem(extension));

                if (list.Items.Count == maxRows)
                {
                    list = StartNewColumn(row);
                }
            }

            if (capacity < items.Count())
            {
                list.Items.Add(new ListItem("..."));
            }

            foreach (TableCell cell in row.Cells)
            {
                cell.Width = Unit.Percentage(100.0 / row.Cells.Count);
            }

            var table = new Table
            {
                Rows = { row },
                Width = Unit.Percentage(100.0),
            };

            return table;
        }

        private static ListControl StartNewColumn(TableRow row)
        {
            var list = new BulletedList();

            var cell = new TableCell
            {
                Controls = { list },
                VerticalAlign = VerticalAlign.Top,
            };

            row.Cells.Add(cell);

            return list;
        }

        private static string RenderControls(params Control[] controls)
        {
            return RenderControl(CreateBundle(controls));
        }

        private static string RenderControl(Control control)
        {
            using (var textWriter = new StringWriter())
            using (var htmlWriter = new HtmlTextWriter(textWriter))
            {
                control.RenderControl(htmlWriter);
                return textWriter.ToString();
            }
        }

        private static string CreateTelerikAntiForgeryToken()
        {
            var secret = GetHostSetting("GUID");
            var salt = new Random().Next(int.MaxValue);
            var token = new { secret, salt };
            var json = JsonConvert.SerializeObject(token);

            return Encrypt(json);
        }

        private static bool TelerikAntiForgeryTokenIsValid(Dictionary<string, string> accountInfo)
        {
            if (!accountInfo.ContainsKey(TelerikAntiForgeryTokenClientID))
            {
                return false; // token missing somehow.
            }

            try
            {
                var encrypted = accountInfo[TelerikAntiForgeryTokenClientID];
                var json = Decrypt(encrypted);
                dynamic token = JsonConvert.DeserializeObject(json);
                var expected = GetHostSetting("GUID");
                if (token.secret != expected)
                {
                    return false; // token mismatch.
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false; // malformed token.
            }

            return true;
        }

        private static string Encrypt(string token)
        {
            return CryptographyProvider.Instance().EncryptString(token, GetHostSetting("GUID"));
        }

        private static string Decrypt(string token)
        {
            return CryptographyProvider.Instance().DecryptString(token, GetHostSetting("GUID"));
        }

        private static string GetHostSetting(string key)
        {
            return Globals.GetCurrentServiceProvider()
                .GetRequiredService<IHostSettingsService>()
                .GetSettingsDictionary()[key];
        }

        private static void SetHostSetting(string key, string value, bool isSecure = false)
        {
            var setting = new ConfigurationSetting
            {
                IsSecure = isSecure,
                Key = key,
                Value = value,
            };

            Globals.GetCurrentServiceProvider()
                .GetRequiredService<IHostSettingsService>()
                .Update(setting);
        }

        private static void GetInstallerLocales(IApplicationStatusInfo applicationStatus)
        {
            var filePath = applicationStatus.ApplicationMapPath + LocalesFile.Replace("/", "\\");

            if (File.Exists(filePath))
            {
                var doc = new XPathDocument(filePath);
                var languages = doc.CreateNavigator().Select("root/language");

                if (languages.Count > 0)
                {
                    supportedLanguages = new string[languages.Count];
                    var i = 0;
                    foreach (XPathNavigator nav in languages)
                    {
                        if (nav.NodeType != XPathNodeType.Comment)
                        {
                            supportedLanguages.SetValue(nav.GetAttribute("key", string.Empty), i);
                        }

                        i++;
                    }
                }
                else
                {
                    supportedLanguages = new string[1];
                    supportedLanguages.SetValue("en-US", 0);
                }
            }
            else
            {
                supportedLanguages = new string[1];
                supportedLanguages.SetValue("en-US", 0);
            }
        }

        private static string LocalizeStringStatic(string key)
        {
            return Localization.GetString(key, localResourceFile, culture);
        }

        private static void LaunchUpgrade()
        {
            // Get current Script time-out
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            // Set Script timeout to MAX value
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

            if (culture != null)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }

            // bail out early if upgrade is in progress
            if (upgradeRunning)
            {
                return;
            }

            var percentForEachStep = 100 / steps.Count;
            var useGenericPercent = false;
            var totalPercent = steps.Sum(step => step.Value);
            if (totalPercent != 100)
            {
                useGenericPercent = true;
            }

            upgradeRunning = true;
            upgradeProgress = 0;

            // Output the current time for the user
            CurrentStepActivity(string.Concat(
                Localization.GetString("UpgradeStarted", localResourceFile),
                ":",
                DateTime.Now.ToString()));

            foreach (var step in steps)
            {
                currentStep = step.Key;

                try
                {
                    currentStep.Activity += CurrentStepActivity;
                    currentStep.Execute();
                }
                catch (Exception ex)
                {
                    CurrentStepActivity(Localization.GetString("ErrorInStep", localResourceFile) + ": " + ex.Message);
                    upgradeRunning = false;
                    return;
                }

                switch (currentStep.Status)
                {
                    case StepStatus.AppRestart:
                        upgradeRunning = false;
                        HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                        break;
                    default:
                        if (currentStep.Status != StepStatus.Done)
                        {
                            CurrentStepActivity(string.Format(
                                Localization.GetString("ErrorInStep", localResourceFile),
                                currentStep.Errors.Count > 0 ? string.Join(",", currentStep.Errors.ToArray()) : currentStep.Details));
                            upgradeRunning = false;
                            return;
                        }

                        break;
                }

                if (useGenericPercent)
                {
                    upgradeProgress += percentForEachStep;
                }
                else
                {
                    upgradeProgress += step.Value;
                }
            }

            currentStep = null;
            upgradeProgress = 100;

            Globals.GetCurrentServiceProvider().GetService<IDamUninstaller>().Execute();

            CurrentStepActivity(Localization.GetString("UpgradeDone", localResourceFile));

            // indicate we are done
            upgradeRunning = false;

            // restore Script timeout
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
        }

        private static void CurrentStepActivity(string status)
        {
            var percentage = (currentStep == null) ? upgradeProgress : upgradeProgress + (currentStep.Percentage / steps.Count);
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
                if (this.applicationStatus.IncrementalVersionExists(this.CurrentVersion))
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
            if (string.IsNullOrEmpty(this.PageLocale.Value) && string.IsNullOrEmpty(culture))
            {
                cultureCode = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(supportedLanguages);
            }
            else if (string.IsNullOrEmpty(this.PageLocale.Value) && !string.IsNullOrEmpty(culture))
            {
                cultureCode = culture;
            }
            else
            {
                cultureCode = this.PageLocale.Value;
            }

            this.PageLocale.Value = cultureCode;
            culture = cultureCode;

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

            Config.Touch(this.applicationStatus);
            this.Response.Redirect("../Default.aspx", true);
        }

        private void SslRequiredCheck()
        {
            if (this.hostSettings.UpgradeForceSsl && !this.Request.IsSecureConnection)
            {
                var sslDomain = this.hostSettings.SslDomain;
                if (string.IsNullOrEmpty(sslDomain))
                {
                    sslDomain = this.Request.Url.Host;
                }
                else if (sslDomain.Contains("://"))
                {
                    sslDomain = sslDomain.Substring(sslDomain.IndexOf("://") + 3);
                }

                var sslUrl = $"https://{sslDomain}{this.Request.RawUrl}";

                this.Response.Redirect(sslUrl, true);
            }
        }
    }
}
