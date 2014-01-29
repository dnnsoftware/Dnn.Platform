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
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Data.PetaPoco;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Modules.Admin.Extensions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : Install
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Supplies the functionality to Install Extensions(packages) to the Portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///     [cnurse]   07/26/2007    Created
    /// </history>
    /// -----------------------------------------------------------------------------
    partial class Install : ModuleUserControlBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Install));
		#region "Members"

        private Installer _Installer;
        private PackageInfo _Package;
        private PackageType _PackageType;
		
		#endregion

		#region "Protected Properties"

        protected bool DeleteFile
        {
            get
            {
                return Convert.ToBoolean(ViewState["DeleteFile"]);
            }
            set
            {
                ViewState["DeleteFile"] = value;
            }
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the FileName for the Uploade file
        /// </summary>
        /// <history>
        ///     [cnurse]   01/20/2009    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string FileName
        {
            get
            {
                return Convert.ToString(ViewState["FileName"]);
            }
            set
            {
                ViewState["FileName"] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Installer
        /// </summary>
        /// <history>
        ///     [cnurse]   08/13/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected Installer Installer
        {
            get
            {
                return _Installer;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Path to the Manifest File
        /// </summary>
        /// <history>
        ///     [cnurse]   08/13/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string ManifestFile
        {
            get
            {
                return Convert.ToString(ViewState["ManifestFile"]);
            }
            set
            {
                ViewState["ManifestFile"] = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Package
        /// </summary>
        /// <history>
        ///     [cnurse]   08/13/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected PackageInfo Package
        {
            get
            {
                return _Package;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Package Type
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected PackageType PackageType
        {
            get
            {
                if (_PackageType == null)
                {
                    string pType = Null.NullString;
                    if (!string.IsNullOrEmpty(Request.QueryString["ptype"]))
                    {
                        pType = Request.QueryString["ptype"];
                    }
                    _PackageType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType == pType);
                }
                return _PackageType;
            }
        }

        protected int InstallPortalId
        {
            get
            {
                int _PortalId = ModuleContext.PortalId;
                if (ModuleContext.IsHostMenu)
                {
                    _PortalId = Null.NullInteger;
                }
                return _PortalId;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string ReturnURL
        {
            get
            {
                string _ReturnUrl = Server.UrlDecode(Request.Params["returnUrl"]);

                if (string.IsNullOrEmpty(_ReturnUrl))
                {
                    int TabID = ModuleContext.PortalSettings.HomeTabId;

                    if (Request.Params["rtab"] != null)
                    {
                        TabID = int.Parse(Request.Params["rtab"]);
                    }
                    _ReturnUrl = Globals.NavigateURL(TabID);
                }
                return _ReturnUrl;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Temporary Installation Folder
        /// </summary>
        /// <history>
        ///     [cnurse]   08/13/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string TempInstallFolder
        {
            get
            {
                return Convert.ToString(ViewState["TempInstallFolder"]);
            }
            set
            {
                ViewState["TempInstallFolder"] = value;
            }
        }

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine binds the package to the Property Editor
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BindPackage()
        {
            CreateInstaller();
            if (Installer.IsValid)
            {
                if (_Installer.Packages.Count > 0)
                {
                    _Package = _Installer.Packages[0].Package;
                }
                
				//Bind Package Info
				packageForm.DataSource = _Package;
                packageForm.DataBind();

                //Bind License Info
                licenseForm.DataSource = _Package;
                licenseForm.DataBind();

                //Bind ReleaseNotes Info
                releaseNotesForm.DataSource = _Package;
                releaseNotesForm.DataBind();
            }
            else
            {
				//Error reading Manifest
                switch (wizInstall.ActiveStepIndex)
                {
                    case 0:
                        lblLoadMessage.Text = Localization.GetString("InstallError", LocalResourceFile);
                        phLoadLogs.Controls.Add(Installer.InstallerInfo.Log.GetLogsTable());
                        break;
                    case 3:
                        lblAcceptMessage.Text = Localization.GetString("InstallError", LocalResourceFile);
                        lblAcceptMessage.Visible = true;
                        phAcceptLogs.Controls.Add(Installer.InstallerInfo.Log.GetLogsTable());
                        break;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine checks the Access Security
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CheckSecurity()
        {
            if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine creates the Installer
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CreateInstaller()
        {
            CheckSecurity();
            _Installer = new Installer(TempInstallFolder, ManifestFile, Request.MapPath("."), false);

            //The Installer is created automatically with a SecurityAccessLevel of Host
            //Check if the User has lowere Security and update as neccessary
            if (!ModuleContext.PortalSettings.UserInfo.IsSuperUser)
            {
                if (ModuleContext.PortalSettings.UserInfo.IsInRole(ModuleContext.PortalSettings.AdministratorRoleName))
                {
					//Admin User
                    Installer.InstallerInfo.SecurityAccessLevel = SecurityAccessLevel.Admin;
                }
                else if (ModulePermissionController.CanAdminModule(ModuleContext.Configuration))
                {
					//Has Edit rights
                    Installer.InstallerInfo.SecurityAccessLevel = SecurityAccessLevel.Edit;
                }
                else if (ModulePermissionController.CanViewModule(ModuleContext.Configuration))
                {
					//Has View rights
                    Installer.InstallerInfo.SecurityAccessLevel = SecurityAccessLevel.View;
                }
                else
                {
                    Installer.InstallerInfo.SecurityAccessLevel = SecurityAccessLevel.Anonymous;
                }
            }
            Installer.InstallerInfo.PortalID = InstallPortalId;

            //Read the manifest
            if (Installer.InstallerInfo.ManifestFile != null)
            {
                Installer.ReadManifest(true);
            }
        }

        private void CreateManifest()
        {
            ManifestFile = Path.Combine(TempInstallFolder, Path.GetFileNameWithoutExtension(FileName) + ".dnn");
            StreamWriter manifestWriter = new StreamWriter(ManifestFile);
            manifestWriter.Write(LegacyUtil.CreateSkinManifest(FileName, rblLegacySkin.SelectedValue, TempInstallFolder));
            manifestWriter.Close();
        }

        private void Initialize()
        {
            var package = String.Empty;
            if (!String.IsNullOrEmpty(Request.QueryString["package"]))
            {
                package = Request.QueryString["package"];
            }

            if (!String.IsNullOrEmpty(package))
            {
                LoadPackage(package);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine installs the uploaded package
        /// </summary>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void InstallPackage(WizardNavigationEventArgs e)
        {
            CreateInstaller();
            if (Installer.IsValid)
            {
				//Reset Log
                Installer.InstallerInfo.Log.Logs.Clear();

                //Set the IgnnoreWhiteList flag
                Installer.InstallerInfo.IgnoreWhiteList = chkIgnoreWhiteList.Checked;

                //Set the Repair flag
                Installer.InstallerInfo.RepairInstall = chkRepairInstall.Checked;

                //Install
                Installer.Install();

                if (!Installer.IsValid)
                {
                    lblInstallMessage.Text = Localization.GetString("InstallError", LocalResourceFile);
                }
                phInstallLogs.Controls.Add(Installer.InstallerInfo.Log.GetLogsTable());

				DeleteInstallFile();
            }
            else
            {
				//Error reading Manifest
                switch (e.CurrentStepIndex)
                {
                    case 3:
                        lblAcceptMessage.Text = Localization.GetString("InstallError", LocalResourceFile);
                        lblAcceptMessage.Visible = true;
                        phAcceptLogs.Controls.Add(Installer.InstallerInfo.Log.GetLogsTable());
                        break;
                    case 4:
                        lblInstallMessage.Text = Localization.GetString("InstallError", LocalResourceFile);
                        phInstallLogs.Controls.Add(Installer.InstallerInfo.Log.GetLogsTable());
                        break;
                }
                e.Cancel = true;
            }

        	lblInstallMessageRow.Visible = !string.IsNullOrEmpty(lblInstallMessage.Text);
        }

        private void LoadPackage(string package)
        {
            var packageType = String.Empty;
            var installFolder = String.Empty;

            if (!String.IsNullOrEmpty(Request.QueryString["ptype"]))
            {
                packageType = Request.QueryString["ptype"];
            }
            switch (packageType)
            {
                case "Auth_System":
                    installFolder = "AuthSystem";
                    break;
                case "CoreLanguagePack":
                case "ExtensionLanguagePack":
                    installFolder = "Language";
                    break;
                case "JavaScript_Library":
                    installFolder = "JavaScriptLibrary";
                    break;
                case "Module":
                case "Skin":
                case "Container":
                case "Provider":
                    installFolder = packageType;
                    break;
                case "Library":
                    installFolder = "Module";
                    break;
                default:
                    break;
            }

            if (!String.IsNullOrEmpty(installFolder))
            {
                FileName = String.Format("{0}\\Install\\{1}\\{2}", Globals.ApplicationMapPath, installFolder, package);
                if (File.Exists(FileName) &&
                        (Path.GetExtension(FileName.ToLower()) == ".zip"
                            || Path.GetExtension(FileName.ToLower()) == ".resources"))
                {
                    _Installer = new Installer(new FileStream(FileName, FileMode.Open, FileAccess.Read), 
                                                    Globals.ApplicationMapPath, 
                                                    true, 
                                                    false);
                    TempInstallFolder = Installer.TempInstallFolder;
                    if (Installer.InstallerInfo.ManifestFile != null)
                    {
                        ManifestFile = Path.GetFileName(Installer.InstallerInfo.ManifestFile.TempFileName);
                    }

                    DeleteFile = true;
                    wizInstall.ActiveStepIndex = 1;
                }
            }           
        }

        private bool ValidatePackage()
        {
            bool isValid = Null.NullBoolean;

            CreateInstaller();
            if (Installer.InstallerInfo.ManifestFile != null)
            {
                ManifestFile = Path.GetFileName(Installer.InstallerInfo.ManifestFile.TempFileName);
            }

            var azureCompact = AzureCompact();
            if (string.IsNullOrEmpty(ManifestFile))
            {
                if (!string.IsNullOrEmpty(rblLegacySkin.SelectedValue))
                {
					//We need to create a manifest file so the installer can continue to run
                    CreateManifest();

                    //Revalidate Package
                    isValid = ValidatePackage();
                }
                else
                {
                    lblWarningMessageWrapper.Visible = true;
                    pnlRepair.Visible = false;
                    pnlWhitelist.Visible = false;
	                pnlAzureCompact.Visible = false;
                    pnlLegacy.Visible = true;
                    lblWarningMessage.Text = Localization.GetString("NoManifest", LocalResourceFile);
                }
            }
            else if (Installer == null)
            {
                lblWarningMessageWrapper.Visible = true;
                pnlRepair.Visible = false;
                pnlWhitelist.Visible = false;
                pnlLegacy.Visible = false;
				pnlAzureCompact.Visible = false;
                lblWarningMessage.Text = Localization.GetString("ZipCriticalError", LocalResourceFile);
                wizInstall.FindControl("StepNavigationTemplateContainerID").FindControl("nextButtonStep").Visible = false;
            }
            else if (!Installer.IsValid)
            {
                lblWarningMessageWrapper.Visible = true;
                pnlRepair.Visible = false;
                pnlWhitelist.Visible = false;
                pnlLegacy.Visible = false;
				pnlAzureCompact.Visible = false;
                lblWarningMessage.Text = Localization.GetString("ZipError", LocalResourceFile);

                //Error parsing zip
                phLoadLogs.Controls.Add(Installer.InstallerInfo.Log.GetLogsTable());
                wizInstall.FindControl("StepNavigationTemplateContainerID").FindControl("nextButtonStep").Visible = false;
            }
            else if (!string.IsNullOrEmpty(Installer.InstallerInfo.LegacyError))
            {
                lblWarningMessageWrapper.Visible = true;
                pnlRepair.Visible = false;
                pnlWhitelist.Visible = false;
                pnlLegacy.Visible = false;
				pnlAzureCompact.Visible = false;
                lblWarningMessage.Text = Localization.GetString(Installer.InstallerInfo.LegacyError, LocalResourceFile);
            }
            else if (!Installer.InstallerInfo.HasValidFiles && !chkIgnoreWhiteList.Checked)
            {
                lblWarningMessageWrapper.Visible = true;
                pnlRepair.Visible = false;
                pnlWhitelist.Visible = true;
                pnlLegacy.Visible = false;
				pnlAzureCompact.Visible = false;
                lblWarningMessage.Text = string.Format(Localization.GetString("InvalidFiles", LocalResourceFile), Installer.InstallerInfo.InvalidFileExtensions);
            }
            else if (Installer.InstallerInfo.Installed && !chkRepairInstall.Checked)
            {
                lblWarningMessageWrapper.Visible = true;
				if (UserController.GetCurrentUserInfo().IsSuperUser || Installer.InstallerInfo.PortalID == InstallPortalId)
                {
                    pnlRepair.Visible = true;
                }
                pnlWhitelist.Visible = false;
                pnlLegacy.Visible = false;
				pnlAzureCompact.Visible = false;
                lblWarningMessage.Text = Localization.GetString("PackageInstalled", LocalResourceFile);
            }
			else if ((!azureCompact.HasValue || !azureCompact.Value) && !chkAzureCompact.Checked)
			{
				lblWarningMessageWrapper.Visible = true;
				pnlRepair.Visible = false;
				pnlWhitelist.Visible = false;
				pnlLegacy.Visible = false;
				pnlAzureCompact.Visible = true;
				lblWarningMessage.Text = Localization.GetString("AzureCompactMessage", LocalResourceFile);
                lblAzureCompact.Text = Localization.GetString("AzureCompactHelp", LocalResourceFile);
                if (azureCompact.HasValue)
                {
                    wizInstall.FindControl("StepNavigationTemplateContainerID").FindControl("nextButtonStep").Visible = chkAzureCompact.Visible = false;

                    lblWarningMessage.Text = Localization.GetString("NoAzureCompactMessage", LocalResourceFile);
                    lblAzureCompact.Text = Localization.GetString("NoAzureCompactHelp", LocalResourceFile);
                }
			}
            else
            {
                isValid = true;
            }
            return isValid;
        }

		private bool? AzureCompact()
		{
			bool? compact = null;
			string manifestFile = null;
            if (Installer.InstallerInfo.ManifestFile != null)
            {
                manifestFile = Installer.InstallerInfo.ManifestFile.TempFileName;
            }
            if (!IsAzureDatabase())
            {
                compact = true;
            }
            else if (manifestFile != null && File.Exists(manifestFile))
			{
				try
				{
					var document = new XmlDocument();
					document.Load(manifestFile);
					var compactNode = document.SelectSingleNode("/dotnetnuke/packages/package/azureCompatible");
					if (compactNode != null && !string.IsNullOrEmpty(compactNode.InnerText))
					{
					    compact = compactNode.InnerText.ToLowerInvariant() == "true";
					}
				}
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
				
			}

			return compact;
		}

		private bool IsAzureDatabase()
		{
			return PetaPocoHelper.ExecuteScalar<int>(DataProvider.Instance().ConnectionString, CommandType.Text, "SELECT CAST(ServerProperty('EngineEdition') as INT)") == 5;
		}

		private void DeleteInstallFile()
		{
			try
			{
				if (!String.IsNullOrEmpty(TempInstallFolder) && Directory.Exists(TempInstallFolder))
				{
					Directory.Delete(TempInstallFolder, true);
				}

				if (DeleteFile && File.Exists(FileName))
				{
					// delete file
					try
					{
						File.SetAttributes(FileName, FileAttributes.Normal);
						File.Delete(FileName);
					}
					catch (Exception exc)
					{
						Logger.Error(exc);
					}
				}
			}
			catch (Exception exc)
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		#endregion

		#region "Protected Methods"

        protected string GetText(string type)
        {
            string text = Null.NullString;
            if (type == "Title")
            {
                text = Localization.GetString(wizInstall.ActiveStep.Title + ".Title", LocalResourceFile);
            }
            else if (type == "Help")
            {
                text = Localization.GetString(wizInstall.ActiveStep.Title + ".Help", LocalResourceFile);
            }
            return text;
        }

		#endregion

		#region "Event Handlers"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load runs when the page loads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]   07/26/2007    Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            chkIgnoreWhiteList.CheckedChanged += chkIgnoreRestrictedFiles_CheckedChanged;
            chkRepairInstall.CheckedChanged += chkRepairInstall_CheckedChanged;
			chkAzureCompact.CheckedChanged += chkAzureCompact_CheckedChanged;
            wizInstall.ActiveStepChanged += wizInstall_ActiveStepChanged;
            wizInstall.CancelButtonClick += wizInstall_CancelButtonClick;
            wizInstall.NextButtonClick += wizInstall_NextButtonClick;
            wizInstall.FinishButtonClick += wizInstall_FinishButtonClick;

            try
            {
                CheckSecurity();

                maxSizeWarningLabel.Text = String.Format(Localization.GetString("FileSizeRestriction", LocalResourceFile), (Config.GetMaxUploadSize() / (1024 * 1024)));

                if(!Page.IsPostBack)
                {
                    Initialize();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void chkIgnoreRestrictedFiles_CheckedChanged(object sender, EventArgs e)
        {
            if ((chkIgnoreWhiteList.Checked))
            {
                lblWarningMessage.Text = Localization.GetString("IgnoreRestrictedFilesWarning", LocalResourceFile);
            }
            else
            {
                lblWarningMessage.Text = "";
            }
        }

        protected void chkRepairInstall_CheckedChanged(object sender, EventArgs e)
        {
            if ((chkRepairInstall.Checked))
            {
                lblWarningMessage.Text = Localization.GetString("RepairInstallWarning", LocalResourceFile);
            }
            else
            {
                lblWarningMessage.Text = Localization.GetString("PackageInstalled", LocalResourceFile);
            }
        }

		protected void chkAzureCompact_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAzureCompact.Checked)
            {
                lblWarningMessage.Text = Localization.GetString("AzureCompactWarning", LocalResourceFile);
            }
            else
            {
				lblWarningMessage.Text = Localization.GetString("AzureCompactMessage", LocalResourceFile);
            }
        }

        protected void wizInstall_ActiveStepChanged(object sender, EventArgs e)
        {
            switch (wizInstall.ActiveStepIndex)
            {
                case 1: //Warning Page
                    if (ValidatePackage())
                    {
						//Skip Warning Page
                        wizInstall.ActiveStepIndex = 2;
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    BindPackage();
                    break;
                case 5:
                    wizInstall.DisplayCancelButton = false;
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizInstall_CancelButtonClick runs when the Cancel Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	08/13/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizInstall_CancelButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(TempInstallFolder) && Directory.Exists(TempInstallFolder))
                {
                    Directory.Delete(TempInstallFolder, true);
                }
				//Redirect to Definitions page
                Response.Redirect(ReturnURL, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// wizInstall_FinishButtonClick runs when the Finish Button on the Wizard is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	08/13/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizInstall_FinishButtonClick(object sender, WizardNavigationEventArgs e)
        {
            Response.Redirect(ReturnURL, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Wizard_NextButtonClickruns when the next Button is clicked.  It provides
        ///	a mechanism for cancelling the page change if certain conditions aren't met.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	08/13/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void wizInstall_NextButtonClick(object sender, WizardNavigationEventArgs e)
        {
            switch (e.CurrentStepIndex)
            {
                case 0:
                    HttpPostedFile postedFile = cmdBrowse.PostedFile;
                    string strMessage = "";
                    FileName = Path.GetFileName(postedFile.FileName);
                    string strExtension = Path.GetExtension(FileName);
                    if (string.IsNullOrEmpty(postedFile.FileName))
                    {
                        strMessage = Localization.GetString("NoFile", LocalResourceFile);
                    }
                    else if (strExtension.ToLower() != ".zip")
                    {
                        strMessage += string.Format(Localization.GetString("InvalidExt", LocalResourceFile), FileName);
                    }
                    if (string.IsNullOrEmpty(strMessage))
                    {
                        _Installer = new Installer(postedFile.InputStream, Request.MapPath("."), true, false);
                        TempInstallFolder = Installer.TempInstallFolder;
                        if (Installer.InstallerInfo.ManifestFile != null)
                        {
                            ManifestFile = Path.GetFileName(Installer.InstallerInfo.ManifestFile.TempFileName);
                        }
                    }
                    else
                    {
                        lblLoadMessage.Text = strMessage;
                        lblLoadMessage.Visible = true;
                        e.Cancel = true;
                    }
                    break;
                case 1: //Warning Page
                    e.Cancel = !ValidatePackage();
                    break;
                case 4: //Accept Terms 
                    if (chkAcceptLicense.Checked)
                    {
                        InstallPackage(e);
                    }
                    else
                    {
                        lblAcceptMessage.Text = Localization.GetString("AcceptTerms", LocalResourceFile);
                        lblAcceptMessage.Visible = true;
                        e.Cancel = true;

                        //Rebind package
                        BindPackage();
                    }
                    break;
            }
        }
		
		#endregion
    }
}