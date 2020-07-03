// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
    using Globals = DotNetNuke.Common.Globals;

    public abstract class UrlControl : UserControlBase
    {
        protected Panel ErrorRow;
        protected Panel FileRow;
        protected Panel ImagesRow;
        protected Panel TabRow;
        protected Panel TypeRow;
        protected Panel URLRow;
        protected Panel UserRow;
        protected DropDownList cboFiles;
        protected DropDownList cboFolders;
        protected DropDownList cboImages;
        protected DropDownList cboTabs;
        protected DropDownList cboUrls;
        protected CheckBox chkLog;
        protected CheckBox chkNewWindow;
        protected CheckBox chkTrack;
        protected LinkButton cmdAdd;
        protected LinkButton cmdCancel;
        protected LinkButton cmdDelete;
        protected LinkButton cmdSave;
        protected LinkButton cmdSelect;
        protected LinkButton cmdUpload;
        protected Image imgStorageLocationType;
        protected Label lblFile;
        protected Label lblFolder;
        protected Label lblImages;
        protected Label lblMessage;
        protected Label lblTab;
        protected Label lblURL;
        protected Label lblURLType;
        protected Label lblUser;
        protected RadioButtonList optType;
        protected HtmlInputFile txtFile;
        protected TextBox txtUrl;
        protected TextBox txtUser;
        private bool _doChangeURL;
        private bool _doReloadFiles;
        private bool _doReloadFolders;
        private bool _doRenderTypeControls;
        private bool _doRenderTypes;
        private string _localResourceFile;
        private PortalInfo _objPortal;

        public bool Log
        {
            get
            {
                if (this.chkLog.Visible)
                {
                    return this.chkLog.Checked;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Track
        {
            get
            {
                if (this.chkTrack.Visible)
                {
                    return this.chkTrack.Checked;
                }
                else
                {
                    return false;
                }
            }
        }

        public string FileFilter
        {
            get
            {
                if (this.ViewState["FileFilter"] != null)
                {
                    return Convert.ToString(this.ViewState["FileFilter"]);
                }
                else
                {
                    return string.Empty;
                }
            }

            set
            {
                this.ViewState["FileFilter"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doReloadFiles = true;
                }
            }
        }

        public bool IncludeActiveTab
        {
            get
            {
                if (this.ViewState["IncludeActiveTab"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["IncludeActiveTab"]);
                }
                else
                {
                    return false; // Set as default
                }
            }

            set
            {
                this.ViewState["IncludeActiveTab"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypeControls = true;
                }
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this._localResourceFile))
                {
                    fileRoot = this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/URLControl.ascx";
                }
                else
                {
                    fileRoot = this._localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this._localResourceFile = value;
            }
        }

        public int ModuleID
        {
            get
            {
                int myMid = -2;
                if (this.ViewState["ModuleId"] != null)
                {
                    myMid = Convert.ToInt32(this.ViewState["ModuleId"]);
                }
                else if (this.Request.QueryString["mid"] != null)
                {
                    int.TryParse(this.Request.QueryString["mid"], out myMid);
                }

                return myMid;
            }

            set
            {
                this.ViewState["ModuleId"] = value;
            }
        }

        public bool NewWindow
        {
            get
            {
                return this.chkNewWindow.Visible && this.chkNewWindow.Checked;
            }

            set
            {
                this.chkNewWindow.Checked = this.chkNewWindow.Visible && value;
            }
        }

        public bool Required
        {
            get
            {
                if (this.ViewState["Required"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["Required"]);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["Required"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypeControls = true;
                }
            }
        }

        public bool ShowFiles
        {
            get
            {
                if (this.ViewState["ShowFiles"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowFiles"]);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowFiles"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypes = true;
                }
            }
        }

        public bool ShowImages
        {
            get
            {
                if (this.ViewState["ShowImages"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowImages"]);
                }
                else
                {
                    return false;
                }
            }

            set
            {
                this.ViewState["ShowImages"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypes = true;
                }
            }
        }

        public bool ShowLog
        {
            get
            {
                return this.chkLog.Visible;
            }

            set
            {
                this.chkLog.Visible = value;
            }
        }

        public bool ShowNewWindow
        {
            get
            {
                return this.chkNewWindow.Visible;
            }

            set
            {
                this.chkNewWindow.Visible = value;
            }
        }

        public bool ShowNone
        {
            get
            {
                if (this.ViewState["ShowNone"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowNone"]);
                }
                else
                {
                    return false; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowNone"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypes = true;
                }
            }
        }

        public bool ShowTabs
        {
            get
            {
                if (this.ViewState["ShowTabs"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowTabs"]);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowTabs"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypes = true;
                }
            }
        }

        public bool ShowTrack
        {
            get
            {
                return this.chkTrack.Visible;
            }

            set
            {
                this.chkTrack.Visible = value;
            }
        }

        public bool ShowUpLoad
        {
            get
            {
                if (this.ViewState["ShowUpLoad"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowUpLoad"]);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowUpLoad"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypeControls = true;
                }
            }
        }

        public bool ShowUrls
        {
            get
            {
                if (this.ViewState["ShowUrls"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowUrls"]);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowUrls"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypes = true;
                }
            }
        }

        public bool ShowUsers
        {
            get
            {
                if (this.ViewState["ShowUsers"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowUsers"]);
                }
                else
                {
                    return false; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowUsers"] = value;
                if (this.IsTrackingViewState)
                {
                    this._doRenderTypes = true;
                }
            }
        }

        public string Url
        {
            get
            {
                string r = string.Empty;
                string strCurrentType = string.Empty;
                if (this.optType.Items.Count > 0 && this.optType.SelectedIndex >= 0)
                {
                    strCurrentType = this.optType.SelectedItem.Value;
                }

                switch (strCurrentType)
                {
                    case "I":
                        if (this.cboImages.SelectedItem != null)
                        {
                            r = this.cboImages.SelectedItem.Value;
                        }

                        break;
                    case "U":
                        if (this.cboUrls.Visible)
                        {
                            if (this.cboUrls.SelectedItem != null)
                            {
                                r = this.cboUrls.SelectedItem.Value;
                                this.txtUrl.Text = r;
                            }
                        }
                        else
                        {
                            string mCustomUrl = this.txtUrl.Text;
                            if (mCustomUrl.Equals("http://", StringComparison.InvariantCultureIgnoreCase))
                            {
                                r = string.Empty;
                            }
                            else
                            {
                                r = Globals.AddHTTP(mCustomUrl);
                            }
                        }

                        break;
                    case "T":
                        string strTab = string.Empty;
                        if (this.cboTabs.SelectedItem != null)
                        {
                            strTab = this.cboTabs.SelectedItem.Value;
                            if (Globals.NumberMatchRegex.IsMatch(strTab) && (Convert.ToInt32(strTab) >= 0))
                            {
                                r = strTab;
                            }
                        }

                        break;
                    case "F":
                        if (this.cboFiles.SelectedItem != null)
                        {
                            if (!string.IsNullOrEmpty(this.cboFiles.SelectedItem.Value))
                            {
                                r = "FileID=" + this.cboFiles.SelectedItem.Value;
                            }
                            else
                            {
                                r = string.Empty;
                            }
                        }

                        break;
                    case "M":
                        if (!string.IsNullOrEmpty(this.txtUser.Text))
                        {
                            UserInfo objUser = UserController.GetCachedUser(this._objPortal.PortalID, this.txtUser.Text);
                            if (objUser != null)
                            {
                                r = "UserID=" + objUser.UserID;
                            }
                            else
                            {
                                this.lblMessage.Text = Localization.GetString("NoUser", this.LocalResourceFile);
                                this.ErrorRow.Visible = true;
                                this.txtUser.Text = string.Empty;
                            }
                        }

                        break;
                }

                return r;
            }

            set
            {
                this.ViewState["Url"] = value;
                this.txtUrl.Text = string.Empty;

                if (this.IsTrackingViewState)
                {
                    this._doChangeURL = true;
                    this._doReloadFiles = true;
                }
            }
        }

        public string UrlType
        {
            get
            {
                return Convert.ToString(this.ViewState["UrlType"]);
            }

            set
            {
                if (value != null && !string.IsNullOrEmpty(value.Trim()))
                {
                    this.ViewState["UrlType"] = value;
                    if (this.IsTrackingViewState)
                    {
                        this._doChangeURL = true;
                    }
                }
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinControlWidth"]);
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.cboUrls.Width = Unit.Parse(value);
                    this.txtUrl.Width = Unit.Parse(value);
                    this.cboImages.Width = Unit.Parse(value);
                    this.cboTabs.Width = Unit.Parse(value);
                    this.cboFolders.Width = Unit.Parse(value);
                    this.cboFiles.Width = Unit.Parse(value);
                    this.txtUser.Width = Unit.Parse(value);
                    this.ViewState["SkinControlWidth"] = value;
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            AJAX.RegisterPostBackControl(this.FindControl("cmdSave"));

            // prevent unauthorized access
            if (this.Request.IsAuthenticated == false)
            {
                this.Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cboFolders.SelectedIndexChanged += this.cboFolders_SelectedIndexChanged;
            this.optType.SelectedIndexChanged += this.optType_SelectedIndexChanged;
            this.cmdAdd.Click += this.cmdAdd_Click;
            this.cmdCancel.Click += this.cmdCancel_Click;
            this.cmdDelete.Click += this.cmdDelete_Click;
            this.cmdSave.Click += this.cmdSave_Click;
            this.cmdSelect.Click += this.cmdSelect_Click;
            this.cmdUpload.Click += this.cmdUpload_Click;

            this.ErrorRow.Visible = false;

            try
            {
                if ((this.Request.QueryString["pid"] != null) && (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    this._objPortal = PortalController.Instance.GetPortal(int.Parse(this.Request.QueryString["pid"]));
                }
                else
                {
                    this._objPortal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                }

                if (this.ViewState["IsUrlControlLoaded"] == null)
                {
                    // If Not Page.IsPostBack Then
                    // let's make at least an initialization
                    // The type radio button must be initialized
                    // The url must be initialized no matter its value
                    this._doRenderTypes = true;
                    this._doChangeURL = true;
                    ClientAPI.AddButtonConfirm(this.cmdDelete, Localization.GetString("DeleteItem"));

                    // The following line was mover to the pre-render event to ensure render for the first time
                    // ViewState("IsUrlControlLoaded") = "Loaded"
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                if (this._doRenderTypes)
                {
                    this.DoRenderTypes();
                }

                if (this._doChangeURL)
                {
                    this.DoChangeURL();
                }

                if (this._doReloadFolders || this._doReloadFiles)
                {
                    this.DoCorrectRadioButtonList();
                    this._doRenderTypeControls = true;
                }

                if (this._doRenderTypeControls)
                {
                    if (!(this._doReloadFolders || this._doReloadFiles))
                    {
                        this.DoCorrectRadioButtonList();
                    }

                    this.DoRenderTypeControls();
                }

                this.ViewState["Url"] = null;
                this.ViewState["IsUrlControlLoaded"] = "Loaded";
            }
            catch (Exception exc)
            {
                // Let's detect possible problems
                Exceptions.LogException(new Exception("Error rendering URLControl subcontrols.", exc));
            }
        }

        protected void cboFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            int PortalId = Null.NullInteger;

            if (!this.IsHostMenu || this.Request.QueryString["pid"] != null)
            {
                PortalId = this._objPortal.PortalID;
            }

            var objFolder = FolderManager.Instance.GetFolder(PortalId, this.cboFolders.SelectedValue);
            if (FolderPermissionController.CanAddFolder((FolderInfo)objFolder))
            {
                if (!this.txtFile.Visible)
                {
                    this.cmdSave.Visible = false;

                    // only show if not already in upload mode and not disabled
                    this.cmdUpload.Visible = this.ShowUpLoad;
                }
            }
            else
            {
                // reset controls
                this.cboFiles.Visible = true;
                this.cmdUpload.Visible = false;
                this.txtFile.Visible = false;
                this.cmdSave.Visible = false;
                this.cmdCancel.Visible = false;
            }

            this.cboFiles.Items.Clear();
            this.cboFiles.DataSource = this.GetFileList(!this.Required);
            this.cboFiles.DataBind();
            this.SetStorageLocationType();
            if (this.cboFolders.SelectedIndex >= 0)
            {
                this.ViewState["LastFolderPath"] = this.cboFolders.SelectedValue;
            }
            else
            {
                this.ViewState["LastFolderPath"] = string.Empty;
            }

            if (this.cboFiles.SelectedIndex >= 0)
            {
                this.ViewState["LastFileName"] = this.cboFiles.SelectedValue;
            }
            else
            {
                this.ViewState["LastFileName"] = string.Empty;
            }

            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            this.cboUrls.Visible = false;
            this.cmdSelect.Visible = true;
            this.txtUrl.Visible = true;
            this.cmdAdd.Visible = false;
            this.cmdDelete.Visible = false;
            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            this.cboFiles.Visible = true;
            this.cmdUpload.Visible = true;
            this.txtFile.Visible = false;
            this.cmdSave.Visible = false;
            this.cmdCancel.Visible = false;
            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            if (this.cboUrls.SelectedItem != null)
            {
                var objUrls = new UrlController();
                objUrls.DeleteUrl(this._objPortal.PortalID, this.cboUrls.SelectedItem.Value);
                this.LoadUrls(); // we must reload the url list
            }

            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void cmdSave_Click(object sender, EventArgs e)
        {
            this.cmdUpload.Visible = false;

            // if no file is selected exit
            if (string.IsNullOrEmpty(this.txtFile.PostedFile.FileName))
            {
                return;
            }

            string ParentFolderName;
            if (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID))
            {
                ParentFolderName = Globals.HostMapPath;
            }
            else
            {
                ParentFolderName = this.PortalSettings.HomeDirectoryMapPath;
            }

            ParentFolderName += this.cboFolders.SelectedItem.Value;

            string strExtension = Path.GetExtension(this.txtFile.PostedFile.FileName).Replace(".", string.Empty);
            if (!string.IsNullOrEmpty(this.FileFilter) && ("," + this.FileFilter.ToLowerInvariant()).IndexOf("," + strExtension.ToLowerInvariant()) == -1)
            {
                // trying to upload a file not allowed for current filter
                this.lblMessage.Text = string.Format(Localization.GetString("UploadError", this.LocalResourceFile), this.FileFilter, strExtension);
                this.ErrorRow.Visible = true;
            }
            else
            {
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;

                var settings = PortalController.Instance.GetCurrentPortalSettings();
                var portalID = (settings.ActiveTab.ParentId == settings.SuperTabId) ? Null.NullInteger : settings.PortalId;

                var fileName = Path.GetFileName(this.txtFile.PostedFile.FileName);
                var folderPath = Globals.GetSubFolderPath(ParentFolderName.Replace("/", "\\") + fileName, portalID);

                var folder = folderManager.GetFolder(portalID, folderPath);
                this.ErrorRow.Visible = false;

                try
                {
                    fileManager.AddFile(folder, fileName, this.txtFile.PostedFile.InputStream, true, true, ((FileManager)fileManager).GetContentType(Path.GetExtension(fileName)));
                }
                catch (Services.FileSystem.PermissionsNotMetException)
                {
                    this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
                    this.ErrorRow.Visible = true;
                }
                catch (NoSpaceAvailableException)
                {
                    this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
                    this.ErrorRow.Visible = true;
                }
                catch (InvalidFileExtensionException)
                {
                    this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                    this.ErrorRow.Visible = true;
                }
                catch (Exception)
                {
                    this.lblMessage.Text += "<br />" + string.Format(Localization.GetString("SaveFileError"), fileName);
                    this.ErrorRow.Visible = true;
                }
            }

            if (this.lblMessage.Text == string.Empty)
            {
                this.cboFiles.Visible = true;
                this.cmdUpload.Visible = this.ShowUpLoad;
                this.txtFile.Visible = false;
                this.cmdSave.Visible = false;
                this.cmdCancel.Visible = false;
                this.ErrorRow.Visible = false;

                var Root = new DirectoryInfo(ParentFolderName);
                this.cboFiles.Items.Clear();
                this.cboFiles.DataSource = this.GetFileList(false);
                this.cboFiles.DataBind();

                string FileName = this.txtFile.PostedFile.FileName.Substring(this.txtFile.PostedFile.FileName.LastIndexOf("\\") + 1);
                if (this.cboFiles.Items.FindByText(FileName) != null)
                {
                    this.cboFiles.Items.FindByText(FileName).Selected = true;
                }

                if (this.cboFiles.SelectedIndex >= 0)
                {
                    this.ViewState["LastFileName"] = this.cboFiles.SelectedValue;
                }
                else
                {
                    this.ViewState["LastFileName"] = string.Empty;
                }
            }

            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void cmdSelect_Click(object sender, EventArgs e)
        {
            this.cboUrls.Visible = true;
            this.cmdSelect.Visible = false;
            this.txtUrl.Visible = false;
            this.cmdAdd.Visible = true;
            this.cmdDelete.Visible = PortalSecurity.IsInRole(this._objPortal.AdministratorRoleName);
            this.LoadUrls();
            if (this.cboUrls.Items.FindByValue(this.txtUrl.Text) != null)
            {
                this.cboUrls.ClearSelection();
                this.cboUrls.Items.FindByValue(this.txtUrl.Text).Selected = true;
            }

            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void cmdUpload_Click(object sender, EventArgs e)
        {
            string strSaveFolder = this.cboFolders.SelectedValue;
            this.LoadFolders("ADD");
            if (this.cboFolders.Items.FindByValue(strSaveFolder) != null)
            {
                this.cboFolders.Items.FindByValue(strSaveFolder).Selected = true;
                this.cboFiles.Visible = false;
                this.cmdUpload.Visible = false;
                this.txtFile.Visible = true;
                this.cmdSave.Visible = true;
                this.cmdCancel.Visible = true;
            }
            else
            {
                if (this.cboFolders.Items.Count > 0)
                {
                    this.cboFolders.Items[0].Selected = true;
                    this.cboFiles.Visible = false;
                    this.cmdUpload.Visible = false;
                    this.txtFile.Visible = true;
                    this.cmdSave.Visible = true;
                    this.cmdCancel.Visible = true;
                }
                else
                {
                    // reset controls
                    this.LoadFolders("BROWSE,ADD");
                    this.cboFolders.Items.FindByValue(strSaveFolder).Selected = true;
                    this.cboFiles.Visible = true;
                    this.cmdUpload.Visible = false;
                    this.txtFile.Visible = false;
                    this.cmdSave.Visible = false;
                    this.cmdCancel.Visible = false;
                    this.lblMessage.Text = Localization.GetString("NoWritePermission", this.LocalResourceFile);
                    this.ErrorRow.Visible = true;
                }
            }

            this._doRenderTypeControls = false; // Must not render on this postback
            this._doRenderTypes = false;
            this._doChangeURL = false;
            this._doReloadFolders = false;
            this._doReloadFiles = false;
        }

        protected void optType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Type changed, render the correct control set
            this.ViewState["UrlType"] = this.optType.SelectedItem.Value;
            this._doRenderTypeControls = true;
        }

        private ArrayList GetFileList(bool NoneSpecified)
        {
            int PortalId = Null.NullInteger;

            if ((!this.IsHostMenu) || (this.Request.QueryString["pid"] != null))
            {
                PortalId = this._objPortal.PortalID;
            }

            return Globals.GetFileList(PortalId, this.FileFilter, NoneSpecified, this.cboFolders.SelectedItem.Value, false);
        }

        private void LoadFolders(string Permissions)
        {
            int PortalId = Null.NullInteger;
            this.cboFolders.Items.Clear();

            if ((!this.IsHostMenu) || (this.Request.QueryString["pid"] != null))
            {
                PortalId = this._objPortal.PortalID;
            }

            var folders = FolderManager.Instance.GetFolders(UserController.Instance.GetCurrentUserInfo(), Permissions);
            foreach (FolderInfo folder in folders)
            {
                var FolderItem = new ListItem();
                if (folder.FolderPath == Null.NullString)
                {
                    FolderItem.Text = Localization.GetString("Root", this.LocalResourceFile);
                }
                else
                {
                    FolderItem.Text = folder.DisplayPath;
                }

                FolderItem.Value = folder.FolderPath;
                this.cboFolders.Items.Add(FolderItem);
            }
        }

        private void LoadUrls()
        {
            var objUrls = new UrlController();
            this.cboUrls.Items.Clear();
            this.cboUrls.DataSource = objUrls.GetUrls(this._objPortal.PortalID);
            this.cboUrls.DataBind();
        }

        private void SetStorageLocationType()
        {
            string FolderName = this.cboFolders.SelectedValue;

            // Check to see if this is the 'Root' folder, if so we cannot rely on its text value because it is something and not an empty string that we need to lookup the 'root' folder
            if (this.cboFolders.SelectedValue == string.Empty)
            {
                FolderName = string.Empty;
            }

            var objFolderInfo = FolderManager.Instance.GetFolder(this.PortalSettings.PortalId, FolderName);
            if (objFolderInfo != null)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(objFolderInfo.PortalID, objFolderInfo.FolderMappingID);
                if (folderMapping.MappingName == "Standard")
                {
                    this.imgStorageLocationType.Visible = false;
                }
                else
                {
                    this.imgStorageLocationType.Visible = true;
                    this.imgStorageLocationType.ImageUrl = FolderProvider.Instance(folderMapping.FolderProviderType).GetFolderProviderIconPath();
                }
            }
        }

        private void DoChangeURL()
        {
            string _Url = Convert.ToString(this.ViewState["Url"]);
            string _Urltype = Convert.ToString(this.ViewState["UrlType"]);
            if (!string.IsNullOrEmpty(_Url))
            {
                var objUrls = new UrlController();
                string TrackingUrl = _Url;

                _Urltype = Globals.GetURLType(_Url).ToString("g").Substring(0, 1);
                if (_Urltype == "U" && _Url.StartsWith("~/" + this.PortalSettings.DefaultIconLocation, StringComparison.InvariantCultureIgnoreCase))
                {
                    _Urltype = "I";
                }

                this.ViewState["UrlType"] = _Urltype;
                if (_Urltype == "F")
                {
                    if (_Url.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        TrackingUrl = _Url;
                        var objFile = FileManager.Instance.GetFile(int.Parse(_Url.Substring(7)));
                        if (objFile != null)
                        {
                            _Url = objFile.Folder + objFile.FileName;
                        }
                    }
                    else
                    {
                        // to handle legacy scenarios before the introduction of the FileServerHandler
                        var fileName = Path.GetFileName(_Url);
                        var folderPath = _Url.Substring(0, _Url.LastIndexOf(fileName));
                        var folder = FolderManager.Instance.GetFolder(this._objPortal.PortalID, folderPath);
                        var fileId = -1;
                        if (folder != null)
                        {
                            var file = FileManager.Instance.GetFile(folder, fileName);
                            if (file != null)
                            {
                                fileId = file.FileId;
                            }
                        }

                        TrackingUrl = "FileID=" + fileId.ToString();
                    }
                }

                if (_Urltype == "M")
                {
                    if (_Url.StartsWith("userid=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        UserInfo objUser = UserController.GetUserById(this._objPortal.PortalID, int.Parse(_Url.Substring(7)));
                        if (objUser != null)
                        {
                            _Url = objUser.Username;
                        }
                    }
                }

                UrlTrackingInfo objUrlTracking = objUrls.GetUrlTracking(this._objPortal.PortalID, TrackingUrl, this.ModuleID);
                if (objUrlTracking != null)
                {
                    this.chkNewWindow.Checked = objUrlTracking.NewWindow;
                    this.chkTrack.Checked = objUrlTracking.TrackClicks;
                    this.chkLog.Checked = objUrlTracking.LogActivity;
                }
                else // the url does not exist in the tracking table
                {
                    this.chkTrack.Checked = false;
                    this.chkLog.Checked = false;
                }

                this.ViewState["Url"] = _Url;
            }
            else
            {
                if (!string.IsNullOrEmpty(_Urltype))
                {
                    this.optType.ClearSelection();
                    if (this.optType.Items.FindByValue(_Urltype) != null)
                    {
                        this.optType.Items.FindByValue(_Urltype).Selected = true;
                    }
                    else
                    {
                        this.optType.Items[0].Selected = true;
                    }
                }
                else
                {
                    if (this.optType.Items.Count > 0)
                    {
                        this.optType.ClearSelection();
                        this.optType.Items[0].Selected = true;
                    }
                }

                this.chkNewWindow.Checked = false; // Need check
                this.chkTrack.Checked = false; // Need check
                this.chkLog.Checked = false; // Need check
            }

            // Url type changed, then we must draw the controlos for that type
            this._doRenderTypeControls = true;
        }

        private void DoRenderTypes()
        {
            // We must clear the list to keep the same item order
            string strCurrent = string.Empty;
            if (this.optType.SelectedIndex >= 0)
            {
                strCurrent = this.optType.SelectedItem.Value; // Save current selected value
            }

            this.optType.Items.Clear();
            if (this.ShowNone)
            {
                if (this.optType.Items.FindByValue("N") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("NoneType", this.LocalResourceFile), "N"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("N") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("N"));
                }
            }

            if (this.ShowUrls)
            {
                if (this.optType.Items.FindByValue("U") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("URLType", this.LocalResourceFile), "U"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("U") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("U"));
                }
            }

            if (this.ShowTabs)
            {
                if (this.optType.Items.FindByValue("T") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("TabType", this.LocalResourceFile), "T"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("T") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("T"));
                }
            }

            if (this.ShowFiles)
            {
                if (this.optType.Items.FindByValue("F") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("FileType", this.LocalResourceFile), "F"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("F") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("F"));
                }
            }

            if (this.ShowImages)
            {
                if (this.optType.Items.FindByValue("I") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("ImageType", this.LocalResourceFile), "I"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("I") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("I"));
                }
            }

            if (this.ShowUsers)
            {
                if (this.optType.Items.FindByValue("M") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("UserType", this.LocalResourceFile), "M"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("M") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("M"));
                }
            }

            if (this.optType.Items.Count > 0)
            {
                if (!string.IsNullOrEmpty(strCurrent))
                {
                    if (this.optType.Items.FindByValue(strCurrent) != null)
                    {
                        this.optType.Items.FindByValue(strCurrent).Selected = true;
                    }
                    else
                    {
                        this.optType.Items[0].Selected = true;
                        this._doRenderTypeControls = true; // Type changed, re-draw
                    }
                }
                else
                {
                    this.optType.Items[0].Selected = true;
                    this._doRenderTypeControls = true; // Type changed, re-draw
                }

                this.TypeRow.Visible = this.optType.Items.Count > 1;
            }
            else
            {
                this.TypeRow.Visible = false;
            }
        }

        private void DoCorrectRadioButtonList()
        {
            string _Urltype = Convert.ToString(this.ViewState["UrlType"]);

            if (this.optType.Items.Count > 0)
            {
                this.optType.ClearSelection();
                if (!string.IsNullOrEmpty(_Urltype))
                {
                    if (this.optType.Items.FindByValue(_Urltype) != null)
                    {
                        this.optType.Items.FindByValue(_Urltype).Selected = true;
                    }
                    else
                    {
                        this.optType.Items[0].Selected = true;
                        _Urltype = this.optType.Items[0].Value;
                        this.ViewState["UrlType"] = _Urltype;
                    }
                }
                else
                {
                    this.optType.Items[0].Selected = true;
                    _Urltype = this.optType.Items[0].Value;
                    this.ViewState["UrlType"] = _Urltype;
                }
            }
        }

        private void DoRenderTypeControls()
        {
            string _Url = Convert.ToString(this.ViewState["Url"]);
            string _Urltype = Convert.ToString(this.ViewState["UrlType"]);
            var objUrls = new UrlController();
            if (!string.IsNullOrEmpty(_Urltype))
            {
                // load listitems
                switch (this.optType.SelectedItem.Value)
                {
                    case "N": // None
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;
                        break;
                    case "I": // System Image
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = true;

                        this.cboImages.Items.Clear();

                        string strImagesFolder = Path.Combine(Globals.ApplicationMapPath, this.PortalSettings.DefaultIconLocation.Replace('/', '\\'));
                        foreach (string strImage in Directory.GetFiles(strImagesFolder))
                        {
                            string img = strImage.Replace(strImagesFolder, string.Empty).Trim('/').Trim('\\');
                            this.cboImages.Items.Add(new ListItem(img, string.Format("~/{0}/{1}", this.PortalSettings.DefaultIconLocation, img).ToLowerInvariant()));
                        }

                        ListItem selecteItem = this.cboImages.Items.FindByValue(_Url.ToLowerInvariant());
                        if (selecteItem != null)
                        {
                            selecteItem.Selected = true;
                        }

                        break;

                    case "U": // Url
                        this.URLRow.Visible = true;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;
                        if (string.IsNullOrEmpty(this.txtUrl.Text))
                        {
                            this.txtUrl.Text = _Url;
                        }

                        if (string.IsNullOrEmpty(this.txtUrl.Text))
                        {
                            this.txtUrl.Text = "http://";
                        }

                        this.txtUrl.Visible = true;

                        this.cmdSelect.Visible = true;

                        this.cboUrls.Visible = false;
                        this.cmdAdd.Visible = false;
                        this.cmdDelete.Visible = false;
                        break;
                    case "T": // tab
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = true;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;

                        this.cboTabs.Items.Clear();

                        PortalSettings _settings = PortalController.Instance.GetCurrentPortalSettings();
                        this.cboTabs.DataSource = TabController.GetPortalTabs(_settings.PortalId, Null.NullInteger, !this.Required, "none available", true, false, false, true, false);
                        this.cboTabs.DataBind();
                        if (this.cboTabs.Items.FindByValue(_Url) != null)
                        {
                            this.cboTabs.Items.FindByValue(_Url).Selected = true;
                        }

                        if (!this.IncludeActiveTab && this.cboTabs.Items.FindByValue(_settings.ActiveTab.TabID.ToString()) != null)
                        {
                            this.cboTabs.Items.FindByValue(_settings.ActiveTab.TabID.ToString()).Attributes.Add("disabled", "disabled");
                        }

                        break;
                    case "F": // file
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = true;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;

                        if (this.ViewState["FoldersLoaded"] == null || this._doReloadFolders)
                        {
                            this.LoadFolders("BROWSE,ADD");
                            this.ViewState["FoldersLoaded"] = "Y";
                        }

                        if (this.cboFolders.Items.Count == 0)
                        {
                            this.lblMessage.Text = Localization.GetString("NoPermission", this.LocalResourceFile);
                            this.ErrorRow.Visible = true;
                            this.FileRow.Visible = false;
                            return;
                        }

                        // select folder
                        // We Must check if selected folder has changed because of a property change (Secure, Database)
                        string FileName = string.Empty;
                        string FolderPath = string.Empty;
                        string LastFileName = string.Empty;
                        string LastFolderPath = string.Empty;
                        bool _MustRedrawFiles = false;

                        // Let's try to remember last selection
                        if (this.ViewState["LastFolderPath"] != null)
                        {
                            LastFolderPath = Convert.ToString(this.ViewState["LastFolderPath"]);
                        }

                        if (this.ViewState["LastFileName"] != null)
                        {
                            LastFileName = Convert.ToString(this.ViewState["LastFileName"]);
                        }

                        if (_Url != string.Empty)
                        {
                            // Let's use the new URL
                            FileName = _Url.Substring(_Url.LastIndexOf("/") + 1);
                            FolderPath = _Url.Replace(FileName, string.Empty);
                        }
                        else
                        {
                            // Use last settings
                            FileName = LastFileName;
                            FolderPath = LastFolderPath;
                        }

                        if (this.cboFolders.Items.FindByValue(FolderPath) != null)
                        {
                            this.cboFolders.ClearSelection();
                            this.cboFolders.Items.FindByValue(FolderPath).Selected = true;
                        }
                        else if (this.cboFolders.Items.Count > 0)
                        {
                            this.cboFolders.ClearSelection();
                            this.cboFolders.Items[0].Selected = true;
                            FolderPath = this.cboFolders.Items[0].Value;
                        }

                        if (this.ViewState["FilesLoaded"] == null || FolderPath != LastFolderPath || this._doReloadFiles)
                        {
                            // Reload files only if property change or not same folder
                            _MustRedrawFiles = true;
                            this.ViewState["FilesLoaded"] = "Y";
                        }
                        else
                        {
                            if (this.cboFiles.Items.Count > 0)
                            {
                                if ((this.Required && string.IsNullOrEmpty(this.cboFiles.Items[0].Value)) || (!this.Required && !string.IsNullOrEmpty(this.cboFiles.Items[0].Value)))
                                {
                                    // Required state has changed, so we need to reload files
                                    _MustRedrawFiles = true;
                                }
                            }
                            else if (!this.Required)
                            {
                                // Required state has changed, so we need to reload files
                                _MustRedrawFiles = true;
                            }
                        }

                        if (_MustRedrawFiles)
                        {
                            this.cboFiles.DataSource = this.GetFileList(!this.Required);
                            this.cboFiles.DataBind();
                            if (this.cboFiles.Items.FindByText(FileName) != null)
                            {
                                this.cboFiles.ClearSelection();
                                this.cboFiles.Items.FindByText(FileName).Selected = true;
                            }
                        }

                        this.cboFiles.Visible = true;
                        this.txtFile.Visible = false;

                        FolderInfo objFolder = (FolderInfo)FolderManager.Instance.GetFolder(this._objPortal.PortalID, FolderPath);
                        this.cmdUpload.Visible = this.ShowUpLoad && FolderPermissionController.CanAddFolder(objFolder);

                        this.SetStorageLocationType();
                        this.txtUrl.Visible = false;
                        this.cmdSave.Visible = false;
                        this.cmdCancel.Visible = false;

                        if (this.cboFolders.SelectedIndex >= 0)
                        {
                            this.ViewState["LastFolderPath"] = this.cboFolders.SelectedValue;
                        }
                        else
                        {
                            this.ViewState["LastFolderPath"] = string.Empty;
                        }

                        if (this.cboFiles.SelectedIndex >= 0)
                        {
                            this.ViewState["LastFileName"] = this.cboFiles.SelectedValue;
                        }
                        else
                        {
                            this.ViewState["LastFileName"] = string.Empty;
                        }

                        break;
                    case "M": // membership users
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = true;
                        this.ImagesRow.Visible = false;
                        if (string.IsNullOrEmpty(this.txtUser.Text))
                        {
                            this.txtUser.Text = _Url;
                        }

                        break;
                }
            }
            else
            {
                this.URLRow.Visible = false;
                this.ImagesRow.Visible = false;
                this.TabRow.Visible = false;
                this.FileRow.Visible = false;
                this.UserRow.Visible = false;
            }
        }
    }
}
