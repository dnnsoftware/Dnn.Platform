// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.Actions;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;

    /// <summary>
    /// TabController provides all operation to tabinfo.
    /// </summary>
    /// <remarks>
    /// Tab is equal to page in DotNetNuke.
    /// Tabs will be a sitemap for a poatal, and every request at first need to check whether there is valid tab information
    /// include in the url, if not it will use default tab to display information.
    /// </remarks>
    public partial class TabController : ServiceLocator<ITabController, TabController>, ITabController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabController));
        private static readonly Regex TabNameCheck1 = new Regex("^LPT[1-9]$|^COM[1-9]$", RegexOptions.IgnoreCase);
        private static readonly Regex TabNameCheck2 = new Regex("^AUX$|^CON$|^NUL$|^SITEMAP$|^LINKCLICK$|^KEEPALIVE$|^DEFAULT$|^ERRORPAGE$|^LOGIN$|^REGISTER$", RegexOptions.IgnoreCase);

        private readonly DataProvider _dataProvider = DataProvider.Instance();

        /// <summary>
        /// Gets the current page in current http request.
        /// </summary>
        /// <value>Current Page Info.</value>
        public static TabInfo CurrentPage
        {
            get
            {
                TabInfo tab = null;
                if (PortalController.Instance.GetCurrentPortalSettings() != null)
                {
                    tab = PortalController.Instance.GetCurrentPortalSettings().ActiveTab;
                }

                return tab;
            }
        }

        /// <summary>
        /// Copies the design to children.
        /// </summary>
        /// <param name="parentTab">The parent tab.</param>
        /// <param name="skinSrc">The skin SRC.</param>
        /// <param name="containerSrc">The container SRC.</param>
        public static void CopyDesignToChildren(TabInfo parentTab, string skinSrc, string containerSrc)
        {
            CopyDesignToChildren(parentTab, skinSrc, containerSrc,
                                 PortalController.GetActivePortalLanguage(parentTab.PortalID));
        }

        /// <summary>
        /// Copies the design to children.
        /// </summary>
        /// <param name="parentTab">The parent tab.</param>
        /// <param name="skinSrc">The skin SRC.</param>
        /// <param name="containerSrc">The container SRC.</param>
        /// <param name="cultureCode">The culture code.</param>
        public static void CopyDesignToChildren(TabInfo parentTab, string skinSrc, string containerSrc, string cultureCode)
        {
            bool clearCache = Null.NullBoolean;
            List<TabInfo> childTabs = Instance.GetTabsByPortal(parentTab.PortalID).DescendentsOf(parentTab.TabID);
            foreach (TabInfo tab in childTabs)
            {
                if (TabPermissionController.CanAdminPage(tab))
                {
                    // Update ContentItem If neccessary
                    if (tab.ContentItemId == Null.NullInteger && tab.TabID != Null.NullInteger)
                    {
                        Instance.CreateContentItem(tab);
                    }

                    DataProvider.Instance().UpdateTab(
                        tab.TabID,
                        tab.ContentItemId,
                        tab.PortalID,
                        tab.VersionGuid,
                        tab.DefaultLanguageGuid,
                        tab.LocalizedVersionGuid,
                        tab.TabName,
                        tab.IsVisible,
                        tab.DisableLink,
                        tab.ParentId,
                        tab.IconFileRaw,
                        tab.IconFileLargeRaw,
                        tab.Title,
                        tab.Description,
                        tab.KeyWords,
                        tab.IsDeleted,
                        tab.Url,
                        skinSrc,
                        containerSrc,
                        tab.StartDate,
                        tab.EndDate,
                        tab.RefreshInterval,
                        tab.PageHeadText,
                        tab.IsSecure,
                        tab.PermanentRedirect,
                        tab.SiteMapPriority,
                        UserController.Instance.GetCurrentUserInfo().UserID,
                        tab.CultureCode,
                        tab.IsSystem);

                    UpdateTabVersion(tab.TabID);

                    EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(),
                                              UserController.Instance.GetCurrentUserInfo().UserID, string.Empty,
                                              EventLogController.EventLogType.TAB_UPDATED);
                    clearCache = true;
                }
            }

            if (clearCache)
            {
                DataCache.ClearTabsCache(childTabs[0].PortalID);
            }
        }

        /// <summary>
        /// Copies the permissions to children.
        /// </summary>
        /// <param name="parentTab">The parent tab.</param>
        /// <param name="newPermissions">The new permissions.</param>
        public static void CopyPermissionsToChildren(TabInfo parentTab, TabPermissionCollection newPermissions)
        {
            bool clearCache = Null.NullBoolean;
            List<TabInfo> childTabs = Instance.GetTabsByPortal(parentTab.PortalID).DescendentsOf(parentTab.TabID);
            foreach (TabInfo tab in childTabs)
            {
                if (TabPermissionController.CanAdminPage(tab))
                {
                    tab.TabPermissions.Clear();
                    tab.TabPermissions.AddRange(newPermissions);
                    TabPermissionController.SaveTabPermissions(tab);
                    UpdateTabVersion(tab.TabID);
                    clearCache = true;
                }
            }

            if (clearCache)
            {
                DataCache.ClearTabsCache(childTabs[0].PortalID);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Processes all panes and modules in the template file.
        /// </summary>
        /// <param name="nodePanes">Template file node for the panes is current tab.</param>
        /// <param name="portalId">PortalId of the new portal.</param>
        /// <param name="tabId">Tab being processed.</param>
        /// <param name="mergeTabs">Tabs need to merge.</param>
        /// <param name="hModules">Modules Hashtable.</param>
        /// <remarks>
        /// </remarks>
        public static void DeserializePanes(XmlNode nodePanes, int portalId, int tabId,
                                            PortalTemplateModuleAction mergeTabs, Hashtable hModules)
        {
            Dictionary<int, ModuleInfo> dicModules = ModuleController.Instance.GetTabModules(tabId);

            // If Mode is Replace remove all the modules already on this Tab
            if (mergeTabs == PortalTemplateModuleAction.Replace)
            {
                foreach (KeyValuePair<int, ModuleInfo> kvp in dicModules)
                {
                    var module = kvp.Value;

                    // when the modules show on all pages are included by the same import process, it need removed.
                    if (!module.AllTabs || hModules.ContainsValue(module.ModuleID))
                    {
                        ModuleController.Instance.DeleteTabModule(tabId, kvp.Value.ModuleID, false);
                    }
                }
            }

            // iterate through the panes
            foreach (XmlNode nodePane in nodePanes.ChildNodes)
            {
                // iterate through the modules
                if (nodePane.SelectSingleNode("modules") != null)
                {
                    XmlNode selectSingleNode = nodePane.SelectSingleNode("modules");
                    if (selectSingleNode != null)
                    {
                        foreach (XmlNode nodeModule in selectSingleNode)
                        {
                            ModuleController.DeserializeModule(nodeModule, nodePane, portalId, tabId, mergeTabs,
                                                               hModules);
                        }
                    }
                }
            }

            // if deserialize tab from install wizard, we need parse desiralize handlers first.
            var installFromWizard = HttpContext.Current != null && HttpContext.Current.Items.Contains("InstallFromWizard");
            if (installFromWizard)
            {
                HttpContext.Current.Items.Remove("InstallFromWizard");
                EventManager.Instance.RefreshTabSyncHandlers();
            }

            EventManager.Instance.OnTabDeserialize(new TabSyncEventArgs { Tab = Instance.GetTab(tabId, portalId), TabNode = nodePanes.ParentNode });
        }

        /// <summary>
        /// Deserializes the tab.
        /// </summary>
        /// <param name="tabNode">The node tab.</param>
        /// <param name="tab">The obj tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <returns></returns>
        public static TabInfo DeserializeTab(XmlNode tabNode, TabInfo tab, int portalId,
                                             PortalTemplateModuleAction mergeTabs)
        {
            return DeserializeTab(tabNode, tab, new Hashtable(), portalId, false, mergeTabs, new Hashtable());
        }

        /// <summary>
        /// Deserializes the tab.
        /// </summary>
        /// <param name="tabNode">The node tab.</param>
        /// <param name="tab">The obj tab.</param>
        /// <param name="tabs">The h tabs.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="isAdminTemplate">if set to <c>true</c> [is admin template].</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="modules">The h modules.</param>
        /// <returns></returns>
        public static TabInfo DeserializeTab(XmlNode tabNode, TabInfo tab, Hashtable tabs, int portalId,
                                             bool isAdminTemplate, PortalTemplateModuleAction mergeTabs,
                                             Hashtable modules)
        {
            string tabName = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "name");
            if (!string.IsNullOrEmpty(tabName))
            {
                if (tab == null)
                {
                    tab = new TabInfo { TabID = Null.NullInteger, ParentId = Null.NullInteger, TabName = tabName };
                }

                tab.PortalID = portalId;
                if (string.IsNullOrEmpty(tab.Title))
                {
                    tab.Title = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "title");
                }

                if (string.IsNullOrEmpty(tab.Description))
                {
                    tab.Description = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "description");
                }

                tab.KeyWords = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "keywords");
                tab.IsVisible = XmlUtils.GetNodeValueBoolean(tabNode, "visible", true);
                tab.DisableLink = XmlUtils.GetNodeValueBoolean(tabNode, "disabled");
                tab.IconFile = Globals.ImportFile(portalId, XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "iconfile"));
                tab.IconFileLarge = Globals.ImportFile(
                    portalId,
                    XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "iconfilelarge"));
                tab.Url = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "url");
                tab.StartDate = XmlUtils.GetNodeValueDate(tabNode, "startdate", Null.NullDate);
                tab.EndDate = XmlUtils.GetNodeValueDate(tabNode, "enddate", Null.NullDate);
                tab.RefreshInterval = XmlUtils.GetNodeValueInt(tabNode, "refreshinterval", Null.NullInteger);
                tab.PageHeadText = XmlUtils.GetNodeValue(tabNode, "pageheadtext", Null.NullString);
                tab.IsSecure = XmlUtils.GetNodeValueBoolean(tabNode, "issecure", false);
                tab.SiteMapPriority = XmlUtils.GetNodeValueSingle(tabNode, "sitemappriority", 0.5F);
                tab.CultureCode = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "cultureCode");

                // objTab.UniqueId = New Guid(XmlUtils.GetNodeValue(nodeTab, "guid", Guid.NewGuid.ToString()));
                // objTab.VersionGuid = New Guid(XmlUtils.GetNodeValue(nodeTab, "versionGuid", Guid.NewGuid.ToString()));
                tab.UseBaseFriendlyUrls = XmlUtils.GetNodeValueBoolean(tabNode, "UseBaseFriendlyUrls", false);

                tab.TabPermissions.Clear();
                DeserializeTabPermissions(tabNode.SelectNodes("tabpermissions/permission"), tab, isAdminTemplate);

                DeserializeTabSettings(tabNode.SelectNodes("tabsettings/tabsetting"), tab);

                // set tab skin and container
                if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "skinsrc", string.Empty)))
                {
                    tab.SkinSrc = XmlUtils.GetNodeValue(tabNode, "skinsrc", string.Empty);
                }

                if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "containersrc", string.Empty)))
                {
                    tab.ContainerSrc = XmlUtils.GetNodeValue(tabNode, "containersrc", string.Empty);
                }

                tabName = tab.TabName;
                if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent")))
                {
                    if (tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent")] != null)
                    {
                        // parent node specifies the path (tab1/tab2/tab3), use saved tabid
                        tab.ParentId = Convert.ToInt32(tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent")]);
                        tabName = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent") + "/" + tab.TabName;
                    }
                    else
                    {
                        // Parent node doesn't spcecify the path, search by name.
                        // Possible incoherence if tabname not unique
                        TabInfo objParent = Instance.GetTabByName(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent"), portalId);
                        if (objParent != null)
                        {
                            tab.ParentId = objParent.TabID;
                            tabName = objParent.TabName + "/" + tab.TabName;
                        }
                        else
                        {
                            // parent tab not found!
                            tab.ParentId = Null.NullInteger;
                            tabName = tab.TabName;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab")))
                {
                    if (tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab")] != null)
                    {
                        // parent node specifies the path (tab1/tab2/tab3), use saved tabid
                        int defaultLanguageTabId = Convert.ToInt32(tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab")]);
                        TabInfo defaultLanguageTab = Instance.GetTab(defaultLanguageTabId, portalId, false);
                        if (defaultLanguageTab != null)
                        {
                            tab.DefaultLanguageGuid = defaultLanguageTab.UniqueId;
                        }
                    }
                    else
                    {
                        // Parent node doesn't spcecify the path, search by name.
                        // Possible incoherence if tabname not unique
                        TabInfo defaultLanguageTab = Instance.GetTabByName(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab"), portalId);
                        if (defaultLanguageTab != null)
                        {
                            tab.DefaultLanguageGuid = defaultLanguageTab.UniqueId;
                        }
                    }
                }

                // create/update tab
                if (tab.TabID == Null.NullInteger)
                {
                    tab.TabID = TabController.Instance.AddTab(tab);
                }
                else
                {
                    Instance.UpdateTab(tab);
                }

                // UpdateTabUrls
                foreach (XmlNode oTabUrlNode in tabNode.SelectNodes("tabUrls/tabUrl"))
                {
                    var tabUrl = new TabUrlInfo();
                    DeserializeTabUrls(oTabUrlNode, tabUrl);
                    DataProvider.Instance().SaveTabUrl(tab.TabID, tabUrl.SeqNum, tabUrl.PortalAliasId, (int)tabUrl.PortalAliasUsage, tabUrl.Url, tabUrl.QueryString, tabUrl.CultureCode, tabUrl.HttpStatus, tabUrl.IsSystem, UserController.Instance.GetCurrentUserInfo().UserID);
                }

                // extra check for duplicate tabs in same level
                if (tabs[tabName] == null)
                {
                    tabs.Add(tabName, tab.TabID);
                }
            }

            // Parse Panes
            if (tabNode.SelectSingleNode("panes") != null)
            {
                DeserializePanes(tabNode.SelectSingleNode("panes"), portalId, tab.TabID, mergeTabs, modules);
            }

            // Finally add "tabid" to node
            tabNode.AppendChild(XmlUtils.CreateElement(tabNode.OwnerDocument, "tabid", tab.TabID.ToString()));
            return tab;
        }

        /// <summary>
        /// Gets the portal tabs.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="excludeTabId">The exclude tab id.</param>
        /// <param name="includeNoneSpecified">if set to <c>true</c> [include none specified].</param>
        /// <param name="includeHidden">if set to <c>true</c> [include hidden].</param>
        /// <returns></returns>
        public static List<TabInfo> GetPortalTabs(int portalId, int excludeTabId, bool includeNoneSpecified,
                                                  bool includeHidden)
        {
            return GetPortalTabs(
                GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true),
                excludeTabId,
                includeNoneSpecified,
                "<" + Localization.GetString("None_Specified") + ">",
                includeHidden,
                false,
                false,
                false,
                false,
                true);
        }

        /// <summary>
        /// Gets the portal tabs.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="excludeTabId">The exclude tab id.</param>
        /// <param name="includeNoneSpecified">if set to <c>true</c> [include none specified].</param>
        /// <param name="includeHidden">if set to <c>true</c> [include hidden].</param>
        /// <param name="includeDeleted">if set to <c>true</c> [include deleted].</param>
        /// <param name="includeURL">if set to <c>true</c> [include URL].</param>
        /// <returns></returns>
        public static List<TabInfo> GetPortalTabs(int portalId, int excludeTabId, bool includeNoneSpecified,
                                                  bool includeHidden, bool includeDeleted, bool includeURL)
        {
            return GetPortalTabs(
                GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true),
                excludeTabId,
                includeNoneSpecified,
                "<" + Localization.GetString("None_Specified") + ">",
                includeHidden,
                includeDeleted,
                includeURL,
                false,
                false,
                true);
        }

        /// <summary>
        /// Gets the portal tabs.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="excludeTabId">The exclude tab id.</param>
        /// <param name="includeNoneSpecified">if set to <c>true</c> [include none specified].</param>
        /// <param name="noneSpecifiedText">The none specified text.</param>
        /// <param name="includeHidden">if set to <c>true</c> [include hidden].</param>
        /// <param name="includeDeleted">if set to <c>true</c> [include deleted].</param>
        /// <param name="includeURL">if set to <c>true</c> [include URL].</param>
        /// <param name="checkViewPermisison">if set to <c>true</c> [check view permisison].</param>
        /// <param name="checkEditPermission">if set to <c>true</c> [check edit permission].</param>
        /// <returns></returns>
        public static List<TabInfo> GetPortalTabs(int portalId, int excludeTabId, bool includeNoneSpecified,
                                                  string noneSpecifiedText, bool includeHidden, bool includeDeleted, bool includeURL,
                                                  bool checkViewPermisison, bool checkEditPermission)
        {
            return GetPortalTabs(
                GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true),
                excludeTabId,
                includeNoneSpecified,
                noneSpecifiedText,
                includeHidden,
                includeDeleted,
                includeURL,
                checkViewPermisison,
                checkEditPermission,
                true);
        }

        /// <summary>
        /// Gets the portal tabs.
        /// </summary>
        /// <param name="tabs">The tabs.</param>
        /// <param name="excludeTabId">The exclude tab id.</param>
        /// <param name="includeNoneSpecified">if set to <c>true</c> [include none specified].</param>
        /// <param name="noneSpecifiedText">The none specified text.</param>
        /// <param name="includeHidden">if set to <c>true</c> [include hidden].</param>
        /// <param name="includeDeleted">if set to <c>true</c> [include deleted].</param>
        /// <param name="includeURL">if set to <c>true</c> [include URL].</param>
        /// <param name="checkViewPermisison">if set to <c>true</c> [check view permisison].</param>
        /// <param name="checkEditPermission">if set to <c>true</c> [check edit permission].</param>
        /// <returns></returns>
        public static List<TabInfo> GetPortalTabs(List<TabInfo> tabs, int excludeTabId, bool includeNoneSpecified,
                                                  string noneSpecifiedText, bool includeHidden, bool includeDeleted, bool includeURL,
                                                  bool checkViewPermisison, bool checkEditPermission)
        {
            return GetPortalTabs(
                tabs,
                excludeTabId,
                includeNoneSpecified,
                noneSpecifiedText,
                includeHidden,
                includeDeleted,
                includeURL,
                checkViewPermisison,
                checkEditPermission,
                true);
        }

        /// <summary>
        /// Gets the portal tabs.
        /// </summary>
        /// <param name="tabs">The tabs.</param>
        /// <param name="excludeTabId">The exclude tab id.</param>
        /// <param name="includeNoneSpecified">if set to <c>true</c> [include none specified].</param>
        /// <param name="noneSpecifiedText">The none specified text.</param>
        /// <param name="includeHidden">if set to <c>true</c> [include hidden].</param>
        /// <param name="includeDeleted">if set to <c>true</c> [include deleted].</param>
        /// <param name="includeURL">if set to <c>true</c> [include URL].</param>
        /// <param name="checkViewPermisison">if set to <c>true</c> [check view permisison].</param>
        /// <param name="checkEditPermission">if set to <c>true</c> [check edit permission].</param>
        /// <param name="includeDeletedChildren">The value of this parameter affects <see cref="TabInfo.HasChildren"></see> property.</param>
        /// <returns></returns>
        public static List<TabInfo> GetPortalTabs(
            List<TabInfo> tabs,
            int excludeTabId,
            bool includeNoneSpecified,
            string noneSpecifiedText,
            bool includeHidden,
            bool includeDeleted,
            bool includeURL,
            bool checkViewPermisison,
            bool checkEditPermission,
            bool includeDeletedChildren)
        {
            var listTabs = new List<TabInfo>();
            if (includeNoneSpecified)
            {
                var tab = new TabInfo { TabID = -1, TabName = noneSpecifiedText, TabOrder = 0, ParentId = -2 };
                listTabs.Add(tab);
            }

            foreach (TabInfo tab in tabs)
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
                if (((excludeTabId < 0) || (tab.TabID != excludeTabId)) &&
                    (!tab.IsSuperTab || objUserInfo.IsSuperUser))
                {
                    if ((tab.IsVisible || includeHidden) && tab.HasAVisibleVersion && (tab.IsDeleted == false || includeDeleted) &&
                        (tab.TabType == TabType.Normal || includeURL))
                    {
                        // Check if User has View/Edit Permission for this tab
                        if (checkEditPermission || checkViewPermisison)
                        {
                            const string permissionList = "ADD,COPY,EDIT,MANAGE";
                            if (checkEditPermission &&
                                TabPermissionController.HasTabPermission(tab.TabPermissions, permissionList))
                            {
                                listTabs.Add(tab);
                            }
                            else if (checkViewPermisison && TabPermissionController.CanViewPage(tab))
                            {
                                listTabs.Add(tab);
                            }
                        }
                        else
                        {
                            // Add Tab to List
                            listTabs.Add(tab);
                        }
                    }

                    // HasChildren should be true in case there is at least one not deleted child
                    tab.HasChildren = tab.HasChildren && (includeDeletedChildren || GetTabsByParent(tab.TabID, tab.PortalID).Any(a => !a.IsDeleted));
                }
            }

            return listTabs;
        }

        /// <summary>
        /// Gets the tab by tab path.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabPath">The tab path.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns></returns>
        public static int GetTabByTabPath(int portalId, string tabPath, string cultureCode)
        {
            Dictionary<string, int> tabpathDic = GetTabPathDictionary(portalId, cultureCode);
            if (tabpathDic.ContainsKey(tabPath))
            {
                return tabpathDic[tabPath];
            }

            return -1;
        }

        /// <summary>
        /// Gets the tab path dictionary.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns></returns>
        public static Dictionary<string, int> GetTabPathDictionary(int portalId, string cultureCode)
        {
            string cacheKey = string.Format(DataCache.TabPathCacheKey, cultureCode, portalId);
            return
                CBO.GetCachedObject<Dictionary<string, int>>(
                    new CacheItemArgs(cacheKey, DataCache.TabPathCacheTimeOut, DataCache.TabPathCachePriority,
                                      cultureCode, portalId),
                    GetTabPathDictionaryCallback);
        }

        /// <summary>
        /// Gets the tabs by parent.
        /// </summary>
        /// <param name="parentId">The parent id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns></returns>
        public static List<TabInfo> GetTabsByParent(int parentId, int portalId)
        {
            return Instance.GetTabsByPortal(portalId).WithParentId(parentId);
        }

        /// <summary>
        /// Gets the tabs by sort order.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <param name="includeNeutral">if set to <c>true</c> [include neutral].</param>
        /// <returns></returns>
        public static List<TabInfo> GetTabsBySortOrder(int portalId, string cultureCode, bool includeNeutral)
        {
            return Instance.GetTabsByPortal(portalId).WithCulture(cultureCode, includeNeutral).AsList();
        }

        /// <summary>
        /// Get all TabInfo for the current culture in SortOrder.
        /// </summary>
        /// <param name="portalId">The portalid to load tabs for.</param>
        /// <returns>
        /// List of TabInfo oredered by default SortOrder.
        /// </returns>
        /// <remarks>
        /// This method uses the Active culture.  There is an overload <seealso cref="TabController.GetTabsBySortOrder(int, string, bool)"/>
        /// which allows the culture information to be specified.
        /// </remarks>
        public static List<TabInfo> GetTabsBySortOrder(int portalId)
        {
            return GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true);
        }

        /// <summary>
        /// Determines whether is special tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns></returns>
        public static bool IsSpecialTab(int tabId, int portalId)
        {
            Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(portalId);
            bool isSpecial = false;
            foreach (Locale locale in locales.Values)
            {
                PortalInfo portal = PortalController.Instance.GetPortal(portalId, locale.Code);
                var portalSettings = new PortalSettings(portal);
                isSpecial = IsSpecialTab(tabId, portalSettings);

                if (isSpecial)
                {
                    break;
                }
            }

            return isSpecial;
        }

        /// <summary>
        /// Determines whether is special tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>
        ///   <c>true</c> if is special tab; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSpecialTab(int tabId, PortalSettings portalSettings)
        {
            return tabId == portalSettings.SplashTabId || tabId == portalSettings.HomeTabId ||
                   tabId == portalSettings.LoginTabId || tabId == portalSettings.UserTabId ||
                   tabId == portalSettings.AdminTabId || tabId == portalSettings.SuperTabId;
        }

        /// <summary>
        /// SerializeTab.
        /// </summary>
        /// <param name="tabXml">The Xml Document to use for the Tab.</param>
        /// <param name="objTab">The TabInfo object to serialize.</param>
        /// <param name="includeContent">A flag used to determine if the Module content is included.</param>
        /// <returns></returns>
        public static XmlNode SerializeTab(XmlDocument tabXml, TabInfo objTab, bool includeContent)
        {
            return SerializeTab(tabXml, null, objTab, null, includeContent);
        }

        /// <summary>
        /// SerializeTab.
        /// </summary>
        /// <param name="tabXml">The Xml Document to use for the Tab.</param>
        /// <param name="tabs">A Hashtable used to store the names of the tabs.</param>
        /// <param name="tab">The TabInfo object to serialize.</param>
        /// <param name="portal">The Portal object to which the tab belongs.</param>
        /// <param name="includeContent">A flag used to determine if the Module content is included.</param>
        /// <returns></returns>
        public static XmlNode SerializeTab(XmlDocument tabXml, Hashtable tabs, TabInfo tab, PortalInfo portal,
                                           bool includeContent)
        {
            XmlNode newnode;
            CBO.SerializeObject(tab, tabXml);

            XmlNode tabNode = tabXml.SelectSingleNode("tab");
            if (tabNode != null)
            {
                if (tabNode.Attributes != null)
                {
                    tabNode.Attributes.Remove(tabNode.Attributes["xmlns:xsd"]);
                    tabNode.Attributes.Remove(tabNode.Attributes["xmlns:xsi"]);
                }

                // remove unwanted elements
                // ReSharper disable AssignNullToNotNullAttribute
                tabNode.RemoveChild(tabNode.SelectSingleNode("tabid"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("moduleID"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("taborder"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("portalid"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("parentid"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("isdeleted"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("tabpath"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("haschildren"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("skindoctype"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("uniqueid"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("versionguid"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("defaultLanguageGuid"));
                tabNode.RemoveChild(tabNode.SelectSingleNode("localizedVersionGuid"));
                XmlNodeList xmlNodeList = tabNode.SelectNodes("tabpermissions/permission");
                if (xmlNodeList != null && xmlNodeList.Count == 0)
                {
                    // for some reason serialization of permissions did not work
                    // we are using a different method here to make sure that
                    // permissions are included in the tabinfo xml
                    XmlDocument tabPermissions = new XmlDocument { XmlResolver = null };
                    CBO.SerializeObject(tab.TabPermissions, tabPermissions);

                    XmlNode permissionsNode = tabXml.CreateElement("tabpermissions");
                    var tabPermissionsNodeList = tabPermissions.SelectNodes("tabpermissions/TabPermissionInfo");
                    if (tabPermissionsNodeList != null)
                    {
                        foreach (XmlNode nodePermission in tabPermissionsNodeList)
                        {
                            var newNode = tabXml.CreateElement("permission");
                            newNode.InnerXml = nodePermission.InnerXml;
                            permissionsNode.AppendChild(newNode);
                        }
                    }

                    tabNode.AppendChild(permissionsNode);

                    // re-select the permissions node
                    xmlNodeList = tabNode.SelectNodes("tabpermissions/permission");
                }

                if (xmlNodeList != null)
                {
                    foreach (XmlNode nodePermission in xmlNodeList)
                    {
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("tabpermissionid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("permissionid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("tabid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("roleid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("userid"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("username"));
                        nodePermission.RemoveChild(nodePermission.SelectSingleNode("displayname"));
                    }
                }

                // ReSharper restore AssignNullToNotNullAttribute
            }

            // Manage Url
            XmlNode urlNode = tabXml.SelectSingleNode("tab/url");
            switch (tab.TabType)
            {
                case TabType.Normal:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "Normal"));
                    break;
                case TabType.Tab:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "Tab"));

                    // Get the tab being linked to
                    TabInfo tempTab = TabController.Instance.GetTab(int.Parse(tab.Url), tab.PortalID, false);
                    if (tempTab != null)
                    {
                        urlNode.InnerXml = tempTab.TabPath;
                    }

                    break;
                case TabType.File:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "File"));
                    IFileInfo file = FileManager.Instance.GetFile(int.Parse(tab.Url.Substring(7)));
                    urlNode.InnerXml = file.RelativePath;
                    break;
                case TabType.Url:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "Url"));
                    break;
            }

            // serialize TabSettings
            XmlUtils.SerializeHashtable(tab.TabSettings, tabXml, tabNode, "tabsetting", "settingname", "settingvalue");
            if (portal != null)
            {
                if (tab.TabID == portal.SplashTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "splashtab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.HomeTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "hometab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.UserTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "usertab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.LoginTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "logintab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.SearchTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "searchtab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.Custom404TabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "404tab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.Custom500TabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "500tab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.TermsTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "termstab";
                    tabNode.AppendChild(newnode);
                }
                else if (tab.TabID == portal.PrivacyTabId)
                {
                    newnode = tabXml.CreateElement("tabtype");
                    newnode.InnerXml = "privacytab";
                    tabNode.AppendChild(newnode);
                }
            }

            if (tabs != null)
            {
                // Manage Parent Tab
                if (!Null.IsNull(tab.ParentId))
                {
                    newnode = tabXml.CreateElement("parent");
                    newnode.InnerXml = HttpContext.Current.Server.HtmlEncode(tabs[tab.ParentId].ToString());
                    tabNode.AppendChild(newnode);

                    // save tab as: ParentTabName/CurrentTabName
                    tabs.Add(tab.TabID, tabs[tab.ParentId] + "/" + tab.TabName);
                }
                else
                {
                    // save tab as: CurrentTabName
                    tabs.Add(tab.TabID, tab.TabName);
                }
            }

            // Manage Content Localization
            if (tab.DefaultLanguageTab != null)
            {
                try
                {
                    newnode = tabXml.CreateElement("defaultLanguageTab");
                    newnode.InnerXml = HttpContext.Current.Server.HtmlEncode(tabs[tab.DefaultLanguageTab.TabID].ToString());
                    tabNode.AppendChild(newnode);
                }
                catch
                {
                    // ignore
                }
            }

            XmlNode panesNode;
            XmlNode paneNode;
            XmlNode nameNode;
            XmlNode modulesNode;
            XmlNode moduleNode;
            XmlDocument moduleXml;
            ModuleInfo module;

            // Serialize modules
            panesNode = tabNode.AppendChild(tabXml.CreateElement("panes"));
            foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(tab.TabID))
            {
                module = kvp.Value;
                if (!module.IsDeleted)
                {
                    moduleXml = new XmlDocument { XmlResolver = null };
                    moduleNode = ModuleController.SerializeModule(moduleXml, module, includeContent);
                    if (panesNode.SelectSingleNode("descendant::pane[name='" + module.PaneName + "']") == null)
                    {
                        // new pane found
                        paneNode = moduleXml.CreateElement("pane");
                        nameNode = paneNode.AppendChild(moduleXml.CreateElement("name"));
                        nameNode.InnerText = module.PaneName;
                        paneNode.AppendChild(moduleXml.CreateElement("modules"));
                        panesNode.AppendChild(tabXml.ImportNode(paneNode, true));
                    }

                    modulesNode = panesNode.SelectSingleNode("descendant::pane[name='" + module.PaneName + "']/modules");
                    modulesNode.AppendChild(tabXml.ImportNode(moduleNode, true));
                }
            }

            // Serialize TabUrls
            var tabUrlsNode = tabNode.AppendChild(tabXml.CreateElement("tabUrls"));
            foreach (var tabUrl in TabController.Instance.GetTabUrls(tab.TabID, tab.PortalID))
            {
                var tabUrlXml = new XmlDocument { XmlResolver = null };
                XmlNode tabUrlNode = tabUrlXml.CreateElement("tabUrl");
                tabUrlNode.AddAttribute("SeqNum", tabUrl.SeqNum.ToString(CultureInfo.InvariantCulture));
                tabUrlNode.AddAttribute("Url", tabUrl.Url);
                tabUrlNode.AddAttribute("QueryString", tabUrl.QueryString);
                tabUrlNode.AddAttribute("HttpStatus", tabUrl.HttpStatus);
                tabUrlNode.AddAttribute("CultureCode", tabUrl.CultureCode);
                tabUrlNode.AddAttribute("IsSystem", tabUrl.IsSystem.ToString());
                tabUrlsNode.AppendChild(tabXml.ImportNode(tabUrlNode, true));
            }

            EventManager.Instance.OnTabSerialize(new TabSyncEventArgs { Tab = tab, TabNode = tabNode });

            return tabNode;
        }

        /// <summary>
        /// check whether have conflict between tab path and portal alias.
        /// </summary>
        /// <param name="portalId">portal id.</param>
        /// <param name="tabPath">tab path.</param>
        /// <returns></returns>
        public static bool IsDuplicateWithPortalAlias(int portalId, string tabPath)
        {
            var aliasLookup = PortalAliasController.Instance.GetPortalAliases();

            foreach (PortalAliasInfo alias in PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId))
            {
                string checkAlias = string.Format("{0}{1}", alias.HTTPAlias, tabPath.Replace("//", "/"));

                foreach (PortalAliasInfo a in aliasLookup.Values)
                {
                    if (a.HTTPAlias.Equals(checkAlias, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsValidTabName(string tabName, out string invalidType)
        {
            invalidType = string.Empty;

            if (string.IsNullOrEmpty(tabName.Trim()))
            {
                invalidType = "EmptyTabName";
                return false;
            }

            var cleanTabName = HtmlUtils.StripNonWord(tabName, false);
            if (TabNameCheck1.IsMatch(tabName) || TabNameCheck2.IsMatch(cleanTabName))
            {
                invalidType = "InvalidTabName";
                return false;
            }

            if (Config.GetFriendlyUrlProvider() == "advanced" && PortalSettings.Current != null)
            {
                var doNotRewriteRegex = new FriendlyUrlSettings(PortalSettings.Current.PortalId).DoNotRewriteRegex;
                if (!string.IsNullOrEmpty(doNotRewriteRegex) &&
                        (Regex.IsMatch(cleanTabName, doNotRewriteRegex, RegexOptions.IgnoreCase)
                            || Regex.IsMatch("/" + cleanTabName, doNotRewriteRegex, RegexOptions.IgnoreCase)
                            || Regex.IsMatch("/" + cleanTabName + "/", doNotRewriteRegex, RegexOptions.IgnoreCase)))
                {
                    invalidType = "InvalidTabName";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds localized copies of the page in all missing languages.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        public void AddMissingLanguages(int portalId, int tabId)
        {
            var currentTab = this.GetTab(tabId, portalId, false);
            if (currentTab.CultureCode != null)
            {
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(portalId);
                var workingTab = currentTab;
                if (workingTab.CultureCode != defaultLocale.Code && workingTab.DefaultLanguageTab == null)
                {
                    // we are adding missing languages to a single culture page that is not in the default language
                    // so we must first add a page in the default culture
                    this.CreateLocalizedCopyInternal(workingTab, defaultLocale, false, true, insertAfterOriginal: true);
                }

                if (currentTab.DefaultLanguageTab != null)
                {
                    workingTab = currentTab.DefaultLanguageTab;
                }

                foreach (Locale locale in LocaleController.Instance.GetLocales(portalId).Values)
                {
                    if (!LocaleController.Instance.IsDefaultLanguage(locale.Code))
                    {
                        bool missing = true;
                        foreach (var localizedTab in workingTab.LocalizedTabs.Values.Where(localizedTab => localizedTab.CultureCode == locale.Code))
                        {
                            missing = false;
                        }

                        if (missing)
                        {
                            this.CreateLocalizedCopyInternal(workingTab, locale, false, true, insertAfterOriginal: true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a tab.
        /// </summary>
        /// <param name="tab">The tab to be added.</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        /// <returns></returns>
        public int AddTab(TabInfo tab)
        {
            return this.AddTab(tab, true);
        }

        /// <summary>
        /// Adds a tab.
        /// </summary>
        /// <param name="tab">The tab to be added.</param>
        /// <param name="includeAllTabsModules">Flag that indicates whether to add the "AllTabs"
        /// Modules.</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        /// <returns></returns>
        public int AddTab(TabInfo tab, bool includeAllTabsModules)
        {
            // Add tab to store
            int tabID = this.AddTabInternal(tab, -1, -1, includeAllTabsModules);

            // Clear the Cache
            this.ClearCache(tab.PortalID);

            return tabID;
        }

        /// <summary>
        /// Adds a tab after the specified tab.
        /// </summary>
        /// <param name="tab">The tab to be added.</param>
        /// <param name="afterTabId">Id of the tab after which this tab is added.</param>
        /// <returns></returns>
        public int AddTabAfter(TabInfo tab, int afterTabId)
        {
            // Add tab to store
            int tabID = this.AddTabInternal(tab, afterTabId, -1, true);

            // Clear the Cache
            this.ClearCache(tab.PortalID);

            return tabID;
        }

        /// <summary>
        /// Adds a tab before the specified tab.
        /// </summary>
        /// <param name="objTab">The tab to be added.</param>
        /// <param name="beforeTabId">Id of the tab before which this tab is added.</param>
        /// <returns></returns>
        public int AddTabBefore(TabInfo objTab, int beforeTabId)
        {
            // Add tab to store
            int tabID = this.AddTabInternal(objTab, -1, beforeTabId, true);

            // Clear the Cache
            this.ClearCache(objTab.PortalID);

            return tabID;
        }

        /// <summary>
        /// Clears tabs and portal cache for the specific portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public void ClearCache(int portalId)
        {
            DataCache.ClearTabsCache(portalId);

            // Clear the Portal cache so the Pages count is correct
            DataCache.ClearPortalCache(portalId, false);

            DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);

            CacheController.FlushPageIndexFromCache();
        }

        public void RefreshCache(int portalId, int tabId)
        {
            var portalTabs = this.GetTabsByPortal(portalId);
            if (portalTabs.WithTabId(tabId) != null)
            {
                var updateTab = this.GetTab(tabId, portalId, true);
                portalTabs.RefreshCache(tabId, updateTab);
            }
        }

        /// <summary>
        /// Converts one single tab to a neutral culture
        /// clears the tab cache optionally.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void ConvertTabToNeutralLanguage(int portalId, int tabId, string cultureCode, bool clearCache)
        {
            // parent tabs can not be deleted
            if (this.GetTabsByPortal(portalId).WithParentId(tabId).Count == 0)
            {
                // delete all translated / localized tabs for this tab
                var tab = this.GetTab(tabId, portalId, true);
                foreach (var localizedTab in tab.LocalizedTabs.Values)
                {
                    this.HardDeleteTabInternal(localizedTab.TabID, portalId);
                }

                // reset culture of current tab back to neutral
                this._dataProvider.ConvertTabToNeutralLanguage(portalId, tabId, cultureCode);
                if (clearCache)
                {
                    this.ClearCache(portalId);
                }
            }
        }

        /// <summary>
        /// Creates content item for the tab..
        /// </summary>
        /// <param name="tab">The updated tab.</param>
        public void CreateContentItem(TabInfo tab)
        {
            // First create ContentItem as we need the ContentItemID
            ContentType contentType = ContentType.Tab;

            IContentController contentController = Util.GetContentController();
            tab.Content = string.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
            if (contentType != null)
            {
                tab.ContentTypeId = contentType.ContentTypeId;
            }

            tab.Indexed = false;
            contentController.AddContentItem(tab);
        }

        /// <summary>
        /// Creates the localized copies.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        public void CreateLocalizedCopies(TabInfo originalTab)
        {
            Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(originalTab.PortalID);
            foreach (Locale subLocale in LocaleController.Instance.GetLocales(originalTab.PortalID).Values)
            {
                if (subLocale.Code != defaultLocale.Code)
                {
                    this.CreateLocalizedCopyInternal(originalTab, subLocale, false, true);
                }
            }
        }

        /// <summary>
        /// Creates the localized copy.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="clearCache">Clear the cache?.</param>
        public void CreateLocalizedCopy(TabInfo originalTab, Locale locale, bool clearCache)
        {
            this.CreateLocalizedCopyInternal(originalTab, locale, true, clearCache);
        }

        /// <summary>
        /// Deletes a tab permanently from the database.
        /// </summary>
        /// <param name="tabId">TabId of the tab to be deleted.</param>
        /// <param name="portalId">PortalId of the portal.</param>
        /// <remarks>
        /// The tab will not delete if it has child tab(s).
        /// </remarks>
        public void DeleteTab(int tabId, int portalId)
        {
            // parent tabs can not be deleted
            if (this.GetTabsByPortal(portalId).WithParentId(tabId).Count == 0)
            {
                this.HardDeleteTabInternal(tabId, portalId);
            }

            this.ClearCache(portalId);
        }

        /// <summary>
        /// Deletes a tab permanently from the database.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="deleteDescendants">if set to <c>true</c> will delete all child tabs.</param>
        public void DeleteTab(int tabId, int portalId, bool deleteDescendants)
        {
            List<TabInfo> descendantList = this.GetTabsByPortal(portalId).DescendentsOf(tabId);
            if (deleteDescendants && descendantList.Count > 0)
            {
                // Iterate through descendants from bottom - which will remove children first
                for (int i = descendantList.Count - 1; i >= 0; i += -1)
                {
                    this.HardDeleteTabInternal(descendantList[i].TabID, portalId);
                }
            }

            this.DeleteTab(tabId, portalId);
        }

        /// <summary>
        /// Delete a Setting of a tab instance.
        /// </summary>
        /// <param name="tabId">ID of the affected tab.</param>
        /// <param name="settingName">Name of the setting to be deleted.</param>
        public void DeleteTabSetting(int tabId, string settingName)
        {
            this._dataProvider.DeleteTabSetting(tabId, settingName);
            var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.TAB_SETTING_DELETED.ToString() };
            log.LogProperties.Add(new LogDetailInfo("TabID", tabId.ToString()));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            LogController.Instance.AddLog(log);

            UpdateTabVersion(tabId);
            this.ClearTabSettingsCache(tabId);
        }

        /// <summary>
        /// Delete all Settings of a tab instance.
        /// </summary>
        /// <param name="tabId">ID of the affected tab.</param>
        public void DeleteTabSettings(int tabId)
        {
            this._dataProvider.DeleteTabSettings(tabId);
            var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.TAB_SETTING_DELETED.ToString() };
            log.LogProperties.Add(new LogDetailInfo("TabId", tabId.ToString()));
            LogController.Instance.AddLog(log);
            UpdateTabVersion(tabId);
            this.ClearTabSettingsCache(tabId);
        }

        /// <summary>
        /// Delete a taburl.
        /// </summary>
        /// <param name="tabUrl">the taburl.</param>
        /// <param name="portalId">the portal.</param>
        /// <param name="clearCache">whether to clear the cache.</param>
        public void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            DataProvider.Instance().DeleteTabUrl(tabUrl.TabId, tabUrl.SeqNum);

            EventLogController.Instance.AddLog(
                "tabUrl.TabId",
                tabUrl.TabId.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.TABURL_DELETED);
            if (clearCache)
            {
                DataCache.RemoveCache(string.Format(DataCache.TabUrlCacheKey, portalId));
                CacheController.ClearCustomAliasesCache();
                var tab = this.GetTab(tabUrl.TabId, portalId);
                tab.ClearTabUrls();
            }
        }

        /// <summary>
        /// Deletes all tabs for a specific language. Double checks if we are not deleting pages for the default language
        /// Clears the tab cache optionally.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        /// <returns></returns>
        public bool DeleteTranslatedTabs(int portalId, string cultureCode, bool clearCache)
        {
            if (PortalController.Instance.GetCurrentPortalSettings() != null)
            {
                var defaultLanguage = PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage;
                if (cultureCode != defaultLanguage)
                {
                    this._dataProvider.DeleteTranslatedTabs(portalId, cultureCode);

                    if (clearCache)
                    {
                        this.ClearCache(portalId);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Reverts page culture back to Neutral (Null), to ensure a non localized site
        /// clears the tab cache optionally.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void EnsureNeutralLanguage(int portalId, string cultureCode, bool clearCache)
        {
            this._dataProvider.EnsureNeutralLanguage(portalId, cultureCode);
            if (clearCache)
            {
                this.ClearCache(portalId);
            }
        }

        /// <summary>
        /// Get the list of skins per alias at tab level.
        /// </summary>
        /// <param name="tabId">the tab id.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>list of TabAliasSkinInfo.</returns>
        public List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId)
        {
            // Get the Portal AliasSkin Dictionary
            Dictionary<int, List<TabAliasSkinInfo>> dicTabAliases = this.GetAliasSkins(portalId);

            // Get the Collection from the Dictionary
            List<TabAliasSkinInfo> tabAliases;
            bool bFound = dicTabAliases.TryGetValue(tabId, out tabAliases);
            if (!bFound)
            {
                // Return empty collection
                tabAliases = new List<TabAliasSkinInfo>();
            }

            return tabAliases;
        }

        /// <summary>
        /// Get the list of custom aliases associated with a page (tab).
        /// </summary>
        /// <param name="tabId">the tab id.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>dictionary of tabid and aliases.</returns>
        public Dictionary<string, string> GetCustomAliases(int tabId, int portalId)
        {
            // Get the Portal CustomAlias Dictionary
            Dictionary<int, Dictionary<string, string>> dicCustomAliases = this.GetCustomAliases(portalId);

            // Get the Collection from the Dictionary
            Dictionary<string, string> customAliases;
            bool bFound = dicCustomAliases.TryGetValue(tabId, out customAliases);
            if (!bFound)
            {
                // Return empty collection
                customAliases = new Dictionary<string, string>();
            }

            return customAliases;
        }

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id or <see cref="P:DotNetNuke.Common.Utilities.Null.NullInteger" />.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTab(int tabId, int portalId)
        {
            return this.GetTab(tabId, portalId, false);
        }

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id or <see cref="P:DotNetNuke.Common.Utilities.Null.NullInteger" />.</param>
        /// <param name="ignoreCache">if set to <c>true</c> will get tab info directly from database.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTab(int tabId, int portalId, bool ignoreCache)
        {
            TabInfo tab = null;

            if (tabId <= 0)
            {
                Logger.WarnFormat("Invalid tabId {0} of portal {1}", tabId, portalId);
            }
            else if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                // if we are using the cache
                tab = CBO.FillObject<TabInfo>(this._dataProvider.GetTab(tabId));
            }
            else
            {
                // if we do not know the PortalId then try to find it in the Portals Dictionary using the TabId
                portalId = GetPortalId(tabId, portalId);

                // if we have the PortalId then try to get the TabInfo object
                tab = this.GetTabsByPortal(portalId).WithTabId(tabId) ??
                      this.GetTabsByPortal(GetPortalId(tabId, Null.NullInteger)).WithTabId(tabId);

                if (tab == null)
                {
                    // recheck the info directly from database to make sure we can avoid error if the cache doesn't update
                    // correctly, this may occurred when install is set up in web farm.
                    tab = CBO.FillObject<TabInfo>(this._dataProvider.GetTab(tabId));

                    // if tab is not null means that the cache doesn't update correctly, we need clear the cache
                    // and let it rebuild cache when request next time.
                    if (tab != null)
                    {
                        this.ClearCache(tab.PortalID);
                    }
                    else
                    {
                        Logger.WarnFormat("Unable to find tabId {0} of portal {1}", tabId, portalId);
                    }
                }
            }

            return tab;
        }

        /// <summary>
        /// Gets the tab by culture.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="locale">The locale.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTabByCulture(int tabId, int portalId, Locale locale)
        {
            TabInfo localizedTab = null;
            TabCollection tabs = this.GetTabsByPortal(portalId);

            // Get Tab specified by Id
            TabInfo originalTab = tabs.WithTabId(tabId);

            if (locale != null && originalTab != null)
            {
                // Check if tab is in the requested culture
                if (string.IsNullOrEmpty(originalTab.CultureCode) || originalTab.CultureCode == locale.Code)
                {
                    localizedTab = originalTab;
                }
                else
                {
                    // See if tab exists for culture
                    if (originalTab.IsDefaultLanguage)
                    {
                        originalTab.LocalizedTabs.TryGetValue(locale.Code, out localizedTab);
                    }
                    else
                    {
                        if (originalTab.DefaultLanguageTab != null)
                        {
                            if (originalTab.DefaultLanguageTab.CultureCode == locale.Code)
                            {
                                localizedTab = originalTab.DefaultLanguageTab;
                            }
                            else
                            {
                                if (
                                    !originalTab.DefaultLanguageTab.LocalizedTabs.TryGetValue(
                                        locale.Code,
                                        out localizedTab))
                                {
                                    localizedTab = originalTab.DefaultLanguageTab;
                                }
                            }
                        }
                    }
                }
            }

            return localizedTab;
        }

        /// <summary>
        /// Gets the name of the tab by name.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTabByName(string tabName, int portalId)
        {
            return this.GetTabsByPortal(portalId).WithTabName(tabName);
        }

        /// <summary>
        /// Gets the name of the tab by name and parent id.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTabByName(string tabName, int portalId, int parentId)
        {
            return this.GetTabsByPortal(portalId).WithTabNameAndParentId(tabName, parentId);
        }

        /// <summary>
        /// Gets the tabs which use the module.
        /// </summary>
        /// <param name="moduleID">The module ID.</param>
        /// <returns>tab collection.</returns>
        public IDictionary<int, TabInfo> GetTabsByModuleID(int moduleID)
        {
            return CBO.FillDictionary<int, TabInfo>("TabID", this._dataProvider.GetTabsByModuleID(moduleID));
        }

        /// <summary>
        /// Gets the tabs which use the module.
        /// </summary>
        /// <param name="tabModuleId">The tabmodule ID.</param>
        /// <returns>tab collection.</returns>
        public IDictionary<int, TabInfo> GetTabsByTabModuleID(int tabModuleId)
        {
            return CBO.FillDictionary<int, TabInfo>("TabID", this._dataProvider.GetTabsByTabModuleID(tabModuleId));
        }

        /// <summary>
        /// Gets the tabs which use the package.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="packageID">The package ID.</param>
        /// <param name="forHost">if set to <c>true</c> [for host].</param>
        /// <returns>tab collection.</returns>
        public IDictionary<int, TabInfo> GetTabsByPackageID(int portalID, int packageID, bool forHost)
        {
            return CBO.FillDictionary<int, TabInfo>("TabID", this._dataProvider.GetTabsByPackageID(portalID, packageID, forHost));
        }

        /// <summary>
        /// Gets the tabs by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab collection.</returns>
        public TabCollection GetTabsByPortal(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabCacheKey, portalId);
            return CBO.GetCachedObject<TabCollection>(
                new CacheItemArgs(
                cacheKey,
                DataCache.TabCacheTimeOut,
                DataCache.TabCachePriority),
                c =>
                                                            {
                                                                List<TabInfo> tabs = CBO.FillCollection<TabInfo>(this._dataProvider.GetTabs(portalId));
                                                                return new TabCollection(tabs);
                                                            });
        }

        /// <summary>
        /// Get the actual visible tabs for a given portal id.
        /// System Tabs and Admin Tabs are excluded from the result set.
        /// </summary>
        /// <param name="portalId"></param>
        ///
        /// <returns></returns>
        public TabCollection GetUserTabsByPortal(int portalId)
        {
            var tabs = this.GetTabsByPortal(portalId);
            var portal = PortalController.Instance.GetPortal(portalId);

            IEnumerable<TabInfo> filteredList = from tab in tabs
                                                where
                                                !tab.Value.IsSystem
                                                && tab.Value.TabID != portal.AdminTabId
                                                && tab.Value.ParentId != portal.AdminTabId
                                                select tab.Value;
            return new TabCollection(filteredList);
        }

        /// <summary>
        /// read all settings for a tab from TabSettings table.
        /// </summary>
        /// <param name="tabId">ID of the Tab to query.</param>
        /// <returns>
        /// (cached) hashtable containing all settings.
        /// </returns>
        public Hashtable GetTabSettings(int tabId)
        {
            var portalId = GetPortalId(tabId, -1);
            Hashtable settings;
            if (!this.GetTabSettingsByPortal(portalId).TryGetValue(tabId, out settings))
            {
                settings = new Hashtable();
            }

            return settings;
        }

        /// <summary>
        /// Get the list of url's associated with a page (tab).
        /// </summary>
        /// <param name="tabId">the tab id.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>list of urls associated with a tab.</returns>
        public List<TabUrlInfo> GetTabUrls(int tabId, int portalId)
        {
            // Get the Portal TabUrl Dictionary
            Dictionary<int, List<TabUrlInfo>> dicTabUrls = this.GetTabUrls(portalId);

            // Get the Collection from the Dictionary
            List<TabUrlInfo> tabRedirects;
            bool bFound = dicTabUrls.TryGetValue(tabId, out tabRedirects);
            if (!bFound)
            {
                // Return empty collection
                tabRedirects = new List<TabUrlInfo>();
            }

            return tabRedirects;
        }

        /// <summary>
        /// Gives the translator role edit rights.
        /// </summary>
        /// <param name="localizedTab">The localized tab.</param>
        /// <param name="users">The users.</param>
        public void GiveTranslatorRoleEditRights(TabInfo localizedTab, Dictionary<int, UserInfo> users)
        {
            var permissionCtrl = new PermissionController();
            ArrayList permissionsList = permissionCtrl.GetPermissionByCodeAndKey("SYSTEM_TAB", "EDIT");

            string translatorRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", localizedTab.CultureCode), localizedTab.PortalID, string.Empty);
            foreach (string translatorRole in translatorRoles.Split(';'))
            {
                if (users != null)
                {
                    foreach (UserInfo translator in RoleController.Instance.GetUsersByRole(localizedTab.PortalID, translatorRole))
                    {
                        users[translator.UserID] = translator;
                    }
                }

                if (permissionsList != null && permissionsList.Count > 0)
                {
                    var translatePermisison = (PermissionInfo)permissionsList[0];
                    string roleName = translatorRole;
                    RoleInfo role = RoleController.Instance.GetRole(
                        localizedTab.PortalID,
                        r => r.RoleName == roleName);
                    if (role != null)
                    {
                        TabPermissionInfo perm =
                            localizedTab.TabPermissions.Where(
                                tp => tp.RoleID == role.RoleID && tp.PermissionKey == "EDIT").SingleOrDefault();
                        if (perm == null)
                        {
                            // Create Permission
                            var tabTranslatePermission = new TabPermissionInfo(translatePermisison)
                            {
                                RoleID = role.RoleID,
                                AllowAccess = true,
                                RoleName = roleName,
                            };
                            localizedTab.TabPermissions.Add(tabTranslatePermission);
                            this.UpdateTab(localizedTab);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns True if a page is missing a translated version in at least one other language.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <returns></returns>
        public bool HasMissingLanguages(int portalId, int tabId)
        {
            var currentTab = this.GetTab(tabId, portalId, false);
            var workingTab = currentTab;
            var locales = LocaleController.Instance.GetLocales(portalId);
            var LocaleCount = locales.Count;
            if (currentTab.DefaultLanguageTab != null)
            {
                workingTab = currentTab.DefaultLanguageTab;
            }

            var localizedCount = 1 +
                                 locales.Values.Where(locale => !LocaleController.Instance.IsDefaultLanguage(locale.Code))
                                        .Count(locale => workingTab.LocalizedTabs.Values.Any(localizedTab => localizedTab.CultureCode == locale.Code));

            return (LocaleCount - localizedCount) != 0;
        }

        /// <summary>
        /// Checks whether the tab is published. Published means: view permissions of tab are identical to the DefaultLanguageTab.
        /// </summary>
        /// <param name="publishTab">The tab that is checked.</param>
        /// <returns>true if tab is published.</returns>
        public bool IsTabPublished(TabInfo publishTab)
        {
            bool returnValue = true;

            // To publish a subsidiary language tab we need to enable the View Permissions
            if (publishTab != null && publishTab.DefaultLanguageTab != null)
            {
                foreach (TabPermissionInfo perm in
                    publishTab.DefaultLanguageTab.TabPermissions.Where(p => p.PermissionKey == "VIEW"))
                {
                    TabPermissionInfo sourcePerm = perm;
                    TabPermissionInfo targetPerm =
                        publishTab.TabPermissions.Where(
                            p =>
                            p.PermissionKey == sourcePerm.PermissionKey && p.RoleID == sourcePerm.RoleID &&
                            p.UserID == sourcePerm.UserID).SingleOrDefault();

                    if (targetPerm == null)
                    {
                        returnValue = false;
                        break;
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Localizes the tab.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        public void LocalizeTab(TabInfo originalTab, Locale locale)
        {
            this.LocalizeTab(originalTab, locale, true);
        }

        /// <summary>
        /// Localizes the tab, with optional clear cache.
        /// </summary>
        /// <param name="originalTab"></param>
        /// <param name="locale"></param>
        /// <param name="clearCache"></param>
        public void LocalizeTab(TabInfo originalTab, Locale locale, bool clearCache)
        {
            this._dataProvider.LocalizeTab(originalTab.TabID, locale.Code, UserController.Instance.GetCurrentUserInfo().UserID);
            if (clearCache)
            {
                DataCache.ClearTabsCache(originalTab.PortalID);
                DataCache.ClearModuleCache(originalTab.TabID);
            }
        }

        /// <summary>
        /// Moves the tab after a specific tab.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="afterTabId">will move objTab after this tab.</param>
        public void MoveTabAfter(TabInfo tab, int afterTabId)
        {
            // Get AfterTab
            var afterTab = this.GetTab(afterTabId, tab.PortalID, false);

            // Create Tab Redirects
            if (afterTab.ParentId != tab.ParentId)
            {
                this.CreateTabRedirects(tab);
            }

            // Move Tab
            this._dataProvider.MoveTabAfter(tab.TabID, afterTabId, UserController.Instance.GetCurrentUserInfo().UserID);

            // Clear the Cache
            this.ClearCache(tab.PortalID);

            var portalId = GetPortalId(tab.TabID, -1);
            var updatedTab = this.GetTab(tab.TabID, portalId, true);
            EventManager.Instance.OnTabUpdated(new TabEventArgs { Tab = updatedTab });
        }

        /// <summary>
        /// Moves the tab before a specific tab.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="beforeTabId">will move objTab before this tab.</param>
        public void MoveTabBefore(TabInfo tab, int beforeTabId)
        {
            // Get AfterTab
            var beforeTab = this.GetTab(beforeTabId, tab.PortalID, false);

            // Create Tab Redirects
            if (beforeTab.ParentId != tab.ParentId)
            {
                this.CreateTabRedirects(tab);
            }

            // Move Tab
            this._dataProvider.MoveTabBefore(tab.TabID, beforeTabId, UserController.Instance.GetCurrentUserInfo().UserID);

            // Clear the Cache
            this.ClearCache(tab.PortalID);

            var portalId = GetPortalId(tab.TabID, -1);
            var updatedTab = this.GetTab(tab.TabID, portalId, true);
            EventManager.Instance.OnTabUpdated(new TabEventArgs { Tab = updatedTab });
        }

        /// <summary>
        /// Moves the tab to a new parent.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="parentId">will move tab to this parent.</param>
        public void MoveTabToParent(TabInfo tab, int parentId)
        {
            // Create Tab Redirects
            if (parentId != tab.ParentId)
            {
                this.CreateTabRedirects(tab);
            }

            // Move Tab
            this._dataProvider.MoveTabToParent(tab.TabID, parentId, UserController.Instance.GetCurrentUserInfo().UserID);

            // Clear the Cache
            this.ClearCache(tab.PortalID);

            var portalId = GetPortalId(tab.TabID, -1);
            var updatedTab = this.GetTab(tab.TabID, portalId, true);
            EventManager.Instance.OnTabUpdated(new TabEventArgs { Tab = updatedTab });
        }

        /// <summary>
        /// Populates the bread crumbs.
        /// </summary>
        /// <param name="tab">The tab.</param>
        public void PopulateBreadCrumbs(ref TabInfo tab)
        {
            if (tab.BreadCrumbs == null)
            {
                var crumbs = new ArrayList();
                this.PopulateBreadCrumbs(tab.PortalID, ref crumbs, tab.TabID);
                tab.BreadCrumbs = crumbs;
            }
        }

        /// <summary>
        /// Populates the bread crumbs.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="breadCrumbs">The bread crumbs.</param>
        /// <param name="tabID">The tab ID.</param>
        public void PopulateBreadCrumbs(int portalID, ref ArrayList breadCrumbs, int tabID)
        {
            // find the tab in the tabs collection
            TabInfo tab;
            TabCollection portalTabs = this.GetTabsByPortal(portalID);
            TabCollection hostTabs = this.GetTabsByPortal(Null.NullInteger);
            bool found = portalTabs.TryGetValue(tabID, out tab);
            if (!found)
            {
                found = hostTabs.TryGetValue(tabID, out tab);
            }

            // if tab was found
            if (found)
            {
                breadCrumbs.Insert(0, tab.Clone());

                // get the tab parent
                if (!Null.IsNull(tab.ParentId))
                {
                    this.PopulateBreadCrumbs(portalID, ref breadCrumbs, tab.ParentId);
                }
            }
        }

        /// <summary>
        /// Publishes the tab. Set the VIEW permission.
        /// </summary>
        /// <param name="publishTab">The publish tab.</param>
        public void PublishTab(TabInfo publishTab)
        {
            // To publish a subsidiary language tab we need to enable the View Permissions
            if (publishTab != null && publishTab.DefaultLanguageTab != null)
            {
                foreach (TabPermissionInfo perm in
                    publishTab.DefaultLanguageTab.TabPermissions.Where(p => p.PermissionKey == "VIEW"))
                {
                    TabPermissionInfo sourcePerm = perm;
                    TabPermissionInfo targetPerm =
                        publishTab.TabPermissions.Where(
                            p =>
                            p.PermissionKey == sourcePerm.PermissionKey && p.RoleID == sourcePerm.RoleID &&
                            p.UserID == sourcePerm.UserID).SingleOrDefault();

                    if (targetPerm == null)
                    {
                        publishTab.TabPermissions.Add(sourcePerm);
                    }

                    TabPermissionController.SaveTabPermissions(publishTab);
                }
            }
        }

        /// <summary>
        /// Publishes the tabs.
        /// </summary>
        /// <param name="tabs">The tabs.</param>
        public void PublishTabs(List<TabInfo> tabs)
        {
            foreach (TabInfo t in tabs)
            {
                if (t.IsTranslated)
                {
                    this.PublishTab(t);
                }
            }
        }

        /// <summary>
        /// Restores the tab.
        /// </summary>
        /// <param name="tab">The obj tab.</param>
        /// <param name="portalSettings">The portal settings.</param>
        public void RestoreTab(TabInfo tab, PortalSettings portalSettings)
        {
            if (tab.DefaultLanguageTab != null)
            {
                // We are trying to restore the child, so recall this function with the master language's tab id
                this.RestoreTab(tab.DefaultLanguageTab, portalSettings);
                return;
            }

            tab.IsDeleted = false;
            this.UpdateTab(tab);

            // Restore any localized children
            foreach (TabInfo localizedtab in tab.LocalizedTabs.Values)
            {
                localizedtab.IsDeleted = false;
                this.UpdateTab(localizedtab);
            }

            EventLogController.Instance.AddLog(tab, portalSettings, portalSettings.UserId, string.Empty, EventLogController.EventLogType.TAB_RESTORED);

            ArrayList allTabsModules = ModuleController.Instance.GetAllTabsModules(tab.PortalID, true);
            var tabModules = ModuleController.Instance.GetTabModules(tab.TabID);
            foreach (ModuleInfo objModule in allTabsModules)
            {
                if (!tabModules.ContainsKey(objModule.ModuleID))
                {
                    ModuleController.Instance.CopyModule(objModule, tab, Null.NullString, true);
                }
            }

            this.ClearCache(tab.PortalID);

            EventManager.Instance.OnTabRestored(new TabEventArgs { Tab = tab });
        }

        /// <summary>
        /// Save url information for a page (tab).
        /// </summary>
        /// <param name="tabUrl">the tab url.</param>
        /// <param name="portalId">the portal id.</param>
        /// <param name="clearCache">whether to clear the cache.</param>
        public void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            var portalAliasId = (tabUrl.PortalAliasUsage == PortalAliasUsageType.Default)
                                  ? Null.NullInteger
                                  : tabUrl.PortalAliasId;

            var saveLog = EventLogController.EventLogType.TABURL_CREATED;

            if (tabUrl.HttpStatus == "200")
            {
                saveLog = EventLogController.EventLogType.TABURL_CREATED;
            }
            else
            {
                // need to see if sequence number exists to decide if insert or update
                List<TabUrlInfo> t = this.GetTabUrls(portalId, tabUrl.TabId);
                var existingSeq = t.FirstOrDefault(r => r.SeqNum == tabUrl.SeqNum);
                if (existingSeq == null)
                {
                    saveLog = EventLogController.EventLogType.TABURL_CREATED;
                }
            }

            DataProvider.Instance().SaveTabUrl(tabUrl.TabId, tabUrl.SeqNum, portalAliasId, (int)tabUrl.PortalAliasUsage, tabUrl.Url, tabUrl.QueryString, tabUrl.CultureCode, tabUrl.HttpStatus, tabUrl.IsSystem, UserController.Instance.GetCurrentUserInfo().UserID);

            EventLogController.Instance.AddLog(
                "tabUrl",
                tabUrl.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                saveLog);

            if (clearCache)
            {
                DataCache.RemoveCache(string.Format(DataCache.TabUrlCacheKey, portalId));
                CacheController.ClearCustomAliasesCache();
                this.ClearCache(portalId);
                var tab = this.GetTab(tabUrl.TabId, portalId);
                tab.ClearTabUrls();
            }
        }

        /// <summary>
        /// Soft Deletes the tab by setting the IsDeleted property to true.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns></returns>
        public bool SoftDeleteTab(int tabId, PortalSettings portalSettings)
        {
            bool deleted;
            TabInfo tab = this.GetTab(tabId, portalSettings.PortalId, false);
            if (tab != null)
            {
                if (tab.DefaultLanguageTab != null && LocaleController.Instance.GetLocales(portalSettings.PortalId).ContainsKey(tab.CultureCode))
                {
                    // We are trying to delete the child, so recall this function with the master language's tab id
                    return this.SoftDeleteTab(tab.DefaultLanguageTab.TabID, portalSettings);
                }

                // Delete the Tab
                deleted = this.SoftDeleteTabInternal(tab, portalSettings);

                // Delete any localized children
                if (deleted)
                {
                    foreach (TabInfo localizedtab in tab.LocalizedTabs.Values)
                    {
                        this.SoftDeleteTabInternal(localizedtab, portalSettings);
                    }
                }
            }
            else
            {
                deleted = false;
            }

            return deleted;
        }

        /// <summary>
        /// Updates the tab to databse.
        /// </summary>
        /// <param name="updatedTab">The updated tab.</param>
        public void UpdateTab(TabInfo updatedTab)
        {
            TabInfo originalTab = this.GetTab(updatedTab.TabID, updatedTab.PortalID, true);

            // Update ContentItem If neccessary
            if (updatedTab.TabID != Null.NullInteger)
            {
                if (updatedTab.ContentItemId == Null.NullInteger)
                {
                    this.CreateContentItem(updatedTab);
                }
                else
                {
                    this.UpdateContentItem(updatedTab);
                }
            }

            // Create Tab Redirects
            if (originalTab.ParentId != updatedTab.ParentId || originalTab.TabName != updatedTab.TabName)
            {
                this.CreateTabRedirects(updatedTab);
            }

            // Update Tab to DataStore
            this._dataProvider.UpdateTab(
                updatedTab.TabID,
                updatedTab.ContentItemId,
                updatedTab.PortalID,
                updatedTab.VersionGuid,
                updatedTab.DefaultLanguageGuid,
                updatedTab.LocalizedVersionGuid,
                updatedTab.TabName,
                updatedTab.IsVisible,
                updatedTab.DisableLink,
                updatedTab.ParentId,
                updatedTab.IconFileRaw,
                updatedTab.IconFileLargeRaw,
                updatedTab.Title,
                updatedTab.Description,
                updatedTab.KeyWords,
                updatedTab.IsDeleted,
                updatedTab.Url,
                updatedTab.SkinSrc,
                updatedTab.ContainerSrc,
                updatedTab.StartDate,
                updatedTab.EndDate,
                updatedTab.RefreshInterval,
                updatedTab.PageHeadText,
                updatedTab.IsSecure,
                updatedTab.PermanentRedirect,
                updatedTab.SiteMapPriority,
                UserController.Instance.GetCurrentUserInfo().UserID,
                updatedTab.CultureCode,
                updatedTab.IsSystem);

            // Update Tags
            List<Term> terms = updatedTab.Terms;
            ITermController termController = Util.GetTermController();
            termController.RemoveTermsFromContent(updatedTab);
            foreach (Term term in terms)
            {
                termController.AddTermToContent(term, updatedTab);
            }

            EventLogController.Instance.AddLog(updatedTab, PortalController.Instance.GetCurrentPortalSettings(),
                                      UserController.Instance.GetCurrentUserInfo().UserID, string.Empty,
                                      EventLogController.EventLogType.TAB_UPDATED);

            // Update Tab permissions
            TabPermissionController.SaveTabPermissions(updatedTab);

            // Update TabSettings - use Try/catch as tabs are added during upgrade ptocess and the sproc may not exist
            try
            {
                this.UpdateTabSettings(ref updatedTab);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            // Update Tab Version
            UpdateTabVersion(updatedTab.TabID);

            // Clear Tab Caches
            this.ClearCache(updatedTab.PortalID);
            if (updatedTab.PortalID != originalTab.PortalID)
            {
                this.ClearCache(originalTab.PortalID);
            }

            EventManager.Instance.OnTabUpdated(new TabEventArgs { Tab = updatedTab });
        }

        /// <summary>
        /// Adds or updates a tab's setting value.
        /// </summary>
        /// <param name="tabId">ID of the tab to update.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will remove the setting, if not preserveIfEmpty is true.</remarks>
        public void UpdateTabSetting(int tabId, string settingName, string settingValue)
        {
            this.UpdateTabSettingInternal(tabId, settingName, settingValue, true);
        }

        /// <summary>
        /// Updates the translation status.
        /// </summary>
        /// <param name="localizedTab">The localized tab.</param>
        /// <param name="isTranslated">if set to <c>true</c> means the tab has already been translated.</param>
        public void UpdateTranslationStatus(TabInfo localizedTab, bool isTranslated)
        {
            if (isTranslated && (localizedTab.DefaultLanguageTab != null))
            {
                localizedTab.LocalizedVersionGuid = localizedTab.DefaultLanguageTab.LocalizedVersionGuid;
            }
            else
            {
                localizedTab.LocalizedVersionGuid = Guid.NewGuid();
            }

            DataProvider.Instance()
                        .UpdateTabTranslationStatus(localizedTab.TabID, localizedTab.LocalizedVersionGuid,
                                                    UserController.Instance.GetCurrentUserInfo().UserID);

            // Clear Tab Caches
            this.ClearCache(localizedTab.PortalID);
        }

        /// <summary>
        /// It marks a page as published at least once.
        /// </summary>
        /// <param name="tab">The Tab to be marked.</param>
        public void MarkAsPublished(TabInfo tab)
        {
            this._dataProvider.MarkAsPublished(tab.TabID);

            // Clear Tab Caches
            this.ClearCache(tab.PortalID);

            EventManager.Instance.OnTabMarkedAsPublished(new TabEventArgs { Tab = tab });
        }

        /// <summary>
        /// Determines whether is host or admin tab.
        /// </summary>
        /// <param name="tab">The tab info.</param>
        /// <returns></returns>
        public bool IsHostOrAdminPage(TabInfo tab)
        {
            return this.IsHostTab(tab) || this.IsAdminTab(tab);
        }

        internal Dictionary<int, List<TabUrlInfo>> GetTabUrls(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabUrlCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, List<TabUrlInfo>>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.TabUrlCacheTimeOut,
                DataCache.TabUrlCachePriority,
                portalId),
                this.GetTabUrlsCallback);
        }

        protected override Func<ITabController> GetFactory()
        {
            return () => new TabController();
        }

        private static void AddAllTabsModules(TabInfo tab)
        {
            var portalSettings = new PortalSettings(tab.TabID, tab.PortalID);
            foreach (ModuleInfo allTabsModule in ModuleController.Instance.GetAllTabsModules(tab.PortalID, true))
            {
                // [DNN-6276]We need to check that the Module is not implicitly deleted.  ie If all instances are on Pages
                // that are all "deleted" then even if the Module itself is not deleted, we would not expect the
                // Module to be added
                var canAdd =
                (from ModuleInfo allTabsInstance in ModuleController.Instance.GetTabModulesByModule(allTabsModule.ModuleID) select Instance.GetTab(allTabsInstance.TabID, tab.PortalID, false)).Any(
                    t => !t.IsDeleted) && (!portalSettings.ContentLocalizationEnabled || allTabsModule.CultureCode == tab.CultureCode);
                if (canAdd)
                {
                    ModuleController.Instance.CopyModule(allTabsModule, tab, Null.NullString, true);
                }
            }
        }

        private static void DeserializeTabSettings(XmlNodeList nodeTabSettings, TabInfo objTab)
        {
            foreach (XmlNode oTabSettingNode in nodeTabSettings)
            {
                string sKey = XmlUtils.GetNodeValue(oTabSettingNode.CreateNavigator(), "settingname");
                string sValue = XmlUtils.GetNodeValue(oTabSettingNode.CreateNavigator(), "settingvalue");
                objTab.TabSettings[sKey] = sValue;
            }
        }

        private static void DeserializeTabPermissions(XmlNodeList nodeTabPermissions, TabInfo tab, bool isAdminTemplate)
        {
            var permissionController = new PermissionController();
            int permissionID = 0;
            foreach (XmlNode tabPermissionNode in nodeTabPermissions)
            {
                string permissionKey = XmlUtils.GetNodeValue(tabPermissionNode.CreateNavigator(), "permissionkey");
                string permissionCode = XmlUtils.GetNodeValue(tabPermissionNode.CreateNavigator(), "permissioncode");
                string roleName = XmlUtils.GetNodeValue(tabPermissionNode.CreateNavigator(), "rolename");
                bool allowAccess = XmlUtils.GetNodeValueBoolean(tabPermissionNode, "allowaccess");
                ArrayList arrPermissions = permissionController.GetPermissionByCodeAndKey(permissionCode, permissionKey);
                int i;
                for (i = 0; i <= arrPermissions.Count - 1; i++)
                {
                    var permission = (PermissionInfo)arrPermissions[i];
                    permissionID = permission.PermissionID;
                }

                int roleID = int.MinValue;
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleID = Convert.ToInt32(Globals.glbRoleAllUsers);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleID = Convert.ToInt32(Globals.glbRoleUnauthUser);
                        break;
                    default:
                        var portal = PortalController.Instance.GetPortal(tab.PortalID);
                        var role = RoleController.Instance.GetRole(
                            portal.PortalID,
                            r => r.RoleName == roleName);
                        if (role != null)
                        {
                            roleID = role.RoleID;
                        }
                        else
                        {
                            if (isAdminTemplate && roleName.ToLowerInvariant() == "administrators")
                            {
                                roleID = portal.AdministratorRoleId;
                            }
                        }

                        break;
                }

                if (roleID != int.MinValue)
                {
                    var tabPermission = new TabPermissionInfo
                    {
                        TabID = tab.TabID,
                        PermissionID = permissionID,
                        RoleID = roleID,
                        UserID = Null.NullInteger,
                        AllowAccess = allowAccess,
                    };

                    bool canAdd = !tab.TabPermissions.Cast<TabPermissionInfo>()
                                      .Any(tp => tp.TabID == tabPermission.TabID
                                                 && tp.PermissionID == tabPermission.PermissionID
                                                 && tp.RoleID == tabPermission.RoleID
                                                 && tp.UserID == tabPermission.UserID);
                    if (canAdd)
                    {
                        tab.TabPermissions.Add(tabPermission);
                    }
                }
            }
        }

        private static void DeserializeTabUrls(XmlNode nodeTabUrl, TabUrlInfo objTabUrl)
        {
            objTabUrl.SeqNum = XmlUtils.GetAttributeValueAsInteger(nodeTabUrl.CreateNavigator(), "SeqNum", 0);
            objTabUrl.Url = string.IsNullOrEmpty(XmlUtils.GetAttributeValue(nodeTabUrl.CreateNavigator(), "Url")) ? "/" : XmlUtils.GetAttributeValue(nodeTabUrl.CreateNavigator(), "Url");
            objTabUrl.QueryString = XmlUtils.GetAttributeValue(nodeTabUrl.CreateNavigator(), "QueryString");
            objTabUrl.CultureCode = XmlUtils.GetAttributeValue(nodeTabUrl.CreateNavigator(), "CultureCode");
            objTabUrl.HttpStatus = string.IsNullOrEmpty(XmlUtils.GetAttributeValue(nodeTabUrl.CreateNavigator(), "HttpStatus")) ? "200" : XmlUtils.GetAttributeValue(nodeTabUrl.CreateNavigator(), "HttpStatus");
            objTabUrl.IsSystem = XmlUtils.GetAttributeValueAsBoolean(nodeTabUrl.CreateNavigator(), "IsSystem", true);
            objTabUrl.PortalAliasId = Null.NullInteger;
            objTabUrl.PortalAliasUsage = PortalAliasUsageType.Default;
        }

        private static int GetIndexOfTab(TabInfo objTab, IEnumerable<TabInfo> tabs)
        {
            return Null.NullInteger + tabs.TakeWhile(tab => tab.TabID != objTab.TabID).Count();
        }

        private static int GetPortalId(int tabId, int portalId)
        {
            if (Null.IsNull(portalId))
            {
                Dictionary<int, int> portalDic = PortalController.GetPortalDictionary();
                if (portalDic != null && portalDic.ContainsKey(tabId))
                {
                    portalId = portalDic[tabId];
                }
            }

            return portalId;
        }

        private static object GetTabPathDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            string cultureCode = Convert.ToString(cacheItemArgs.ParamList[0]);
            var portalID = (int)cacheItemArgs.ParamList[1];
            var tabpathDic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            IDataReader dr = DataProvider.Instance().GetTabPaths(portalID, cultureCode);
            try
            {
                while (dr.Read())
                {
                    tabpathDic[Null.SetNullString(dr["TabPath"])] = Null.SetNullInteger(dr["TabID"]);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return tabpathDic;
        }

        private static void UpdateTabVersion(int tabId)
        {
            DataProvider.Instance().UpdateTabVersion(tabId, Guid.NewGuid());
        }

        private static void ValidateTabPath(TabInfo tab)
        {
            string tabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);
            int tabId = GetTabByTabPath(tab.PortalID, tabPath, tab.CultureCode);
            if (tabId > Null.NullInteger)
            {
                // Tab exists so Throw
                throw new TabExistsException(
                    tabId,
                    string.Format(
                                                 "Page Exists in portal: {0}, path: {1}, culture: {2}",
                                                 tab.PortalID, tab.TabPath, tab.CultureCode));
            }
        }

        private static void EnableTabVersioningAndWorkflow(TabInfo tab)
        {
            if (TabVersionSettings.Instance.IsVersioningEnabled(tab.PortalID))
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, true);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(tab.PortalID))
            {
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, true);
            }
        }

        private static void DisableTabVersioningAndWorkflow(TabInfo tab)
        {
            if (TabVersionSettings.Instance.IsVersioningEnabled(tab.PortalID))
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, false);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(tab.PortalID))
            {
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }
        }

        private bool IsAdminTab(TabInfo tab)
        {
            var portal = PortalController.Instance.GetPortal(tab.PortalID);
            return portal.AdminTabId == tab.TabID || this.IsAdminTabRecursive(tab, portal.AdminTabId);
        }

        private bool IsAdminTabRecursive(TabInfo tab, int adminTabId)
        {
            if (tab.ParentId == Null.NullInteger)
            {
                return false;
            }

            if (tab.ParentId == adminTabId)
            {
                return true;
            }

            var parentTab = this.GetTab(tab.ParentId, tab.PortalID);
            return this.IsAdminTabRecursive(parentTab, adminTabId);
        }

        private bool IsHostTab(TabInfo tab)
        {
            return tab.PortalID == Null.NullInteger;
        }

        private int AddTabInternal(TabInfo tab, int afterTabId, int beforeTabId, bool includeAllTabsModules)
        {
            ValidateTabPath(tab);

            // First create ContentItem as we need the ContentItemID
            this.CreateContentItem(tab);

            // Add Tab
            if (afterTabId > 0)
            {
                tab.TabID = this._dataProvider.AddTabAfter(tab, afterTabId, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                tab.TabID = beforeTabId > 0
                                ? this._dataProvider.AddTabBefore(tab, beforeTabId, UserController.Instance.GetCurrentUserInfo().UserID)
                                : this._dataProvider.AddTabToEnd(tab, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // Clear the Cache
            this.ClearCache(tab.PortalID);

            ITermController termController = Util.GetTermController();
            termController.RemoveTermsFromContent(tab);
            foreach (Term term in tab.Terms)
            {
                termController.AddTermToContent(term, tab);
            }

            EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID,
                            string.Empty, EventLogController.EventLogType.TAB_CREATED);

            // Add Tab Permissions
            TabPermissionController.SaveTabPermissions(tab);

            // Add TabSettings - use Try/catch as tabs are added during upgrade ptocess and the sproc may not exist
            try
            {
                this.UpdateTabSettings(ref tab);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            // Add AllTabs Modules
            if (includeAllTabsModules && tab.PortalID != Null.NullInteger)
            {
                AddAllTabsModules(tab);
            }

            // Check Tab Versioning
            if (tab.PortalID == Null.NullInteger || !TabVersionSettings.Instance.IsVersioningEnabled(tab.PortalID, tab.TabID))
            {
                this.MarkAsPublished(tab);
            }

            EventManager.Instance.OnTabCreated(new TabEventArgs { Tab = tab });

            return tab.TabID;
        }

        private void CreateLocalizedCopyInternal(TabInfo originalTab, Locale locale, bool allTabsModulesFromDefault, bool clearCache, bool insertAfterOriginal = false)
        {
            try
            {
                Logger.TraceFormat("Localizing TabId: {0}, TabPath: {1}, Locale: {2}", originalTab.TabID, originalTab.TabPath, locale.Code);
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(originalTab.PortalID);

                // First Clone the Tab
                TabInfo localizedCopy = originalTab.Clone();
                localizedCopy.TabID = Null.NullInteger;
                localizedCopy.StateID = Null.NullInteger;
                localizedCopy.ContentItemId = Null.NullInteger;

                // Set Guids and Culture Code
                localizedCopy.UniqueId = Guid.NewGuid();
                localizedCopy.VersionGuid = Guid.NewGuid();
                localizedCopy.LocalizedVersionGuid = Guid.NewGuid();
                localizedCopy.CultureCode = locale.Code;
                localizedCopy.TabName = localizedCopy.TabName + " (" + locale.Code + ")";

                // copy page tags
                foreach (var term in originalTab.Terms)
                {
                    localizedCopy.Terms.Add(term);
                }

                if (locale == defaultLocale)
                {
                    originalTab.DefaultLanguageGuid = localizedCopy.UniqueId;
                    this.UpdateTab(originalTab);
                }
                else
                {
                    localizedCopy.DefaultLanguageGuid = originalTab.UniqueId;
                }

                // Copy Permissions from original Tab for Admins only
                // If original tab is user tab or its parent tab is user tab, then copy full permission
                // from original tab.
                PortalInfo portal = PortalController.Instance.GetPortal(originalTab.PortalID);
                if (originalTab.TabID == portal.UserTabId || originalTab.ParentId == portal.UserTabId)
                {
                    localizedCopy.TabPermissions.AddRange(originalTab.TabPermissions);
                }
                else
                {
                    localizedCopy.TabPermissions.AddRange(originalTab.TabPermissions.Where(p => p.RoleID == portal.AdministratorRoleId));
                }

                // Get the original Tabs Parent
                // check the original whether have parent.
                if (!Null.IsNull(originalTab.ParentId))
                {
                    TabInfo originalParent = this.GetTab(originalTab.ParentId, originalTab.PortalID, false);

                    // Get the localized parent
                    TabInfo localizedParent = this.GetTabByCulture(originalParent.TabID, originalParent.PortalID, locale);
                    localizedCopy.ParentId = localizedParent.TabID;
                }

                // Save Tab
                var afterTabId = insertAfterOriginal ? originalTab.TabID : -1;
                const int beforeTabId = -1;
                const bool includeAllModules = false;
                this.AddTabInternal(localizedCopy, afterTabId, beforeTabId, includeAllModules); // not include modules show on all page, it will handled in copy modules action.

                // if the tab has custom stylesheet defined, then also copy the stylesheet to the localized version.
                if (originalTab.TabSettings.ContainsKey("CustomStylesheet"))
                {
                    this.UpdateTabSetting(localizedCopy.TabID, "CustomStylesheet", originalTab.TabSettings["CustomStylesheet"].ToString());
                }

                /* Tab versioning and workflow is disabled
                 * during the creation of the Localized copy
                 */
                DisableTabVersioningAndWorkflow(localizedCopy);

                // Make shallow copies of all modules
                ModuleController.Instance.CopyModules(originalTab, localizedCopy, true, allTabsModulesFromDefault);

                // Convert these shallow copies to deep copies
                foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(localizedCopy.TabID))
                {
                    ModuleController.Instance.LocalizeModule(kvp.Value, locale);
                }

                // if not copy modules which show on all pages from default language, we need add all modules in current culture.
                if (!allTabsModulesFromDefault)
                {
                    AddAllTabsModules(localizedCopy);
                }

                // Add Translator Role
                this.GiveTranslatorRoleEditRights(localizedCopy, null);

                /* Tab versioning and workflow is re-enabled
                 * when the Localized copy is created
                 */
                EnableTabVersioningAndWorkflow(localizedCopy);
                this.MarkAsPublished(localizedCopy);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }

            // Clear the Cache
            if (clearCache)
            {
                this.ClearCache(originalTab.PortalID);
            }
        }

        private void ClearTabSettingsCache(int tabId)
        {
            var portalId = GetPortalId(tabId, -1);
            string cacheKey = string.Format(DataCache.TabSettingsCacheKey, portalId);
            DataCache.RemoveCache(cacheKey);

            // aslo clear the settings from tab object in cache.
            var tab = this.GetTab(tabId, portalId, false);
            if (tab != null)
            {
                tab.ClearSettingsCache();
            }
        }

        private void CreateTabRedirect(TabInfo tab)
        {
            var settings = PortalController.Instance.GetCurrentPortalSettings();

            if (settings != null && tab.TabID != settings.HomeTabId && tab.TabUrls.Count(u => u.HttpStatus == "200") == 0)
            {
                var domainRoot = TestableGlobals.Instance.AddHTTP(settings.PortalAlias.HTTPAlias);

                if (!string.IsNullOrEmpty(domainRoot))
                {
                    var url = TestableGlobals.Instance.NavigateURL(tab.TabID);

                    url = url.Replace(domainRoot, string.Empty);

                    var seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;
                    var tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = seqNum,
                        PortalAliasId = -1,
                        PortalAliasUsage = PortalAliasUsageType.Default,
                        Url = url,
                        QueryString = string.Empty,
                        CultureCode = tab.CultureCode,
                        HttpStatus = "301",
                        IsSystem = true,
                    };

                    this.SaveTabUrl(tabUrl, tab.PortalID, false);
                }
            }
        }

        private void CreateTabRedirects(TabInfo tab)
        {
            this.CreateTabRedirect(tab);

            var descendants = this.GetTabsByPortal(tab.PortalID).DescendentsOf(tab.TabID);

            // Create Redirect for descendant tabs
            foreach (TabInfo descendantTab in descendants)
            {
                this.CreateTabRedirect(descendantTab);
            }
        }

        private Dictionary<int, List<TabAliasSkinInfo>> GetAliasSkins(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabAliasSkinCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, List<TabAliasSkinInfo>>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.TabAliasSkinCacheTimeOut,
                DataCache.TabAliasSkinCachePriority,
                portalId),
                this.GetAliasSkinsCallback);
        }

        private object GetAliasSkinsCallback(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            var dic = new Dictionary<int, List<TabAliasSkinInfo>>();
            if (portalID > -1)
            {
                IDataReader dr = DataProvider.Instance().GetTabAliasSkins(portalID);
                try
                {
                    while (dr.Read())
                    {
                        // fill business object
                        var tabAliasSkin = CBO.FillObject<TabAliasSkinInfo>(dr, false);

                        // add Tab Alias Skin to dictionary
                        if (dic.ContainsKey(tabAliasSkin.TabId))
                        {
                            // Add Tab Alias Skin to Tab Alias Skin Collection already in dictionary for TabId
                            dic[tabAliasSkin.TabId].Add(tabAliasSkin);
                        }
                        else
                        {
                            // Create new Tab Alias Skin Collection for TabId
                            var collection = new List<TabAliasSkinInfo> { tabAliasSkin };

                            // Add Collection to Dictionary
                            dic.Add(tabAliasSkin.TabId, collection);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.LogException(exc);
                }
                finally
                {
                    // close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }

            return dic;
        }

        private Dictionary<int, Dictionary<string, string>> GetCustomAliases(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabCustomAliasCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, Dictionary<string, string>>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.TabCustomAliasCacheTimeOut,
                DataCache.TabCustomAliasCachePriority,
                portalId),
                this.GetCustomAliasesCallback);
        }

        private object GetCustomAliasesCallback(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            var dic = new Dictionary<int, Dictionary<string, string>>();
            if (portalID > -1)
            {
                IDataReader dr = DataProvider.Instance().GetTabCustomAliases(portalID);
                try
                {
                    while (dr.Read())
                    {
                        // fill business object
                        var tabId = (int)dr["TabId"];
                        var customAlias = (string)dr["httpAlias"];
                        var cultureCode = (string)dr["cultureCode"];

                        // add Custom Alias to dictionary
                        if (dic.ContainsKey(tabId))
                        {
                            // Add Custom Alias to Custom Alias Collection already in dictionary for TabId
                            dic[tabId][cultureCode] = customAlias;
                        }
                        else
                        {
                            // Create new Custom Alias Collection for TabId
                            var collection = new Dictionary<string, string> { { cultureCode, customAlias } };

                            // Add Collection to Dictionary
                            dic.Add(tabId, collection);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.LogException(exc);
                }
                finally
                {
                    // close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }

            return dic;
        }

        private IEnumerable<TabInfo> GetSiblingTabs(TabInfo objTab)
        {
            return this.GetTabsByPortal(objTab.PortalID).WithCulture(objTab.CultureCode, true).WithParentId(objTab.ParentId);
        }

        private Dictionary<int, Hashtable> GetTabSettingsByPortal(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabSettingsCacheKey, portalId);
            return CBO.GetCachedObject<Dictionary<int, Hashtable>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.TabCacheTimeOut,
                DataCache.TabCachePriority),
                c =>
                        {
                            var tabSettings = new Dictionary<int, Hashtable>();
                            using (var dr = this._dataProvider.GetTabSettings(portalId))
                            {
                                while (dr.Read())
                                {
                                    int tabId = dr.GetInt32(0);
                                    Hashtable settings;
                                    if (!tabSettings.TryGetValue(tabId, out settings))
                                    {
                                        settings = new Hashtable();
                                        tabSettings[tabId] = settings;
                                    }

                                    if (!dr.IsDBNull(2))
                                    {
                                        settings[dr.GetString(1)] = dr.GetString(2);
                                    }
                                    else
                                    {
                                        settings[dr.GetString(1)] = string.Empty;
                                    }
                                }
                            }

                            return tabSettings;
                        });
        }

        private object GetTabUrlsCallback(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            var dic = new Dictionary<int, List<TabUrlInfo>>();

            if (portalID > -1)
            {
                IDataReader dr = DataProvider.Instance().GetTabUrls(portalID);
                try
                {
                    while (dr.Read())
                    {
                        // fill business object
                        var tabRedirect = CBO.FillObject<TabUrlInfo>(dr, false);

                        // add Tab Redirect to dictionary
                        if (dic.ContainsKey(tabRedirect.TabId))
                        {
                            // Add Tab Redirect to Tab Redirect Collection already in dictionary for TabId
                            dic[tabRedirect.TabId].Add(tabRedirect);
                        }
                        else
                        {
                            // Create new Tab Redirect Collection for TabId
                            var collection = new List<TabUrlInfo> { tabRedirect };

                            // Add Collection to Dictionary
                            dic.Add(tabRedirect.TabId, collection);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.LogException(exc);
                }
                finally
                {
                    // close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }

            return dic;
        }

        private void HardDeleteTabInternal(int tabId, int portalId)
        {
            // Delete all tabModule Instances
            foreach (ModuleInfo m in ModuleController.Instance.GetTabModules(tabId).Values)
            {
                ModuleController.Instance.DeleteTabModule(m.TabID, m.ModuleID, false);
            }

            var tab = this.GetTab(tabId, portalId, false);

            // Delete Tab
            this._dataProvider.DeleteTab(tabId);

            // Log deletion
            EventLogController.Instance.AddLog(
                "TabID",
                tabId.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.TAB_DELETED);

            // queue remove tab/page from search index
            var document = new SearchDocumentToDelete
            {
                TabId = tabId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);

            // Remove the Content Item
            if (tab != null && tab.ContentItemId > Null.NullInteger)
            {
                IContentController contentController = Util.GetContentController();
                contentController.DeleteContentItem(tab);
            }

            EventManager.Instance.OnTabDeleted(new TabEventArgs { Tab = tab });
        }

        private bool SoftDeleteChildTabs(int intTabid, PortalSettings portalSettings)
        {
            bool bDeleted = true;
            foreach (TabInfo objtab in GetTabsByParent(intTabid, portalSettings.PortalId))
            {
                bDeleted = this.SoftDeleteTabInternal(objtab, portalSettings);
                if (!bDeleted)
                {
                    break;
                }
            }

            return bDeleted;
        }

        private bool SoftDeleteTabInternal(TabInfo tabToDelete, PortalSettings portalSettings)
        {
            Dto.ChangeControlState changeControlStateForTab = null;
            if (tabToDelete.PortalID > -1)
            {
                changeControlStateForTab = TabChangeSettings.Instance.GetChangeControlState(
                    tabToDelete.PortalID,
                    tabToDelete.TabID);
                if (changeControlStateForTab.IsChangeControlEnabledForTab)
                {
                    TabVersionSettings.Instance.SetEnabledVersioningForTab(tabToDelete.TabID, false);
                    TabWorkflowSettings.Instance.SetWorkflowEnabled(tabToDelete.PortalID, tabToDelete.TabID, false);
                }
            }

            var deleted = false;
            if (!IsSpecialTab(tabToDelete.TabID, portalSettings))
            {
                if (this.SoftDeleteChildTabs(tabToDelete.TabID, portalSettings))
                {
                    tabToDelete.IsDeleted = true;
                    this.UpdateTab(tabToDelete);

                    foreach (ModuleInfo m in ModuleController.Instance.GetTabModules(tabToDelete.TabID).Values)
                    {
                        ModuleController.Instance.DeleteTabModule(m.TabID, m.ModuleID, true);
                    }

                    EventLogController.Instance.AddLog(tabToDelete, portalSettings, portalSettings.UserId, string.Empty,
                                              EventLogController.EventLogType.TAB_SENT_TO_RECYCLE_BIN);
                    deleted = true;

                    EventManager.Instance.OnTabRemoved(new TabEventArgs { Tab = tabToDelete });
                }
            }

            if (changeControlStateForTab != null && changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tabToDelete.TabID, changeControlStateForTab.IsVersioningEnabledForTab);
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tabToDelete.PortalID, tabToDelete.TabID, changeControlStateForTab.IsWorkflowEnabledForTab);
            }

            return deleted;
        }

        private void UpdateTabSettingInternal(int tabId, string settingName, string settingValue, bool clearCache)
        {
            using (var dr = this._dataProvider.GetTabSetting(tabId, settingName))
            {
                if (dr.Read())
                {
                    if (dr.GetString(0) != settingValue)
                    {
                        this._dataProvider.UpdateTabSetting(tabId, settingName, settingValue,
                            UserController.Instance.GetCurrentUserInfo().UserID);
                        EventLogController.AddSettingLog(
                            EventLogController.EventLogType.TAB_SETTING_UPDATED,
                            "TabId", tabId, settingName, settingValue,
                            UserController.Instance.GetCurrentUserInfo().UserID);
                    }
                }
                else
                {
                    this._dataProvider.UpdateTabSetting(tabId, settingName, settingValue,
                        UserController.Instance.GetCurrentUserInfo().UserID);
                    EventLogController.AddSettingLog(
                        EventLogController.EventLogType.TAB_SETTING_CREATED,
                        "TabId", tabId, settingName, settingValue,
                        UserController.Instance.GetCurrentUserInfo().UserID);
                }

                dr.Close();
            }

            UpdateTabVersion(tabId);
            if (clearCache)
            {
                this.ClearTabSettingsCache(tabId);
            }
        }

        private void UpdateTabSettings(ref TabInfo updatedTab)
        {
            foreach (string sKeyLoopVariable in updatedTab.TabSettings.Keys)
            {
                string sKey = sKeyLoopVariable;
                this.UpdateTabSettingInternal(updatedTab.TabID, sKey, Convert.ToString(updatedTab.TabSettings[sKey]), false);
            }
        }

        /// <summary>
        /// update content item for the tab when tab name changed.
        /// </summary>
        /// <param name="tab">The updated tab.</param>
        private void UpdateContentItem(TabInfo tab)
        {
            IContentController contentController = Util.GetContentController();
            var newContent = string.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
            if (tab.Content != newContent)
            {
                tab.Content = newContent;
                contentController.UpdateContentItem(tab);
            }
        }
    }
}
