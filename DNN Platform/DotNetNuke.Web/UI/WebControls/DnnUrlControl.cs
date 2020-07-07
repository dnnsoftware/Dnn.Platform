// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Common;

    using Globals = DotNetNuke.Common.Globals;

    public abstract class DnnUrlControl : UserControlBase
    {
        protected Panel ErrorRow;
        protected Panel FileRow;
        protected Panel ImagesRow;
        protected Panel TabRow;
        protected Panel TypeRow;
        protected Panel URLRow;
        protected Panel UserRow;
        protected DropDownList cboImages;
        protected DnnPageDropDownList cboTabs;
        protected DropDownList cboUrls;
        protected CheckBox chkLog;
        protected CheckBox chkNewWindow;
        protected CheckBox chkTrack;
        protected LinkButton cmdAdd;
        protected LinkButton cmdDelete;
        protected LinkButton cmdSelect;
        protected Label lblImages;
        protected Label lblMessage;
        protected Label lblTab;
        protected Label lblURL;
        protected Label lblURLType;
        protected Label lblUser;
        protected RadioButtonList optType;
        protected TextBox txtUrl;
        protected TextBox txtUser;
        protected DnnFilePickerUploader ctlFile;
        private bool _doChangeURL;
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
                    return this.ViewState["FileFilter"].ToString();
                }

                return string.Empty;
            }

            set
            {
                this.ViewState["FileFilter"] = value;
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
                            int id;
                            if (int.TryParse(strTab, out id) && id >= 0)
                            {
                                r = strTab;
                            }
                        }

                        break;
                    case "F":
                        if (this.ctlFile.FileID > Null.NullInteger)
                        {
                            r = "FileID=" + this.ctlFile.FileID;
                        }
                        else
                        {
                            r = string.Empty;
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
                    this.txtUser.Width = Unit.Parse(value);
                    this.ViewState["SkinControlWidth"] = value;
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // prevent unauthorized access
            if (this.Request.IsAuthenticated == false)
            {
                this.Visible = false;
            }

            ClientResourceManager.EnableAsyncPostBackHandler();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.optType.SelectedIndexChanged += this.optType_SelectedIndexChanged;
            this.cmdAdd.Click += this.cmdAdd_Click;
            this.cmdDelete.Click += this.cmdDelete_Click;
            this.cmdSelect.Click += this.cmdSelect_Click;

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

                this.DoCorrectRadioButtonList();

                if (this._doRenderTypeControls)
                {
                    this.DoRenderTypeControls();
                }

                this.ViewState["Url"] = null;
                this.ViewState["IsUrlControlLoaded"] = "Loaded";

                this.ctlFile.FileFilter = this.FileFilter;
            }
            catch (Exception exc)
            {
                // Let's detect possible problems
                Exceptions.LogException(new Exception("Error rendering URLControl subcontrols.", exc));
            }
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
        }

        protected void optType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Type changed, render the correct control set
            this.ViewState["UrlType"] = this.optType.SelectedItem.Value;
            this._doRenderTypeControls = true;
        }

        private void LoadUrls()
        {
            var objUrls = new UrlController();
            this.cboUrls.Items.Clear();
            this.cboUrls.DataSource = objUrls.GetUrls(this._objPortal.PortalID);
            this.cboUrls.DataBind();
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

                        this.cboTabs.IncludeAllTabTypes = false;
                        this.cboTabs.IncludeActiveTab = this.IncludeActiveTab;
                        this.cboTabs.IncludeDisabledTabs = true;
                        this.cboTabs.DisabledNotSelectable = true;
                        this.cboTabs.UndefinedItem = new ListItem(DynamicSharedConstants.Unspecified, string.Empty);

                        if (!string.IsNullOrEmpty(_Url))
                        {
                            PortalSettings _settings = PortalController.Instance.GetCurrentPortalSettings();
                            var tabId = int.Parse(_Url);
                            var page = TabController.Instance.GetTab(tabId, _settings.PortalId);
                            this.cboTabs.SelectedPage = page;
                        }

                        break;
                    case "F": // file
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = true;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;

                        // select folder
                        // We Must check if selected folder has changed because of a property change (Secure, Database)
                        string FileName = string.Empty;
                        string FolderPath = string.Empty;
                        string LastFileName = string.Empty;
                        string LastFolderPath = string.Empty;

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

                        this.ctlFile.FilePath = FolderPath + FileName;

                        this.txtUrl.Visible = false;
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
