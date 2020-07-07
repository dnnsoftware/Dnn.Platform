// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Install
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Security;
    using System.Web.UI.WebControls;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization.Internal;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Services.Upgrade.InternalController.Steps;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
    using DotNetNuke.Services.Upgrade.Internals.Steps;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// The InstallWizard class provides the Installation Wizard for DotNetNuke.
    /// </summary>
    public partial class InstallWizard : PageBase, IClientAPICallbackEventHandler
    {
        /// <summary>
        /// The install status file name.
        /// </summary>
        protected static readonly string StatusFilename = "installstat.log.resources.txt";

        private const string LocalesFile = "/Install/App_LocalResources/Locales.xml";

        // steps shown in UI
        private static readonly IInstallationStep FilePermissionCheck = new FilePermissionCheckStep();
        private static readonly IInstallationStep InstallDatabaseStep = new InstallDatabaseStep();
        private static readonly IInstallationStep InstallExtensionsStep = new InstallExtensionsStep();
        private static readonly IInstallationStep InstallSiteStep = new InstallSiteStep();
        private static readonly IInstallationStep InstallSuperUserStep = new InstallSuperUserStep();
        private static readonly IInstallationStep ActivateLicenseStep = new ActivateLicenseStep();

        // Hide Licensing Step for Community Edition
        private static readonly bool IsProOrEnterprise = File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Professional.dll")) || File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Enterprise.dll"));
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallWizard));

        // Ordered List of Steps (and weight in percentage) to be executed
        private static readonly IDictionary<IInstallationStep, int> Steps = new Dictionary<IInstallationStep, int>
        {
            { FilePermissionCheck, 10 },
            { new IISVerificationStep(), 5 },
            { InstallDatabaseStep, 20 },
            { InstallExtensionsStep, 25 },
            { new InitializeHostSettingsStep(), 5 },
            { new UpdateLanguagePackStep(), 5 },
            { InstallSiteStep, 20 },
            { InstallSuperUserStep, 5 },
            { new AddFcnModeStep(), 1 },
            { ActivateLicenseStep, 4 },
            { new InstallVersionStep(), 1 },
        };

        private static string localResourceFile = "~/Install/App_LocalResources/InstallWizard.aspx.resx";

        private static string[] supportedLanguages;
        private static ConnectionConfig connectionConfig;
        private static string connectionResult;
        private static InstallConfig installConfig;
        private static string culture;
        private static IInstallationStep currentStep;
        private static bool installerRunning;
        private static int installerProgress;
        private static object @lock = new object();

        private readonly DataProvider dataProvider = DataProvider.Instance();
        private Version dataBaseVersion;
        private XmlDocument installTemplate;

        /// <summary>
        /// Gets the current applicatoin version.
        /// </summary>
        protected Version ApplicationVersion
        {
            get
            {
                return DotNetNukeContext.Current.Application.Version;
            }
        }

        /// <summary>
        /// Gets the database version.
        /// </summary>
        protected Version DatabaseVersion
        {
            get
            {
                return this.dataBaseVersion ?? (this.dataBaseVersion = DataProvider.Instance().GetVersion());
            }
        }

        /// <summary>
        /// Gets the base version from the install template.
        /// </summary>
        protected Version BaseVersion
        {
            get
            {
                return Upgrade.GetInstallVersion(this.InstallTemplate);
            }
        }

        /// <summary>
        /// Gets the install template.
        /// </summary>
        protected XmlDocument InstallTemplate
        {
            get
            {
                if (this.installTemplate == null)
                {
                    this.installTemplate = new XmlDocument { XmlResolver = null };
                    Upgrade.GetInstallTemplate(this.installTemplate);
                }

                return this.installTemplate;
            }
        }

        /// <summary>
        /// Gets a value indicating whether localization is supported.
        /// </summary>
        protected bool SupportLocalization
        {
            get { return installConfig.SupportLocalization; }
        }

        /// <summary>
        /// Gets a value indicating whether the user needs to accept the license terms.
        /// </summary>
        protected bool NeedAcceptTerms
        {
            get { return File.Exists(Path.Combine(Globals.ApplicationMapPath, "Licenses\\Dnn_Corp_License.pdf")); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the permissions are valid.
        /// </summary>
        protected bool PermissionsValid
        {
            get
            {
                bool valid = false;
                if (this.ViewState["PermissionsValid"] != null)
                {
                    valid = Convert.ToBoolean(this.ViewState["PermissionsValid"]);
                }

                return valid;
            }

            set
            {
                this.ViewState["PermissionsValid"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the portal id.
        /// </summary>
        protected int PortalId
        {
            get
            {
                int portalId = Null.NullInteger;
                if (this.ViewState["PortalId"] != null)
                {
                    portalId = Convert.ToInt32(this.ViewState["PortalId"]);
                }

                return portalId;
            }

            set
            {
                this.ViewState["PortalId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the versions.
        /// </summary>
        protected string Versions
        {
            get
            {
                string versions = Null.NullString;
                if (this.ViewState["Versions"] != null)
                {
                    versions = Convert.ToString(this.ViewState["Versions"]);
                }

                return versions;
            }

            set
            {
                this.ViewState["Versions"] = value;
            }
        }

        private static string StatusFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", StatusFilename);
            }
        }

        /// <summary>
        /// Runs the intaller.
        /// </summary>
        [System.Web.Services.WebMethod]
        public static void RunInstall()
        {
            installerRunning = false;
            LaunchAutoInstall();
        }

        /// <summary>
        /// Gets the installatoin log.
        /// </summary>
        /// <param name="startRow">At which line to start obtaining log lines.</param>
        /// <returns>Log string from the provided line number forward.</returns>
        [System.Web.Services.WebMethod]
        public static object GetInstallationLog(int startRow)
        {
            var data = string.Empty;
            var logFile = InstallController.Instance.InstallerLogName;
            try
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
                var errorLogged = false;
                if (lines.Length > startRow)
                {
                    var count = Math.Min(lines.Length - startRow, 500);
                    var sb = new System.Text.StringBuilder();
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
                    Localization.Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/InstallWizard.aspx.resx");
                }
            }
            catch (Exception)
            {
            }

            return data;
        }

        /// <summary>
        /// Validates the information provided for the install.
        /// </summary>
        /// <param name="installInfo">The information about the installation.</param>
        /// <returns>A value indicating if the input is valid and an error message if not.</returns>
        [System.Web.Services.WebMethod]
        public static Tuple<bool, string> ValidateInput(Dictionary<string, string> installInfo)
        {
            var result = true;
            var errorMsg = string.Empty;

            // Check Required Fields
            if (!installInfo.ContainsKey("acceptTerms") || installInfo["acceptTerms"] != "Y" ||
                installInfo["username"] == string.Empty || installInfo["password"] == string.Empty || installInfo["confirmPassword"] == string.Empty
                 || installInfo["websiteName"] == string.Empty || installInfo["email"] == string.Empty)
            {
                result = false;
                errorMsg = LocalizeStringStatic("InputErrorMissingRequiredFields");
            }
            else if (installInfo["password"] != installInfo["confirmPassword"])
            {
                result = false;
                errorMsg = LocalizeStringStatic("PasswordMismatch");
            }

            if (result)
            {
                UpdateInstallConfig(installInfo);
                new SynchConnectionStringStep().Execute();
            }

            return new Tuple<bool, string>(result, errorMsg);
        }

        /// <summary>
        /// Validates file access permissions.
        /// </summary>
        /// <returns>Returns if the permissions are valid or an error message.</returns>
        [System.Web.Services.WebMethod]
        public static Tuple<bool, string> ValidatePermissions()
        {
            var permissionsValid = true;
            var errorMessage = string.Empty;

            var verifiers = new List<FileSystemPermissionVerifier>
                                {
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~"), 3),
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~/App_Data"), 3),
                                };

            var failedList = verifiers.Where(v => !v.VerifyAll()).ToArray();
            if (failedList.Any())
            {
                permissionsValid = false;
            }

            if (!permissionsValid)
            {
                errorMessage = string.Format(LocalizeStringStatic("FileAndFolderPermissionCheckFailed"), string.Join("; ", from v in verifiers select v.BasePath));
            }

            return new Tuple<bool, string>(permissionsValid, errorMessage);
        }

        /// <summary>
        /// Validates the password for password requirements.
        /// </summary>
        /// <param name="password">The entered password.</param>
        /// <returns>A value indicating whether the password is valid.</returns>
        [System.Web.Services.WebMethod]
        public static bool ValidatePassword(string password)
        {
            // Check Length
            var result = !(password.Length < Membership.MinRequiredPasswordLength);

            // Check against regex
            if (!string.IsNullOrEmpty(Membership.PasswordStrengthRegularExpression) && !Regex.IsMatch(password, Membership.PasswordStrengthRegularExpression))
            {
                result = false;
            }

            // Check non-alphaNumeric
            var nonAlnumCount = password.Where((t, i) => !char.IsLetterOrDigit(password, i)).Count();
            if (nonAlnumCount < Membership.MinRequiredNonAlphanumericCharacters)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks if the application can connect to the database on load.
        /// </summary>
        /// <returns>A value indicating whether the application can connect to the database.</returns>
        [System.Web.Services.WebMethod]
        public static bool VerifyDatabaseConnectionOnLoad()
        {
            return CheckDatabaseConnection();
        }

        /// <summary>
        /// Verifies that the application can connect to the database with the provided settings.
        /// </summary>
        /// <param name="installInfo">The install information.</param>
        /// <returns>Returns if the connection is valid and an error message if not.</returns>
        [System.Web.Services.WebMethod]
        public static Tuple<bool, string> VerifyDatabaseConnection(Dictionary<string, string> installInfo)
        {
            var connectionConfig = new ConnectionConfig();

            // Database Config
            if (installInfo["databaseSetup"] == "standard")
            {
                connectionConfig = InstallWizard.connectionConfig;
            }
            else
            {
                connectionConfig.Server = installInfo["databaseServerName"];
                connectionConfig.Qualifier = installInfo["databaseObjectQualifier"];
                connectionConfig.Integrated = installInfo["databaseSecurity"] == "integrated";
                connectionConfig.User = string.IsNullOrEmpty(installInfo["databaseUsername"]) ? null : installInfo["databaseUsername"];
                connectionConfig.Password = string.IsNullOrEmpty(installInfo["databasePassword"]) ? null : installInfo["databasePassword"];
                connectionConfig.RunAsDbowner = installInfo["databaseRunAsOwner"] == "on";

                if (installInfo["databaseType"] == "express")
                {
                    connectionConfig.File = installInfo["databaseFilename"];
                    connectionConfig.Database = string.Empty;
                }
                else
                {
                    connectionConfig.Database = installInfo["databaseName"];
                    connectionConfig.File = string.Empty;
                }
            }

            var result = CheckDatabaseConnection(connectionConfig);
            var validConnection = !result.StartsWith("ERROR:");
            if (validConnection)
            {
                UpdateDatabaseInstallConfig(installInfo);
                InstallWizard.connectionConfig = connectionConfig;
                installConfig.Connection = connectionConfig;
            }

            return new Tuple<bool, string>(validConnection, result);
        }

        /// <summary>
        /// Indicate if the Installer is running.
        /// </summary>
        /// <returns>True or False.</returns>
        /// <remarks>Checks the local static variable or the existence of status file.</remarks>
        [System.Web.Services.WebMethod]
        public static bool IsInstallerRunning()
        {
            bool isRunning;

            if (installerRunning || InstallBlocker.Instance.IsInstallInProgress())
            {
                isRunning = true;
            }
            else if (File.Exists(StatusFile))
            {
                var file = new FileInfo(StatusFile);
                isRunning = file.Length > 0;
            }
            else
            {
                isRunning = false;
            }

            return isRunning;
        }

        /// <summary>
        /// Raises a client api callback event.
        /// </summary>
        /// <param name="eventArgument">The event arguments.</param>
        /// <returns>A string representing the result of the action.</returns>
        public string RaiseClientAPICallbackEvent(string eventArgument)
        {
            return this.ProcessAction(eventArgument);
        }

        /// <summary>
        /// Runs when there is a ClientCallback.
        /// </summary>
        /// <param name="someAction">The action to perform.</param>
        /// <returns>A string representing the result of the action.</returns>
        public string ProcessAction(string someAction)
        {
            // First check that we are not being targetted by an AJAX HttpPost, so get the current DB version
            string strProviderPath = this.dataProvider.GetProviderPath();
            string nextVersion = this.GetNextScriptVersion(strProviderPath, this.DatabaseVersion);
            if (someAction != nextVersion)
            {
                Exceptions.Exceptions.LogException(new Exception("Attempt made to run a Database Script - Possible Attack"));
                return "Error: Possible Attack";
            }

            if (someAction == this.GetBaseDatabaseVersion())
            {
                string result = this.InstallDatabase();
                if (result == "Done")
                {
                    // Complete Installation
                    Upgrade.UpgradeApplication();
                }

                return result;
            }
            else if (someAction.Contains("."))
            {
                // Upgrade Database
                string result = this.InstallVersion(someAction);
                if (result == "Done")
                {
                    ArrayList arrVersions = Upgrade.GetUpgradeScripts(strProviderPath, this.BaseVersion);
                    string strErrorMessage = Null.NullString;
                    for (int i = 0; i <= arrVersions.Count - 1; i++)
                    {
                        string strVersion = Path.GetFileNameWithoutExtension(Convert.ToString(arrVersions[i]));
                        var version = new Version(strVersion);

                        strErrorMessage += Upgrade.UpgradeApplication(strProviderPath, version, false);

                        // delete files which are no longer used
                        strErrorMessage += Upgrade.DeleteFiles(strProviderPath, version, false);

                        // execute config file updates
                        strErrorMessage += Upgrade.UpdateConfig(strProviderPath, version, false);
                    }

                    // Complete Installation
                    Upgrade.UpgradeApplication();
                    if (!string.IsNullOrEmpty(strErrorMessage))
                    {
                        result = "ERROR: " + strErrorMessage;
                    }
                }

                return result;
            }
            else
            {
                return "Done";
            }
        }

        /// <summary>
        /// GetBaseDataBaseVersion gets the Base Database Version as a string.
        /// </summary>
        /// <returns>The database version.</returns>
        protected string GetBaseDatabaseVersion()
        {
            return Upgrade.GetStringVersion(this.BaseVersion);
        }

        /// <summary>
        /// LocalizeString is a helper function for localizing strings.
        /// </summary>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized string.</returns>
        protected string LocalizeString(string key)
        {
            return Localization.Localization.GetString(key, localResourceFile, culture);
        }

        /// <inheritdoc/>
        protected override void OnError(EventArgs e)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // if previous config deleted create new empty one
            string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
            if (!File.Exists(installConfig))
            {
                File.Copy(installConfig + ".resources", installConfig);
            }

            GetInstallerLocales();
            if (!this.Page.IsPostBack || InstallWizard.installConfig == null)
            {
                InstallWizard.installConfig = InstallController.Instance.GetInstallConfig();
                connectionConfig = InstallWizard.installConfig.Connection;
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!this.SupportLocalization)
            {
                this.languageFlags.Visible = false;
                this.languagesRow.Attributes.Add("style", "display: none");
            }

            this.passwordContainer.CssClass = "password-strength-container";
            this.txtPassword.CssClass = "password-strength";

            var options = new DnnPaswordStrengthOptions();
            var optionsAsJsonString = Json.Serialize(options);
            var script = string.Format(
                "dnn.initializePasswordStrength('.{0}', {1});{2}",
                "password-strength",
                optionsAsJsonString,
                Environment.NewLine);

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "PasswordStrength", script, true);

            this.txtConfirmPassword.CssClass = "password-confirm";
            var confirmPasswordOptions = new DnnConfirmPasswordOptions()
            {
                FirstElementSelector = "#" + this.passwordContainer.ClientID + " input[type=password]",
                SecondElementSelector = ".password-confirm",
                ContainerSelector = ".dnnFormPassword",
                UnmatchedCssClass = "unmatched",
                MatchedCssClass = "matched",
            };

            var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
            var confirmScript = string.Format("dnn.initializePasswordComparer({0});{1}", confirmOptionsAsJsonString, Environment.NewLine);

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "ConfirmPassword", confirmScript, true);
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                this.Response.Redirect("Install.aspx", true);
            }

            this.SetBrowserLanguage();
            this.LocalizePage();

            base.OnLoad(e);
            this.visitSite.Click += VisitSiteClick;

            // Create Status Files
            if (!File.Exists(StatusFile))
            {
                File.CreateText(StatusFile).Close();
            }

            // Hide Licensing Step if no License Info is available
            this.LicenseActivation.Visible = IsProOrEnterprise && !string.IsNullOrEmpty(installConfig.License.AccountEmail) && !string.IsNullOrEmpty(installConfig.License.InvoiceNumber);
            this.pnlAcceptTerms.Visible = this.NeedAcceptTerms;

            if ((!IsProOrEnterprise) && this.templateList.FindItemByValue("Mobile Website.template") != null)
            {
                this.templateList.Items.Remove(this.templateList.FindItemByValue("Mobile Website.template"));
            }

            if (HttpContext.Current.Request.RawUrl.EndsWith("&initiateinstall"))
            {
                var synchConnectionString = new SynchConnectionStringStep();
                synchConnectionString.Execute();
                this.Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&initiateinstall", "&executeinstall"), true);
            }
            else if (HttpContext.Current.Request.RawUrl.EndsWith("&executeinstall"))
            {
                try
                {
                    if (HttpContext.Current.Request.QueryString["acceptterms"] != "true")
                    {
                        // Redirect back to first page if not accept terms.
                        this.Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&executeinstall", string.Empty), true);
                    }

                    installerRunning = true;
                    LaunchAutoInstall();
                }
                catch (Exception)
                {
                    // Redirect back to first page
                    this.Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&executeinstall", string.Empty), true);
                }
            }
            else if (!this.Page.IsPostBack)
            {
                if (installerRunning)
                {
                    LaunchAutoInstall();
                }
                else
                {
                    this.SetupDatabaseInfo();
                    this.BindLanguageList();
                    this.BindTemplates();

                    if (CheckDatabaseConnection())
                    {
                        this.Initialise();
                    }
                    else
                    {
                        // Install but connection string not configured to point at a valid SQL Server
                        UpdateMachineKey();
                    }

                    if (!Regex.IsMatch(this.Request.Url.Host, "^([a-zA-Z0-9.-]+)$"))
                    {
                        this.lblError.Visible = true;
                        this.lblError.Text = Localization.Localization.GetString("HostWarning", localResourceFile);
                    }

                    // ensure web.config is not read-only
                    var configPath = this.Server.MapPath("~/web.config");
                    try
                    {
                        var attributes = File.GetAttributes(configPath);

                        // file is read only
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            attributes = attributes & ~FileAttributes.ReadOnly;
                            File.SetAttributes(configPath, attributes);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.lblError.Visible = true;
                        this.lblError.Text = ex.ToString();
                        return;
                    }

                    // Adding ClientDependency Resources config to web.config
                    if (!ClientResourceManager.IsInstalled() && ValidatePermissions().Item1)
                    {
                        ClientResourceManager.AddConfiguration();
                        this.Response.Redirect(this.Request.RawUrl);

                        // TODO - this may cause infinite loop
                    }

                    // Ensure connection strings are in synch
                    var synchConnectionString = new SynchConnectionStringStep();
                    synchConnectionString.Execute();
                    if (synchConnectionString.Status == StepStatus.AppRestart)
                    {
                        this.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                    }

                    this.txtUsername.Text = installConfig.SuperUser.UserName;
                    this.txtEmail.Text = installConfig.SuperUser.Email;
                    if (installConfig.Portals.Count > 0)
                    {
                        this.txtWebsiteName.Text = installConfig.Portals[0].PortalName;

                        // TODO Language and Template
                    }

                    this.valEmailValid.ValidationExpression = Globals.glbEmailRegEx;
                }
            }
        }

        private static void LaunchAutoInstall()
        {
            if (Globals.Status == Globals.UpgradeStatus.None)
            {
                HttpContext.Current.Response.Redirect("~/");
                return;
            }

            // Get current Script time-out
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            // Set Script timeout to MAX value
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

            if (culture != null)
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }

            Install();

            // restore Script timeout
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
        }

        private static void Install()
        {
            // bail out early if we are already running
            if (installerRunning || InstallBlocker.Instance.IsInstallInProgress() || (Globals.Status != Globals.UpgradeStatus.Install))
            {
                return;
            }

            var percentForEachStep = 100 / Steps.Count;
            var useGenericPercent = false;
            var totalPercent = Steps.Sum(step => step.Value);
            if (totalPercent != 100)
            {
                useGenericPercent = true;
            }

            installerRunning = true;
            installerProgress = 0;
            foreach (var step in Steps)
            {
                currentStep = step.Key;

                if (currentStep.GetType().Name == "ActivateLicenseStep" && !IsProOrEnterprise)
                {
                    continue;
                }

                try
                {
                    currentStep.Activity += CurrentStepActivity;
                    currentStep.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("WIZARD ERROR:" + ex);
                    CurrentStepActivity("ERROR:" + ex.Message);
                    installerRunning = false;
                    return;
                }

                switch (currentStep.Status)
                {
                    case StepStatus.AppRestart:
                        installerRunning = false;
                        HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                        break;
                    default:
                        if (currentStep.Status != StepStatus.Done)
                        {
                            CurrentStepActivity(string.Format(
                                Localization.Localization.GetString("ErrorInStep", "~/Install/App_LocalResources/InstallWizard.aspx.resx"),
                                currentStep.Errors.Count > 0 ? string.Join(",", currentStep.Errors.ToArray()) : currentStep.Details));

                            installerRunning = false;
                            return;
                        }

                        break;
                }

                if (useGenericPercent)
                {
                    installerProgress += percentForEachStep;
                }
                else
                {
                    installerProgress += step.Value;
                }
            }

            currentStep = null;

            installerProgress = 100;
            CurrentStepActivity(Localization.Localization.GetString("InstallationDone", "~/Install/App_LocalResources/InstallWizard.aspx.resx"));

            // indicate we are done
            installerRunning = false;
        }

        private static void CurrentStepActivity(string status)
        {
            var percentage = (currentStep == null) ? installerProgress : installerProgress + (currentStep.Percentage / Steps.Count);
            var obj = new
            {
                progress = percentage,
                details = status,
                check0 = FilePermissionCheck.Status.ToString() + (FilePermissionCheck.Errors.Count == 0 ? string.Empty : " Errors " + FilePermissionCheck.Errors.Count),
                check1 = InstallDatabaseStep.Status.ToString() + (InstallDatabaseStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallDatabaseStep.Errors.Count),
                check2 = InstallExtensionsStep.Status.ToString() + (InstallExtensionsStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallExtensionsStep.Errors.Count),
                check3 = InstallSiteStep.Status.ToString() + (InstallSiteStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallSiteStep.Errors.Count),
                check4 = InstallSuperUserStep.Status.ToString() + (InstallSuperUserStep.Errors.Count == 0 ? string.Empty : " Errors " + InstallSuperUserStep.Errors.Count),
                check5 = ActivateLicenseStep.Status.ToString() + (ActivateLicenseStep.Errors.Count == 0 ? string.Empty : " Errors " + ActivateLicenseStep.Errors.Count),
            };

            try
            {
                lock (@lock)
                {
                    using (var sw = new StreamWriter(StatusFile, false))
                    {
                        sw.WriteLine(obj.ToJson());
                    }
                }
            }
            catch (Exception)
            {
                // TODO - do something
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
            return Localization.Localization.GetString(key, localResourceFile, culture);
        }

        private static bool CheckDatabaseConnection()
        {
            var success = false;
            connectionResult = CheckDatabaseConnection(connectionConfig);
            if (!connectionResult.StartsWith("ERROR:"))
            {
                success = true;
            }

            return success;
        }

        private static string CheckDatabaseConnection(ConnectionConfig connectionConfig)
        {
            connectionResult = InstallController.Instance.TestDatabaseConnection(connectionConfig);
            if (connectionResult.StartsWith("ERROR:"))
            {
                return connectionResult;
            }

            var connectionString = connectionResult;
            var details = Localization.Localization.GetString("IsAbleToPerformDatabaseActionsCheck", localResourceFile);
            if (!InstallController.Instance.IsAbleToPerformDatabaseActions(connectionString))
            {
                connectionResult = "ERROR: " + string.Format(Localization.Localization.GetString("IsAbleToPerformDatabaseActions", localResourceFile), details);
            }

            // database actions check-running sql 2008 or higher
            details = Localization.Localization.GetString("IsValidSqlServerVersionCheck", localResourceFile);
            if (!InstallController.Instance.IsValidSqlServerVersion(connectionString))
            {
                connectionResult = "ERROR: " + string.Format(Localization.Localization.GetString("IsValidSqlServerVersion", localResourceFile), details);
            }

            return connectionResult;
        }

        private static void UpdateMachineKey()
        {
            var installationDate = Config.GetSetting("InstallationDate");

            if (string.IsNullOrEmpty(installationDate))
            {
                string strError = Config.UpdateMachineKey();
                if (string.IsNullOrEmpty(strError))
                {
                    // send a new request to the application to initiate step 2
                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                }
                else
                {
                    // 403-3 Error - Redirect to ErrorPage
                    var strUrl = "~/ErrorPage.aspx?status=403_3&error=" + strError;
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer(strUrl);
                }
            }
        }

        /// <summary>
        /// Updates and synchronizes DotNetNuke.install.config with Web.config.
        /// </summary>
        /// <param name="installInfo">The information about this installation.</param>
        private static void UpdateInstallConfig(Dictionary<string, string> installInfo)
        {
            installConfig = new InstallConfig
            {
                SuperUser = new SuperUserConfig
                {
                    UserName = installInfo["username"],
                    Password = installInfo["password"],
                    Locale = culture,
                    Email = installInfo["email"],
                    FirstName = "SuperUser",
                    LastName = "Account",
                },
                InstallCulture = installInfo["language"],
                Settings = new List<HostSettingConfig>
                {
                    new HostSettingConfig
                    {
                        Name = "DnnImprovementProgram",
                        Value = installInfo["dnnImprovementProgram"],
                        IsSecure = false,
                    },
                },
            };

            // Website Portal Config
            var portalConfig = new PortalConfig()
            {
                PortalName = installInfo["websiteName"],
                TemplateFileName = installInfo["template"],
                IsChild = false,
            };
            installConfig.Portals = new List<PortalConfig>() { portalConfig };

            InstallController.Instance.SetInstallConfig(installConfig);
        }

        private static void UpdateDatabaseInstallConfig(Dictionary<string, string> installInfo)
        {
            // Database Config
            if (installInfo["databaseSetup"] == "advanced")
            {
                connectionConfig = new ConnectionConfig()
                {
                    Server = installInfo["databaseServerName"],
                    Qualifier = installInfo["databaseObjectQualifier"],
                    Integrated = installInfo["databaseSecurity"] == "integrated",
                    User = installInfo["databaseUsername"],
                    Password = installInfo["databasePassword"],
                    RunAsDbowner = installInfo["databaseRunAsOwner"] == "on",
                };

                if (installInfo["databaseType"] == "express")
                {
                    connectionConfig.File = installInfo["databaseFilename"];
                    connectionConfig.Database = string.Empty;
                }
                else
                {
                    connectionConfig.Database = installInfo["databaseName"];
                    connectionConfig.File = string.Empty;
                }
            }

            installConfig.Connection = connectionConfig;
            InstallController.Instance.SetInstallConfig(installConfig);
        }

        private static void VisitSiteClick(object sender, EventArgs eventArgs)
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

            // delete the initial install config -check readonly status first
            try
            {
                string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
                if (File.Exists(installConfig))
                {
                    // make sure file is not read-only
                    File.SetAttributes(installConfig, FileAttributes.Normal);
                    File.Delete(installConfig);
                }
            }
            catch (Exception)
            {
                // Do nothing
            }

            Config.Touch();
            HttpContext.Current.Response.Redirect("../Default.aspx");
        }

        private void SetBrowserLanguage()
        {
            string cultureCode;
            if (string.IsNullOrEmpty(this.PageLocale.Value) && string.IsNullOrEmpty(culture))
            {
                cultureCode = !string.IsNullOrEmpty(HttpContext.Current.Request.Params.Get("culture")) ? HttpContext.Current.Request.Params.Get("culture") : TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(supportedLanguages);
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

        private string GetNextScriptVersion(string strProviderPath, Version currentVersion)
        {
            var strNextVersion = "Done";

            if (currentVersion == null)
            {
                strNextVersion = this.GetBaseDatabaseVersion();
            }
            else
            {
                var strScriptVersion = Null.NullString;
                var arrScripts = Upgrade.GetUpgradeScripts(strProviderPath, currentVersion);

                if (arrScripts.Count > 0)
                {
                    // First Script is next script
                    strScriptVersion = Path.GetFileNameWithoutExtension(Convert.ToString(arrScripts[0]));
                }

                if (!string.IsNullOrEmpty(strScriptVersion))
                {
                    strNextVersion = strScriptVersion;
                }
            }

            return strNextVersion;
        }

        private void Initialise()
        {
            if (this.TestDataBaseInstalled())
            {
                // running current version, so redirect to site home page
                this.Response.Redirect("~/Default.aspx", true);
            }
            else
            {
                if (this.DatabaseVersion > new Version(0, 0, 0))
                {
                    // Upgrade
                    this.lblIntroDetail.Text = string.Format(this.LocalizeString("Upgrade"), Upgrade.GetStringVersion(this.DatabaseVersion));
                }
                else
                {
                    // Install
                    UpdateMachineKey();
                }
            }
        }

        private string InstallDatabase()
        {
            var strProviderPath = this.dataProvider.GetProviderPath();
            var strErrorMessage = strProviderPath.StartsWith("ERROR:")
                ? strProviderPath
                : Upgrade.InstallDatabase(this.BaseVersion, strProviderPath, this.InstallTemplate, false);
            if (string.IsNullOrEmpty(strErrorMessage))
            {
                // Get Next Version
                strErrorMessage = this.GetNextScriptVersion(strProviderPath, this.BaseVersion);
            }
            else if (!strErrorMessage.StartsWith("ERROR:"))
            {
                strErrorMessage = "ERROR: " + string.Format(this.LocalizeString("ScriptError"), Upgrade.GetLogFile(strProviderPath, this.BaseVersion));
            }

            return strErrorMessage;
        }

        private string InstallVersion(string strVersion)
        {
            var strErrorMessage = Null.NullString;
            var version = new Version(strVersion);
            var strScriptFile = Null.NullString;
            var strProviderPath = this.dataProvider.GetProviderPath();
            if (!strProviderPath.StartsWith("ERROR:"))
            {
                // Install Version
                strScriptFile = Upgrade.GetScriptFile(strProviderPath, version);
                strErrorMessage += Upgrade.UpgradeVersion(strScriptFile, false);
                this.Versions += "," + strVersion;
            }
            else
            {
                strErrorMessage = strProviderPath;
            }

            if (string.IsNullOrEmpty(strErrorMessage))
            {
                // Get Next Version
                strErrorMessage = this.GetNextScriptVersion(strProviderPath, version);
            }
            else
            {
                strErrorMessage = "ERROR: (see " + (Path.GetFileName(strScriptFile) ?? string.Empty)
                    .Replace("." + Upgrade.DefaultProvider, ".log") + " for more information)";
            }

            return strErrorMessage;
        }

        private void LocalizePage()
        {
            this.Page.Title = this.LocalizeString("PageTitle");
            this.lblIntroDetail.Text = this.LocalizeString("IntroDetail");
        }

        /// <summary>
        /// TestDataBaseInstalled checks whether the Database is the current version.
        /// </summary>
        /// <returns>A value indicating whether the database is the current version.</returns>
        private bool TestDataBaseInstalled()
        {
            var success = !(this.DatabaseVersion == null || this.DatabaseVersion.Major != this.ApplicationVersion.Major || this.DatabaseVersion.Minor != this.ApplicationVersion.Minor || this.DatabaseVersion.Build != this.ApplicationVersion.Build);
            return success;
        }

        private void SetupDatabaseInfo()
        {
            // Try to use connection information in DotNetNuke.Install.Config. If not found use from web.config
            connectionConfig = installConfig.Connection;
            if (connectionConfig == null || string.IsNullOrEmpty(connectionConfig.Server))
            {
                connectionConfig = InstallController.Instance.GetConnectionFromWebConfig();
            }

            if (connectionConfig != null)
            {
                this.txtDatabaseServerName.Text = connectionConfig.Server;

                // SQL Express Or SQL Server
                if (!string.IsNullOrEmpty(connectionConfig.File))
                {
                    this.txtDatabaseFilename.Text = connectionConfig.File;
                    this.databaseType.SelectedIndex = 0;
                }
                else
                {
                    this.txtDatabaseName.Text = connectionConfig.Database;
                    this.databaseType.SelectedIndex = 1;
                }

                // Integrated or Custom
                if (connectionConfig.Integrated)
                {
                    this.databaseSecurityType.SelectedIndex = 0;
                }
                else
                {
                    this.databaseSecurityType.SelectedIndex = 1;
                    this.txtDatabaseUsername.Text = connectionConfig.User;
                    this.txtDatabasePassword.Text = connectionConfig.Password;
                }

                // Owner or Not
                this.databaseRunAs.Checked = connectionConfig.RunAsDbowner;
            }
        }

        private void BindLanguageList()
        {
            try
            {
                var myResponseReader = UpdateService.GetLanguageList();

                // empty language list
                this.languageList.Items.Clear();

                // Loading into XML doc
                var xmlDoc = new XmlDocument { XmlResolver = null };
                xmlDoc.Load(myResponseReader);
                var languages = xmlDoc.SelectNodes("available/language");
                var packages = new List<PackageInfo>();

                if (languages != null)
                {
                    foreach (XmlNode language in languages)
                    {
                        string cultureCode = string.Empty;
                        string version = string.Empty;
                        foreach (XmlNode child in language.ChildNodes)
                        {
                            if (child.Name == "culturecode")
                            {
                                cultureCode = child.InnerText;
                            }

                            if (child.Name == "version")
                            {
                                version = child.InnerText;
                            }
                        }

                        if (!string.IsNullOrEmpty(cultureCode) && !string.IsNullOrEmpty(version) && version.Length == 6)
                        {
                            var myCIintl = new CultureInfo(cultureCode, true);
                            version = version.Insert(4, ".").Insert(2, ".");
                            var package = new PackageInfo { Name = "LanguagePack-" + myCIintl.Name, FriendlyName = myCIintl.NativeName };
                            package.Name = myCIintl.NativeName;
                            package.Description = cultureCode;
                            Version ver;
                            Version.TryParse(version, out ver);
                            package.Version = ver;

                            if (packages.Any(p => p.Name == package.Name))
                            {
                                var existPackage = packages.First(p => p.Name == package.Name);
                                if (package.Version > existPackage.Version)
                                {
                                    packages.Remove(existPackage);
                                    packages.Add(package);
                                }
                            }
                            else
                            {
                                packages.Add(package);
                            }
                        }
                    }
                }

                foreach (var package in packages)
                {
                    var li = new ListItem { Value = package.Description, Text = package.Name };
                    this.languageList.AddItem(li.Text, li.Value);
                    var lastItem = this.languageList.Items[this.languageList.Items.Count - 1];
                    if (DotNetNukeContext.Current.Application.Version.Major != package.Version.Major
                        || DotNetNukeContext.Current.Application.Version.Minor != package.Version.Minor
                        || DotNetNukeContext.Current.Application.Version.Build != package.Version.Build)
                    {
                        lastItem.Attributes.Add("onclick", "javascript:LegacyLangaugePack('" + package.Version + "');");
                    }
                }
            }
            catch (Exception)
            {
                // suppress for now - need to decide what to do when webservice is unreachable
                // throw;
            }
            finally
            {
                // ensure there is always an en-us
                if (this.languageList.FindItemByValue("en-US") == null)
                {
                    var myCIintl = new CultureInfo("en-US", true);
                    var li = new ListItem { Value = @"en-US", Text = myCIintl.NativeName };
                    this.languageList.AddItem(li.Text, li.Value);
                    var lastItem = this.languageList.Items[this.languageList.Items.Count - 1];
                    lastItem.Attributes.Add("onclick", "javascript:ClearLegacyLangaugePack();");
                }

                var item = this.languageList.FindItemByValue(culture);
                this.languageList.SelectedIndex = item != null ? this.languageList.Items.IndexOf(item) : this.languageList.Items.IndexOf(this.languageList.FindItemByValue("en-US"));
            }
        }

        private void BindTemplates()
        {
            var templates = PortalController.Instance.GetAvailablePortalTemplates();

            foreach (var template in templates)
            {
                this.templateList.AddItem(template.Name, Path.GetFileName(template.TemplateFilePath));
            }

            this.templateList.SelectedIndex = this.templateList.Items.IndexOf(this.templateList.FindItemByValue("Default Website.template"));
        }
    }
}
