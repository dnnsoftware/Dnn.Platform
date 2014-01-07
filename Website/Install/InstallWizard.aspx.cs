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
#region Usings

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
using DotNetNuke.Framework;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization.Internal;
using DotNetNuke.Services.Upgrade;
using DotNetNuke.Services.Upgrade.InternalController.Steps;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;
using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

using Telerik.Web.UI;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Services.Install
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The InstallWizard class provides the Installation Wizard for DotNetNuke
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	01/23/2007  Created
    ///
    ///     [vnguyen]   07/09/2012  Modified
    ///     [aprasad]
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class InstallWizard : PageBase, IClientAPICallbackEventHandler
    {
        #region Private Members
        // Hide Licensing Step for Community Edition
        private static readonly bool IsProOrEnterprise = (File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Professional.dll")) || File.Exists(HttpContext.Current.Server.MapPath("~\\bin\\DotNetNuke.Enterprise.dll")));
        
        private readonly DataProvider _dataProvider = DataProvider.Instance();
        private const string LocalesFile = "/Install/App_LocalResources/Locales.xml";
        protected static readonly string StatusFilename = "installstat.log.resources.txt";
        protected static new string LocalResourceFile = "~/Install/App_LocalResources/InstallWizard.aspx.resx";
        private Version _dataBaseVersion;
        private XmlDocument _installTemplate;
        private static string[] _supportedLanguages;
        
		private static ConnectionConfig _connectionConfig;
        private static string _connectionResult;
        private static InstallConfig _installConfig;
        private static string _culture;

        private static IInstallationStep _currentStep;
        private static bool _installerRunning;
        private static int _installerProgress;
        //private static bool _isValidConnection = false;
        //private static bool _isValidInput = false;
        
        #endregion

		#region Private Properties
        private static string StatusFile
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", StatusFilename);
            }
        }

		#endregion

		#region Protected Members

		protected Version ApplicationVersion
        {
            get
            {
                return DotNetNukeContext.Current.Application.Version;
            }
        }

        protected Version DatabaseVersion
        {
            get
            {
                return _dataBaseVersion ?? (_dataBaseVersion = DataProvider.Instance().GetVersion());
            }
        }

        protected Version BaseVersion
        {
            get
            {
                return Upgrade.Upgrade.GetInstallVersion(InstallTemplate);
            }
        }

        protected XmlDocument InstallTemplate
        {
            get
            {
                if (_installTemplate == null)
                {
                    _installTemplate = new XmlDocument();
                    Upgrade.Upgrade.GetInstallTemplate(_installTemplate);
                }
                return _installTemplate;
            }
        }

        protected bool PermissionsValid
        {
            get
            {
                bool valid = false;
                if (ViewState["PermissionsValid"] != null)
                {
                    valid = Convert.ToBoolean(ViewState["PermissionsValid"]);
                }
                return valid;
            }
            set
            {
                ViewState["PermissionsValid"] = value;
            }
        }

        protected int PortalId
        {
            get
            {
                int portalId = Null.NullInteger;
                if (ViewState["PortalId"] != null)
                {
                    portalId = Convert.ToInt32(ViewState["PortalId"]);
                }
                return portalId;
            }
            set
            {
                ViewState["PortalId"] = value;
            }
        }

        protected string Versions
        {
            get
            {
                string versions = Null.NullString;
                if (ViewState["Versions"] != null)
                {
                    versions = Convert.ToString(ViewState["Versions"]);
                }
                return versions;
            }
            set
            {
                ViewState["Versions"] = value;
            }
        }

        #endregion

        #region IClientAPICallbackEventHandler Members

        public string RaiseClientAPICallbackEvent(string eventArgument)
        {
            return ProcessAction(eventArgument);
        }

        #endregion

        #region Private Methods

        private void SetBrowserLanguage()
        {
            string cultureCode;
            if (string.IsNullOrEmpty(PageLocale.Value) && string.IsNullOrEmpty(_culture))
            {
                cultureCode = !string.IsNullOrEmpty(HttpContext.Current.Request.Params.Get("culture")) ? HttpContext.Current.Request.Params.Get("culture") : TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(_supportedLanguages);
            }
            else if (string.IsNullOrEmpty(PageLocale.Value) && !string.IsNullOrEmpty(_culture))
            {
                cultureCode = _culture;
            }
            else 
            {
                cultureCode = PageLocale.Value;
            }

            PageLocale.Value = cultureCode;
            _culture = cultureCode;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
        }

        private string GetNextScriptVersion(string strProviderPath, Version currentVersion)
        {
            var strNextVersion = "Done";

            if (currentVersion == null)
            {
                strNextVersion = GetBaseDatabaseVersion();
            }
            else
            {
                var strScriptVersion = Null.NullString;
                var arrScripts = Upgrade.Upgrade.GetUpgradeScripts(strProviderPath, currentVersion);

                if (arrScripts.Count > 0)
                {
                    //First Script is next script
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
            if (TestDataBaseInstalled())
            {
                //running current version, so redirect to site home page
                Response.Redirect("~/Default.aspx", true);
            }
            else
            {
                if (DatabaseVersion > new Version(0, 0, 0))
                {
                    //Upgrade
                    lblIntroDetail.Text = string.Format(LocalizeString("Upgrade"), Upgrade.Upgrade.GetStringVersion(DatabaseVersion));
                }
                else
                {
                    //Install
                    UpdateMachineKey();
                }
            }
        }
        
        private static void LaunchAutoInstall()
        {
            //Get current Script time-out
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            //Set Script timeout to MAX value
            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;

            if (_culture != null) Thread.CurrentThread.CurrentUICulture = new CultureInfo(_culture);

            Install();

            //restore Script timeout
            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
        }

        private static void Install()
        {
            //bail out early if we are already running
            if (_installerRunning)
                return;

            var percentForEachStep = 100 / _steps.Count;
            var useGenericPercent = false;
            var totalPercent = _steps.Sum(step => step.Value);
            if (totalPercent != 100) useGenericPercent = true;

            _installerRunning = true;
            _installerProgress = 0;
            foreach (var step in _steps)
            {
                _currentStep = step.Key;
                
                if (_currentStep.GetType().Name == "ActivateLicenseStep" && !IsProOrEnterprise) continue;

                try
                {
                    _currentStep.Activity += CurrentStepActivity;
                    _currentStep.Execute();
                }
                catch (Exception ex)
                {
                    CurrentStepActivity("ERROR:" + ex.Message);
                    _installerRunning = false;
                    return;
                }
                switch (_currentStep.Status)
                {
                    case StepStatus.AppRestart:
                        _installerRunning = false;
                        HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                        break;
                    default:
                        if (_currentStep.Status != StepStatus.Done)
                        {
                            CurrentStepActivity(string.Format(Localization.Localization.GetString("ErrorInStep", "~/Install/App_LocalResources/InstallWizard.aspx.resx")
                                                                                                  , _currentStep.Errors.Count > 0 ? string.Join(",", _currentStep.Errors.ToArray()) : _currentStep.Details));

							_installerRunning = false;
                            return;
                        }
                        break;
                }
                if (useGenericPercent)
                    _installerProgress += percentForEachStep;
                else
                    _installerProgress += step.Value;
            }

            _currentStep = null;

            _installerProgress = 100;
            CurrentStepActivity(Localization.Localization.GetString("InstallationDone", "~/Install/App_LocalResources/InstallWizard.aspx.resx"));

            //indicate we are done
            _installerRunning = false;
        }

        private string InstallDatabase()
        {
            string strErrorMessage;

            var strProviderPath = _dataProvider.GetProviderPath();
            if (!strProviderPath.StartsWith("ERROR:"))
            {
                //Install Base Version
                strErrorMessage = Upgrade.Upgrade.InstallDatabase(BaseVersion, strProviderPath, InstallTemplate, false);
            }
            else
            {
                strErrorMessage = strProviderPath;
            }
            if (string.IsNullOrEmpty(strErrorMessage))
            {
                //Get Next Version
                strErrorMessage = GetNextScriptVersion(strProviderPath, BaseVersion);
            }
            else if (!strErrorMessage.StartsWith("ERROR:"))
            {
                strErrorMessage = "ERROR: " + string.Format(LocalizeString("ScriptError"), Upgrade.Upgrade.GetLogFile(strProviderPath, BaseVersion));
            }
            return strErrorMessage;
        }

        private string InstallVersion(string strVersion)
        {
            var strErrorMessage = Null.NullString;
            var version = new Version(strVersion);
            var strScriptFile = Null.NullString;
            var strProviderPath = _dataProvider.GetProviderPath();
            if (!strProviderPath.StartsWith("ERROR:"))
            {
                //Install Version
                strScriptFile = Upgrade.Upgrade.GetScriptFile(strProviderPath, version);
                strErrorMessage += Upgrade.Upgrade.UpgradeVersion(strScriptFile, false);
                Versions += "," + strVersion;
            }
            else
            {
                strErrorMessage = strProviderPath;
            }
            if (string.IsNullOrEmpty(strErrorMessage))
            {
                //Get Next Version
                strErrorMessage = GetNextScriptVersion(strProviderPath, version);
            }
            else
            {
                strErrorMessage = "ERROR: (see " + Path.GetFileName(strScriptFile).Replace("." + Upgrade.Upgrade.DefaultProvider, ".log") + " for more information)";
            }
            return strErrorMessage;
        }

        private static void CurrentStepActivity(string status)
        {
            var percentage = (_currentStep == null) ? _installerProgress : _installerProgress + (_currentStep.Percentage / _steps.Count);
            var obj = new
            {
                progress = percentage,
                details = status,
                check0 = filePermissionCheck.Status.ToString() + (filePermissionCheck.Errors.Count == 0 ? "" : " Errors " + filePermissionCheck.Errors.Count),
                check1 = installDatabase.Status.ToString() + (installDatabase.Errors.Count == 0 ? "" : " Errors " + installDatabase.Errors.Count),
                check2 = installExtensions.Status.ToString() + (installExtensions.Errors.Count == 0 ? "" : " Errors " + installExtensions.Errors.Count),
                check3 = installSite.Status.ToString() + (installSite.Errors.Count == 0 ? "" : " Errors " + installSite.Errors.Count),
                check4 = createSuperUser.Status.ToString() + (createSuperUser.Errors.Count == 0 ? "" : " Errors " + createSuperUser.Errors.Count),
                check5 = activateLicense.Status.ToString() + (activateLicense.Errors.Count == 0 ? "" : " Errors " + activateLicense.Errors.Count)
            };

            try
            {
                if (!File.Exists(StatusFile)) File.CreateText(StatusFile);
                var sw = new StreamWriter(StatusFile, true);
                sw.WriteLine(obj.ToJson());
                sw.Close();
            }
            catch (Exception)
            {
                //TODO - do something                
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
                            _supportedLanguages.SetValue(nav.GetAttribute("key", ""), i);
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

        private void LocalizePage()
        {
            Page.Title = LocalizeString("PageTitle");
            lblIntroDetail.Text = LocalizeString("IntroDetail");      
        }

        private static string LocalizeStringStatic(string key)
        {
            return Localization.Localization.GetString(key, LocalResourceFile, _culture);
        }
        
        /// <summary>
        /// TestDataBaseInstalled checks whether the Database is the current version
        /// </summary>
        /// <returns></returns>
        private bool TestDataBaseInstalled()
        {
            var success = !(DatabaseVersion == null || DatabaseVersion.Major != ApplicationVersion.Major || DatabaseVersion.Minor != ApplicationVersion.Minor || DatabaseVersion.Build != ApplicationVersion.Build);
            return success;
        }
       
		private void SetupDatabaseInfo()
        {
            //Try to use connection information in DotNetNuke.Install.Config. If not found use from web.config
            _connectionConfig = _installConfig.Connection;
            if (_connectionConfig == null || string.IsNullOrEmpty(_connectionConfig.Server))
            {
                _connectionConfig = InstallController.Instance.GetConnectionFromWebConfig();
            }

            if (_connectionConfig != null)
            {
                txtDatabaseServerName.Text = _connectionConfig.Server;
                txtDatabaseObjectQualifier.Text = _connectionConfig.Qualifier;

                //SQL Express Or SQL Server
                if (!string.IsNullOrEmpty(_connectionConfig.File))
                {
                    txtDatabaseFilename.Text = _connectionConfig.File;
                    databaseType.SelectedIndex = 0;
                }
                else
                {
                    txtDatabaseName.Text = _connectionConfig.Database;
                    databaseType.SelectedIndex = 1;
                }

                //Integrated or Custom
                if (_connectionConfig.Integrated)
                {
                    databaseSecurityType.SelectedIndex = 0;
                }
                else
                {
                    databaseSecurityType.SelectedIndex = 1;
                    txtDatabaseUsername.Text = _connectionConfig.User;
                    txtDatabasePassword.Text = _connectionConfig.Password;
                }

                //Owner or Not
                databaseRunAs.Checked = _connectionConfig.RunAsDbowner;
            }
        }

        private static bool CheckDatabaseConnection()
        {
            var success = false;
            _connectionResult = CheckDatabaseConnection(_connectionConfig);
            if (!_connectionResult.StartsWith("ERROR:"))
                success = true;

            return success;
        }

		private static string CheckDatabaseConnection(ConnectionConfig connectionConfig)
        {
            _connectionResult = InstallController.Instance.TestDatabaseConnection(connectionConfig);		    
            if (_connectionResult.StartsWith("ERROR:"))
                return _connectionResult;

            var connectionString = _connectionResult;            
            var details = Localization.Localization.GetString("IsAbleToPerformDatabaseActionsCheck", LocalResourceFile);
            if (!InstallController.Instance.IsAbleToPerformDatabaseActions(connectionString))
                _connectionResult = "ERROR: " + string.Format(Localization.Localization.GetString("IsAbleToPerformDatabaseActions", LocalResourceFile), details);

            //database actions check-running sql 2008 or higher
            details = Localization.Localization.GetString("IsValidSqlServerVersionCheck", LocalResourceFile);
            if (!InstallController.Instance.IsValidSqlServerVersion(connectionString))
                _connectionResult = "ERROR: " + string.Format(Localization.Localization.GetString("IsValidSqlServerVersion", LocalResourceFile), details);

            return _connectionResult;
        }

        private static void UpdateMachineKey()
        {
            var installationDate = Config.GetSetting("InstallationDate");

            if (installationDate == null || String.IsNullOrEmpty(installationDate))
            {
                string strError = Config.UpdateMachineKey();
                if (String.IsNullOrEmpty(strError))
                {
                    //send a new request to the application to initiate step 2
                    HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl, true);
                }
                else
                {
                    //403-3 Error - Redirect to ErrorPage
                    var strUrl = "~/ErrorPage.aspx?status=403_3&error=" + strError;
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Server.Transfer(strUrl);
                }
            }
        }

        /// <summary>
        /// Updates and synchronizes DotNetNuke.install.config with Web.config
        /// </summary>
        /// <param name="installInfo"></param>
        private static void UpdateInstallConfig(Dictionary<string, string> installInfo)
        {
            _installConfig = new InstallConfig();
            // SuperUser Config
            _installConfig.SuperUser = new SuperUserConfig();
            _installConfig.SuperUser.UserName = installInfo["username"];
            _installConfig.SuperUser.Password = installInfo["password"];
            _installConfig.SuperUser.Locale = _culture;
            // Defaults
            _installConfig.SuperUser.Email = "host@change.me";
            _installConfig.SuperUser.FirstName = "SuperUser";
            _installConfig.SuperUser.LastName = "Account";

            // website culture
            _installConfig.InstallCulture = installInfo["language"];

            // Website Portal Config
            var portalConfig = new PortalConfig();
            portalConfig.PortalName = installInfo["websiteName"];
            portalConfig.TemplateFileName = installInfo["template"];
            portalConfig.IsChild = false;
            _installConfig.Portals = new List<PortalConfig>();
            _installConfig.Portals.Add(portalConfig);

            InstallController.Instance.SetInstallConfig(_installConfig);
        }

        private static void UpdateDatabaseInstallConfig(Dictionary<string, string> installInfo)
        {
            // Database Config
            if (installInfo["databaseSetup"] == "advanced")
            {
                _connectionConfig = new ConnectionConfig();
                _connectionConfig.Server = installInfo["databaseServerName"];
                _connectionConfig.Qualifier = installInfo["databaseObjectQualifier"];
                _connectionConfig.Integrated = installInfo["databaseSecurity"] == "integrated";
                _connectionConfig.User = installInfo["databaseUsername"];
                _connectionConfig.Password = installInfo["databasePassword"];
                _connectionConfig.RunAsDbowner = installInfo["databaseRunAsOwner"] == "on";

                if (installInfo["databaseType"] == "express")
                {
                    _connectionConfig.File = installInfo["databaseFilename"];
                    _connectionConfig.Database = "";
                }
                else
                {
                    _connectionConfig.Database = installInfo["databaseName"];
                    _connectionConfig.File = "";
                }
            }

            _installConfig.Connection = _connectionConfig;
            InstallController.Instance.SetInstallConfig(_installConfig);
        }
        
        private void BindLanguageList()
        {
            try
            {
                var myResponseReader = UpdateService.GetLanguageList();
                //empty language list
                languageList.Items.Clear();

                //Loading into XML doc
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(myResponseReader);
                var languages = xmlDoc.SelectNodes("available/language");
	            var packages = new List<PackageInfo>();

				if (languages != null)
				{
					foreach (XmlNode language in languages)
					{
						string cultureCode = "";
						string version = "";
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
							Version ver = null;
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
		            languageList.AddItem(li.Text, li.Value);
		            RadComboBoxItem lastItem = languageList.Items[languageList.Items.Count - 1];
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
                //suppress for now - need to decide what to do when webservice is unreachable
                //throw;
            }
            finally
            {
                //ensure there is always an en-us
                if (languageList.Items.FindItemByValue("en-US") == null)
                {
                    var myCIintl = new CultureInfo("en-US", true);
                    var li = new ListItem {Value = "en-US", Text = myCIintl.NativeName};
                    languageList.AddItem(li.Text, li.Value);
                    RadComboBoxItem lastItem = languageList.Items[languageList.Items.Count - 1];
                    lastItem.Attributes.Add("onclick", "javascript:ClearLegacyLangaugePack();");
                    languageList.Sort = RadComboBoxSort.Ascending;
                    languageList.Items.Sort();
                }
                var item = languageList.Items.FindItemByValue(_culture);
                languageList.SelectedIndex = item != null ? item.Index : languageList.Items.FindItemByValue("en-US").Index;
                languageList.Sort = RadComboBoxSort.Ascending;
                languageList.Items.Sort();
            }
        }

        private static void VisitSiteClick(object sender, EventArgs eventArgs)
        {    
            //Delete the status file.
            try
            {
                File.Delete(StatusFile);
                
            }
            catch (Exception)
            {
                //Do nothing
            }
            
            //delete the initial install config -check readonly status first
            try
            {
                string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
                if (File.Exists(installConfig))
                {
                    //make sure file is not read-only
                    File.SetAttributes(installConfig, FileAttributes.Normal);
                    File.Delete(installConfig);
                } 
            
            }
            catch (Exception)
            {

                //Do nothing
            }
            Config.Touch();
            HttpContext.Current.Response.Redirect("../Default.aspx");
        }
        #endregion 

        #region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetBaseDataBaseVersion gets the Base Database Version as a string
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/02/2007 Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string GetBaseDatabaseVersion()
        {
            return Upgrade.Upgrade.GetStringVersion(BaseVersion);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LocalizeString is a helper function for localizing strings
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	01/23/2007 Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string LocalizeString(string key)
        {
            return Localization.Localization.GetString(key, LocalResourceFile, _culture);
        }
        
        protected override void OnError(EventArgs e)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
        }

        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the Page is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/14/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //if previous config deleted create new empty one
            string installConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Install", "DotNetNuke.install.config");
            if (!File.Exists(installConfig))
            {
                File.Copy(installConfig + ".resources", installConfig);
            }
            GetInstallerLocales();
            if (!Page.IsPostBack || _installConfig == null)
            {
                _installConfig = InstallController.Instance.GetInstallConfig();
                _connectionConfig = _installConfig.Connection;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the Page loads
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/09/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            SetBrowserLanguage();
            LocalizePage();
            
            base.OnLoad(e);
            visitSite.Click += VisitSiteClick;           

            //Create Status Files
            if (!File.Exists(StatusFile)) File.CreateText(StatusFile).Close();

            // Hide Licensing Step if no License Info is available
            LicenseActivation.Visible = IsProOrEnterprise && !String.IsNullOrEmpty(_installConfig.License.AccountEmail) && !String.IsNullOrEmpty(_installConfig.License.InvoiceNumber);

            if ((!IsProOrEnterprise) && templateList.Items.FindItemByValue("Mobile Website.template") != null)
            {
                templateList.Items.Remove(templateList.Items.FindItemByValue("Mobile Website.template"));
            }

            if (HttpContext.Current.Request.RawUrl.EndsWith("&initiateinstall"))
            {
                var synchConnectionString = new SynchConnectionStringStep();
                synchConnectionString.Execute();
                Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&initiateinstall", "&executeinstall"), true);
            }
            else if (HttpContext.Current.Request.RawUrl.EndsWith("&executeinstall"))
            {
                try
                {
                    _installerRunning = true;
                    LaunchAutoInstall();
                }
                catch(Exception)
                {
                    //Redirect back to first page
                    Response.Redirect(HttpContext.Current.Request.RawUrl.Replace("&executeinstall", ""), true);
                }
            }
            else if (!Page.IsPostBack)
            {
                if (_installerRunning)
                {
                    LaunchAutoInstall();
                }
                else
                {
                    SetupDatabaseInfo();
                    BindLanguageList();
                    
                    if (CheckDatabaseConnection())
                    {
                        Initialise();
                    }
                    else
                    {
                        //Install but connection string not configured to point at a valid SQL Server
                        UpdateMachineKey();
                    }

                    if (!Regex.IsMatch(Request.Url.Host, "^([a-zA-Z0-9.-]+)$", RegexOptions.IgnoreCase))
                    {
                        lblError.Visible = true;
                        lblError.Text = Localization.Localization.GetString("HostWarning", LocalResourceFile);
                    }

                    //ensure web.config is not read-only
                    var configPath = Server.MapPath("~/web.config");
                    try
                    {
                        var attributes = File.GetAttributes(configPath);
                        //file is read only
                        if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            attributes = attributes & ~FileAttributes.ReadOnly;
                            File.SetAttributes(configPath, attributes);
                        }
                    }
                    catch (Exception ex)
                    {
                        lblError.Visible = true;
                        lblError.Text = ex.ToString();
                        return;
                    }

                    //Adding ClientDependency Resources config to web.config                    
                    if (!ClientResourceManager.IsInstalled() && ValidatePermissions().Item1)
                    {
                        ClientResourceManager.AddConfiguration();
                        Response.Redirect(Request.RawUrl);
                        //TODO - this may cause infinite loop
                    }

                    //Ensure connection strings are in synch
                    var synchConnectionString = new SynchConnectionStringStep();
                    synchConnectionString.Execute();
                    if (synchConnectionString.Status == StepStatus.AppRestart) Response.Redirect(HttpContext.Current.Request.RawUrl, true);

                    txtUsername.Text = _installConfig.SuperUser.UserName;
                    if (_installConfig.Portals.Count > 0)
                    {
                        txtWebsiteName.Text = _installConfig.Portals[0].PortalName;
                        //TODO Language and Template
                    }
                }
            }

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when there is a ClientCallback
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/09/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ProcessAction(string someAction)
        {
            //First check that we are not being targetted by an AJAX HttpPost, so get the current DB version
            string strProviderPath = _dataProvider.GetProviderPath();
            string nextVersion = GetNextScriptVersion(strProviderPath, DatabaseVersion);
            if (someAction != nextVersion)
            {
                Exceptions.Exceptions.LogException(new Exception("Attempt made to run a Database Script - Possible Attack"));
                return "Error: Possible Attack";
            }
            if (someAction == GetBaseDatabaseVersion())
            {
                string result = InstallDatabase();
                if (result == "Done")
                {
                    //Complete Installation
                    Upgrade.Upgrade.UpgradeApplication();
                }
                return result;
            }
            else if (someAction.Contains("."))
            {
                //Upgrade Database
                string result = InstallVersion(someAction);
                if (result == "Done")
                {
                    ArrayList arrVersions = Upgrade.Upgrade.GetUpgradeScripts(strProviderPath, BaseVersion);
                    string strErrorMessage = Null.NullString;
                    for (int i = 0; i <= arrVersions.Count - 1; i++)
                    {
                        string strVersion = Path.GetFileNameWithoutExtension(Convert.ToString(arrVersions[i]));
                        var version = new Version(strVersion);
                        if (version != null)
                        {
                            strErrorMessage += Upgrade.Upgrade.UpgradeApplication(strProviderPath, version, false);

                            //delete files which are no longer used
                            strErrorMessage += Upgrade.Upgrade.DeleteFiles(strProviderPath, version, false);

                            //execute config file updates
                            strErrorMessage += Upgrade.Upgrade.UpdateConfig(strProviderPath, version, false);
                        }
                    }

                    //Complete Installation
                    Upgrade.Upgrade.UpgradeApplication();
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

        #endregion

        #region WebMethods

        //steps shown in UI
        static readonly IInstallationStep filePermissionCheck = new FilePermissionCheckStep();
        static readonly IInstallationStep installDatabase = new InstallDatabaseStep();
        static readonly IInstallationStep installExtensions = new InstallExtensionsStep();
        static readonly IInstallationStep installSite = new InstallSiteStep();
        static readonly IInstallationStep createSuperUser = new InstallSuperUserStep();
        static readonly IInstallationStep activateLicense = new ActivateLicenseStep();

        //Ordered List of Steps (and weight in percentage) to be executed
        private static readonly IDictionary<IInstallationStep, int> _steps = new Dictionary<IInstallationStep, int>
                                        {   {filePermissionCheck, 10},
                                            {new IISVerificationStep(), 5},
                                            {installDatabase, 20},
                                            {installExtensions, 25},
                                            {new InitializeHostSettingsStep(), 5},
											{new UpdateLanguagePackStep(), 5},
                                            {installSite, 20},
                                            {createSuperUser, 5},
                                            {activateLicense, 4},
                                            {new InstallVersionStep(), 1}
                                        };

        [System.Web.Services.WebMethod]
        public static void RunInstall()
        {
            _installerRunning = false;
            LaunchAutoInstall();
        }

        [System.Web.Services.WebMethod]
        public static object GetInstallationLog(int startRow)
        {
            var data = string.Empty;
            string logFile = "InstallerLog" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".resources";
            try
            {
                var lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "_default", "logs", logFile));
                var errorLogged = false;
                if (lines.Length > startRow)
                {
                    var count = lines.Length - startRow > 500  ? 500 : lines.Length - startRow;
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
                if (errorLogged ==false)
                {
                    Localization.Localization.GetString("NoErrorsLogged", "~/Install/App_LocalResources/InstallWizard.aspx.resx");
                }
            }
            catch (Exception)
            {
            }

            return data;
        }
		
		[System.Web.Services.WebMethod]
        public static Tuple<bool, string> ValidateInput(Dictionary<string, string> installInfo)
        {
            var result = true;
		    var errorMsg=string.Empty;
            
            // Check Required Fields
			if (installInfo["username"] == string.Empty || installInfo["password"] == string.Empty || installInfo["confirmPassword"] == string.Empty || installInfo["websiteName"] == string.Empty)
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

		[System.Web.Services.WebMethod]
		public static Tuple<bool, string> ValidatePermissions()
		{
			var permissionsValid = true;
			IEnumerable<FileSystemPermissionVerifier> failedList;
			var errorMessage = string.Empty;

			var verifiers = new List<FileSystemPermissionVerifier>
                                {
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~"), 3),
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~/App_Data"), 3)
                                };

			failedList = verifiers.Where(v => !v.VerifyFolderCreate()).ToArray();
			if (failedList.Any())
			{
				permissionsValid = false;
			}

			if (permissionsValid)
			{
				failedList = verifiers.Where(v => !v.VerifyFileCreate()).ToArray();
				if (failedList.Any())
				{
					permissionsValid = false;
				}
			}

			if (permissionsValid)
			{
				failedList = verifiers.Where(v => !v.VerifyFileDelete()).ToArray();
				if (failedList.Any())
				{
					permissionsValid = false;
				}
			}

			if (permissionsValid)
			{
				failedList = verifiers.Where(v => !v.VerifyFolderDelete()).ToArray();
				if (failedList.Any())
				{
					permissionsValid = false;
				}
			}

			if (!permissionsValid)
			{
				errorMessage = string.Format(LocalizeStringStatic("FileAndFolderPermissionCheckFailed"), string.Join("; ", (from v in verifiers select v.BasePath)));
			}
			return new Tuple<bool, string>(permissionsValid, errorMessage);
		}

        [System.Web.Services.WebMethod]
        public static bool ValidatePassword(string password)
        {
            // Check Length
            var result = !(password.Length < Membership.MinRequiredPasswordLength);

            // Check against regex    
            if (!string.IsNullOrEmpty(Membership.PasswordStrengthRegularExpression) && !Regex.IsMatch(password, Membership.PasswordStrengthRegularExpression))
                result = false;

            // Check non-alphaNumeric
            var nonAlnumCount = password.Where((t, i) => !char.IsLetterOrDigit(password, i)).Count();
            if (nonAlnumCount < Membership.MinRequiredNonAlphanumericCharacters) result = false;

            return result;
        }      

        [System.Web.Services.WebMethod]
        public static bool VerifyDatabaseConnectionOnLoad()
        {
            return CheckDatabaseConnection();
        }

        [System.Web.Services.WebMethod]
        public static Tuple<bool, string> VerifyDatabaseConnection(Dictionary<string, string> installInfo)
        {
            var connectionConfig = new ConnectionConfig();

            // Database Config
            if (installInfo["databaseSetup"] == "standard")
            {
                connectionConfig = _connectionConfig;
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
                    connectionConfig.Database = "";
                }
                else
                {
                    connectionConfig.Database = installInfo["databaseName"];
                    connectionConfig.File = "";
                }
            }

            var result = CheckDatabaseConnection(connectionConfig);
            var validConnection = !result.StartsWith("ERROR:");
            if (validConnection)
            {
                UpdateDatabaseInstallConfig(installInfo);
                _connectionConfig = connectionConfig;
                _installConfig.Connection = connectionConfig;
            }
            return new Tuple<bool, string>(validConnection, result);
        }

        /// <summary>
        /// Indicate if the Installer is running
        /// </summary>
        /// <returns>True or False</returns>
        /// <remarks>Checks the local static variable or the existence of status file</remarks>
        [System.Web.Services.WebMethod]
        public static bool IsInstallerRunning()
        {
            bool isRunning;

            if (_installerRunning) 
            {
                isRunning =  true;
            }
            else if (File.Exists(StatusFile))
            {
                var file = new FileInfo(StatusFile);
                isRunning = (file.Length > 0);
            }
            else
            {
                isRunning = false;
            }

            return isRunning;
        }

        #endregion
    } 
}
