#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

#endregion

namespace DotNetNuke.UI.UserControls
{
    public abstract class UrlControl : UserControlBase
    {
        #region "Private Members"

        protected Panel ErrorRow;
        protected Panel FileRow;
        protected Panel ImagesRow;
        protected Panel TabRow;
        protected Panel TypeRow;
        protected Panel URLRow;
        protected Panel UserRow;
        private bool _doChangeURL;
        private bool _doReloadFiles;
        private bool _doReloadFolders;
        private bool _doRenderTypeControls;
        private bool _doRenderTypes;
        private string _localResourceFile;
        private PortalInfo _objPortal;
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

        #endregion

        #region "Public Properties"

        public string FileFilter
        {
            get
            {
                if (ViewState["FileFilter"] != null)
                {
                    return Convert.ToString(ViewState["FileFilter"]);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["FileFilter"] = value;
                if (IsTrackingViewState)
                {
                    _doReloadFiles = true;
                }
            }
        }

        public bool IncludeActiveTab
        {
            get
            {
                if (ViewState["IncludeActiveTab"] != null)
                {
                    return Convert.ToBoolean(ViewState["IncludeActiveTab"]);
                }
                else
                {
                    return false; //Set as default
                }
            }
            set
            {
                ViewState["IncludeActiveTab"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypeControls = true;
                }
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (String.IsNullOrEmpty(_localResourceFile))
                {
                    fileRoot = TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/URLControl.ascx";
                }
                else
                {
                    fileRoot = _localResourceFile;
                }
                return fileRoot;
            }
            set
            {
                _localResourceFile = value;
            }
        }

        public bool Log
        {
            get
            {
                if (chkLog.Visible)
                {
                    return chkLog.Checked;
                }
                else
                {
                    return false;
                }
            }
        }

        public int ModuleID
        {
            get
            {
                int myMid = -2;
                if (ViewState["ModuleId"] != null)
                {
                    myMid = Convert.ToInt32(ViewState["ModuleId"]);
                }
                else if (Request.QueryString["mid"] != null)
                {
                    Int32.TryParse(Request.QueryString["mid"], out myMid);
                }
                return myMid;
            }
            set
            {
                ViewState["ModuleId"] = value;
            }
        }

        public bool NewWindow
        {
            get
            {
                return chkNewWindow.Visible && chkNewWindow.Checked;
            }
            set
            {
                chkNewWindow.Checked = chkNewWindow.Visible && value;
            }
        }

        public bool Required
        {
            get
            {
                if (ViewState["Required"] != null)
                {
                    return Convert.ToBoolean(ViewState["Required"]);
                }
                else
                {
                    return true; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["Required"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypeControls = true;
                }
            }
        }

        public bool ShowFiles
        {
            get
            {
                if (ViewState["ShowFiles"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowFiles"]);
                }
                else
                {
                    return true; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["ShowFiles"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypes = true;
                }
            }
        }

        public bool ShowImages
        {
            get
            {
                if (ViewState["ShowImages"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowImages"]);
                }
                else
                {
                    return false;
                }
            }
            set
            {
                ViewState["ShowImages"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypes = true;
                }
            }
        }

        public bool ShowLog
        {
            get
            {
                return chkLog.Visible;
            }
            set
            {
                chkLog.Visible = value;
            }
        }

        public bool ShowNewWindow
        {
            get
            {
                return chkNewWindow.Visible;
            }
            set
            {
                chkNewWindow.Visible = value;
            }
        }

        public bool ShowNone
        {
            get
            {
                if (ViewState["ShowNone"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowNone"]);
                }
                else
                {
                    return false; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["ShowNone"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypes = true;
                }
            }
        }

        public bool ShowTabs
        {
            get
            {
                if (ViewState["ShowTabs"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowTabs"]);
                }
                else
                {
                    return true; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["ShowTabs"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypes = true;
                }
            }
        }

        public bool ShowTrack
        {
            get
            {
                return chkTrack.Visible;
            }
            set
            {
                chkTrack.Visible = value;
            }
        }

        public bool ShowUpLoad
        {
            get
            {
                if (ViewState["ShowUpLoad"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowUpLoad"]);
                }
                else
                {
                    return true; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["ShowUpLoad"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypeControls = true;
                }
            }
        }

        public bool ShowUrls
        {
            get
            {
                if (ViewState["ShowUrls"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowUrls"]);
                }
                else
                {
                    return true; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["ShowUrls"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypes = true;
                }
            }
        }

        public bool ShowUsers
        {
            get
            {
                if (ViewState["ShowUsers"] != null)
                {
                    return Convert.ToBoolean(ViewState["ShowUsers"]);
                }
                else
                {
                    return false; //Set as default in the old variable
                }
            }
            set
            {
                ViewState["ShowUsers"] = value;
                if (IsTrackingViewState)
                {
                    _doRenderTypes = true;
                }
            }
        }

        public bool Track
        {
            get
            {
                if (chkTrack.Visible)
                {
                    return chkTrack.Checked;
                }
                else
                {
                    return false;
                }
            }
        }

        public string Url
        {
            get
            {
                string r = "";
                string strCurrentType = "";
                if (optType.Items.Count > 0 && optType.SelectedIndex >= 0)
                {
                    strCurrentType = optType.SelectedItem.Value;
                }
                switch (strCurrentType)
                {
                    case "I":
                        if (cboImages.SelectedItem != null)
                        {
                            r = cboImages.SelectedItem.Value;
                        }
                        break;
                    case "U":
                        if (cboUrls.Visible)
                        {
                            if (cboUrls.SelectedItem != null)
                            {
                                r = cboUrls.SelectedItem.Value;
                                txtUrl.Text = r;
                            }
                        }
                        else
                        {
                            string mCustomUrl = txtUrl.Text;
                            if (mCustomUrl.ToLower() == "http://")
                            {
                                r = "";
                            }
                            else
                            {
                                r = Globals.AddHTTP(mCustomUrl);
                            }
                        }
                        break;
                    case "T":
                        string strTab = "";
                        if (cboTabs.SelectedItem != null)
                        {
                            strTab = cboTabs.SelectedItem.Value;
                            if (Globals.NumberMatchRegex.IsMatch(strTab) && (Convert.ToInt32(strTab) >= 0))
                            {
                                r = strTab;
                            }
                        }
                        break;
                    case "F":
                        if (cboFiles.SelectedItem != null)
                        {
                            if (!String.IsNullOrEmpty(cboFiles.SelectedItem.Value))
                            {
                                r = "FileID=" + cboFiles.SelectedItem.Value;
                            }
                            else
                            {
                                r = "";
                            }
                        }
                        break;
                    case "M":
                        if (!String.IsNullOrEmpty(txtUser.Text))
                        {
                            UserInfo objUser = UserController.GetCachedUser(_objPortal.PortalID, txtUser.Text);
                            if (objUser != null)
                            {
                                r = "UserID=" + objUser.UserID;
                            }
                            else
                            {
                                lblMessage.Text = Localization.GetString("NoUser", LocalResourceFile);
                                ErrorRow.Visible = true;
                                txtUser.Text = "";
                            }
                        }
                        break;
                }
                return r;
            }
            set
            {
                ViewState["Url"] = value;
                txtUrl.Text = string.Empty;

                if (IsTrackingViewState)
                {
                    _doChangeURL = true;
                    _doReloadFiles = true;
                }
            }
        }

        public string UrlType
        {
            get
            {
                return Convert.ToString(ViewState["UrlType"]);
            }
            set
            {
                if (value != null && !String.IsNullOrEmpty(value.Trim()))
                {
                    ViewState["UrlType"] = value;
                    if (IsTrackingViewState)
                    {
                        _doChangeURL = true;
                    }
                }
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(ViewState["SkinControlWidth"]);
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    cboUrls.Width = Unit.Parse(value);
                    txtUrl.Width = Unit.Parse(value);
                    cboImages.Width = Unit.Parse(value);
                    cboTabs.Width = Unit.Parse(value);
                    cboFolders.Width = Unit.Parse(value);
                    cboFiles.Width = Unit.Parse(value);
                    txtUser.Width = Unit.Parse(value);
                    ViewState["SkinControlWidth"] = value;
                }
            }
        }

        #endregion

        #region "Private Methods"

        private ArrayList GetFileList(bool NoneSpecified)
        {
            int PortalId = Null.NullInteger;

            if ((!IsHostMenu) || (Request.QueryString["pid"] != null))
            {
                PortalId = _objPortal.PortalID;
            }
            return Globals.GetFileList(PortalId, FileFilter, NoneSpecified, cboFolders.SelectedItem.Value, false);
        }

        private void LoadFolders(string Permissions)
        {
            int PortalId = Null.NullInteger;
            cboFolders.Items.Clear();

            if ((!IsHostMenu) || (Request.QueryString["pid"] != null))
            {
                PortalId = _objPortal.PortalID;
            }
            var folders = FolderManager.Instance.GetFolders(UserController.Instance.GetCurrentUserInfo(), Permissions);
            foreach (FolderInfo folder in folders)
            {
                var FolderItem = new ListItem();
                if (folder.FolderPath == Null.NullString)
                {
                    FolderItem.Text = Localization.GetString("Root", LocalResourceFile);
                }
                else
                {
                    FolderItem.Text = folder.DisplayPath;
                }
                FolderItem.Value = folder.FolderPath;
                cboFolders.Items.Add(FolderItem);
            }
        }

        private void LoadUrls()
        {
            var objUrls = new UrlController();
            cboUrls.Items.Clear();
            cboUrls.DataSource = objUrls.GetUrls(_objPortal.PortalID);
            cboUrls.DataBind();
        }

        private void SetStorageLocationType()
        {
            string FolderName = cboFolders.SelectedValue;

            //Check to see if this is the 'Root' folder, if so we cannot rely on its text value because it is something and not an empty string that we need to lookup the 'root' folder
            if (cboFolders.SelectedValue == string.Empty)
            {
                FolderName = "";
            }
            var objFolderInfo = FolderManager.Instance.GetFolder(PortalSettings.PortalId, FolderName);
            if (objFolderInfo != null)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(objFolderInfo.PortalID, objFolderInfo.FolderMappingID);
                if (folderMapping.MappingName == "Standard")
                {
                    imgStorageLocationType.Visible = false;
                }
                else
                {
                    imgStorageLocationType.Visible = true;
                    imgStorageLocationType.ImageUrl = FolderProvider.Instance(folderMapping.FolderProviderType).GetFolderProviderIconPath();
                }
            }
        }

        private void DoChangeURL()
        {
            string _Url = Convert.ToString(ViewState["Url"]);
            string _Urltype = Convert.ToString(ViewState["UrlType"]);
            if (!String.IsNullOrEmpty(_Url))
            {
                var objUrls = new UrlController();
                string TrackingUrl = _Url;

                _Urltype = Globals.GetURLType(_Url).ToString("g").Substring(0, 1);
                if (_Urltype == "U" && (_Url.StartsWith("~/" + PortalSettings.DefaultIconLocation, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _Urltype = "I";
                }
                ViewState["UrlType"] = _Urltype;
                if (_Urltype == "F")
                {
                    if (_Url.ToLower().StartsWith("fileid="))
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
                        //to handle legacy scenarios before the introduction of the FileServerHandler
                        var fileName = Path.GetFileName(_Url);
                        var folderPath = _Url.Substring(0, _Url.LastIndexOf(fileName));
                        var folder = FolderManager.Instance.GetFolder(_objPortal.PortalID, folderPath);
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
                    if (_Url.ToLower().StartsWith("userid="))
                    {
                        UserInfo objUser = UserController.GetUserById(_objPortal.PortalID, int.Parse(_Url.Substring(7)));
                        if (objUser != null)
                        {
                            _Url = objUser.Username;
                        }
                    }
                }
                UrlTrackingInfo objUrlTracking = objUrls.GetUrlTracking(_objPortal.PortalID, TrackingUrl, ModuleID);
                if (objUrlTracking != null)
                {
                    chkNewWindow.Checked = objUrlTracking.NewWindow;
                    chkTrack.Checked = objUrlTracking.TrackClicks;
                    chkLog.Checked = objUrlTracking.LogActivity;
                }
                else //the url does not exist in the tracking table
                {
                    chkTrack.Checked = false;
                    chkLog.Checked = false;
                }
                ViewState["Url"] = _Url;
            }
            else
            {
                if (!String.IsNullOrEmpty(_Urltype))
                {
                    optType.ClearSelection();
                    if (optType.Items.FindByValue(_Urltype) != null)
                    {
                        optType.Items.FindByValue(_Urltype).Selected = true;
                    }
                    else
                    {
                        optType.Items[0].Selected = true;
                    }
                }
                else
                {
                    if (optType.Items.Count > 0)
                    {
                        optType.ClearSelection();
                        optType.Items[0].Selected = true;
                    }
                }
                chkNewWindow.Checked = false; //Need check
                chkTrack.Checked = false; //Need check
                chkLog.Checked = false; //Need check
            }

            //Url type changed, then we must draw the controlos for that type
            _doRenderTypeControls = true;
        }

        private void DoRenderTypes()
        {
            //We must clear the list to keep the same item order
            string strCurrent = "";
            if (optType.SelectedIndex >= 0)
            {
                strCurrent = optType.SelectedItem.Value; //Save current selected value
            }
            optType.Items.Clear();
            if (ShowNone)
            {
                if (optType.Items.FindByValue("N") == null)
                {
                    optType.Items.Add(new ListItem(Localization.GetString("NoneType", LocalResourceFile), "N"));
                }
            }
            else
            {
                if (optType.Items.FindByValue("N") != null)
                {
                    optType.Items.Remove(optType.Items.FindByValue("N"));
                }
            }
            if (ShowUrls)
            {
                if (optType.Items.FindByValue("U") == null)
                {
                    optType.Items.Add(new ListItem(Localization.GetString("URLType", LocalResourceFile), "U"));
                }
            }
            else
            {
                if (optType.Items.FindByValue("U") != null)
                {
                    optType.Items.Remove(optType.Items.FindByValue("U"));
                }
            }
            if (ShowTabs)
            {
                if (optType.Items.FindByValue("T") == null)
                {
                    optType.Items.Add(new ListItem(Localization.GetString("TabType", LocalResourceFile), "T"));
                }
            }
            else
            {
                if (optType.Items.FindByValue("T") != null)
                {
                    optType.Items.Remove(optType.Items.FindByValue("T"));
                }
            }
            if (ShowFiles)
            {
                if (optType.Items.FindByValue("F") == null)
                {
                    optType.Items.Add(new ListItem(Localization.GetString("FileType", LocalResourceFile), "F"));
                }
            }
            else
            {
                if (optType.Items.FindByValue("F") != null)
                {
                    optType.Items.Remove(optType.Items.FindByValue("F"));
                }
            }
            if (ShowImages)
            {
                if (optType.Items.FindByValue("I") == null)
                {
                    optType.Items.Add(new ListItem(Localization.GetString("ImageType", LocalResourceFile), "I"));
                }
            }
            else
            {
                if (optType.Items.FindByValue("I") != null)
                {
                    optType.Items.Remove(optType.Items.FindByValue("I"));
                }
            }
            if (ShowUsers)
            {
                if (optType.Items.FindByValue("M") == null)
                {
                    optType.Items.Add(new ListItem(Localization.GetString("UserType", LocalResourceFile), "M"));
                }
            }
            else
            {
                if (optType.Items.FindByValue("M") != null)
                {
                    optType.Items.Remove(optType.Items.FindByValue("M"));
                }
            }
            if (optType.Items.Count > 0)
            {
                if (!String.IsNullOrEmpty(strCurrent))
                {
                    if (optType.Items.FindByValue(strCurrent) != null)
                    {
                        optType.Items.FindByValue(strCurrent).Selected = true;
                    }
                    else
                    {
                        optType.Items[0].Selected = true;
                        _doRenderTypeControls = true; //Type changed, re-draw
                    }
                }
                else
                {
                    optType.Items[0].Selected = true;
                    _doRenderTypeControls = true; //Type changed, re-draw
                }
                TypeRow.Visible = optType.Items.Count > 1;
            }
            else
            {
                TypeRow.Visible = false;
            }
        }

        private void DoCorrectRadioButtonList()
        {
            string _Urltype = Convert.ToString(ViewState["UrlType"]);

            if (optType.Items.Count > 0)
            {
                optType.ClearSelection();
                if (!String.IsNullOrEmpty(_Urltype))
                {
                    if (optType.Items.FindByValue(_Urltype) != null)
                    {
                        optType.Items.FindByValue(_Urltype).Selected = true;
                    }
                    else
                    {
                        optType.Items[0].Selected = true;
                        _Urltype = optType.Items[0].Value;
                        ViewState["UrlType"] = _Urltype;
                    }
                }
                else
                {
                    optType.Items[0].Selected = true;
                    _Urltype = optType.Items[0].Value;
                    ViewState["UrlType"] = _Urltype;
                }
            }
        }

        private void DoRenderTypeControls()
        {
            string _Url = Convert.ToString(ViewState["Url"]);
            string _Urltype = Convert.ToString(ViewState["UrlType"]);
            var objUrls = new UrlController();
            if (!String.IsNullOrEmpty(_Urltype))
            {
                //load listitems
                switch (optType.SelectedItem.Value)
                {
                    case "N": //None
                        URLRow.Visible = false;
                        TabRow.Visible = false;
                        FileRow.Visible = false;
                        UserRow.Visible = false;
                        ImagesRow.Visible = false;
                        break;
                    case "I": //System Image
                        URLRow.Visible = false;
                        TabRow.Visible = false;
                        FileRow.Visible = false;
                        UserRow.Visible = false;
                        ImagesRow.Visible = true;

                        cboImages.Items.Clear();

                        string strImagesFolder = Path.Combine(Globals.ApplicationMapPath, PortalSettings.DefaultIconLocation.Replace('/', '\\'));
                        foreach (string strImage in Directory.GetFiles(strImagesFolder))
                        {
                            string img = strImage.Replace(strImagesFolder, "").Trim('/').Trim('\\');
                            cboImages.Items.Add(new ListItem(img, string.Format("~/{0}/{1}", PortalSettings.DefaultIconLocation, img).ToLower()));
                        }

                        ListItem selecteItem = cboImages.Items.FindByValue(_Url.ToLower());
                        if (selecteItem != null)
                        {
                            selecteItem.Selected = true;
                        }
                        break;

                    case "U": //Url
                        URLRow.Visible = true;
                        TabRow.Visible = false;
                        FileRow.Visible = false;
                        UserRow.Visible = false;
                        ImagesRow.Visible = false;
                        if (String.IsNullOrEmpty(txtUrl.Text))
                        {
                            txtUrl.Text = _Url;
                        }
                        if (String.IsNullOrEmpty(txtUrl.Text))
                        {
                            txtUrl.Text = "http://";
                        }
                        txtUrl.Visible = true;

                        cmdSelect.Visible = true;

                        cboUrls.Visible = false;
                        cmdAdd.Visible = false;
                        cmdDelete.Visible = false;
                        break;
                    case "T": //tab
                        URLRow.Visible = false;
                        TabRow.Visible = true;
                        FileRow.Visible = false;
                        UserRow.Visible = false;
                        ImagesRow.Visible = false;

                        cboTabs.Items.Clear();

                        PortalSettings _settings = PortalController.Instance.GetCurrentPortalSettings();
                        cboTabs.DataSource = TabController.GetPortalTabs(_settings.PortalId, Null.NullInteger, !Required, "none available", true, false, false, true, false);
                        cboTabs.DataBind();
                        if (cboTabs.Items.FindByValue(_Url) != null)
                        {
                            cboTabs.Items.FindByValue(_Url).Selected = true;
                        }

                        if (!IncludeActiveTab && cboTabs.Items.FindByValue(_settings.ActiveTab.TabID.ToString()) != null)
                        {
                            cboTabs.Items.FindByValue(_settings.ActiveTab.TabID.ToString()).Attributes.Add("disabled", "disabled");
                        }
                        break;
                    case "F": //file
                        URLRow.Visible = false;
                        TabRow.Visible = false;
                        FileRow.Visible = true;
                        UserRow.Visible = false;
                        ImagesRow.Visible = false;

                        if (ViewState["FoldersLoaded"] == null || _doReloadFolders)
                        {
                            LoadFolders("BROWSE,ADD");
                            ViewState["FoldersLoaded"] = "Y";
                        }
                        if (cboFolders.Items.Count == 0)
                        {
                            lblMessage.Text = Localization.GetString("NoPermission", LocalResourceFile);
                            ErrorRow.Visible = true;
                            FileRow.Visible = false;
                            return;
                        }

                        //select folder
                        //We Must check if selected folder has changed because of a property change (Secure, Database)
                        string FileName = string.Empty;
                        string FolderPath = string.Empty;
                        string LastFileName = string.Empty;
                        string LastFolderPath = string.Empty;
                        bool _MustRedrawFiles = false;
                        //Let's try to remember last selection
                        if (ViewState["LastFolderPath"] != null)
                        {
                            LastFolderPath = Convert.ToString(ViewState["LastFolderPath"]);
                        }
                        if (ViewState["LastFileName"] != null)
                        {
                            LastFileName = Convert.ToString(ViewState["LastFileName"]);
                        }
                        if (_Url != string.Empty)
                        {
                            //Let's use the new URL
                            FileName = _Url.Substring(_Url.LastIndexOf("/") + 1);
                            FolderPath = _Url.Replace(FileName, "");
                        }
                        else
                        {
                            //Use last settings
                            FileName = LastFileName;
                            FolderPath = LastFolderPath;
                        }
                        if (cboFolders.Items.FindByValue(FolderPath) != null)
                        {
                            cboFolders.ClearSelection();
                            cboFolders.Items.FindByValue(FolderPath).Selected = true;
                        }
                        else if (cboFolders.Items.Count > 0)
                        {
                            cboFolders.ClearSelection();
                            cboFolders.Items[0].Selected = true;
                            FolderPath = cboFolders.Items[0].Value;
                        }
                        if (ViewState["FilesLoaded"] == null || FolderPath != LastFolderPath || _doReloadFiles)
                        {
                            //Reload files only if property change or not same folder
                            _MustRedrawFiles = true;
                            ViewState["FilesLoaded"] = "Y";
                        }
                        else
                        {
                            if (cboFiles.Items.Count > 0)
                            {
                                if ((Required && String.IsNullOrEmpty(cboFiles.Items[0].Value)) || (!Required && !String.IsNullOrEmpty(cboFiles.Items[0].Value)))
                                {
                                    //Required state has changed, so we need to reload files
                                    _MustRedrawFiles = true;
                                }
                            }
                            else if (!Required)
                            {
                                //Required state has changed, so we need to reload files
                                _MustRedrawFiles = true;
                            }
                        }
                        if (_MustRedrawFiles)
                        {
                            cboFiles.DataSource = GetFileList(!Required);
                            cboFiles.DataBind();
                            if (cboFiles.Items.FindByText(FileName) != null)
                            {
                                cboFiles.ClearSelection();
                                cboFiles.Items.FindByText(FileName).Selected = true;
                            }
                        }
                        cboFiles.Visible = true;
                        txtFile.Visible = false;

                        FolderInfo objFolder = (FolderInfo)FolderManager.Instance.GetFolder(_objPortal.PortalID, FolderPath);
                        cmdUpload.Visible = ShowUpLoad && FolderPermissionController.CanAddFolder(objFolder);

                        SetStorageLocationType();
                        txtUrl.Visible = false;
                        cmdSave.Visible = false;
                        cmdCancel.Visible = false;

                        if (cboFolders.SelectedIndex >= 0)
                        {
                            ViewState["LastFolderPath"] = cboFolders.SelectedValue;
                        }
                        else
                        {
                            ViewState["LastFolderPath"] = "";
                        }
                        if (cboFiles.SelectedIndex >= 0)
                        {
                            ViewState["LastFileName"] = cboFiles.SelectedValue;
                        }
                        else
                        {
                            ViewState["LastFileName"] = "";
                        }
                        break;
                    case "M": //membership users
                        URLRow.Visible = false;
                        TabRow.Visible = false;
                        FileRow.Visible = false;
                        UserRow.Visible = true;
                        ImagesRow.Visible = false;
                        if (String.IsNullOrEmpty(txtUser.Text))
                        {
                            txtUser.Text = _Url;
                        }
                        break;
                }
            }
            else
            {
                URLRow.Visible = false;
                ImagesRow.Visible = false;
                TabRow.Visible = false;
                FileRow.Visible = false;
                UserRow.Visible = false;
            }
        }

        #endregion

        #region "Event Handlers"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            AJAX.RegisterPostBackControl(FindControl("cmdSave"));

            //prevent unauthorized access
            if (Request.IsAuthenticated == false)
            {
                Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboFolders.SelectedIndexChanged += cboFolders_SelectedIndexChanged;
            optType.SelectedIndexChanged += optType_SelectedIndexChanged;
            cmdAdd.Click += cmdAdd_Click;
            cmdCancel.Click += cmdCancel_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdSave.Click += cmdSave_Click;
            cmdSelect.Click += cmdSelect_Click;
            cmdUpload.Click += cmdUpload_Click;

            ErrorRow.Visible = false;

            try
            {
                if ((Request.QueryString["pid"] != null) && (Globals.IsHostTab(PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    _objPortal = PortalController.Instance.GetPortal(Int32.Parse(Request.QueryString["pid"]));
                }
                else
                {
                    _objPortal = PortalController.Instance.GetPortal(PortalSettings.PortalId);
                }
                if (ViewState["IsUrlControlLoaded"] == null)
                {
                    //If Not Page.IsPostBack Then
                    //let's make at least an initialization
                    //The type radio button must be initialized
                    //The url must be initialized no matter its value
                    _doRenderTypes = true;
                    _doChangeURL = true;
                    ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));
                    //The following line was mover to the pre-render event to ensure render for the first time
                    //ViewState("IsUrlControlLoaded") = "Loaded"
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                if (_doRenderTypes)
                {
                    DoRenderTypes();
                }
                if (_doChangeURL)
                {
                    DoChangeURL();
                }
                if (_doReloadFolders || _doReloadFiles)
                {
                    DoCorrectRadioButtonList();
                    _doRenderTypeControls = true;
                }
                if (_doRenderTypeControls)
                {
                    if (!(_doReloadFolders || _doReloadFiles))
                    {
                        DoCorrectRadioButtonList();
                    }
                    DoRenderTypeControls();
                }
                ViewState["Url"] = null;
                ViewState["IsUrlControlLoaded"] = "Loaded";
            }
            catch (Exception exc)
            {
                //Let's detect possible problems
                Exceptions.LogException(new Exception("Error rendering URLControl subcontrols.", exc));
            }
        }

        protected void cboFolders_SelectedIndexChanged(Object sender, EventArgs e)
        {
            int PortalId = Null.NullInteger;

            if (!IsHostMenu || Request.QueryString["pid"] != null)
            {
                PortalId = _objPortal.PortalID;
            }
            var objFolder = FolderManager.Instance.GetFolder(PortalId, cboFolders.SelectedValue);
            if (FolderPermissionController.CanAddFolder((FolderInfo)objFolder))
            {
                if (!txtFile.Visible)
                {
                    cmdSave.Visible = false;
                    //only show if not already in upload mode and not disabled
                    cmdUpload.Visible = ShowUpLoad;
                }
            }
            else
            {
                //reset controls
                cboFiles.Visible = true;
                cmdUpload.Visible = false;
                txtFile.Visible = false;
                cmdSave.Visible = false;
                cmdCancel.Visible = false;
            }
            cboFiles.Items.Clear();
            cboFiles.DataSource = GetFileList(!Required);
            cboFiles.DataBind();
            SetStorageLocationType();
            if (cboFolders.SelectedIndex >= 0)
            {
                ViewState["LastFolderPath"] = cboFolders.SelectedValue;
            }
            else
            {
                ViewState["LastFolderPath"] = "";
            }
            if (cboFiles.SelectedIndex >= 0)
            {
                ViewState["LastFileName"] = cboFiles.SelectedValue;
            }
            else
            {
                ViewState["LastFileName"] = "";
            }
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            cboUrls.Visible = false;
            cmdSelect.Visible = true;
            txtUrl.Visible = true;
            cmdAdd.Visible = false;
            cmdDelete.Visible = false;
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void cmdCancel_Click(Object sender, EventArgs e)
        {
            cboFiles.Visible = true;
            cmdUpload.Visible = true;
            txtFile.Visible = false;
            cmdSave.Visible = false;
            cmdCancel.Visible = false;
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            if (cboUrls.SelectedItem != null)
            {
                var objUrls = new UrlController();
                objUrls.DeleteUrl(_objPortal.PortalID, cboUrls.SelectedItem.Value);
                LoadUrls(); //we must reload the url list
            }
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void cmdSave_Click(Object sender, EventArgs e)
        {
            cmdUpload.Visible = false;

            //if no file is selected exit
            if (String.IsNullOrEmpty(txtFile.PostedFile.FileName))
            {
                return;
            }
            string ParentFolderName;
            if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
            {
                ParentFolderName = Globals.HostMapPath;
            }
            else
            {
                ParentFolderName = PortalSettings.HomeDirectoryMapPath;
            }
            ParentFolderName += cboFolders.SelectedItem.Value;

            string strExtension = Path.GetExtension(txtFile.PostedFile.FileName).Replace(".", "");
            if (!String.IsNullOrEmpty(FileFilter) && ("," + FileFilter.ToLower()).IndexOf("," + strExtension.ToLower()) == -1)
            {
                //trying to upload a file not allowed for current filter
                lblMessage.Text = string.Format(Localization.GetString("UploadError", LocalResourceFile), FileFilter, strExtension);
                ErrorRow.Visible = true;
            }
            else
            {
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;

                var settings = PortalController.Instance.GetCurrentPortalSettings();
                var portalID = (settings.ActiveTab.ParentId == settings.SuperTabId) ? Null.NullInteger : settings.PortalId;

                var fileName = Path.GetFileName(txtFile.PostedFile.FileName);
                var folderPath = Globals.GetSubFolderPath(ParentFolderName.Replace("/", "\\") + fileName, portalID);

                var folder = folderManager.GetFolder(portalID, folderPath);
                ErrorRow.Visible = false;

                try
                {
                    fileManager.AddFile(folder, fileName, txtFile.PostedFile.InputStream, true, true, ((FileManager)fileManager).GetContentType(Path.GetExtension(fileName)));
                }
                catch (Services.FileSystem.PermissionsNotMetException)
                {
                    lblMessage.Text += "<br />" + string.Format(Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
                    ErrorRow.Visible = true;
                }
                catch (NoSpaceAvailableException)
                {
                    lblMessage.Text += "<br />" + string.Format(Localization.GetString("DiskSpaceExceeded"), fileName);
                    ErrorRow.Visible = true;
                }
                catch (InvalidFileExtensionException)
                {
                    lblMessage.Text += "<br />" + string.Format(Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                    ErrorRow.Visible = true;
                }
                catch (Exception)
                {
                    lblMessage.Text += "<br />" + string.Format(Localization.GetString("SaveFileError"), fileName);
                    ErrorRow.Visible = true;
                }
            }
            if (lblMessage.Text == string.Empty)
            {
                cboFiles.Visible = true;
                cmdUpload.Visible = ShowUpLoad;
                txtFile.Visible = false;
                cmdSave.Visible = false;
                cmdCancel.Visible = false;
                ErrorRow.Visible = false;

                var Root = new DirectoryInfo(ParentFolderName);
                cboFiles.Items.Clear();
                cboFiles.DataSource = GetFileList(false);
                cboFiles.DataBind();

                string FileName = txtFile.PostedFile.FileName.Substring(txtFile.PostedFile.FileName.LastIndexOf("\\") + 1);
                if (cboFiles.Items.FindByText(FileName) != null)
                {
                    cboFiles.Items.FindByText(FileName).Selected = true;
                }
                if (cboFiles.SelectedIndex >= 0)
                {
                    ViewState["LastFileName"] = cboFiles.SelectedValue;
                }
                else
                {
                    ViewState["LastFileName"] = "";
                }
            }
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void cmdSelect_Click(object sender, EventArgs e)
        {
            cboUrls.Visible = true;
            cmdSelect.Visible = false;
            txtUrl.Visible = false;
            cmdAdd.Visible = true;
            cmdDelete.Visible = PortalSecurity.IsInRole(_objPortal.AdministratorRoleName);
            LoadUrls();
            if (cboUrls.Items.FindByValue(txtUrl.Text) != null)
            {
                cboUrls.ClearSelection();
                cboUrls.Items.FindByValue(txtUrl.Text).Selected = true;
            }
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void cmdUpload_Click(object sender, EventArgs e)
        {
            string strSaveFolder = cboFolders.SelectedValue;
            LoadFolders("ADD");
            if (cboFolders.Items.FindByValue(strSaveFolder) != null)
            {
                cboFolders.Items.FindByValue(strSaveFolder).Selected = true;
                cboFiles.Visible = false;
                cmdUpload.Visible = false;
                txtFile.Visible = true;
                cmdSave.Visible = true;
                cmdCancel.Visible = true;
            }
            else
            {
                if (cboFolders.Items.Count > 0)
                {
                    cboFolders.Items[0].Selected = true;
                    cboFiles.Visible = false;
                    cmdUpload.Visible = false;
                    txtFile.Visible = true;
                    cmdSave.Visible = true;
                    cmdCancel.Visible = true;
                }
                else
                {
                    //reset controls
                    LoadFolders("BROWSE,ADD");
                    cboFolders.Items.FindByValue(strSaveFolder).Selected = true;
                    cboFiles.Visible = true;
                    cmdUpload.Visible = false;
                    txtFile.Visible = false;
                    cmdSave.Visible = false;
                    cmdCancel.Visible = false;
                    lblMessage.Text = Localization.GetString("NoWritePermission", LocalResourceFile);
                    ErrorRow.Visible = true;
                }
            }
            _doRenderTypeControls = false; //Must not render on this postback
            _doRenderTypes = false;
            _doChangeURL = false;
            _doReloadFolders = false;
            _doReloadFiles = false;
        }

        protected void optType_SelectedIndexChanged(Object sender, EventArgs e)
        {
            //Type changed, render the correct control set
            ViewState["UrlType"] = optType.SelectedItem.Value;
            _doRenderTypeControls = true;
        }

        #endregion
    }
}