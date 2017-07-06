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
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.Common;
using DotNetNuke.Web.UI.WebControls;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Web.Components.Controllers;
using DotNetNuke.Web.Components.Controllers.Models;
using Globals = DotNetNuke.Common.Globals;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.ControlPanels
// ReSharper restore CheckNamespace
{
    public partial class ControlBar : ControlPanelBase
    {
        protected DnnFileUpload FileUploader;

        protected string CurrentUICulture { get; set; }

        protected string LoginUrl { get; set; }

        protected string LoadTabModuleMessage { get; set; }

        protected string BookmarkModuleCategory 
        {
            get
            {
                return ControlBarController.Instance.GetBookmarkCategory(PortalSettings.PortalId);
            }
        }

        protected string BookmarkedModuleKeys
        {
            get
            {
                var bookmarkModules = Personalization.GetProfile("ControlBar", "module" + PortalSettings.PortalId);
                if (bookmarkModules == null)
                {
                    return string.Empty;
                }
                return bookmarkModules.ToString();
            }
        }

        public override bool IsDockable { get; set; }

        public override bool IncludeInControlHierarchy
        {
            get
            {
                return base.IncludeInControlHierarchy && (IsPageAdmin() || IsModuleAdmin());
            }
        }
      
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //page will be null if the control panel initial twice, it will be removed in the second time.
            if (Page != null)
            {
                ID = "ControlBar";

                var gettingStarted = DnnGettingStarted.GetCurrent(Page);
                if (gettingStarted == null)
                {
                    gettingStarted = new DnnGettingStarted();
                    Page.Form.Controls.Add(gettingStarted);
                }
                FileUploader = new DnnFileUpload {ID = "fileUploader", SupportHost = false};
                Page.Form.Controls.Add(FileUploader);

                LoadCustomMenuItems();
            }
        }

        protected override void OnLoad(EventArgs e)
        {           
            base.OnLoad(e);

            if (PortalSettings.EnablePopUps && Host.EnableModuleOnLineHelp)
            {
                helpLink.Text = string.Format(@"<li><a href=""{0}"">{1}</a></li>", UrlUtils.PopUpUrl(Host.HelpURL, this, PortalSettings, false, false), GetString("Tool.Help.ToolTip"));
            }
            else if (Host.EnableModuleOnLineHelp)
            {
                helpLink.Text = string.Format(@"<li><a href=""{0}"" target=""_blank"">{1}</a></li>", Host.HelpURL, GetString("Tool.Help.ToolTip"));
            }

            LoginUrl = ResolveClientUrl(@"~/Login.aspx");

            if (ControlPanel.Visible && IncludeInControlHierarchy)
            {
                ClientResourceManager.RegisterStyleSheet(Page, "~/admin/ControlPanel/ControlBar.css");
                jQuery.RegisterDnnJQueryPlugins(Page);
                ClientResourceManager.RegisterScript(Page, "~/resources/shared/scripts/dnn.controlBar.js");

                // Is there more than one site in this group?
                var multipleSites = GetCurrentPortalsGroup().Count() > 1;
                ClientAPI.RegisterClientVariable(Page, "moduleSharing", multipleSites.ToString().ToLowerInvariant(), true);
            }

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            var multipleSite = false;

            conrolbar_logo.ImageUrl = ControlBarController.Instance.GetControlBarLogoURL();
            if (!IsPostBack)
            {
                LoadCategoryList();
                multipleSite = LoadSiteList();
                LoadVisibilityList();
                AutoSetUserMode();
	            BindPortalsList();
	            BindLanguagesList();
            }

            LoadTabModuleMessage = multipleSite ? GetString("LoadingTabModuleCE.Text") : GetString("LoadingTabModule.Text");
		}
	
		#endregion

		#region Protected Methods

        protected bool CheckPageQuota()
        {
            UserInfo objUser = UserController.Instance.GetCurrentUserInfo();
            return (objUser != null && objUser.IsSuperUser) || PortalSettings.PageQuota == 0 || PortalSettings.Pages < PortalSettings.PageQuota;
        }

        protected string GetUpgradeIndicator()
        {
            UserInfo objUser = UserController.Instance.GetCurrentUserInfo();
           
            if (objUser != null && objUser.IsSuperUser)
            {
                var upgradeIndicator = ControlBarController.Instance.GetUpgradeIndicator(DotNetNukeContext.Current.Application.Version,
                    Request.IsLocal, Request.IsSecureConnection);
                if (upgradeIndicator == null)
                {
                    return String.Empty;
                }
                return GetUpgradeIndicatorButton(upgradeIndicator);
            }

            return string.Empty;
        }

        private void LoadCustomMenuItems()
        {
            foreach (var menuItem in ControlBarController.Instance.GetCustomMenuItems())
            {
                var liElement = new HtmlGenericControl("li");                
                liElement.Attributes.Add("id", menuItem.ID + "_tab");

                var control = Page.LoadControl(menuItem.Source);
                control.ID = menuItem.ID;
                            
                liElement.Controls.Add(control);
                
                CustomMenuItems.Controls.Add(liElement);
            }
        }

        private string GetUpgradeIndicatorButton(UpgradeIndicatorViewModel upgradeIndicator)
        {            
            return string.Format("<a id=\"{0}\" href=\"#\" onclick=\"{1}\" class=\"{2}\"><img src=\"{3}\" alt=\"{4}\" title=\"{5}\"/></a>",
                upgradeIndicator.ID, upgradeIndicator.WebAction, upgradeIndicator.CssClass, ResolveClientUrl(upgradeIndicator.ImageUrl), upgradeIndicator.AltText, upgradeIndicator.ToolTip);
        }

		protected string PreviewPopup()
		{
			var previewUrl = string.Format("{0}/Default.aspx?ctl={1}&previewTab={2}&TabID={2}", 
										Globals.AddHTTP(PortalSettings.PortalAlias.HTTPAlias), 
										"MobilePreview",
										PortalSettings.ActiveTab.TabID);

			if(PortalSettings.EnablePopUps)
			{
				return UrlUtils.PopUpUrl(previewUrl, this, PortalSettings, true, false, 660, 800);
			}

            return string.Format("location.href = \"{0}\"", previewUrl);
		}

        protected IEnumerable<string[]> LoadPaneList()
        {
            ArrayList panes = PortalSettings.Current.ActiveTab.Panes;
            var resultPanes = new List<string[]>();

            if(panes.Count < 4 )
            {
                foreach (var p in panes)
                {
                    var topPane = new string[]{
                        string.Format(GetString("Pane.AddTop.Text"), p),
                        p.ToString(),
                        "TOP"
                    };

                    var botPane = new string[]{
                        string.Format(GetString("Pane.AddBottom.Text"), p),
                        p.ToString(),
                        "BOTTOM"
                    };

                    resultPanes.Add(topPane);
                    resultPanes.Add(botPane);
                }
            }
            else
            {
                foreach (var p in panes)
                {

                    var botPane = new string[]{
                        string.Format(GetString("Pane.Add.Text"), p),
                        p.ToString(),
                        "BOTTOM"
                    };
                   
                    resultPanes.Add(botPane);
                }
            }

            return resultPanes;
        }


        protected string GetString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        protected string BuildToolUrl(string toolName, bool isHostTool, string moduleFriendlyName, 
                                      string controlKey, string navigateUrl, bool showAsPopUp)
        {
            if ((isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser))
            {
                return "javascript:void(0);";
            }

            if ((!string.IsNullOrEmpty(navigateUrl)))
            {
                return navigateUrl;
            }

            string returnValue = "javascript:void(0);";
            switch (toolName)
            {
                case "PageSettings":
                    if (TabPermissionController.CanManagePage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=settingTab");
                    }
                    break;
                case "CopyPage":
                    if (TabPermissionController.CanCopyPage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=copy&activeTab=copyTab");
                    }
                    break;
                case "DeletePage":
                    if (TabPermissionController.CanDeletePage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=delete");
                    }
                    break;
                case "PageTemplate":
                    if (TabPermissionController.CanManagePage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=advancedTab");
                    }
                    break;
                case "PageLocalization":
                    if (TabPermissionController.CanManagePage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=localizationTab");
                    }
                    break;
                case "PagePermission":
                    if (TabPermissionController.CanAdminPage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "Tab", "action=edit&activeTab=permissionsTab");
                    }
                    break;
                case "ImportPage":
                    if (TabPermissionController.CanImportPage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "ImportTab");
                    }
                    break;
                case "ExportPage":
                    if (TabPermissionController.CanExportPage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID, "ExportTab");
                    }
                    break;
                case "NewPage":
                    if (TabPermissionController.CanAddPage())
                    {
                        returnValue = Globals.NavigateURL("Tab", "activeTab=settingTab");
                    }
                    break;
                case "PublishPage":
                    if (TabPermissionController.CanAdminPage())
                    {
                        returnValue = Globals.NavigateURL(PortalSettings.ActiveTab.TabID);
                    }
                    break;
                default:
                    if ((!string.IsNullOrEmpty(moduleFriendlyName)))
                    {
                        var additionalParams = new List<string>();
                        returnValue = GetTabURL(additionalParams, toolName, isHostTool, 
                                                moduleFriendlyName, controlKey, showAsPopUp);
                    }
                    break;
            }
            return returnValue;
        }

        protected string GetTabURL(List<string> additionalParams, string toolName, bool isHostTool, 
                                   string moduleFriendlyName, string controlKey, bool showAsPopUp)
        {
            int portalId = (isHostTool) ? Null.NullInteger : PortalSettings.PortalId;

            string strURL = string.Empty;

            if (((additionalParams == null)))
            {
                additionalParams = new List<string>();
            }

            var moduleInfo = ModuleController.Instance.GetModuleByDefinition(portalId, moduleFriendlyName);

            if (((moduleInfo != null)))
            {
                bool isHostPage = (portalId == Null.NullInteger);
                if ((!string.IsNullOrEmpty(controlKey)))
                {
                    additionalParams.Insert(0, "mid=" + moduleInfo.ModuleID);
                    if (showAsPopUp && PortalSettings.EnablePopUps)
                    {
                        additionalParams.Add("popUp=true");
                    }
                }

                string currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                strURL = Globals.NavigateURL(moduleInfo.TabID, isHostPage, PortalSettings, controlKey, currentCulture, additionalParams.ToArray());
            }

            return strURL;
        }

		protected string GetTabURL(string tabName, bool isHostTool)
		{
			return GetTabURL(tabName, isHostTool, null);
		}

        protected string GetTabURL(string tabName, bool isHostTool, int? parentId)
        {
            if ((isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser))
            {
                return "javascript:void(0);";
            }

            int portalId = (isHostTool) ? Null.NullInteger : PortalSettings.PortalId;
            return GetTabURL(tabName, portalId, parentId);
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
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, PortalSettings.PortalId) ? "true" : "false";
        }

        protected string GetPublishActionText()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, PortalSettings.PortalId)
                    ? ClientAPI.GetSafeJSString(GetString("Tool.UnpublishPage.Text"))
                    : ClientAPI.GetSafeJSString(GetString("Tool.PublishPage.Text"));
        }

        protected string GetPublishConfirmText()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, PortalSettings.PortalId)
                    ? ClientAPI.GetSafeJSString(GetButtonConfirmMessage("UnpublishPage"))
                    : ClientAPI.GetSafeJSString(GetButtonConfirmMessage("PublishPage"));
        }

        protected string GetPublishConfirmHeader()
        {
            return TabPublishingController.Instance.IsTabPublished(TabController.CurrentPage.TabID, PortalSettings.PortalId)
                    ? ClientAPI.GetSafeJSString(GetButtonConfirmHeader("UnpublishPage"))
                    : ClientAPI.GetSafeJSString(GetButtonConfirmHeader("PublishPage"));
        }
        protected string GetMenuItem(string tabName, bool isHostTool)
        {
            if ((isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser))
            {
                return string.Empty;
            }

            List<TabInfo> tabList = null;
            if(isHostTool)
            {
                if(_hostTabs == null) GetHostTabs();
                tabList = _hostTabs;
            }
            else
            {
                if(_adminTabs == null) GetAdminTabs();
                tabList = _adminTabs;
            }

            var tab = tabList.SingleOrDefault(t => t.TabName == tabName);
            return GetMenuItem(tab);
        }

        protected string GetMenuItem(string tabName, bool isHostTool, bool isRemoveBookmark, bool isHideBookmark = false)
        {
            if ((isHostTool && !UserController.Instance.GetCurrentUserInfo().IsSuperUser))
            {
                return string.Empty;
            }

            List<TabInfo> tabList = null;
            if (isHostTool)
            {
                if (_hostTabs == null) GetHostTabs();
                tabList = _hostTabs;
            }
            else
            {
                if (_adminTabs == null) GetAdminTabs();
                tabList = _adminTabs;
            }

            var tab = tabList.SingleOrDefault(t => t.TabName == tabName);
            return GetMenuItem(tab, isRemoveBookmark, isHideBookmark);
        }

        protected string GetMenuItem(TabInfo tab, bool isRemoveBookmark = false, bool isHideBookmark = false)
        {
            if (tab == null) return string.Empty;
            if (tab.IsVisible && !tab.IsDeleted && !tab.DisableLink)
            {
                string name = !string.IsNullOrEmpty(tab.LocalizedTabName) ? tab.LocalizedTabName : tab.Title;
	            var linkClass = DotNetNukeContext.Current.Application.Name == "DNNCORP.CE" && tab.FullUrl.Contains("ProfessionalFeatures") ? "class=\"PE\"" : string.Empty;
                if (!isRemoveBookmark)
                {
                    if(!isHideBookmark)
						return string.Format("<li data-tabname=\"{3}\"><a href=\"{0}\" {4}>{1}</a><a href=\"javascript:void(0)\" class=\"bookmark\" title=\"{2}\"><span></span></a></li>",
                                             tab.FullUrl,
                                             name,
                                             ClientAPI.GetSafeJSString(GetString("Tool.AddToBookmarks.ToolTip")),
                                             ClientAPI.GetSafeJSString(tab.TabName),
											 linkClass);
                    else
						return string.Format("<li data-tabname=\"{3}\"><a href=\"{0}\" {4}>{1}</a><a href=\"javascript:void(0)\" class=\"bookmark hideBookmark\" data-title=\"{2}\"><span></span></a></li>",
                                            tab.FullUrl,
                                            name,
                                            ClientAPI.GetSafeJSString(GetString("Tool.AddToBookmarks.ToolTip")),
                                            ClientAPI.GetSafeJSString(tab.TabName),
											linkClass);
                }

                return string.Format("<li data-tabname=\"{3}\"><a href=\"{0}\" {4}>{1}</a><a href=\"javascript:void(0)\" class=\"removeBookmark\" title=\"{2}\"><span></span></a></li>",
                                        tab.FullUrl,
                                        name,
                                        ClientAPI.GetSafeJSString(GetString("Tool.RemoveFromBookmarks.ToolTip")),
                                        ClientAPI.GetSafeJSString(tab.TabName),
										linkClass);
            }
            return string.Empty;
        }
    

        protected string GetAdminBaseMenu()
        {
            var tabs = AdminBaseTabs;
            var sb = new StringBuilder();
            foreach(var tab in tabs)
            {
                var hideBookmark = AdminBookmarkItems.Contains(tab.TabName);
                sb.Append(GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetAdminAdvancedMenu()
        {
            var tabs = AdminAdvancedTabs;
            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = AdminBookmarkItems.Contains(tab.TabName);
                sb.Append(GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetHostBaseMenu()
        {
            var tabs = HostBaseTabs;
            
            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = HostBookmarkItems.Contains(tab.TabName);
                sb.Append(GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetHostAdvancedMenu()
        {
            var tabs = HostAdvancedTabs;
            var sb = new StringBuilder();
            foreach (var tab in tabs)
            {
                var hideBookmark = HostBookmarkItems.Contains(tab.TabName);
                sb.Append(GetMenuItem(tab, false, hideBookmark));
            }

            return sb.ToString();
        }

        protected string GetBookmarkItems(string title)
        {
            var isHostTool = title == "host";
            var bookmarkItems = isHostTool ? HostBookmarkItems : AdminBookmarkItems;
            
            if(bookmarkItems != null && bookmarkItems.Any())
            {
                var sb = new StringBuilder();
                foreach(var itemKey in bookmarkItems)
                {
                    sb.Append(GetMenuItem(itemKey, isHostTool, true));
                }
                return sb.ToString();
            }

            return string.Empty;
        }

        protected string GetButtonConfirmMessage(string toolName)
        {
            return ClientAPI.GetSafeJSString(Localization.GetString("Tool."+toolName+".ConfirmText", LocalResourceFile));
        }

        protected string GetButtonConfirmHeader(string toolName)
        {
            return ClientAPI.GetSafeJSString(Localization.GetString("Tool." + toolName + ".ConfirmHeader", LocalResourceFile));
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
                        pi.PortalID.ToString("D")
                    };

                    result.Add(p);
                }
            }

            return result;
        }

        protected IEnumerable<string[]> LoadLanguagesList()
        {
            var result = new List<string[]>();

            if (PortalSettings.AllowUserUICulture)
            {
                if(CurrentUICulture  == null)
                {
                    object oCulture = Personalization.GetProfile("Usability", "UICulture");
                    
                    if (oCulture != null)
                    {
                        CurrentUICulture = oCulture.ToString();
                    }
                    else
                    {
                        var l = new Localization();
                        CurrentUICulture = l.CurrentUICulture;
                        SetLanguage(true, CurrentUICulture);
                    }
                }
                

                IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, CurrentUICulture, "", false);
                foreach (var cultureItem in cultureListItems)
                {
                    var selected = cultureItem.Value == CurrentUICulture ? "true" : "false";
                    string[] p = new string[]
                                     {
                                         cultureItem.Text,
                                         cultureItem.Value,
                                         selected
                                     };
                    result.Add(p);
                }
            }

            return result;
        }

        protected bool ShowSwitchLanguagesPanel()
        {
             if (PortalSettings.AllowUserUICulture && PortalSettings.ContentLocalizationEnabled)
             {
                 if (CurrentUICulture == null)
                 {
                     object oCulture = Personalization.GetProfile("Usability", "UICulture");

                     if (oCulture != null)
                     {
                         CurrentUICulture = oCulture.ToString();
                     }
                     else
                     {
                         var l = new Localization();
                         CurrentUICulture = l.CurrentUICulture;
                     }
                 }

                 IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, CurrentUICulture, "", false);
                 return cultureListItems.Count() > 1;
             }

            return false;

        }

        protected string CheckedWhenInLayoutMode()
        {
            return UserMode == PortalSettings.Mode.Layout ? "checked='checked'" : string.Empty;
        }

        protected string CheckedWhenStayInEditMode()
        {
            string checkboxState = string.Empty;
            var cookie = Request.Cookies["StayInEditMode"];
            if(cookie != null && cookie.Value == "YES")
            {
                checkboxState = "checked='checked'";
            }

            if(UserMode == PortalSettings.Mode.Layout)
            {
                checkboxState += " disabled='disabled'";
            }

            return checkboxState;
        }

        protected string SpecialClassWhenNotInViewMode()
        {
            return UserMode == PortalSettings.Mode.View ? string.Empty : "controlBar_editPageInEditMode";
        }

        protected string GetModeForAttribute()
        {
            return UserMode.ToString().ToUpperInvariant();
        }

        protected string GetEditButtonLabel()
        {
            return UserMode == PortalSettings.Mode.Edit ? GetString("Tool.CloseEditMode.Text") : GetString("Tool.EditThisPage.Text");
        }

        protected virtual bool ActiveTabHasChildren()
        {
            var children = TabController.GetTabsByParent(PortalSettings.ActiveTab.TabID, PortalSettings.ActiveTab.PortalID);

            if (((children == null) || children.Count < 1))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Private Methods

        private new string LocalResourceFile
        {
            get
            {
                return string.Format("{0}/{1}/{2}.ascx.resx", TemplateSourceDirectory, Localization.LocalResourceDirectory, GetType().BaseType.Name);
            }
        }

        private void LoadCategoryList()
        {
            ITermController termController = Util.GetTermController();
            var terms = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Where(t => t.Name != "< None >").ToList();
            var allTerm = new Term("All", Localization.GetString("AllCategories", LocalResourceFile));
            terms.Add(allTerm);
            CategoryList.DataSource = terms;
            CategoryList.DataBind();
            if (!IsPostBack)
            {
                CategoryList.Select(!String.IsNullOrEmpty(BookmarkedModuleKeys) ? BookmarkModuleCategory : "All", false);
            }
        }

        private bool LoadSiteList()
        {
            // Is there more than one site in this group?
            var multipleSites = GetCurrentPortalsGroup().Count() > 1;
            if (multipleSites)
            {
                PageList.Services.GetTreeMethod = "ItemListService/GetPagesInPortalGroup";
                PageList.Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendantsInPortalGroup";
                PageList.Services.SearchTreeMethod = "ItemListService/SearchPagesInPortalGroup";
                PageList.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPageInPortalGroup";
                PageList.Services.SortTreeMethod = "ItemListService/SortPagesInPortalGroup";
            }

            PageList.UndefinedItem = new ListItem(SharedConstants.Unspecified, string.Empty);
            PageList.OnClientSelectionChanged.Add("dnn.controlBar.ControlBar_Module_PageList_Changed");
            return multipleSites;
        }

        private void LoadVisibilityList()
        { 
            var items = new Dictionary<string, string> { { "0", GetString("PermissionView") }, { "1", GetString("PermissionEdit") } };

            VisibilityLst.Items.Clear();
            VisibilityLst.DataValueField = "key";
            VisibilityLst.DataTextField = "value";
            VisibilityLst.DataSource = items;
            VisibilityLst.DataBind();
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

        private void AutoSetUserMode()
        {
            int tabId = PortalSettings.ActiveTab.TabID;
            int portalId = PortalSettings.Current.PortalId;
            string pageId = string.Format("{0}:{1}", portalId, tabId);

            HttpCookie cookie = Request.Cookies["StayInEditMode"];
            if (cookie != null && cookie.Value == "YES")
            {
                if (PortalSettings.Current.UserMode != Entities.Portals.PortalSettings.Mode.Edit)
                {
                    SetUserMode("EDIT");
                    SetLastPageHistory(pageId);
                    Response.Redirect(Request.RawUrl, true);
                    
                }

                return;
            }

            string lastPageId = GetLastPageHistory();
	        var isShowAsCustomError = Request.QueryString.AllKeys.Contains("aspxerrorpath");

			if (lastPageId != pageId && !isShowAsCustomError)
            {
                // navigate between pages
                if (PortalSettings.Current.UserMode != PortalSettings.Mode.View)
                {
                    SetUserMode("VIEW");
                    SetLastPageHistory(pageId);
                    Response.Redirect(Request.RawUrl, true);
                }
            }

	        if (!isShowAsCustomError)
	        {
		        SetLastPageHistory(pageId);
	        }
        }

        private void SetLastPageHistory(string pageId)
        {
            Response.Cookies.Add(new HttpCookie("LastPageId", pageId) { Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/") });
        }

        private string GetLastPageHistory()
        {
            HttpCookie cookie = Request.Cookies["LastPageId"];
            if (cookie != null)
                return cookie.Value;

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
			foreach (var portal in LoadPortalsList())
			{
				controlBar_SwitchSite.Items.Add(new DnnComboBoxItem(portal[0], portal[1]));
			}
		}

		private void BindLanguagesList()
		{
            if (ShowSwitchLanguagesPanel())
            {
                const string FlagImageUrlFormatString = "~/images/Flags/{0}.gif";
                foreach (var lang in LoadLanguagesList())
                {
                    var item = new DnnComboBoxItem(lang[0], lang[1]);
                    item.ImageUrl = string.Format(FlagImageUrlFormatString, item.Value);
                    if (lang[2] == "true")
                    {
                        item.Selected = true;
                    }

                    controlBar_SwitchLanguage.Items.Add(item);
                }

            }
		}

        #endregion

        #region Menu Items Properties

        private List<string> _adminBookmarkItems;
        protected List<string> AdminBookmarkItems
        {
            get
            {
                if (_adminBookmarkItems == null)
                {
                    var bookmarkItems = Personalization.GetProfile("ControlBar", "admin" + PortalSettings.PortalId);

                    _adminBookmarkItems = bookmarkItems != null 
                                                ? bookmarkItems.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() 
                                                : new List<string>();
                }

                return _adminBookmarkItems;
            }
        }

        private List<string> _hostBookmarkItems;
        protected List<string> HostBookmarkItems
        {
            get
            {
                if(_hostBookmarkItems == null)
                {
                    var bookmarkItems = Personalization.GetProfile("ControlBar", "host" + PortalSettings.PortalId);

                    _hostBookmarkItems = bookmarkItems != null 
                                            ? bookmarkItems.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() 
                                            : new List<string>();
                }

                return _hostBookmarkItems;
            }
        } 

        private List<TabInfo> _adminTabs;
        private List<TabInfo> _adminBaseTabs;
        private List<TabInfo> _adminAdvancedTabs;
        private List<TabInfo> _hostTabs;
        private List<TabInfo> _hostBaseTabs;
        private List<TabInfo> _hostAdvancedTabs;

        protected List<TabInfo> AdminBaseTabs
        {
            get
            {
                if (_adminBaseTabs == null)
                {
                    GetAdminTabs();
                }
                return _adminBaseTabs;
            }
        }

        protected List<TabInfo> AdminAdvancedTabs
        {
            get
            {
                if (_adminAdvancedTabs == null)
                {
                    GetAdminTabs();
                }
                return _adminAdvancedTabs;
            }
        }

        protected List<TabInfo> HostBaseTabs
        {
            get
            {
                if (_hostBaseTabs == null)
                {
                    GetHostTabs();
                }
                return _hostBaseTabs;
            }
        }

        protected List<TabInfo> HostAdvancedTabs
        {
            get
            {
                if (_hostAdvancedTabs == null)
                {
                    GetHostTabs();
                }
                return _hostAdvancedTabs;
            }
        }

        private void GetHostTabs()
        {
            var hostTab = TabController.GetTabByTabPath(Null.NullInteger, "//Host", string.Empty);
            var hosts = TabController.GetTabsByParent(hostTab, -1);

            var professionalTab = TabController.Instance.GetTabByName("Professional Features", -1);
            List<TabInfo> professionalTabs;
            if (professionalTab != null)
            {
                professionalTabs = TabController.GetTabsByParent(professionalTab.TabID, -1);
            }
            else
            {
                professionalTabs = new List<TabInfo>();
            }

            _hostTabs = new List<TabInfo>();
            _hostTabs.AddRange(hosts);
            _hostTabs.AddRange(professionalTabs);
            _hostTabs = _hostTabs.OrderBy(t => t.LocalizedTabName).ToList();

            _hostBaseTabs = new List<TabInfo>();
            _hostAdvancedTabs = new List<TabInfo>();

            foreach (var tabInfo in _hostTabs)
            {
                switch (tabInfo.TabName)
                {
                    case "Host Settings":
                    case "Site Management":
                    case "File Management":
                    case "Extensions":
                    case "Dashboard":
                    case "Health Monitoring":
                    case "Technical Support":
                    case "Knowledge Base":
                    case "Software and Documentation":
                        _hostBaseTabs.Add(tabInfo);
                        break;
                    default:
                        _hostAdvancedTabs.Add(tabInfo);
                        break;
                }
            }
        }

        private void GetAdminTabs()
        {
            var adminTab = TabController.GetTabByTabPath(PortalSettings.PortalId, "//Admin", string.Empty);
            _adminTabs = TabController.GetTabsByParent(adminTab, PortalSettings.PortalId).OrderBy(t => t.LocalizedTabName).ToList();

            _adminBaseTabs = new List<TabInfo>();
            _adminAdvancedTabs = new List<TabInfo>();

            foreach (var tabInfo in _adminTabs)
            {
                switch (tabInfo.TabName)
                {
                    case "Site Settings":
                    case "Pages":
                    case "Security Roles":
                    case "User Accounts":
                    case "File Management":
                    case "Recycle Bin":
                    case "Log Viewer":
                        _adminBaseTabs.Add(tabInfo);
                        break;
                    default:
                        _adminAdvancedTabs.Add(tabInfo);
                        break;
                }
            }

        }

        #endregion
    }
    
}
