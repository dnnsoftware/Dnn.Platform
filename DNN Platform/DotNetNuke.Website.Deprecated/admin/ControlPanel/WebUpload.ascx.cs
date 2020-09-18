// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.FileManager
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.Common;
    using Microsoft.Extensions.DependencyInjection;

    using Host = DotNetNuke.Entities.Host.Host;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : WebUpload
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Supplies the functionality for uploading files to the Portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class WebUpload : PortalModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WebUpload));
        private readonly INavigationManager _navigationManager;

        private string _DestinationFolder;
        private UploadType _FileType;
        private string _FileTypeName;

        private string _RootFolder;
        private string _UploadRoles;

        public WebUpload()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string DestinationFolder
        {
            get
            {
                if (this._DestinationFolder == null)
                {
                    this._DestinationFolder = string.Empty;
                    if (this.Request.QueryString["dest"] != null)
                    {
                        this._DestinationFolder = Globals.QueryStringDecode(this.Request.QueryString["dest"]);
                    }
                }

                return PathUtils.Instance.RemoveTrailingSlash(this._DestinationFolder.Replace("\\", "/"));
            }
        }

        public UploadType FileType
        {
            get
            {
                this._FileType = UploadType.File;
                if (this.Request.QueryString["ftype"] != null)
                {
                    // The select statement ensures that the parameter can be converted to UploadType
                    switch (this.Request.QueryString["ftype"].ToLowerInvariant())
                    {
                        case "file":
                            this._FileType = (UploadType)Enum.Parse(typeof(UploadType), this.Request.QueryString["ftype"]);
                            break;
                    }
                }

                return this._FileType;
            }
        }

        public string FileTypeName
        {
            get
            {
                if (this._FileTypeName == null)
                {
                    this._FileTypeName = Localization.GetString(this.FileType.ToString(), this.LocalResourceFile);
                }

                return this._FileTypeName;
            }
        }

        public int FolderPortalID
        {
            get
            {
                if (this.IsHostMenu)
                {
                    return Null.NullInteger;
                }
                else
                {
                    return this.PortalId;
                }
            }
        }

        public string RootFolder
        {
            get
            {
                if (this._RootFolder == null)
                {
                    if (this.IsHostMenu)
                    {
                        this._RootFolder = Globals.HostMapPath;
                    }
                    else
                    {
                        this._RootFolder = this.PortalSettings.HomeDirectoryMapPath;
                    }
                }

                return this._RootFolder;
            }
        }

        public string UploadRoles
        {
            get
            {
                if (this._UploadRoles == null)
                {
                    this._UploadRoles = string.Empty;

                    if (Convert.ToString(this.Settings["uploadroles"]) != null)
                    {
                        this._UploadRoles = Convert.ToString(this.Settings["uploadroles"]);
                    }
                }

                return this._UploadRoles;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine determines the Return Url.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public string ReturnURL()
        {
            int TabID = this.PortalSettings.HomeTabId;

            if (this.Request.Params["rtab"] != null)
            {
                TabID = int.Parse(this.Request.Params["rtab"]);
            }

            return this._navigationManager.NavigateURL(TabID);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Customise the Control Title
            this.ModuleConfiguration.ModuleTitle = Localization.GetString("UploadType" + this.FileType, this.LocalResourceFile);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Page_Load runs when the page loads.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdAdd.Click += this.cmdAdd_Click;
            this.cmdReturn1.Click += this.cmdReturn_Click;
            this.cmdReturn2.Click += this.cmdReturn_Click;

            try
            {
                this.CheckSecurity();

                // Get localized Strings
                string strHost = Localization.GetString("HostRoot", this.LocalResourceFile);
                string strPortal = Localization.GetString("PortalRoot", this.LocalResourceFile);

                this.maxSizeWarningLabel.Text = string.Format(Localization.GetString("FileSizeRestriction", this.LocalResourceFile), Config.GetMaxUploadSize() / (1024 * 1024));

                if (!this.Page.IsPostBack)
                {
                    this.cmdAdd.Text = Localization.GetString("UploadType" + this.FileType, this.LocalResourceFile);
                    if (this.FileType == UploadType.File)
                    {
                        this.foldersRow.Visible = true;
                        this.rootRow.Visible = true;
                        this.unzipRow.Visible = true;

                        if (this.IsHostMenu)
                        {
                            this.lblRootType.Text = strHost + ":";
                            this.lblRootFolder.Text = this.RootFolder;
                        }
                        else
                        {
                            this.lblRootType.Text = strPortal + ":";
                            this.lblRootFolder.Text = this.RootFolder;
                        }

                        this.LoadFolders();
                    }

                    this.chkUnzip.Checked = false;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine checks the Access Security.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void CheckSecurity()
        {
            if (!ModulePermissionController.HasModulePermission(this.ModuleConfiguration.ModulePermissions, "CONTENT,EDIT") && !UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                this.Response.Redirect(this._navigationManager.NavigateURL("Access Denied"), true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This routine populates the Folder List Drop Down
        /// There is no reference to permissions here as all folders should be available to the admin.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void LoadFolders()
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            var folders = FolderManager.Instance.GetFolders(this.FolderPortalID, "ADD", user.UserID);
            this.ddlFolders.Services.Parameters.Add("permission", "ADD");
            if (!string.IsNullOrEmpty(this.DestinationFolder))
            {
                this.ddlFolders.SelectedFolder = folders.SingleOrDefault(f => f.FolderPath == this.DestinationFolder);
            }
            else
            {
                var rootFolder = folders.SingleOrDefault(f => f.FolderPath == string.Empty);
                if (rootFolder != null)
                {
                    this.ddlFolders.SelectedItem = new ListItem() { Text = DynamicSharedConstants.RootFolder, Value = rootFolder.FolderID.ToString() };
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The cmdAdd_Click runs when the Add Button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdAdd_Click(object sender, EventArgs e)
        {
            try
            {
                this.CheckSecurity();
                var strMessage = string.Empty;

                var postedFile = this.cmdBrowse.PostedFile;

                // Get localized Strings
                Localization.GetString("InvalidExt", this.LocalResourceFile);
                var strFileName = Path.GetFileName(postedFile.FileName);
                if (!string.IsNullOrEmpty(postedFile.FileName))
                {
                    switch (this.FileType)
                    {
                        case UploadType.File: // content files
                            try
                            {
                                var folder = FolderManager.Instance.GetFolder(this.ddlFolders.SelectedItemValueAsInt);
                                var fileManager = Services.FileSystem.FileManager.Instance;
                                var file = fileManager.AddFile(folder, strFileName, postedFile.InputStream, true, true, postedFile.ContentType);
                                if (this.chkUnzip.Checked && file.Extension == "zip")
                                {
                                    fileManager.UnzipFile(file, folder);
                                }
                            }
                            catch (PermissionsNotMetException exc)
                            {
                                Logger.Warn(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), this.ddlFolders.SelectedItemValueAsInt);
                            }
                            catch (NoSpaceAvailableException exc)
                            {
                                Logger.Warn(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), strFileName);
                            }
                            catch (InvalidFileExtensionException exc)
                            {
                                Logger.Warn(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), strFileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                            }
                            catch (Exception exc)
                            {
                                Logger.Error(exc);
                                strMessage += "<br />" + string.Format(Localization.GetString("SaveFileError"), strFileName);
                            }

                            break;
                    }
                }
                else
                {
                    strMessage = Localization.GetString("NoFile", this.LocalResourceFile);
                }

                if (this.phPaLogs.Controls.Count > 0)
                {
                    this.tblLogs.Visible = true;
                }
                else if (string.IsNullOrEmpty(strMessage))
                {
                    Skin.AddModuleMessage(this, string.Format(Localization.GetString("FileUploadSuccess", this.LocalResourceFile), strFileName), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
                else
                {
                    this.lblMessage.Text = strMessage;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The cmdReturn_Click runs when the Return Button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdReturn_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.ReturnURL(), true);
        }
    }
}
