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
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.OutputCache;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Common;
using DotNetNuke.Web.UI;
using DotNetNuke.Web.UI.WebControls;
using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;
using Reflection = DotNetNuke.Framework.Reflection;

#endregion

namespace DotNetNuke.Modules.Admin.Tabs
{
    /// <summary>
    ///   The ManageTabs PortalModuleBase is used to manage a Tab/Page
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///   [cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
    ///   and localisation
    /// </history>
    public partial class ManageTabs : PortalModuleBase
    {
        private TabInfo _tab;
        private string _strAction = "";

        #region Protected Properties

        protected TabInfo Tab
        {
            get
            {
                if (_tab == null)
                {
                    switch (_strAction)
                    {
                        case "":
                        case "add":
                            _tab = new TabInfo { TabID = Null.NullInteger, PortalID = PortalId };
                            break;
                        case "copy":
                            var originalTab = TabController.Instance.GetTab(TabId, PortalId, false);
                            _tab = originalTab.Clone();
                            InitializeWorkflow(_tab);
                            _tab.TabID = Null.NullInteger;
                            _tab.VersionGuid = Guid.NewGuid();
                            _tab.LocalizedVersionGuid = Guid.NewGuid();
                            _tab.UniqueId = Guid.NewGuid();
                            _tab.TabPath = Null.NullString;
                            _tab.DefaultLanguageGuid = Null.NullGuid;
                            _tab.CultureCode = Null.NullString;
                            foreach (var key in originalTab.TabSettings.Keys)
                            {
                                _tab.TabSettings[key] = originalTab.TabSettings[key];
                            }

                            break;
                        default:
                            _tab = TabController.Instance.GetTab(TabId, PortalId, false);
                            break;
                    }
                }
                return _tab;
            }
        }

        protected string ActiveDnnTab
        {
            get
            {
                var activeTab = Request.QueryString["activeTab"];
                if (!string.IsNullOrEmpty(activeTab))
                {
                    var tabControl = FindControl(activeTab);
                    if (tabControl != null)
                    {
                        return tabControl.ClientID;
                    }
                }

                return string.Empty;
            }
        }

        #endregion

        #region Private Methods
        private void InitializeWorkflow(ContentItem contentItem)
        {
            contentItem.StateID = Null.NullInteger;
        }

        private void AddTranslationSubmittedNotification(TabInfo tabInfo, UserInfo translator)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType("TranslationSubmitted");

            var subject = Localization.GetString("NewContentMessage.Subject", LocalResourceFile);
            var body = string.Format(Localization.GetString("NewContentMessage.Body", LocalResourceFile),
                tabInfo.TabName,
                Globals.NavigateURL(tabInfo.TabID, false, PortalSettings, Null.NullString, tabInfo.CultureCode, new string[] { }),
                txtTranslationComment.Text);

            var sender = UserController.GetUserById(PortalSettings.PortalId, PortalSettings.AdministratorId);

            var notification = new Notification { NotificationTypeID = notificationType.NotificationTypeId, Subject = subject, Body = body, IncludeDismissAction = true, SenderUserID = sender.UserID };

            NotificationsController.Instance.SendNotification(notification, PortalSettings.PortalId, null, new List<UserInfo> { translator });
        }

        private void BindBeforeAfterTabControls()
        {
            List<TabInfo> listTabs = null;
            TabInfo parentTab = null;

            if (cboParentTab.SelectedItem != null)
            {
                var parentTabID = cboParentTab.SelectedItemValueAsInt;
                parentTab = TabController.Instance.GetTab(parentTabID, -1, false);
            }

            if (parentTab != null)
            {
                var parentTabCulture = parentTab.CultureCode;
                if (string.IsNullOrEmpty(parentTabCulture))
                {
                    parentTabCulture = PortalController.GetActivePortalLanguage(PortalId);
                }
                listTabs = TabController.Instance.GetTabsByPortal(parentTab.PortalID).WithCulture(parentTabCulture, true).WithParentId(parentTab.TabID);
            }
            else
            {
                listTabs = TabController.Instance.GetTabsByPortal(PortalId).WithCulture(PortalController.GetActivePortalLanguage(PortalId), true).WithParentId(Null.NullInteger);
            }
            listTabs = TabController.GetPortalTabs(listTabs, Null.NullInteger, false, Null.NullString, false, false, false, false, true);
            cboPositionTab.DataSource = listTabs;
            cboPositionTab.DataBind();

            if (parentTab != null && parentTab.IsSuperTab)
            {
                ShowPermissions(false);
            }
            else
            {
                ShowPermissions(true);
            }
        }

        private void BindLocalization(bool rebind)
        {
            cultureLanguageLabel.Language = Tab.CultureCode;
            cultureRow.Visible = false;

            if (Tab.IsNeutralCulture)
            {
                cultureRow.Visible = true;
                readyForTranslationButton.Visible = false;
                defaultCultureMessageLabel.Visible = false;
                defaultCultureMessage.Visible = false;
                if (string.IsNullOrEmpty(_strAction) || _strAction == "add" || _strAction == "copy")
                {
                    cultureRow.Visible = false;
                    cultureTypeRow.Visible = true;
                }
            }
            else if (Tab.DefaultLanguageTab != null)
            {
                defaultCultureMessageLabel.Visible = false;
                defaultCultureMessage.Visible = false;


            }
            else
            {
                defaultCultureMessageLabel.Visible = true;
                defaultCultureMessage.Visible = true;
            }
            if (!Page.IsPostBack)
            {
                BindCLControl();
            }

        }

        protected void BindCLControl()
        {
            if (!localizationPanel.Visible)
            {
                return;
            }

            MakeTranslatable.Visible = false;
            MakeNeutral.Visible = false;
            cmdUpdateLocalization.Visible = false;
            AddMissing.Visible = false;
            if (String.IsNullOrEmpty(_tab.CultureCode))
            {
                CLControl1.Visible = false;
                if (!(string.IsNullOrEmpty(_strAction) || _strAction == "add" || _strAction == "copy"))
                {
                    MakeTranslatable.Visible = true;
                }
            }
            else
            {
                CLControl1.Visible = true;
                CLControl1.enablePageEdit = true;
                CLControl1.BindAll(_tab.TabID);
                cmdUpdateLocalization.Visible = true;

                // only show "Convert to neutral" if page has no child pages
                MakeNeutral.Visible = (TabController.Instance.GetTabsByPortal(PortalId).WithParentId(_tab.TabID).Count == 0);

                // only show "add missing languages" if not all languages are available
                AddMissing.Visible = TabController.Instance.HasMissingLanguages(PortalId, _tab.TabID);
            }
        }

