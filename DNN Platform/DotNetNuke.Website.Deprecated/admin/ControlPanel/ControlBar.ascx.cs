// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.ControlPanels

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ImprovementsProgram;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Common;
    using DotNetNuke.Web.Components.Controllers;
    using DotNetNuke.Web.Components.Controllers.Models;
    using DotNetNuke.Web.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    public partial class ControlBar : ControlPanelBase
    {
        protected DnnFileUpload FileUploader;

        private readonly INavigationManager _navigationManager;

        private readonly IList<string> _adminCommonTabs = new List<string>
        {
            "Site Settings",
                                                                            "Security Roles",
                                                                            "User Accounts",
                                                                            "File Management",
        };

        private readonly IList<string> _hostCommonTabs = new List<string>
        {
            "Host Settings",
                                                                            "Site Management",
                                                                            "File Management",
                                                                            "Extensions",
                                                                            "Dashboard",
                                                                            "Health Monitoring",
                                                                            "Technical Support",
                                                                            "Knowledge Base",
                                                                            "Software and Documentation",
        };

        private List<string> _adminBookmarkItems;

        private List<string> _hostBookmarkItems;

        private List<TabInfo> _adminTabs;
        private List<TabInfo> _adminBaseTabs;
        private List<TabInfo> _adminAdvancedTabs;
        private List<TabInfo> _hostTabs;
        private List<TabInfo> _hostBaseTabs;
        private List<TabInfo> _hostAdvancedTabs;

        public ControlBar()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public override bool IncludeInControlHierarchy
        {
            get
            {
                return base.IncludeInControlHierarchy && (this.IsPageAdmin() || this.IsModuleAdmin());
            }
        }

        public override bool IsDockable { get; set; }

        protected string BookmarkModuleCategory
        {
            get
            {
                return ControlBarController.Instance.GetBookmarkCategory(this.PortalSettings.PortalId);
            }
        }

        protected string BookmarkedModuleKeys
        {
            get
            {
                var bookmarkModules = Personalization.GetProfile("ControlBar", "module" + this.PortalSettings.PortalId);
                if (bookmarkModules == null)
                {
                    return string.Empty;
                }

                return bookmarkModules.ToString();
            }
        }

        protected List<string> AdminBookmarkItems
        {
            get
            {
                if (this._adminBookmarkItems == null)
                {
                    var bookmarkItems = Personalization.GetProfile("ControlBar", "admin" + this.PortalSettings.PortalId);

                    this._adminBookmarkItems = bookmarkItems != null
                                                ? bookmarkItems.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                                                : new List<string>();
                }

                return this._adminBookmarkItems;
            }
        }

        protected List<string> HostBookmarkItems
        {
            get
            {
                if (this._hostBookmarkItems == null)
                {
                    var bookmarkItems = Personalization.GetProfile("ControlBar", "host" + this.PortalSettings.PortalId);

                    this._hostBookmarkItems = bookmarkItems != null
                                            ? bookmarkItems.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                                            : new List<string>();
                }

                return this._hostBookmarkItems;
            }
        }

        protected List<TabInfo> AdminBaseTabs
        {
            get
            {
                if (this._adminBaseTabs == null)
                {
                    this.GetAdminTabs();
                }

                return this._adminBaseTabs;
            }
        }

        protected List<TabInfo> AdminAdvancedTabs
        {
            get
            {
                if (this._adminAdvancedTabs == null)
                {
                    this.GetAdminTabs();
                }

                return this._adminAdvancedTabs;
            }
        }

        protected List<TabInfo> HostBaseTabs
        {
            get
            {
                if (this._hostBaseTabs == null)
                {
                    this.GetHostTabs();
                }

                return this._hostBaseTabs;
            }
        }

        protected List<TabInfo> HostAdvancedTabs
        {
            get
            {
                if (this._hostAdvancedTabs == null)
                {
                    this.GetHostTabs();
                }

                return this._hostAdvancedTabs;
            }
        }

        protected bool IsBeaconEnabled
        {
            get
            {
                var user = UserController.Instance.GetCurrentUserInfo();
                return BeaconService.Instance.IsBeaconEnabledForControlBar(user);
            }
        }

        protected string CurrentUICulture { get; set; }

        protected string LoginUrl { get; set; }

        protected string LoadTabModuleMessage { get; set; }

        private new string LocalResourceFile
        {
            get
            {
                return string.Format("{0}/{1}/{2}.ascx.resx", this.TemplateSourceDirectory, Localization.LocalResourceDirectory, this.GetType().BaseType?.Name);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // page will be null if the control panel initial twice, it will be removed in the second time.
            if (this.Page != null)
            {
                this.ID = "ControlBar";

                this.FileUploader = new DnnFileUpload { ID = "fileUploader", SupportHost = false };
                this.Page.Form.Controls.Add(this.FileUploader);

                this.LoadCustomMenuItems();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.PortalSettings.EnablePopUps && Host.EnableModuleOnLineHelp)
            {
                this.helpLink.Text = string.Format(@"<li><a href=""{0}"">{1}</a></li>", UrlUtils.PopUpUrl(Host.HelpURL, this, this.PortalSettings, false, false), this.GetString("Tool.Help.ToolTip"));
            }
            else if (Host.EnableModuleOnLineHelp)
            {
                this.helpLink.Text = string.Format(@"<li><a href=""{0}"" target=""_blank"">{1}</a></li>", Host.HelpURL, this.GetString("Tool.Help.ToolTip"));
            }

            this.LoginUrl = this.ResolveClientUrl(@"~/Login.aspx");

            if (this.ControlPanel.Visible && this.IncludeInControlHierarchy)
            {
                ClientResourceManager.RegisterStyleSheet(this.Page, "~/admin/ControlPanel/ControlBar.css", FileOrder.Css.ResourceCss);
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                ClientResourceManager.RegisterScript(this.Page, "~/resources/shared/scripts/dnn.controlBar.js");

                // Is there more than one site in this group?
                var multipleSites = GetCurrentPortalsGroup().Count() > 1;
                ClientAPI.RegisterClientVariable(this.Page, "moduleSharing", multipleSites.ToString().ToLowerInvariant(), true);
            }

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            var multipleSite = false;

            this.conrolbar_logo.ImageUrl = ControlBarController.Instance.GetControlBarLogoURL();
            if (!this.IsPostBack)
            {
                this.LoadCategoryList();
                multipleSite = this.LoadSiteList();
                this.LoadVisibilityList();
                this.AutoSetUserMode();
                this.BindPortalsList();
                this.BindLanguagesList();
            }

            this.LoadTabModuleMessage = multipleSite ? this.GetString("LoadingTabModuleCE.Text") : this.GetString("LoadingTabModule.Text");
        }

        protected bool CheckPageQuota()
        {
            UserInfo objUser = UserController.Instance.GetCurrentUserInfo();
            return (objUser != null && objUser.IsSuperUser) || this.PortalSettings.PageQuota == 0 || this.PortalSettings.Pages < this.PortalSettings.PageQuota;
        }

        protected string GetUpgradeIndicator()
        {
            UserInfo objUser = UserController.Instance.GetCurrentUserInfo();

            if (objUser != null && objUser.IsSuperUser)
            {
                var upgradeIndicator = ControlBarController.Instance.GetUpgradeIndicator(
                    DotNetNukeContext.Current.Application.Version,
                    this.Request.IsLocal, this.Request.IsSecureConnection);
                if (upgradeIndicator == null)
                {
                    return string.Empty;
                }

                return this.GetUpgradeIndicatorButton(upgradeIndicator);
            }

            return string.Empty;
        }

        protected string PreviewPopup()
        {
            var previewUrl = string.Format(
                "{0}/Default.aspx?ctl={1}&previewTab={2}&TabID={2}",
                Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias),
                "MobilePreview",
                this.PortalSettings.ActiveTab.TabID);

            if (this.PortalSettings.EnablePopUps)
            {
                return UrlUtils.PopUpUrl(previewUrl, this, this.PortalSettings, true, false, 660, 800);
            }

            return string.Format("location.href = \"{0}\"", previewUrl);
        }

        protected IEnumerable<string[]> LoadPaneList()
        {
            ArrayList panes = PortalSettings.Current.ActiveTab.Panes;
            var resultPanes = new List<string[]>();

            if (panes.Count < 4)
            {
                foreach (var p in panes)
                {
                    var topPane = new[]
                    {
                        string.Format(this.GetString("Pane.AddTop.Text"), p),
                        p.ToString(),
                        "TOP",
                    };

                    var botPane = new[]
                    {
                        string.Format(this.GetString("Pane.AddBottom.Text"), p),
                        p.ToString(),
                        "BOTTOM",
                    };

                    resultPanes.Add(topPane);
                    resultPanes.Add(botPane);
                }
            }
            else
            {
                foreach (var p in panes)
                {
                    var botPane = new[]
                    {
                        string.Format(this.GetString("Pane.Add.Text"), p),
                        p.ToString(),
                        "BOTTOM",
                    };

                    resultPanes.Add(botPane);
                }
            }

            return resultPanes;
        }

        protected string GetString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected string BuildToolUrl(string toolName, bool isHostTool, string moduleFriendlyName,
                                      string controlKey, string navigateUrl, bool showAsPopUp)
        {
            if (isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return "javascript:void(0);";
            }

            if (!string.IsNullOrEmpty(navigateUrl))
            {
                return navigateUrl;
            }

            string returnValue = "javascript:void(0);";
            switch (toolName)
            {
                case "PageSettings":
                    if (TabPermissionController.CanManagePage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=settingTab");
                    }

                    break;
                case "CopyPage":
                    if (TabPermissionController.CanCopyPage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Tab", "action=copy&activeTab=copyTab");
                    }

                    break;
                case "DeletePage":
                    if (TabPermissionController.CanDeletePage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Tab", "action=delete");
                    }

                    break;
                case "PageTemplate":
                    if (TabPermissionController.CanManagePage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=advancedTab");
                    }

                    break;
                case "PageLocalization":
                    if (TabPermissionController.CanManagePage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=localizationTab");
                    }

                    break;
                case "PagePermission":
                    if (TabPermissionController.CanAdminPage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=permissionsTab");
                    }

                    break;
                case "ImportPage":
                    if (TabPermissionController.CanImportPage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "ImportTab");
                    }

                    break;
                case "ExportPage":
                    if (TabPermissionController.CanExportPage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "ExportTab");
                    }

                    break;
                case "NewPage":
                    if (TabPermissionController.CanAddPage())
                    {
                        returnValue = this._navigationManager.NavigateURL("Tab", "activeTab=settingTab");
                    }

                    break;
                case "PublishPage":
                    if (TabPermissionController.CanAdminPage())
                    {
                        returnValue = this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID);
                    }

                    break;
                default:
                    if (!string.IsNullOrEmpty(moduleFriendlyName))
                    {
                        var additionalParams = new List<string>();
                        returnValue = this.GetTabURL(additionalParams, toolName, isHostTool,
                                                moduleFriendlyName, controlKey, showAsPopUp);
                    }

                    break;
            }

            return returnValue;
        }

        protected string GetTabURL(List<string> additionalParams, string toolName, bool isHostTool,
                                   string moduleFriendlyName, string controlKey, bool showAsPopUp)
        {
            int portalId = isHostTool ? Null.NullInteger : this.PortalSettings.PortalId;

            string strURL = string.Empty;

            if (additionalParams == null)
            {
                additionalParams = new List<string>();
            }

            var moduleInfo = ModuleController.Instance.GetModuleByDefinition(portalId, moduleFriendlyName);

            if (moduleInfo != null)
            {
                bool isHostPage = portalId == Null.NullInteger;
                if (!string.IsNullOrEmpty(controlKey))
                {
                    additionalParams.Insert(0, "mid=" + moduleInfo.ModuleID);
                    if (showAsPopUp && this.PortalSettings.EnablePopUps)
                    {
                        additionalParams.Add("popUp=true");
                    }
                }

                string currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                strURL = this._navigationManager.NavigateURL(moduleInfo.TabID, isHostPage, this.PortalSettings, controlKey, currentCulture, additionalParams.ToArray());
            }

            return strURL;
        }

        protected string GetTabURL(string tabName, bool isHostTool)
        {
            return this.GetTabURL(tabName, isHostTool, null);
        }

        protected string GetTabURL(string tabName, bool isHostTool, int? parentId)
        {
            if (isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return "javascript:void(0);";
            }

            int portalId = isHostTool ? Null.NullInteger : this.PortalSettings.PortalId;
            return this.GetTabURL(tabName, portalId, parentId);
        }

        protected string GetTabURL(string tabName, int portalId, int? parentId)
        {
            var tab = parentId.HasValue ? TabController.Instance.GetTabByName(tabName, portalId, parentId.Value) : TabController.Instance.GetTabByName(tabName, portalId);

            if (tab != null)
            {
                return tab.FullUrl;
            }

            return string.Empty;
        }

        protected string GetTabPublishing()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, this.PortalSettings.PortalId) ? "true" : "false";
        }

        protected string GetPublishActionText()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, this.PortalSettings.PortalId)
                    ? ClientAPI.GetSafeJSString(this.GetString("Tool.UnpublishPage.Text"))
                    : ClientAPI.GetSafeJSString(this.GetString("Tool.PublishPage.Text"));
        }

        protected string GetPublishConfirmText()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, this.PortalSettings.PortalId)
                    ? this.GetButtonConfirmMessage("UnpublishPage")
                    : this.GetButtonConfirmMessage("PublishPage");
        }

        protected string GetPublishConfirmHeader()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, this.PortalSettings.PortalId)
                    ? this.GetButtonConfirmHeader("UnpublishPage")
                    : this.GetButtonConfirmHeader("PublishPage");
        }

        protected string GetMenuItem(string tabName, bool isHostTool)
        {
            if (isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return string.Empty;
            }

            List<TabInfo> tabList;
            if (isHostTool)
            {
                if (this._hostTabs == null)
                {
                    this.GetHostTabs();
                }

                tabList = this._hostTabs;
            }
            else
            {
                if (this._adminTabs == null)
                {
                    this.GetAdminTabs();
                }

                tabList = this._adminTabs;
            }

            var tab = tabList?.SingleOrDefault(t => t.TabName == tabName);
            return this.GetMenuItem(tab);
        }

        protected string GetMenuItem(string tabName, bool isHostTool, bool isRemoveBookmark, bool isHideBookmark = false)
        {
            if (isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                return string.Empty;
            }

            List<TabInfo> tabList;
            if (isHostTool)
            {
                if (this._hostTabs == null)
                {
                    this.GetHostTabs();
                }

                tabList = this._hostTabs;
            }
            else
            {
                if (this._adminTabs == null)
                {
                    this.GetAdminTabs();
                }

                tabList = this._adminTabs;
            }

            var tab = tabList?.SingleOrDefault(t => t.TabName == tabName);
            return this.GetMenuItem(tab, isRemoveBookmark, isHideBookmark);
        }

        protected string GetMenuItem(TabInfo tab, bool isRemoveBookmark = false, bool isHideBookmark = false)
        {
            if (tab == null)
            {
                return string.Empty;
            }

            if (tab.IsVisible && !tab.IsDeleted && !tab.DisableLink)
            {
                string name = !string.IsNullOrEmpty(tab.LocalizedTabName) ? tab.LocalizedTabName : tab.Title;
                var linkClass = DotNetNukeContext.Current.Application.Name == "DNNCORP.CE" && tab.FullUrl.Contains("ProfessionalFeatures") ? "class=\"PE\"" : string.Empty;
                if (!isRemoveBookmark)
                {
                    if (!isHideBookmark)
                    {
                        return string.Format(
                            "<li data-tabname=\"{3}\"><a href=\"{0}\" {4}>{1}</a><a href=\"javascript:void(0)\" class=\"bookmark\" title=\"{2}\"><span></span></a></li>",
                            tab.FullUrl,
                            name,
                            ClientAPI.GetSafeJSString(this.GetString("Tool.AddToBookmarks.ToolTip")),
                            ClientAPI.GetSafeJSString(tab.TabName),
                            linkClass);
                    }
                    else
                    {
                        return string.Format(
                            "<li data-tabname=\"{3}\"><a href=\"{0}\" {4}>{1}</a><a href=\"javascript:void(0)\" class=\"bookmark hideBookmark\" data-title=\"{2}\"><span></span></a></li>",
                            tab.FullUrl,
                            name,
                            ClientAPI.GetSafeJSString(this.GetString("Tool.AddToBookmarks.ToolTip")),
                            ClientAPI.GetSafeJSString(tab.TabName),
                            linkClass);
                    }
                }

                return string.Format(
                    "<li data-tabname=\"{3}\"><a href=\"{0}\" {4}>{1}</a><a href=\"javascript:void(0)\" class=\"removeBookmark\" title=\"{2}\"><span></span></a></li>",
                    tab.FullUrl,
                    name,
                    ClientAPI.GetSafeJSString(this.GetString("Tool.RemoveFromBookmarks.ToolTip")),
                    ClientAPI.GetSafeJSString(tab.TabName),
                    linkClass);
            }

            return string.Empty;
        }

        protected string GetAdminBaseMenu()
        {
            var tabs = this.AdminBaseTabs;
            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = this.AdminBookmarkItems.Contains(tab.TabName);
                sb.Append(this.GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetAdminAdvancedMenu()
        {
            var tabs = this.AdminAdvancedTabs;
            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = this.AdminBookmarkItems.Contains(tab.TabName);
                sb.Append(this.GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetHostBaseMenu()
        {
            var tabs = this.HostBaseTabs;

            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = this.HostBookmarkItems.Contains(tab.TabName);
                sb.Append(this.GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetHostAdvancedMenu()
        {
            var tabs = this.HostAdvancedTabs;
            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = this.HostBookmarkItems.Contains(tab.TabName);
                sb.Append(this.GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetBookmarkItems(string title)
        {
            var isHostTool = title == "host";
            var bookmarkItems = isHostTool ? this.HostBookmarkItems : this.AdminBookmarkItems;

            if (bookmarkItems != null && bookmarkItems.Any())
            {
                var sb = new StringBuilder();
                foreach (var itemKey in bookmarkItems)
                {
                    sb.Append(this.GetMenuItem(itemKey, isHostTool, true));
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        protected string GetButtonConfirmMessage(string toolName)
        {
            return ClientAPI.GetSafeJSString(Localization.GetString("Tool." + toolName + ".ConfirmText", this.LocalResourceFile));
        }

        protected string GetButtonConfirmHeader(string toolName)
        {
            return ClientAPI.GetSafeJSString(Localization.GetString("Tool." + toolName + ".ConfirmHeader", this.LocalResourceFile));
        }

        protected IEnumerable<string[]> LoadPortalsList()
        {
            var portals = PortalController.Instance.GetPortals();

            var result = new List<string[]>();
            foreach (var portal in portals)
            {
                var pi = portal as PortalInfo;

                if (pi != null)
                {
                    string[] p =
                    {
                        pi.PortalName,
                        pi.PortalID.ToString("D"),
                    };

                    result.Add(p);
                }
            }

            return result;
        }

        protected IEnumerable<string[]> LoadLanguagesList()
        {
            var result = new List<string[]>();

            if (this.PortalSettings.AllowUserUICulture)
            {
                if (this.CurrentUICulture == null)
                {
                    object oCulture = Personalization.GetProfile("Usability", "UICulture");

                    if (oCulture != null)
                    {
                        this.CurrentUICulture = oCulture.ToString();
                    }
                    else
                    {
                        var l = new Localization();
                        this.CurrentUICulture = l.CurrentUICulture;
                        this.SetLanguage(true, this.CurrentUICulture);
                    }
                }

                IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, this.CurrentUICulture, string.Empty, false);
                foreach (var cultureItem in cultureListItems)
                {
                    var selected = cultureItem.Value == this.CurrentUICulture ? "true" : "false";
                    string[] p = new string[]
                                     {
                                         cultureItem.Text,
                                         cultureItem.Value,
                                         selected,
                                     };
                    result.Add(p);
                }
            }

            return result;
        }

        protected bool ShowSwitchLanguagesPanel()
        {
            if (this.PortalSettings.AllowUserUICulture && this.PortalSettings.ContentLocalizationEnabled)
            {
                if (this.CurrentUICulture == null)
                {
                    object oCulture = Personalization.GetProfile("Usability", "UICulture");

                    if (oCulture != null)
                    {
                        this.CurrentUICulture = oCulture.ToString();
                    }
                    else
                    {
                        var l = new Localization();
                        this.CurrentUICulture = l.CurrentUICulture;
                    }
                }

                IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, this.CurrentUICulture, string.Empty, false);
                return cultureListItems.Count() > 1;
            }

            return false;
        }

        protected string CheckedWhenInLayoutMode()
        {
            return this.UserMode == PortalSettings.Mode.Layout ? "checked='checked'" : string.Empty;
        }

        protected string CheckedWhenStayInEditMode()
        {
            string checkboxState = string.Empty;
            var cookie = this.Request.Cookies["StayInEditMode"];
            if (cookie != null && cookie.Value == "YES")
            {
                checkboxState = "checked='checked'";
            }

            if (this.UserMode == PortalSettings.Mode.Layout)
            {
                checkboxState += " disabled='disabled'";
            }

            return checkboxState;
        }

        protected string SpecialClassWhenNotInViewMode()
        {
            return this.UserMode == PortalSettings.Mode.View ? string.Empty : "controlBar_editPageInEditMode";
        }

        protected string GetModeForAttribute()
        {
            return this.UserMode.ToString().ToUpperInvariant();
        }

        protected string GetEditButtonLabel()
        {
            return this.UserMode == PortalSettings.Mode.Edit ? this.GetString("Tool.CloseEditMode.Text") : this.GetString("Tool.EditThisPage.Text");
        }

        protected virtual bool ActiveTabHasChildren()
        {
            var children = TabController.GetTabsByParent(this.PortalSettings.ActiveTab.TabID, this.PortalSettings.ActiveTab.PortalID);

            if ((children == null) || children.Count < 1)
            {
                return false;
            }

            return true;
        }

        protected bool IsLanguageModuleInstalled()
        {
            return DesktopModuleController.GetDesktopModuleByFriendlyName("Languages") != null;
        }

        protected string GetBeaconUrl()
        {
            var beaconService = BeaconService.Instance;
            var user = UserController.Instance.GetCurrentUserInfo();
            var path = this.PortalSettings.ActiveTab.TabPath;
            return beaconService.GetBeaconUrl(user, path);
        }

        private static IEnumerable<PortalInfo> GetCurrentPortalsGroup()
        {
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();

            var result = (from @group in groups
                          select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId)
                              into portals
                          where portals.Any(x => x.PortalID == PortalSettings.Current.PortalId)
                          select portals.ToArray()).FirstOrDefault();

            // Are we in a group of one?
            if (result == null || result.Length == 0)
            {
                result = new[] { PortalController.Instance.GetPortal(PortalSettings.Current.PortalId) };
            }

            return result;
        }

        private void LoadCustomMenuItems()
        {
            foreach (var menuItem in ControlBarController.Instance.GetCustomMenuItems())
            {
                var liElement = new HtmlGenericControl("li");
                liElement.Attributes.Add("id", menuItem.ID + "_tab");

                var control = this.Page.LoadControl(menuItem.Source);
                control.ID = menuItem.ID;

                liElement.Controls.Add(control);

                this.CustomMenuItems.Controls.Add(liElement);
            }
        }

        private string GetUpgradeIndicatorButton(UpgradeIndicatorViewModel upgradeIndicator)
        {
            return string.Format(
                "<a id=\"{0}\" href=\"#\" onclick=\"{1}\" class=\"{2}\"><img src=\"{3}\" alt=\"{4}\" title=\"{5}\"/></a>",
                upgradeIndicator.ID, upgradeIndicator.WebAction, upgradeIndicator.CssClass, this.ResolveClientUrl(upgradeIndicator.ImageUrl), upgradeIndicator.AltText, upgradeIndicator.ToolTip);
        }

        private void LoadCategoryList()
        {
            ITermController termController = Util.GetTermController();
            var terms = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Where(t => t.Name != "< None >").ToList();
            var allTerm = new Term("All", Localization.GetString("AllCategories", this.LocalResourceFile));
            terms.Add(allTerm);
            this.CategoryList.DataSource = terms;
            this.CategoryList.DataBind();
            if (!this.IsPostBack)
            {
                this.CategoryList.Select(!string.IsNullOrEmpty(this.BookmarkedModuleKeys) ? this.BookmarkModuleCategory : "All", false);
            }
        }

        private bool LoadSiteList()
        {
            // Is there more than one site in this group?
            var multipleSites = GetCurrentPortalsGroup().Count() > 1;
            if (multipleSites)
            {
                this.PageList.Services.GetTreeMethod = "ItemListService/GetPagesInPortalGroup";
                this.PageList.Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendantsInPortalGroup";
                this.PageList.Services.SearchTreeMethod = "ItemListService/SearchPagesInPortalGroup";
                this.PageList.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPageInPortalGroup";
                this.PageList.Services.SortTreeMethod = "ItemListService/SortPagesInPortalGroup";
            }

            this.PageList.UndefinedItem = new ListItem(DynamicSharedConstants.Unspecified, string.Empty);
            this.PageList.OnClientSelectionChanged.Add("dnn.controlBar.ControlBar_Module_PageList_Changed");
            return multipleSites;
        }

        private void LoadVisibilityList()
        {
            var items = new Dictionary<string, string> { { "0", this.GetString("PermissionView") }, { "1", this.GetString("PermissionEdit") } };

            this.VisibilityLst.Items.Clear();
            this.VisibilityLst.DataValueField = "key";
            this.VisibilityLst.DataTextField = "value";
            this.VisibilityLst.DataSource = items;
            this.VisibilityLst.DataBind();
        }

        private void AutoSetUserMode()
        {
            int tabId = this.PortalSettings.ActiveTab.TabID;
            int portalId = PortalSettings.Current.PortalId;
            string pageId = string.Format("{0}:{1}", portalId, tabId);

            HttpCookie cookie = this.Request.Cookies["StayInEditMode"];
            if (cookie != null && cookie.Value == "YES")
            {
                if (PortalSettings.Current.UserMode != PortalSettings.Mode.Edit)
                {
                    this.SetUserMode("EDIT");
                    this.SetLastPageHistory(pageId);
                    this.Response.Redirect(this.Request.RawUrl, true);
                }

                return;
            }

            string lastPageId = this.GetLastPageHistory();
            var isShowAsCustomError = this.Request.QueryString.AllKeys.Contains("aspxerrorpath");

            if (lastPageId != pageId && !isShowAsCustomError)
            {
                // navigate between pages
                if (PortalSettings.Current.UserMode != PortalSettings.Mode.View)
                {
                    this.SetUserMode("VIEW");
                    this.SetLastPageHistory(pageId);
                    this.Response.Redirect(this.Request.RawUrl, true);
                }
            }

            if (!isShowAsCustomError)
            {
                this.SetLastPageHistory(pageId);
            }
        }

        private void SetLastPageHistory(string pageId)
        {
            this.Response.Cookies.Add(new HttpCookie("LastPageId", pageId) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
        }

        private string GetLastPageHistory()
        {
            var cookie = this.Request.Cookies["LastPageId"];
            if (cookie != null)
            {
                return cookie.Value;
            }

            return "NEW";
        }

        private void SetLanguage(bool update, string currentCulture)
        {
            if (update)
            {
                Personalization.SetProfile("Usability", "UICulture", currentCulture);
            }
        }

        private void BindPortalsList()
        {
            foreach (var portal in this.LoadPortalsList())
            {
                this.controlBar_SwitchSite.Items.Add(new DnnComboBoxItem(portal[0], portal[1]));
            }
        }

        private void BindLanguagesList()
        {
            if (this.ShowSwitchLanguagesPanel())
            {
                const string FlagImageUrlFormatString = "~/images/Flags/{0}.gif";
                foreach (var lang in this.LoadLanguagesList())
                {
                    var item = new DnnComboBoxItem(lang[0], lang[1]);
                    item.ImageUrl = string.Format(FlagImageUrlFormatString, item.Value);
                    if (lang[2] == "true")
                    {
                        item.Selected = true;
                    }

                    this.controlBar_SwitchLanguage.Items.Add(item);
                }
            }
        }

        private void GetHostTabs()
        {
            var hostTab = TabController.GetTabByTabPath(Null.NullInteger, "//Host", string.Empty);
            var hosts = TabController.GetTabsByParent(hostTab, -1);

            var professionalTab = TabController.Instance.GetTabByName("Professional Features", -1);
            var professionalTabs = professionalTab != null
                ? TabController.GetTabsByParent(professionalTab.TabID, -1)
                : new List<TabInfo>();

            this._hostTabs = new List<TabInfo>();
            this._hostTabs.AddRange(hosts);
            this._hostTabs.AddRange(professionalTabs);
            this._hostTabs = this._hostTabs.OrderBy(t => t.LocalizedTabName).ToList();

            this._hostBaseTabs = new List<TabInfo>();
            this._hostAdvancedTabs = new List<TabInfo>();

            foreach (var tabInfo in this._hostTabs)
            {
                if (this.IsCommonTab(tabInfo, true))
                {
                    this._hostBaseTabs.Add(tabInfo);
                }
                else
                {
                    this._hostAdvancedTabs.Add(tabInfo);
                }
            }
        }

        private void GetAdminTabs()
        {
            var adminTab = TabController.GetTabByTabPath(this.PortalSettings.PortalId, "//Admin", string.Empty);
            this._adminTabs = TabController.GetTabsByParent(adminTab, this.PortalSettings.PortalId).OrderBy(t => t.LocalizedTabName).ToList();

            this._adminBaseTabs = new List<TabInfo>();
            this._adminAdvancedTabs = new List<TabInfo>();

            foreach (var tabInfo in this._adminTabs)
            {
                if (this.IsCommonTab(tabInfo))
                {
                    this._adminBaseTabs.Add(tabInfo);
                }
                else
                {
                    this._adminAdvancedTabs.Add(tabInfo);
                }
            }
        }

        private bool IsCommonTab(TabInfo tab, bool isHost = false)
        {
            if (tab.TabSettings.ContainsKey("ControlBar_CommonTab") &&
                tab.TabSettings["ControlBar_CommonTab"].ToString() == "Y")
            {
                return true;
            }

            return isHost ? this._hostCommonTabs.Contains(tab.TabName) : this._adminCommonTabs.Contains(tab.TabName);
        }
    }
}