        private void BindPageDetails()
        {
            txtTitle.Text = Tab.Title;
            txtDescription.Text = Tab.Description;
            txtKeyWords.Text = Tab.KeyWords;

            pageUrlPanel.Visible = !Tab.IsSuperTab && (Config.GetFriendlyUrlProvider() == "advanced");
            doNotRedirectPanel.Visible = (Config.GetFriendlyUrlProvider() == "advanced");

            if (_strAction != "copy")
            {
                txtTabName.Text = Tab.TabName;
                if (Tab.TabUrls.Count > 0)
                {
                    var tabUrl = Tab.TabUrls.SingleOrDefault(t => t.IsSystem && t.HttpStatus == "200" && t.SeqNum == 0);

                    if (tabUrl != null)
                    {
                        urlTextBox.Text = tabUrl.Url;
                    }
                }

                var friendlyUrlSettings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(PortalId);
                if (String.IsNullOrEmpty(urlTextBox.Text) && Tab.TabID > -1 && !Tab.IsSuperTab)
                {
                    var baseUrl = Globals.AddHTTP(PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + Tab.TabID;
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(Tab,
                                                                                baseUrl,
                                                                                Globals.glbDefaultPage,
                                                                                PortalAlias.HTTPAlias,
                                                                                false, //dnndev-27493 :we want any custom Urls that apply
                                                                                friendlyUrlSettings,
                                                                                Guid.Empty);

                    urlTextBox.Text = path.Replace(Globals.AddHTTP(PortalAlias.HTTPAlias), "");
                }
                string tabPath = (Tab.TabPath.Replace("//", "/") + ";").ToLower();
                if (friendlyUrlSettings.UseBaseFriendlyUrls.ToLower().Contains(tabPath))
                {
                    doNotRedirectCheckBox.Enabled = false;
                }

                doNotRedirectCheckBox.Checked = Tab.DoNotRedirect;
            }
        }

        private void BindSkins()
        {
            pageSkinCombo.PortalId = Tab.PortalID;
            pageSkinCombo.RootPath = SkinController.RootSkin;
            pageSkinCombo.Scope = SkinScope.All;
            pageSkinCombo.IncludeNoneSpecificItem = true;
            pageSkinCombo.NoneSpecificText = "<" + Localization.GetString("None_Specified") + ">";
            pageSkinCombo.SelectedValue =Tab.SkinSrc;

            pageContainerCombo.PortalId = Tab.PortalID;
            pageContainerCombo.RootPath = SkinController.RootContainer;
            pageContainerCombo.Scope = SkinScope.All;
            pageContainerCombo.IncludeNoneSpecificItem = true;
            pageContainerCombo.NoneSpecificText = "<" + Localization.GetString("None_Specified") + ">";
            pageContainerCombo.SelectedValue = Tab.ContainerSrc;
        }

        private void BindTab()
        {
            //Load TabControls
            BindTabControls(Tab);

            if (Tab != null)
            {
                BindPageDetails();
                PageDetailsExtensionControl.BindAction(PortalId, Tab.TabID, ModuleId);

                ctlURL.Url = Tab.Url;
                bool newWindow = false;
                if (Tab.TabSettings["LinkNewWindow"] != null && Boolean.TryParse((string)Tab.TabSettings["LinkNewWindow"], out newWindow) && newWindow)
                {
                    ctlURL.NewWindow = newWindow;
                }

                ctlIcon.Url = Tab.IconFileRaw;
                ctlIconLarge.Url = Tab.IconFileLargeRaw;
                chkMenu.Checked = Tab.IsVisible;

                chkDisableLink.Checked = Tab.DisableLink;
                if (TabId == PortalSettings.AdminTabId || TabId == PortalSettings.SplashTabId || TabId == PortalSettings.HomeTabId || TabId == PortalSettings.LoginTabId ||
                    TabId == PortalSettings.UserTabId || TabId == PortalSettings.SuperTabId)
                {
                    chkDisableLink.Enabled = false;
                }

                BindSkins();

                if (PortalSettings.SSLEnabled)
                {
                    chkSecure.Enabled = true;
                    chkSecure.Checked = Tab.IsSecure;
                }
                else
                {
                    chkSecure.Enabled = false;
                    chkSecure.Checked = Tab.IsSecure;
                }
                var allowIndex = false;
                chkAllowIndex.Checked = !Tab.TabSettings.ContainsKey("AllowIndex") || !bool.TryParse(Tab.TabSettings["AllowIndex"].ToString(), out allowIndex) || allowIndex;
                txtPriority.Text = Tab.SiteMapPriority.ToString();

                if (!Null.IsNull(Tab.StartDate))
                {
                    startDatePicker.SelectedDate = Tab.StartDate;
                }
                if (!Null.IsNull(Tab.EndDate))
                {
                    endDatePicker.SelectedDate = Tab.EndDate;
                }

                endDatePicker.MinDate = DateTime.Now;

                if (Tab.RefreshInterval != Null.NullInteger)
                {
                    txtRefreshInterval.Text = Tab.RefreshInterval.ToString();
                }

                txtPageHeadText.Text = Tab.PageHeadText;
                chkPermanentRedirect.Checked = Tab.PermanentRedirect;

                ShowPermissions(!Tab.IsSuperTab && TabPermissionController.CanAdminPage());

                termsSelector.PortalId = Tab.PortalID;
                termsSelector.Terms = Tab.Terms;
                termsSelector.DataBind();

                txtCustomStylesheet.Text = Tab.TabSettings.ContainsKey("CustomStylesheet") ? Tab.TabSettings["CustomStylesheet"].ToString() : string.Empty;
            }

            if (string.IsNullOrEmpty(_strAction) || _strAction == "add" || _strAction == "copy")
            {
                InitializeTab();
            }

            // copy page options
            modulesRow.Visible = false;
            switch (_strAction)
            {
                case "copy":
                    var tabs = GetTabs(true, false, false, true);
                    var tab = tabs.SingleOrDefault(t => t.TabID == TabId);
                    if (tab != null) cboCopyPage.SelectedPage = tab;
                    DisplayTabModules();
                    break;
            }
        }

        private void BindTabControls(TabInfo tab)
        {
            // only superusers and administrators can manage parent pages
            if ((string.IsNullOrEmpty(_strAction) || _strAction == "copy" || _strAction == "add")
                    && !UserInfo.IsSuperUser && !UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
            {
                var tabList = GetTabs(true, true, false, true);
                var selectedParentTab = tabList.SingleOrDefault(t => t.TabID == PortalSettings.ActiveTab.TabID);
                if (selectedParentTab != null &&  (selectedParentTab.TabPath.StartsWith("//Admin") == false && selectedParentTab.TabPath.StartsWith("//Host") == false))
                {
                    cboParentTab.SelectedPage = selectedParentTab;
                }  
            }
            else
            {
                var tabList = GetTabs(true, true, true, true);
                var selectedParentTab = tabList.SingleOrDefault(t => t.TabID == PortalSettings.ActiveTab.ParentId);
                cboParentTab.SelectedPage = selectedParentTab;            
            }

            if (string.IsNullOrEmpty(_strAction) || _strAction == "add" || _strAction == "copy")
            {
                BindBeforeAfterTabControls();
                insertPositionRow.Visible = cboPositionTab.Items.Count > 0;
                cboParentTab.AutoPostBack = true;
                cultureTypeList.SelectedValue = PortalController.GetPortalSetting("CreateNewPageCultureType", PortalId, "Localized");

            }
            else
            {
                DisablePositionDropDown();
            }

            cboCacheProvider.DataSource = OutputCachingProvider.GetProviderList();
            cboCacheProvider.DataBind();
            cboCacheProvider.InsertItem(0, Localization.GetString("None_Specified"), "");
            if (tab == null)
            {
                cboCacheProvider.ClearSelection();
                cboCacheProvider.Items[0].Selected = true;
                rblCacheIncludeExclude.ClearSelection();
                rblCacheIncludeExclude.Items[0].Selected = true;
            }

            var tabSettings = TabController.Instance.GetTabSettings(TabId);
            SetValue(cboCacheProvider, tabSettings, "CacheProvider");
            SetValue(txtCacheDuration, tabSettings, "CacheDuration");
            SetValue(rblCacheIncludeExclude, tabSettings, "CacheIncludeExclude");
            SetValue(txtIncludeVaryBy, tabSettings, "IncludeVaryBy");
            SetValue(txtExcludeVaryBy, tabSettings, "ExcludeVaryBy");
            SetValue(txtMaxVaryByCount, tabSettings, "MaxVaryByCount");

            ShowCacheRows();
        }

        private void DisablePositionDropDown()
        {
            insertPositionRow.Visible = false;
            cboParentTab.AutoPostBack = false;
        }

        private void CheckLocalizationVisibility()
        {
            if (PortalSettings.ContentLocalizationEnabled 
                && LocaleController.Instance.GetLocales(PortalId).Count > 1
                && _tab.TabID != PortalSettings.AdminTabId
                && _tab.ParentId != PortalSettings.AdminTabId)
            {
                localizationTab.Visible = true;
                localizationPanel.Visible = true;
            }
            else
            {
                localizationTab.Visible = false;
                localizationPanel.Visible = false;
            }
        }

        private void CheckQuota()
        {
            if (PortalSettings.Pages < PortalSettings.PageQuota || UserInfo.IsSuperUser || PortalSettings.PageQuota == 0)
            {
                cmdUpdate.Enabled = true;
            }
            else
            {
                cmdUpdate.Enabled = false;
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ExceededQuota", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        private bool DeleteTab(int deleteTabId)
        {
            var bDeleted = Null.NullBoolean;
            if (TabPermissionController.CanDeletePage())
            {
                bDeleted = TabController.Instance.SoftDeleteTab(deleteTabId, PortalSettings);
                if (!bDeleted)
                {
                    Skin.AddModuleMessage(this, Localization.GetString("DeleteSpecialPage", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
            }
            else
            {
                Skin.AddModuleMessage(this, Localization.GetString("DeletePermissionError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }

            return bDeleted;
        }

        private void DisplayTabModules()
        {
            switch (cboCopyPage.SelectedItem.Value)
            {
                case "":
                    modulesRow.Visible = false;
                    break;
                default:
                    // selected tab
                    if (TabPermissionController.CanAddContentToPage())
                    {
                        var tabId = int.Parse(cboCopyPage.SelectedItem.Value);
                        var dataSource = LoadTabModules(tabId);
                        if (dataSource.Any())
                        {
                            grdModules.DataSource = dataSource;
                            grdModules.DataBind();

                            modulesRow.Visible = true;
                            //find out how many pages has the copy name to avoid duplicate page name
                            var parentId = cboParentTab.SelectedItemValueAsInt;
                            var copyName = string.Format("{0} - Copy", cboCopyPage.SelectedItem.Text.Replace("...", string.Empty));
                            var existsNum =
                                TabController.GetTabsByParent(parentId, PortalSettings.PortalId).Count(t => t.TabName.StartsWith(copyName));
                            if (existsNum > 0)
                            {
                                copyName = string.Format("{0}({1})", copyName, existsNum + 1);
                            }
                            txtTabName.Text = copyName;
                        }
                        else
                        {
                            modulesRow.Visible = false;
                        }
                    }
                    else
                    {
                        modulesRow.Visible = false;
                    }
                    break;
            }
        }

        private void GetHostTabs(List<TabInfo> tabs)
        {
            foreach (var kvp in TabController.Instance.GetTabsByPortal(Null.NullInteger))
            {
                tabs.Add(kvp.Value);
            }
        }

        private IEnumerable<TabInfo> GetTabs(bool includeCurrent, bool includeURL, bool includeParent, bool includeDescendants)
        {
            var tabs = new List<TabInfo>();

            var excludeTabId = Null.NullInteger;
            if (!includeCurrent)
            {
                excludeTabId = PortalSettings.ActiveTab.TabID;
            }

            if (PortalSettings.ActiveTab.IsSuperTab)
            {
                GetHostTabs(tabs);
            }
            else
            {
                tabs = TabController.GetPortalTabs(PortalId, excludeTabId, false, string.Empty, true, false, includeURL, false, true);

                var parentTab = (from tab in tabs where tab.TabID == PortalSettings.ActiveTab.ParentId select tab).FirstOrDefault();

                //Need to include the Parent Tab if its not already in the list of tabs
                if (includeParent && PortalSettings.ActiveTab.ParentId != Null.NullInteger && parentTab == null)
                {
                    tabs.Add(TabController.Instance.GetTab(PortalSettings.ActiveTab.ParentId, PortalId, false));
                }

                if (UserInfo.IsSuperUser && TabId == Null.NullInteger)
                {
                    GetHostTabs(tabs);
                }

                if (!includeDescendants)
                {
                    tabs = (from t in tabs where !t.TabPath.StartsWith(PortalSettings.ActiveTab.TabPath) && !t.TabPath.Equals(PortalSettings.ActiveTab.TabPath) select t).ToList();
                }
            }

            return tabs;
        }

        private void InitializeTab()
        {
            if ((cboPositionTab.FindItemByValue(TabId.ToString()) != null))
            {
                cboPositionTab.ClearSelection();
                cboPositionTab.FindItemByValue(TabId.ToString()).Selected = true;
            }
            cboFolders.Services.Parameters.Add("permission", "ADD");
            var user = UserController.Instance.GetCurrentUserInfo();
            var folders = FolderManager.Instance.GetFolders(user, "BROWSE, ADD");
            var templateFolder = folders.SingleOrDefault(f => f.DisplayPath == "Templates/");
            if (templateFolder != null)
            {
                cboFolders.SelectedFolder = templateFolder;
                LoadTemplates();
            }
        }

        /// <summary>
        ///   Checks if parent tab will cause a circular reference
        /// </summary>
        /// <param name = "intTabId">Tabid</param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [VMasanas]	28/11/2004	Created
        /// </history>
        private bool IsCircularReference(int intTabId, int portalId)
        {
            if (intTabId != -1)
            {
                var tabInfo = TabController.Instance.GetTab(intTabId, portalId, false);

                if (tabInfo.Level == 0)
                {
                    return false;
                }
                return TabId == tabInfo.ParentId || IsCircularReference(tabInfo.ParentId, portalId);
            }
            return false;
        }

        private List<ModuleInfo> LoadTabModules(int TabID)
        {
            var moduleList = new List<ModuleInfo>();

            foreach (var m in ModuleController.Instance.GetTabModules(TabID).Values)
            {
                if (TabPermissionController.CanAddContentToPage() && !m.IsDeleted && !m.AllTabs)
                {
                    moduleList.Add(m);
                }
            }

            return moduleList;
        }

        private void LoadTemplates()
        {
            cboTemplate.Items.Clear();
            if (cboFolders.SelectedItem != null)
            {
                var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                if (folder != null)
                {
                    var arrFiles = Globals.GetFileList(PortalId, "page.template", false, folder.FolderPath);
                    foreach (FileItem objFile in arrFiles)
                    {
                        var fileItem = new ListItem { Text = objFile.Text.Replace(".page.template", ""), Value = objFile.Text };
                        cboTemplate.AddItem(fileItem.Text, fileItem.Value);
                        if (!Page.IsPostBack && fileItem.Text == "Default")
                        {
                            cboTemplate.ClearSelection();
                            cboTemplate.FindItemByText("Default").Selected = true;
                        }
                    }
                    cboTemplate.InsertItem(0, Localization.GetString("None_Specified"), "");
                    if (cboTemplate.SelectedIndex == -1)
                    {
                        cboTemplate.SelectedIndex = 0;
                    }
                }

            }
        }

        /// <summary>
        ///   SaveTabData saves the Tab to the Database
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "strAction">The action to perform "edit" or "add"</param>
        /// <history>
        ///   [cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///   and localisation
        ///   [jlucarino]	2/26/2009	Added CreatedByUserID and LastModifiedByUserID
        /// </history>
        private int SaveTabData(string strAction)
        {
            string strIcon = ctlIcon.Url;
            string strIconLarge = ctlIconLarge.Url;

            Tab.TabName = txtTabName.Text;
            Tab.Title = txtTitle.Text;
            Tab.Description = txtDescription.Text;
            Tab.KeyWords = txtKeyWords.Text;
            Tab.IsVisible = chkMenu.Checked;
            Tab.DisableLink = chkDisableLink.Checked;

            TabInfo parentTab = null;
            if (cboParentTab.SelectedItem != null)
            {
                var parentTabId = cboParentTab.SelectedItemValueAsInt;
                parentTab = TabController.Instance.GetTab(parentTabId, -1, false);
            }

            if (parentTab != null)
            {
                Tab.PortalID = parentTab.PortalID;
                Tab.ParentId = parentTab.TabID;
            }
            else
            {
                Tab.ParentId = Null.NullInteger;
            }
            Tab.IconFile = strIcon;
            Tab.IconFileLarge = strIconLarge;
            Tab.IsDeleted = false;
            Tab.Url = ctlURL.Url;

            Tab.TabPermissions.Clear();
            if (Tab.PortalID != Null.NullInteger)
            {
                Tab.TabPermissions.AddRange(dgPermissions.Permissions);
            }

            Tab.Terms.Clear();
            Tab.Terms.AddRange(termsSelector.Terms);

            Tab.SkinSrc = pageSkinCombo.SelectedValue;
            Tab.ContainerSrc = pageContainerCombo.SelectedValue;
            Tab.TabPath = Globals.GenerateTabPath(Tab.ParentId, Tab.TabName);

            //Check for invalid
            string invalidType;
            if (!TabController.IsValidTabName(Tab.TabName, out invalidType))
            {
                ShowWarningMessage(string.Format(Localization.GetString(invalidType, LocalResourceFile), Tab.TabName));
                return Null.NullInteger;
            }

            //Validate Tab Path
            if (!IsValidTabPath(Tab, Tab.TabPath))
            {
                return Null.NullInteger;
            }

            //Set Tab's position
            var positionTabId = Null.NullInteger;
            if (!string.IsNullOrEmpty(cboPositionTab.SelectedValue))
            {
                positionTabId = Int32.Parse(cboPositionTab.SelectedValue);
            }

            //Set Culture Code
            if (strAction != "edit")
            {
                if (PortalSettings.ContentLocalizationEnabled)
                {
                    switch (cultureTypeList.SelectedValue)
                    {
                        case "Localized":
                            var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);
                            Tab.CultureCode = defaultLocale.Code;
                            break;
                        case "Culture":
                            Tab.CultureCode = PortalSettings.CultureCode;
                            break;
                        default:
                            Tab.CultureCode = Null.NullString;
                            break;
                    }
                    if (PortalController.GetPortalSetting("CreateNewPageCultureType", PortalId, "Localized") != cultureTypeList.SelectedValue)
                    {
                        PortalController.UpdatePortalSetting(PortalId, "CreateNewPageCultureType", cultureTypeList.SelectedValue, true);
                    }

                    var tabLocale = LocaleController.Instance.GetLocale(Tab.CultureCode) ?? LocaleController.Instance.GetDefaultLocale(PortalId);

                    //Fix parent 
                    if (Tab.ParentId > Null.NullInteger)
                    {
                        parentTab = TabController.Instance.GetTab(Tab.ParentId, PortalId, false);
                        if (parentTab.CultureCode != Tab.CultureCode)
                        {
                            parentTab = TabController.Instance.GetTabByCulture(Tab.ParentId, PortalId, tabLocale);
                        }
                        if (parentTab != null)
                        {
                            Tab.ParentId = parentTab.TabID;
                        }
                    }

                    //Fix position TabId
                    if (positionTabId > Null.NullInteger)
                    {
                        var positionTab = TabController.Instance.GetTab(positionTabId, PortalId, false);
                        if (positionTab.CultureCode != Tab.CultureCode)
                        {
                            positionTab = TabController.Instance.GetTabByCulture(positionTabId, PortalId, tabLocale);
                        }
                        if (positionTab != null)
                        {
                            positionTabId = positionTab.TabID;
                        }
                    }
                }
                else
                {
                    Tab.CultureCode = Null.NullString;
                }
            }

            //Validate Tab Path
            if (string.IsNullOrEmpty(strAction))
            {
                var tabID = TabController.GetTabByTabPath(Tab.PortalID, Tab.TabPath, Tab.CultureCode);

                if (tabID != Null.NullInteger)
                {
                    var existingTab = TabController.Instance.GetTab(tabID, PortalId, false);
                    if (existingTab != null && existingTab.IsDeleted)
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabRecycled", LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("TabExists", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                    return Null.NullInteger;
                }
            }

            Tab.StartDate = startDatePicker.SelectedDate != null ? startDatePicker.SelectedDate.Value : Null.NullDate;
            Tab.EndDate = endDatePicker.SelectedDate != null ? endDatePicker.SelectedDate.Value : Null.NullDate;

            if (Tab.StartDate > Null.NullDate && Tab.EndDate > Null.NullDate && Tab.StartDate >= Tab.EndDate)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("InvalidTabDates", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                return Null.NullInteger;
            }

            if (!valRefreshInterval.IsValid)
            {
                return Null.NullInteger;
            }
            Tab.RefreshInterval = txtRefreshInterval.Text == "" ? Null.NullInteger : Convert.ToInt32(txtRefreshInterval.Text);

            if (!valPriorityRequired.IsValid || !valPriority.IsValid)
            {
                return Null.NullInteger;
            }
            Tab.SiteMapPriority = float.Parse(txtPriority.Text);
            Tab.PageHeadText = txtPageHeadText.Text;
            Tab.IsSecure = chkSecure.Checked;
            Tab.PermanentRedirect = chkPermanentRedirect.Checked;

            UpdateTabSettings(Tab);
            if (strAction == "edit")
            {
                // trap circular tab reference
                if (cboParentTab.SelectedItem != null && Tab.TabID != cboParentTab.SelectedItemValueAsInt && !IsCircularReference(cboParentTab.SelectedItemValueAsInt, Tab.PortalID))
                {
                    TabController.Instance.UpdateTab(Tab);
                    if (IsHostMenu && Tab.PortalID != Null.NullInteger)
                    {
                        //Host Tab moved to Portal so clear Host cache
                        TabController.Instance.ClearCache(Null.NullInteger);
                    }
                    if (!IsHostMenu && Tab.PortalID == Null.NullInteger)
                    {
                        //Portal Tab moved to Host so clear portal cache
                        TabController.Instance.ClearCache(PortalId);
                    }
                }
            }
            else
            {
                if (positionTabId == Null.NullInteger)
                {
                    Tab.TabID = TabController.Instance.AddTab(Tab);
                }
                else
                {
                    if (rbInsertPosition.SelectedValue == "After" && positionTabId > Null.NullInteger)
                    {
                        Tab.TabID = TabController.Instance.AddTabAfter(Tab, positionTabId);
                    }
                    else if (rbInsertPosition.SelectedValue == "Before" && positionTabId > Null.NullInteger)
                    {
                        Tab.TabID = TabController.Instance.AddTabBefore(Tab, positionTabId);
                    }
                    else
                    {
                        Tab.TabID = TabController.Instance.AddTab(Tab);
                    }
                }

                //Create Localized versions
                if (PortalSettings.ContentLocalizationEnabled && cultureTypeList.SelectedValue == "Localized")
                {
                    TabController.Instance.CreateLocalizedCopies(Tab);
                    //Refresh tab
                    _tab = TabController.Instance.GetTab(Tab.TabID, Tab.PortalID, true);
                }

                var copyTabId = cboCopyPage.Visible && cboCopyPage.SelectedItem != null ? cboCopyPage.SelectedItemValueAsInt : Null.NullInteger;

                if (copyTabId != Null.NullInteger)
                {
                    ModuleInfo objModule;
                    CheckBox chkModule;
                    RadioButton optCopy;
                    RadioButton optReference;
                    TextBox txtCopyTitle;

                    foreach (DataGridItem objDataGridItem in grdModules.Items)
                    {
                        chkModule = (CheckBox)objDataGridItem.FindControl("chkModule");
                        if (chkModule.Checked)
                        {
                            var intModuleID = Convert.ToInt32(grdModules.DataKeys[objDataGridItem.ItemIndex]);
                            optCopy = (RadioButton)objDataGridItem.FindControl("optCopy");
                            optReference = (RadioButton)objDataGridItem.FindControl("optReference");
                            txtCopyTitle = (TextBox)objDataGridItem.FindControl("txtCopyTitle");

                            objModule = ModuleController.Instance.GetModule(intModuleID, copyTabId, false);
                            ModuleInfo newModule = null;
                            if ((objModule != null))
                            {
                                //Clone module as it exists in the cache and changes we make will update the cached object
                                newModule = objModule.Clone();

                                newModule.TabID = Tab.TabID;
                                newModule.DefaultLanguageGuid = Null.NullGuid;
                                newModule.CultureCode = Tab.CultureCode;
                                newModule.ModuleTitle = txtCopyTitle.Text;

                                if (!optReference.Checked)
                                {
                                    newModule.ModuleID = Null.NullInteger;
                                    ModuleController.Instance.InitialModulePermission(newModule, newModule.TabID, 0);
                                }

                                newModule.ModuleID = ModuleController.Instance.AddModule(newModule);

                                if (optCopy.Checked)
                                {
                                    if (!string.IsNullOrEmpty(newModule.DesktopModule.BusinessControllerClass))
                                    {
                                        var objObject = Reflection.CreateObject(newModule.DesktopModule.BusinessControllerClass, newModule.DesktopModule.BusinessControllerClass);
                                        if (objObject is IPortable)
                                        {
                                            var content = Convert.ToString(((IPortable)objObject).ExportModule(intModuleID));
                                            if (!string.IsNullOrEmpty(content))
                                            {
                                                ((IPortable)objObject).ImportModule(newModule.ModuleID, content, newModule.DesktopModule.Version, UserInfo.UserID);
                                            }
                                        }
                                    }
                                }
                            }

                            if (optReference.Checked)
                            {
                                //Make reference copies on secondary language
                                foreach (var m in objModule.LocalizedModules.Values)
                                {
                                    var newLocalizedModule = m.Clone();
                                    var localizedTab = Tab.LocalizedTabs[m.CultureCode];
                                    newLocalizedModule.TabID = localizedTab.TabID;
                                    newLocalizedModule.CultureCode = localizedTab.CultureCode;
                                    newLocalizedModule.ModuleTitle = txtCopyTitle.Text;
                                    newLocalizedModule.DefaultLanguageGuid = newModule.UniqueId;
                                    newLocalizedModule.ModuleID = ModuleController.Instance.AddModule(newLocalizedModule);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // create the page from a template
                    if (!string.IsNullOrEmpty(cboTemplate.SelectedValue))
                    {
                        var xmlDoc = new XmlDocument();
                        try
                        {
                            // open the XML file
                            var folder = FolderManager.Instance.GetFolder(cboFolders.SelectedItemValueAsInt);
                            if (folder != null)
                                xmlDoc.Load(PortalSettings.HomeDirectoryMapPath + folder.FolderPath + cboTemplate.SelectedValue);
                        }
                        catch (Exception ex)
                        {
                            Exceptions.LogException(ex);

                            Skin.AddModuleMessage(this, Localization.GetString("BadTemplate", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                            return Null.NullInteger;
                        }
                        TabController.DeserializePanes(xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), Tab.PortalID, Tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());
                        //save tab permissions
                        RibbonBarManager.DeserializeTabPermissions(xmlDoc.SelectNodes("//portal/tabs/tab/tabpermissions/permission"), Tab);

                        var tabIndex = 0;
                        var exceptions = string.Empty;
                        foreach (XmlNode tabNode in xmlDoc.SelectSingleNode("//portal/tabs").ChildNodes)
                        {
                            //Create second tab onward tabs. Note first tab is already created above.
                            if (tabIndex > 0)
                            {
                                try
                                {
                                    TabController.DeserializeTab(tabNode, null, PortalId, PortalTemplateModuleAction.Replace);
                                }
                                catch (Exception ex)
                                {
                                    Exceptions.LogException(ex);
                                    exceptions += string.Format("Template Tab # {0}. Error {1}<br/>", tabIndex + 1, ex.Message);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(Tab.SkinSrc) && !String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "skinsrc", "")))
                                {
                                    Tab.SkinSrc = XmlUtils.GetNodeValue(tabNode, "skinsrc", "");
                                }
                                if (string.IsNullOrEmpty(Tab.ContainerSrc) && !String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "containersrc", "")))
                                {
                                    Tab.ContainerSrc = XmlUtils.GetNodeValue(tabNode, "containersrc", "");
                                }
                                TabController.Instance.UpdateTab(Tab);
                            }
                            tabIndex++;
                        }

                        if (!string.IsNullOrEmpty(exceptions))
                        {
                            Skin.AddModuleMessage(this, exceptions, ModuleMessage.ModuleMessageType.RedError);
                            return Null.NullInteger;
                        }
                    }
                }
            }

            PageDetailsExtensionControl.SaveAction(PortalId, Tab.TabID, ModuleId);

            // url tracking
            var objUrls = new UrlController();
            objUrls.UpdateUrl(PortalId, ctlURL.Url, ctlURL.UrlType, 0, Null.NullDate, Null.NullDate, ctlURL.Log, ctlURL.Track, Null.NullInteger, ctlURL.NewWindow);

            //Clear the Tab's Cached modules
            DataCache.ClearModuleCache(TabId);

            //Update Cached Tabs as TabPath may be needed before cache is cleared
            TabInfo tempTab;
            if (TabController.Instance.GetTabsByPortal(PortalId).TryGetValue(Tab.TabID, out tempTab))
            {
                tempTab.TabPath = Tab.TabPath;
            }

            //Update Custom Url
            if (!Tab.IsSuperTab)
            {
                var tabUrl = Tab.TabUrls.SingleOrDefault(t => t.IsSystem
                                                                && t.HttpStatus == "200"
                                                                && t.SeqNum == 0);
                var url = urlTextBox.Text;


                if (!String.IsNullOrEmpty(url))
                {
                    if (!url.StartsWith("/"))
                    {
                        url = "/" + url;
                    }

                    string currentUrl = String.Empty;
                    var friendlyUrlSettings = new FriendlyUrlSettings(PortalId);
                    if (Tab.TabID > -1 && !Tab.IsSuperTab)
                    {
                        var baseUrl = Globals.AddHTTP(PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + Tab.TabID;
                        var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(Tab,
                                                                                    baseUrl,
                                                                                    Globals.glbDefaultPage,
                                                                                    PortalAlias.HTTPAlias,
                                                                                    false,
                                                                                    friendlyUrlSettings,
                                                                                    Guid.Empty);

                        currentUrl = path.Replace(Globals.AddHTTP(PortalAlias.HTTPAlias), "");
                    }

                    if (url != currentUrl)
                    {
                        if (tabUrl == null)
                        {
                            //Add new custom url
                            tabUrl = new TabUrlInfo()
                                            {
                                                TabId = Tab.TabID,
                                                SeqNum = 0,
                                                PortalAliasId = -1,
                                                PortalAliasUsage = PortalAliasUsageType.Default,
                                                QueryString = String.Empty,
                                                Url = url,
                                                HttpStatus = "200",
                                                CultureCode = String.Empty,
                                                IsSystem = true
                                            };
                            //Save url
                            TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
                        }
                        else
                        {
                            //Change the original 200 url to a redirect
                            tabUrl.HttpStatus = "301";
                            tabUrl.SeqNum = Tab.TabUrls.Max(t => t.SeqNum) + 1;
                            TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);

                            //Add new custom url
                            tabUrl.Url = url;
                            tabUrl.HttpStatus = "200";
                            tabUrl.SeqNum = 0;
                            TabController.Instance.SaveTabUrl(tabUrl, PortalId, true);
                        }


                        //Delete any redirects to the same url
                        foreach (var redirecturl in TabController.Instance.GetTabUrls(Tab.TabID, Tab.PortalID))
                        {
                            if (redirecturl.Url == url && redirecturl.HttpStatus != "200")
                            {
                                TabController.Instance.DeleteTabUrl(redirecturl, Tab.PortalID, true);
                            }
                        }
                    }
                }
                else
                {
                    if (tabUrl != null)
                    {
                        TabController.Instance.DeleteTabUrl(tabUrl, PortalId, true);
                    }
                }
            }

            return Tab.TabID;
        }

        private static void SetValue(Control control, Hashtable tabSettings, string tabSettingsKey)
        {
            if (ReferenceEquals(control.GetType(), typeof(TextBox)))
            {
                ((TextBox)control).Text = string.IsNullOrEmpty(Convert.ToString(tabSettings[tabSettingsKey])) ? "" : tabSettings[tabSettingsKey].ToString();
            }
            else if (ReferenceEquals(control.GetType(), typeof(DnnComboBox)))
            {
                var dnnComboBox = (DnnComboBox)control;
                if (!string.IsNullOrEmpty(Convert.ToString(tabSettings[tabSettingsKey])))
                {
                    dnnComboBox.ClearSelection();
                    dnnComboBox.FindItemByValue(tabSettings[tabSettingsKey].ToString()).Selected = true;
                }
                else
                {
                    dnnComboBox.ClearSelection();
                    dnnComboBox.FindItemByValue("").Selected = true;

                }
            }
            else if (ReferenceEquals(control.GetType(), typeof(RadioButtonList)))
            {
                var dnnRadioList = (RadioButtonList)control;
                if (!string.IsNullOrEmpty(Convert.ToString(tabSettings[tabSettingsKey])))
                {
                    dnnRadioList.ClearSelection();
                    dnnRadioList.Items.FindByValue(tabSettings[tabSettingsKey].ToString()).Selected = true;
                }
                else
                {
                    dnnRadioList.ClearSelection();
                }
            }
        }

        private void ShowCacheRows()
        {
            if (!string.IsNullOrEmpty(cboCacheProvider.SelectedValue))
            {
                CacheDurationRow.Visible = true;
                CacheIncludeExcludeRow.Visible = true;
                MaxVaryByCountRow.Visible = true;
                cmdClearAllPageCache.Visible = true;
                cmdClearPageCache.Visible = true;
                ShowCacheIncludeExcludeRows();
                CacheStatusRow.Visible = true;
                var cachedItemCount = OutputCachingProvider.Instance(cboCacheProvider.SelectedValue).GetItemCount(TabId);
                if (cachedItemCount == 0)
                {
                    cmdClearAllPageCache.Enabled = false;
                    cmdClearPageCache.Enabled = false;
                }
                else
                {
                    cmdClearAllPageCache.Enabled = true;
                    cmdClearPageCache.Enabled = true;
                }
                lblCachedItemCount.Text = string.Format(Localization.GetString("lblCachedItemCount.Text", LocalResourceFile), cachedItemCount);
            }
            else
            {
                CacheStatusRow.Visible = false;
                CacheDurationRow.Visible = false;
                CacheIncludeExcludeRow.Visible = false;
                MaxVaryByCountRow.Visible = false;
                ExcludeVaryByRow.Visible = false;
                IncludeVaryByRow.Visible = false;
            }
        }

        private void ShowCacheIncludeExcludeRows()
        {
            if (rblCacheIncludeExclude.SelectedItem == null)
            {
                rblCacheIncludeExclude.Items[0].Selected = true;
            }
            if (rblCacheIncludeExclude.SelectedValue == "0")
            {
                ExcludeVaryByRow.Visible = false;
                IncludeVaryByRow.Visible = true;
            }
            else
            {
                ExcludeVaryByRow.Visible = true;
                IncludeVaryByRow.Visible = false;
            }
        }

        private void ShowPermissions(bool show)
        {
            permissionsTab.Visible = show;
            permissionRow.Visible = show;
        }

        private bool IsValidTabPath(TabInfo tab, string newTabPath)
        {
            var valid = true;

            //get default culture if the tab's culture is null
            var cultureCode = tab.CultureCode;
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = PortalSettings.DefaultLanguage;
            }

            //Validate Tab Path
            var tabID = TabController.GetTabByTabPath(tab.PortalID, newTabPath, cultureCode);
            if (tabID != Null.NullInteger && tabID != tab.TabID)
            {
                var existingTab = TabController.Instance.GetTab(tabID, tab.PortalID, false);
                if (existingTab != null && existingTab.IsDeleted)
                    ShowWarningMessage(Localization.GetString("TabRecycled", LocalResourceFile));
                else
                    ShowErrorMessage(Localization.GetString("TabExists", LocalResourceFile));

                valid = false;
            }

            //check whether have conflict between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(tab.PortalID, newTabPath))
            {
                ShowWarningMessage(Localization.GetString("PathDuplicateWithAlias", LocalResourceFile));
                valid = false;
            }

            return valid;
        }

        private void ShowWarningMessage(string message)
        {
            UI.Skins.Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.YellowWarning);
        }

        private void ShowErrorMessage(string message)
        {
            Skin.AddModuleMessage(this, message, ModuleMessage.ModuleMessageType.RedError);
        }

        private void UpdateTabSettings(TabInfo tab)
        {
            tab.TabSettings["CacheProvider"] = cboCacheProvider.SelectedValue;
            tab.TabSettings["CacheDuration"] = txtCacheDuration.Text;
            tab.TabSettings["CacheIncludeExclude"] = rblCacheIncludeExclude.SelectedValue;
            tab.TabSettings["IncludeVaryBy"] = txtIncludeVaryBy.Text;
            tab.TabSettings["ExcludeVaryBy"] = txtExcludeVaryBy.Text;
            tab.TabSettings["MaxVaryByCount"] = txtMaxVaryByCount.Text;
            tab.TabSettings["LinkNewWindow"] = ctlURL.NewWindow.ToString();
            tab.TabSettings["AllowIndex"] = chkAllowIndex.Checked.ToString();
            tab.TabSettings["CustomStylesheet"] = txtCustomStylesheet.Text;
            tab.TabSettings["DoNotRedirect"] = doNotRedirectCheckBox.Checked.ToString();

        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            jQuery.RequestDnnPluginsRegistration();

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Scripts/dnn.extensions.js", FileOrder.Js.DefaultPriority);
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js", FileOrder.Js.DefaultPriority + 1);
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Admin/Tabs/ClientScripts/dnn.PageUrl.js", FileOrder.Js.DefaultPriority + 2);
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            cboCacheProvider.SelectedIndexChanged += cboCacheProvider_Change;
            cboCopyPage.SelectionChanged += cboCopyPage_SelectedIndexChanged;
            cboFolders.SelectionChanged += cboFolders_SelectedIndexChanged;
            cboParentTab.SelectionChanged += cboParentTab_SelectedIndexChanged;
            cmdClearAllPageCache.Click += cmdClearAllPageCache_Click;
            cmdClearPageCache.Click += cmdClearPageCache_Click;
            cmdCopyPerm.Click += cmdCopyPerm_Click;
            cmdCopySkin.Click += cmdCopySkin_Click;
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            rblCacheIncludeExclude.SelectedIndexChanged += rblCacheIncludeExclude_Change;
            readyForTranslationButton.Click += readyForTranslationButton_Click;
            cmdSubmitTranslation.Click += submitTranslation_Click;
            cmdCancelTranslation.Click += cancelTranslation_Click;
            // Verify that the current user has access to edit this module
            if (!TabPermissionController.HasTabPermission("ADD,EDIT,COPY,DELETE,MANAGE"))
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }
            if ((Request.QueryString["action"] != null))
            {
                _strAction = Request.QueryString["action"].ToLower();
            }

            if (Tab.ContentItemId == Null.NullInteger && Tab.TabID != Null.NullInteger)
            {
                //This tab does not have a valid ContentItem
                TabController.Instance.CreateContentItem(Tab);
                TabController.Instance.UpdateTab(Tab);
            }

            DisableHostAdminFunctions();

            if (PortalSettings.ActiveTab.IsSuperTab || PortalSecurity.IsInRole("Administrators") || PortalSettings.ActiveTab.ParentId == Null.NullInteger)
            {
                // Add Non Specified if user is Admin or if current tab is already on the top level
                cboParentTab.UndefinedItem = new ListItem(SharedConstants.Unspecified, string.Empty);
            }

            cboCopyPage.UndefinedItem = new ListItem(SharedConstants.Unspecified, string.Empty);

            PortalAliasCaption.Text = PortalAlias.HTTPAlias;
            PortalAliasCaption.ToolTip = PortalAlias.HTTPAlias;
            UrlContainer.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "um-page-url-container");
        }

        private void DisableHostAdminFunctions()
        {
            var children = TabController.GetTabsByParent(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID);

            if (children == null || children.Count < 1)
            {
                cmdCopySkin.Enabled = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {

                if (Page.IsPostBack == false)
                {
                    // load the list of files found in the upload directory
                    ctlIcon.ShowFiles = true;
                    ctlIcon.ShowImages = true;
                    ctlIcon.ShowTabs = false;
                    ctlIcon.ShowUrls = false;
                    ctlIcon.Required = false;

                    ctlIcon.ShowLog = false;
                    ctlIcon.ShowNewWindow = false;
                    ctlIcon.ShowTrack = false;
                    ctlIcon.FileFilter = Globals.glbImageFileTypes;
                    ctlIcon.Width = "275px";

                    ctlIconLarge.ShowFiles = ctlIcon.ShowFiles;
                    ctlIconLarge.ShowImages = ctlIcon.ShowImages;
                    ctlIconLarge.ShowTabs = ctlIcon.ShowTabs;
                    ctlIconLarge.ShowUrls = ctlIcon.ShowUrls;
                    ctlIconLarge.Required = ctlIcon.Required;

                    ctlIconLarge.ShowLog = ctlIcon.ShowLog;
                    ctlIconLarge.ShowNewWindow = ctlIcon.ShowNewWindow;
                    ctlIconLarge.ShowTrack = ctlIcon.ShowTrack;
                    ctlIconLarge.FileFilter = ctlIcon.FileFilter;
                    ctlIconLarge.Width = ctlIcon.Width;

                    // tab administrators can only manage their own tab
                    if (!UserInfo.IsSuperUser && !UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        cboParentTab.Enabled = false;
                    }

                    ctlURL.Width = "275px";

                    rowCopySkin.Visible = false;
                    copyPermissionRow.Visible = false;
                    cmdUpdate.Visible = TabPermissionController.HasTabPermission("ADD,EDIT,COPY,MANAGE");
                    cmdUpdate.Text = Localization.GetString(_strAction == "edit" ? "Update" : "Add", LocalResourceFile);

                    bool usingDefaultLocale = LocaleController.Instance.IsDefaultLanguage(LocaleController.Instance.GetCurrentLocale(PortalId).Code);
                    switch (_strAction)
                    {
                        case "":
                        case "add":
                            // add
                            CheckQuota();
                            templateRow1.Visible = true;
                            templateRow2.Visible = true;
                            copyPanel.Visible = TabPermissionController.CanCopyPage() && usingDefaultLocale;
                            cmdDelete.Visible = false;
                            ctlURL.IncludeActiveTab = true;
                            ctlAudit.Visible = false;
                            AdvancedSettingExtensionControl.Visible = false;
                            break;
                        case "edit":
                            copyPermissionRow.Visible = (TabPermissionController.CanAdminPage() && TabController.Instance.GetTabsByPortal(PortalId).DescendentsOf(TabId).Count > 0);
                            rowCopySkin.Visible = true;
                            copyPanel.Visible = false;
                            cmdDelete.Visible = TabPermissionController.CanDeletePage() && !TabController.IsSpecialTab(TabId, PortalSettings);
                            ctlURL.IncludeActiveTab = false;
                            ctlAudit.Visible = true;
                            AdvancedSettingExtensionControl.Visible = (Config.GetFriendlyUrlProvider() == "advanced");
                            break;
                        case "copy":
                            CheckQuota();
                            copyPanel.Visible = TabPermissionController.CanCopyPage() && usingDefaultLocale;
                            cmdDelete.Visible = false;
                            ctlURL.IncludeActiveTab = true;
                            ctlAudit.Visible = false;
                            AdvancedSettingExtensionControl.Visible = false;
                            break;
                        case "delete":
                            if (DeleteTab(TabId))
                            {
                                Response.Redirect(Globals.AddHTTP(PortalAlias.HTTPAlias), true);
                            }
                            else
                            {
                                _strAction = "edit";
                                copyPanel.Visible = false;
                                cmdDelete.Visible = TabPermissionController.CanDeletePage();
                            }
                            ctlURL.IncludeActiveTab = false;
                            ctlAudit.Visible = true;
                            AdvancedSettingExtensionControl.Visible = false;
                            break;
                    }
                    copyTab.Visible = copyPanel.Visible;

                    BindTab();

                    //Set the tab id of the permissions grid to the TabId (Note If in add mode
                    //this means that the default permissions inherit from the parent)
                    if (_strAction == "edit" || _strAction == "delete" || _strAction == "copy" || !TabPermissionController.CanAdminPage())
                    {
                        dgPermissions.TabID = TabId;
                    }
                    else
                    {
                        var parentTabId = (cboParentTab.SelectedItem != null) ? cboParentTab.SelectedItemValueAsInt : Null.NullInteger;
                        dgPermissions.TabID = parentTabId;
                    }
                }

                CheckLocalizationVisibility();

                BindLocalization(false);

                cancelHyperLink.NavigateUrl = Globals.NavigateURL();

                if (Request.QueryString["returntabid"] != null)
                {
                    // return to admin tab
                    var navigateUrl = Globals.NavigateURL(Convert.ToInt32(Request.QueryString["returntabid"]));
                    // add localtion hash to let it select in admin tab intially
                    var hash = "#" + (Tab.PortalID == Null.NullInteger ? "H" : "P") + "&" + Tab.TabID;
                    cancelHyperLink.NavigateUrl = navigateUrl + hash;
                }
                else if (!string.IsNullOrEmpty(UrlUtils.ValidReturnUrl(Request.QueryString["returnurl"])))
                {
                    cancelHyperLink.NavigateUrl = Request.QueryString["returnurl"];
                }

                if (ctlAudit.Visible)
                {
                    ctlAudit.Entity = Tab;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            redirectRow.Visible = ctlURL.UrlType != "N";

            var locales = LocaleController.Instance.GetLocales(PortalId);
            var urlLocale = locales.Values.FirstOrDefault(local => local.Code == PortalAlias.CultureCode);

            //if (_strAction == "edit")
            //{
            //    var options = new PageUrlOptions
            //    {
            //        PageUrlInputId = urlTextBox.ClientID,
            //        UpdateUrlButtonTooltip = LocalizeString("UpdateUrlButton.Tooltip"),
            //        UpdateUrlButtonCaption = LocalizeString("UpdateUrlButton.Caption"),
            //        PageUrlContainerId = UrlContainer.ClientID,
            //        UpdateUrlDto = new SaveUrlDto
            //                                {
            //                                    LocaleKey = (urlLocale != null) ? urlLocale.KeyID : -1,
            //                                    SiteAliasKey = -1,
            //                                    SiteAliasUsage = (int)PortalAliasUsageType.Default,
            //                                    StatusCodeKey = 200,
            //                                    Id = 0,
            //                                    IsSystem = false,
            //                                    QueryString = String.Empty,
            //                                    Path = string.Empty
            //                                }
            //    };

            //    var optionsAsJsonString = Json.Serialize(options);

            //    var script = string.Format("dnn.createPageUrl({0});", optionsAsJsonString);
            //    if (ScriptManager.GetCurrent(Page) != null)
            //    {
            //        // respect MS AJAX
            //        ScriptManager.RegisterStartupScript(Page, GetType(), "PageUrl", script, true);
            //    }
            //    else
            //    {
            //        Page.ClientScript.RegisterStartupScript(GetType(), "PageUrl", script, true);
            //    }
            //}
        }


        #endregion

        #region EventHandlers

        private void cboCacheProvider_Change(object sender, EventArgs e)
        {
            ShowCacheRows();
        }

        private void cboCopyPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayTabModules();
        }

        private void cboFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTemplates();
        }

        private void cboParentTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindBeforeAfterTabControls();

            insertPositionRow.Visible = cboPositionTab.Items.Count > 0;
        }

        private void cmdClearAllPageCache_Click(object sender, EventArgs e)
        {
            OutputCachingProvider.Instance(cboCacheProvider.SelectedValue).PurgeCache(PortalId);
            ShowCacheRows();
        }

        private void cmdClearPageCache_Click(object sender, EventArgs e)
        {
            OutputCachingProvider.Instance(cboCacheProvider.SelectedValue).Remove(TabId);
            ShowCacheRows();
        }

        private void cmdCopyPerm_Click(object sender, EventArgs e)
        {
            try
            {
                TabController.CopyPermissionsToChildren(TabController.Instance.GetTab(TabId, PortalId, false), dgPermissions.Permissions);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PermissionsCopied", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PermissionCopyError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void cmdCopySkin_Click(object sender, EventArgs e)
        {
            try
            {
                TabController.CopyDesignToChildren(TabController.Instance.GetTab(TabId, PortalId, false), pageSkinCombo.SelectedValue, pageContainerCombo.SelectedValue);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DesignCopied", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
            }
            catch (Exception ex)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DesignCopyError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   cmdDelete_Click runs when the Delete Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///   and localisation
        ///   [VMasanas]  30/09/2004  When a parent tab is deleted all child are also marked as deleted.
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdDelete_Click(object Sender, EventArgs e)
        {
            try
            {
                if (DeleteTab(TabId) && TabPermissionController.CanDeletePage())
                {
                    string strURL = Globals.GetPortalDomainName(PortalAlias.HTTPAlias, Request, true);

                    if ((Request.QueryString["returntabid"] != null))
                    {
                        // return to admin tab
                        strURL = Globals.NavigateURL(Convert.ToInt32(Request.QueryString["returntabid"]));
                    }

                    Response.Redirect(strURL, true);
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private bool IsPageUrlValid()
        {
            var url = urlTextBox.Text;

            if (string.IsNullOrEmpty(url))
            {
                return true;
            }

            var urlPath = url.TrimStart('/');
            bool modified;
            //Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs( UrlRewriterUtils.GetOptionsFromSettings(new DotNetNuke.Entities.Urls.FriendlyUrlSettings(PortalSettings.PortalId)) );
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                ShowWarningMessage(Localization.GetString("UrlPathCleaned.Error", LocalResourceFile));
                urlTextBox.Text = '/' + urlPath;
                urlTextBox.CssClass += " um-page-url-modified";
                return false;
            }

            //Validate for uniqueness
            int tabIdToValidate = -1;
            if (_strAction == "edit")
            {
                tabIdToValidate = Tab.TabID;
            }
            urlPath = FriendlyUrlController.ValidateUrl(urlPath, tabIdToValidate, PortalSettings, out modified);
            if (modified)
            {
                ShowWarningMessage(Localization.GetString("UrlPathNotUnique.Error", LocalResourceFile));
                urlTextBox.Text = '/' + urlPath;
                urlTextBox.CssClass += " um-page-url-modified";
                return false;
            }

            //update the text field with update value, because space char may replaced but the modified flag will not change to true.
            //in this case we should update the value back so that it can create tab with new path.
            urlTextBox.Text = '/' + urlPath;
            urlTextBox.CssClass = urlTextBox.CssClass.Replace(" um-page-url-modified", string.Empty);
            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   cmdUpdate_Click runs when the Update Button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///   [cnurse]	9/10/2004	Updated to reflect design changes for Help, 508 support
        ///   [aprasad]	3/21/2011	DNN-14685. Modified Redirect behavior after Save. Stays to the same page
        ///                         if more than one langugae is present, else redirects to the updated/new page
        ///   and localisation
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(object Sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid && TabPermissionController.HasTabPermission("ADD,EDIT,COPY,MANAGE") && IsPageUrlValid())
                {
                    int tabId = SaveTabData(_strAction);

                    if (tabId != Null.NullInteger)
                    {
                        var newCookie = new HttpCookie("LastPageId", string.Format("{0}:{1}", PortalSettings.PortalId, tabId))
                        {
                            Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/")
                        };
                        Response.Cookies.Add(newCookie);

                        if (PortalSettings.UserMode != PortalSettings.Mode.Edit)
                        {
                            Personalization.SetProfile("Usability", "UserMode" + PortalSettings.PortalId, "EDIT");
                        }

                        var redirectUrl = Globals.NavigateURL(tabId);

                        if ((Request.QueryString["returntabid"] != null))
                        {
                            // return to admin tab
                            redirectUrl = Globals.NavigateURL(Convert.ToInt32(Request.QueryString["returntabid"]));
                        }
                        else if (!string.IsNullOrEmpty(UrlUtils.ValidReturnUrl(Request.QueryString["returnurl"])))
                        {
                            redirectUrl = Request.QueryString["returnurl"];
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ctlURL.Url) || chkDisableLink.Checked)
                            {
                                // redirect to current tab if URL was specified ( add or copy )
                                redirectUrl = Globals.NavigateURL(TabId);
                            }
                        }

                        Response.Redirect(redirectUrl, true);
                    }
                }
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void languageTranslatedCheckbox_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if ((sender) is DnnCheckBox)
                {
                    var translatedCheckbox = (DnnCheckBox)sender;
                    int tabId = int.Parse(translatedCheckbox.CommandArgument);
                    TabInfo localizedTab = TabController.Instance.GetTab(tabId, PortalId, false);

                    TabController.Instance.UpdateTranslationStatus(localizedTab, translatedCheckbox.Checked);

                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }


        private void rblCacheIncludeExclude_Change(object sender, EventArgs e)
        {
            ShowCacheIncludeExcludeRows();
        }

        protected void cancelTranslation_Click(object sender, EventArgs e)
        {
            txtTranslationComment.Text = "";
            sendTranslationMessageRow.Visible = false;
        }

        protected void readyForTranslationButton_Click(object sender, EventArgs e)
        {
            sendTranslationMessageRow.Visible = true;
            sendTranslationMessageConfirm.Visible = false;

        }

        protected void submitTranslation_Click(object sender, EventArgs e)
        {
            sendTranslationMessageConfirm.Visible = true;
            try
            {
                // loop through all localized version of this page
                foreach (TabInfo localizedTab in Tab.LocalizedTabs.Values)
                {
                    var users = new Dictionary<int, UserInfo>();

                    //Give default translators for this language and administrators permissions
                    TabController.Instance.GiveTranslatorRoleEditRights(localizedTab, users);

                    //Send Messages to all the translators of new content
                    foreach (var translator in users.Values.Where(user => user.UserID != PortalSettings.AdministratorId))
                    {
                        AddTranslationSubmittedNotification(localizedTab, translator);
                    }
                }
                txtTranslationComment.Text = "";
                sendTranslationMessageRow.Visible = false;
                sendTranslationMessageConfirm.Attributes.Remove("class");
                sendTranslationMessageConfirm.Attributes.Add("class", "dnnFormMessage dnnFormSuccess");

                sendTranslationMessageConfirmMessage.Text = LocalizeString("TranslationMessageConfirmMessage.Text");
            }
            catch (Exception)
            {
                sendTranslationMessageConfirm.Attributes.Remove("class");
                sendTranslationMessageConfirm.Attributes.Add("class", "dnnFormMessage dnnFormError");
                sendTranslationMessageConfirmMessage.Text = LocalizeString("TranslationMessageConfirmMessage.Error");
                throw;
            }
        }

        protected void MakeNeutral_Click(object sender, EventArgs e)
        {
            if (TabController.Instance.GetTabsByPortal(PortalId).WithParentId(_tab.TabID).Count == 0)
            {
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);

                TabController.Instance.ConvertTabToNeutralLanguage(PortalId, _tab.TabID, defaultLocale.Code, true);

                Response.Redirect(Request.RawUrl, false);
            }
        }

        protected void AddMissing_Click(object sender, EventArgs e)
        {
            TabController.Instance.AddMissingLanguages(PortalId, _tab.TabID);

            BindCLControl();
        }

        protected void MakeTranslatable_Click(object sender, EventArgs e)
        {
            var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);
            TabController.Instance.LocalizeTab(_tab, defaultLocale);
            TabController.Instance.AddMissingLanguages(PortalId, _tab.TabID);
            TabController.Instance.ClearCache(PortalId);
            Response.Redirect(Request.RawUrl, false);
        }

        protected void cmdUpdateLocalization_Click(object sender, EventArgs e)
        {
            CLControl1.SaveData();

            var returnPath = Globals.NavigateURL();

            if (Request.QueryString["returntabid"] != null)
            {
                // return to admin tab
                var navigateUrl = Globals.NavigateURL(Convert.ToInt32(Request.QueryString["returntabid"]));
                // add location hash to let it select in admin tab intially
                var hash = "#" + (Tab.PortalID == Null.NullInteger ? "H" : "P") + "&" + Tab.TabID;
                returnPath = navigateUrl + hash;
            }
            else if (!string.IsNullOrEmpty(UrlUtils.ValidReturnUrl(Request.QueryString["returnurl"])))
            {
                returnPath = Request.QueryString["returnurl"];
            }
            Response.Redirect(returnPath);
         
        }


        #endregion
    }

    [DataContract]
    public class PageUrlOptions
    {
        [DataMember(Name = "pageUrlInputId")]
        public string PageUrlInputId;

        [DataMember(Name = "pageUrlContainerId")]
        public string PageUrlContainerId;

        [DataMember(Name = "updateUrlButtonCaption")]
        public string UpdateUrlButtonCaption;

        [DataMember(Name = "updateUrlButtonTooltip")]
        public string UpdateUrlButtonTooltip;

        [DataMember(Name = "updateUrlDto")]
        public SaveUrlDto UpdateUrlDto;

    }

}