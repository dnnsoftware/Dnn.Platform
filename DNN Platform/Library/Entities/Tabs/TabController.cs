#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Tabs.Internal;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Tabs
{
    /// <summary>
    /// TabController provides all operation to tabinfo.
    /// </summary>
    /// <remarks>
    /// Tab is equal to page in DotNetNuke.
    /// Tabs will be a sitemap for a poatal, and every request at first need to check whether there is valid tab information
    /// include in the url, if not it will use default tab to display information.
    /// </remarks>
    public class TabController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabController));
        private static readonly DataProvider Provider = DataProvider.Instance();

        /// <summary>
        /// Gets the current page in current http request.
        /// </summary>
        /// <value>Current Page Info.</value>
        public static TabInfo CurrentPage
        {
            get
            {
                TabInfo tab = null;
                if (PortalController.GetCurrentPortalSettings() != null)
                {
                    tab = PortalController.GetCurrentPortalSettings().ActiveTab;
                }
                return tab;
            }
        }

        #region Private Methods

        private static void AddAllTabsModules(TabInfo tab)
        {
            var objmodules = new ModuleController();
            var portalSettings = new PortalSettings(tab.TabID, tab.PortalID);
            foreach (ModuleInfo allTabsModule in objmodules.GetAllTabsModules(tab.PortalID, true))
            {
                //[DNN-6276]We need to check that the Module is not implicitly deleted.  ie If all instances are on Pages
                //that are all "deleted" then even if the Module itself is not deleted, we would not expect the 
                //Module to be added
                var canAdd =
                (from ModuleInfo allTabsInstance in objmodules.GetModuleTabs(allTabsModule.ModuleID) select new TabController().GetTab(allTabsInstance.TabID, tab.PortalID, false)).Any(
                    t => !t.IsDeleted) && (!portalSettings.ContentLocalizationEnabled || allTabsModule.CultureCode == tab.CultureCode);
                if (canAdd)
                {
                    objmodules.CopyModule(allTabsModule, tab, Null.NullString, true);
                }
            }
        }

        private int AddTabInternal(TabInfo tab, int afterTabId, int beforeTabId, bool includeAllTabsModules)
        {
            ValidateTabPath(tab);

            //First create ContentItem as we need the ContentItemID
            CreateContentItem(tab);

            //Add Tab
            if (afterTabId > 0)
            {
                tab.TabID = Provider.AddTabAfter(tab, afterTabId, UserController.GetCurrentUserInfo().UserID);
            }
            else
            {
                tab.TabID = beforeTabId > 0
                                ? Provider.AddTabBefore(tab, beforeTabId, UserController.GetCurrentUserInfo().UserID)
                                : Provider.AddTabToEnd(tab, UserController.GetCurrentUserInfo().UserID);
            }

            //Clear the Cache
            ClearCache(tab.PortalID);

            ITermController termController = Util.GetTermController();
            termController.RemoveTermsFromContent(tab);
            foreach (Term term in tab.Terms)
            {
                termController.AddTermToContent(term, tab);
            }

            var eventLog = new EventLogController();
            eventLog.AddLog(tab, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID,
                            "", EventLogController.EventLogType.TAB_CREATED);

            //Add Tab Permissions
            TabPermissionController.SaveTabPermissions(tab);

            //Add TabSettings - use Try/catch as tabs are added during upgrade ptocess and the sproc may not exist
            try
            {
                UpdateTabSettings(ref tab);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            //Add AllTabs Modules
            if (includeAllTabsModules && tab.PortalID != Null.NullInteger)
            {
                AddAllTabsModules(tab);
            }

            return tab.TabID;
        }

        private void CreateTabRedirect(TabInfo tab)
        {
            var settings = PortalController.GetCurrentPortalSettings();


            if (settings != null && tab.TabUrls.Count(u => u.HttpStatus == "200") == 0)
            {
                var domainRoot = Globals.AddHTTP(settings.PortalAlias.HTTPAlias);

                if (!String.IsNullOrEmpty(domainRoot))
                {
                    var url = Globals.NavigateURL(tab.TabID);

                    url = url.Replace(domainRoot, "");

                    var seqNum = (tab.TabUrls.Count > 0) ? tab.TabUrls.Max(t => t.SeqNum) + 1 : 1;
                    var tabUrl = new TabUrlInfo()
                    {
                        TabId = tab.TabID,
                        SeqNum = seqNum,
                        PortalAliasId = -1,
                        PortalAliasUsage = PortalAliasUsageType.Default,
                        Url = url,
                        QueryString = String.Empty,
                        CultureCode = tab.CultureCode,
                        HttpStatus = "301",
                        IsSystem = true
                    };

                    TestableTabController.Instance.SaveTabUrl(tabUrl, tab.PortalID, false);
                }
            }
        }

        private void CreateTabRedirects(TabInfo tab)
        {
            CreateTabRedirect(tab);

            var descendants = GetTabsByPortal(tab.PortalID).DescendentsOf(tab.TabID);

            //Create Redirect for descendant tabs
            foreach (TabInfo descendantTab in descendants)
            {
                CreateTabRedirect(descendantTab);
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
                        var portalController = new PortalController();
                        PortalInfo portal = portalController.GetPortal(tab.PortalID);
                        RoleInfo role = TestableRoleController.Instance.GetRole(portal.PortalID,
                                                                                r => r.RoleName == roleName);
                        if (role != null)
                        {
                            roleID = role.RoleID;
                        }
                        else
                        {
                            if (isAdminTemplate && roleName.ToLower() == "administrators")
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
                            AllowAccess = allowAccess
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

        private static object GetTabsByPortalCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalID = (int)cacheItemArgs.ParamList[0];
            List<TabInfo> tabs = CBO.FillCollection<TabInfo>(Provider.GetTabs(portalID));
            return new TabCollection(tabs);
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

        private IEnumerable<TabInfo> GetSiblingTabs(TabInfo objTab)
        {
            return GetTabsByPortal(objTab.PortalID).WithCulture(objTab.CultureCode, true).WithParentId(objTab.ParentId);
        }

        private void HardDeleteTabInternal(int tabId)
        {
            //Delete all tabModule Instances
            var moduleController = new ModuleController();
            foreach (ModuleInfo m in moduleController.GetTabModules(tabId).Values)
            {
                moduleController.DeleteTabModule(m.TabID, m.ModuleID, false);
            }

            //Delete Tab
            Provider.DeleteTab(tabId);

            //Log deletion
            var eventLog = new EventLogController();
            eventLog.AddLog("TabID",
                            tabId.ToString(),
                            PortalController.GetCurrentPortalSettings(),
                            UserController.GetCurrentUserInfo().UserID,
                            EventLogController.EventLogType.TAB_DELETED);
        }

        private bool SoftDeleteChildTabs(int intTabid, PortalSettings portalSettings)
        {
            bool bDeleted = true;
            foreach (TabInfo objtab in GetTabsByParent(intTabid, portalSettings.PortalId))
            {
                bDeleted = SoftDeleteTabInternal(objtab, portalSettings);
                if (!bDeleted)
                {
                    break;
                }
            }
            return bDeleted;
        }

        private bool SoftDeleteTabInternal(TabInfo tabToDelete, PortalSettings portalSettings)
        {
            bool deleted = true;
            if (!IsSpecialTab(tabToDelete.TabID, portalSettings))
            {
                if (SoftDeleteChildTabs(tabToDelete.TabID, portalSettings))
                {
                    tabToDelete.IsDeleted = true;
                    UpdateTab(tabToDelete);

                    var moduleCtrl = new ModuleController();
                    foreach (ModuleInfo m in moduleCtrl.GetTabModules(tabToDelete.TabID).Values)
                    {
                        moduleCtrl.DeleteTabModule(m.TabID, m.ModuleID, true);
                    }

                    var eventLogController = new EventLogController();
                    eventLogController.AddLog(tabToDelete, portalSettings, portalSettings.UserId, "",
                                              EventLogController.EventLogType.TAB_SENT_TO_RECYCLE_BIN);
                }
                else
                {
                    deleted = false;
                }
            }
            else
            {
                deleted = false;
            }
            return deleted;
        }

        private static void UpdateTabVersion(int tabId)
        {
            Provider.UpdateTabVersion(tabId, Guid.NewGuid());
        }

        private static void ValidateTabPath(TabInfo tab)
        {
            string tabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);
            int tabId = GetTabByTabPath(tab.PortalID, tabPath, tab.CultureCode);
            if (tabId > Null.NullInteger)
            {
                //Tab exists so Throw
                throw new TabExistsException(tabId,
                                             string.Format("Page Exists in portal: {0}, path: {1}, culture: {2}",
                                                           tab.PortalID, tab.TabPath, tab.CultureCode));
            }
        }

        #endregion

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a tab
        /// </summary>
        /// <param name="objTab">The tab to be added</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        /// <history>
        /// 	[cnurse]	04/30/2008	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public int AddTab(TabInfo objTab)
        {
            return AddTab(objTab, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a tab
        /// </summary>
        /// <param name="objTab">The tab to be added</param>
        /// <param name="includeAllTabsModules">Flag that indicates whether to add the "AllTabs"
        /// Modules</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        /// <history>
        /// 	[cnurse]	04/30/2008	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        public int AddTab(TabInfo objTab, bool includeAllTabsModules)
        {
            //Add tab to store
            int tabID = AddTabInternal(objTab, -1, -1, includeAllTabsModules);

            //Clear the Cache
            ClearCache(objTab.PortalID);

            return tabID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a tab after the specified tab
        /// </summary>
        /// <param name="tab">The tab to be added</param>
        /// <param name="afterTabId">Id of the tab after which this tab is added</param>
        /// <history>
        /// 	[cnurse]	04/30/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int AddTabAfter(TabInfo tab, int afterTabId)
        {
            //Add tab to store
            int tabID = AddTabInternal(tab, afterTabId, -1, true);

            //Clear the Cache
            ClearCache(tab.PortalID);

            return tabID;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a tab before the specified tab
        /// </summary>
        /// <param name="objTab">The tab to be added</param>
        /// <param name="beforeTabId">Id of the tab before which this tab is added</param>
        /// <history>
        /// 	[cnurse]	04/30/2008	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int AddTabBefore(TabInfo objTab, int beforeTabId)
        {
            //Add tab to store
            int tabID = AddTabInternal(objTab, -1, beforeTabId, true);

            //Clear the Cache
            ClearCache(objTab.PortalID);

            return tabID;
        }

        /// <summary>
        /// Clears tabs and portal cache for the specific portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public void ClearCache(int portalId)
        {
            DataCache.ClearTabsCache(portalId);

            //Clear the Portal cache so the Pages count is correct
            DataCache.ClearPortalCache(portalId, false);

            DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);

            CacheController.FlushPageIndexFromCache();
        }

        /// <summary>
        /// Creates content item for the tab..
        /// </summary>
        /// <param name="tab">The updated tab.</param>
        public void CreateContentItem(TabInfo tab)
        {
            //First create ContentItem as we need the ContentItemID
            var typeController = new ContentTypeController();
            ContentType contentType =
                (from t in typeController.GetContentTypes() where t.ContentType == "Tab" select t).SingleOrDefault();

            IContentController contentController = Util.GetContentController();
            tab.Content = String.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
            if (contentType != null)
            {
                tab.ContentTypeId = contentType.ContentTypeId;
            }
            tab.Indexed = false;
            contentController.AddContentItem(tab);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a tab permanently from the database
        /// </summary>
        /// <param name="tabId">TabId of the tab to be deleted</param>
        /// <param name="portalId">PortalId of the portal</param>
        /// <remarks>
        /// The tab will not delete if it has child tab(s).
        /// </remarks>
        /// <history>
        /// 	[Vicenç]	19/09/2004	Added skin deassignment before deleting the tab.
        /// </history>
        /// -----------------------------------------------------------------------------
        public void DeleteTab(int tabId, int portalId)
        {
            //parent tabs can not be deleted
            if (GetTabsByPortal(portalId).WithParentId(tabId).Count == 0)
            {
                HardDeleteTabInternal(tabId);
            }
            ClearCache(portalId);
        }

        /// <summary>
        /// Deletes a tab permanently from the database
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="deleteDescendants">if set to <c>true</c> will delete all child tabs.</param>
        /// <remarks>
        /// 
        /// </remarks>
        public void DeleteTab(int tabId, int portalId, bool deleteDescendants)
        {
            List<TabInfo> descendantList = GetTabsByPortal(portalId).DescendentsOf(tabId);
            if (deleteDescendants && descendantList.Count > 0)
            {
                //Iterate through descendants from bottom - which will remove children first
                for (int i = descendantList.Count - 1; i >= 0; i += -1)
                {
                    HardDeleteTabInternal(descendantList[i].TabID);
                }
            }
            DeleteTab(tabId, portalId);

            ClearCache(portalId);
        }

        /// <summary>
        /// Delete a Setting of a tab instance
        /// </summary>
        /// <param name="tabId">ID of the affected tab</param>
        /// <param name="settingName">Name of the setting to be deleted</param>
        /// <history>
        ///    [jlucarino] 2009-10-01 Created
        /// </history>
        public void DeleteTabSetting(int tabId, string settingName)
        {
            Provider.DeleteTabSetting(tabId, settingName);
            var eventLogController = new EventLogController();
            var eventLogInfo = new LogInfo();
            eventLogInfo.LogProperties.Add(new LogDetailInfo("TabID", tabId.ToString()));
            eventLogInfo.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            eventLogInfo.LogTypeKey = EventLogController.EventLogType.TAB_SETTING_DELETED.ToString();
            eventLogController.AddLog(eventLogInfo);

            UpdateTabVersion(tabId);
            DataCache.RemoveCache("GetTabSettings" + tabId);
        }

        /// <summary>
        /// Delete all Settings of a tab instance
        /// </summary>
        /// <param name="tabId">ID of the affected tab</param>
        /// <history>
        ///    [jlucarino] 2009-10-01 Created
        /// </history>
        public void DeleteTabSettings(int tabId)
        {
            Provider.DeleteTabSettings(tabId);
            var eventLogController = new EventLogController();
            var eventLogInfo = new LogInfo();
            eventLogInfo.LogProperties.Add(new LogDetailInfo("TabId", tabId.ToString()));
            eventLogInfo.LogTypeKey = EventLogController.EventLogType.TAB_SETTING_DELETED.ToString();
            eventLogController.AddLog(eventLogInfo);
            UpdateTabVersion(tabId);
            DataCache.RemoveCache("GetTabSettings" + tabId);
        }

        /// <summary>
        /// Deletes all tabs for a specific language. Double checks if we are not deleting pages for the default language
        /// Clears the tab cache optionally
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public bool DeleteTranslatedTabs(int portalId, string cultureCode, bool clearCache)
        {
            bool returnValue = true;
            if (PortalController.GetCurrentPortalSettings() != null)
            {
                var defaultLanguage = PortalController.GetCurrentPortalSettings().DefaultLanguage;
                if (cultureCode != defaultLanguage)
                {
                    Provider.DeleteTranslatedTabs(portalId, cultureCode);

                    if (clearCache)
                    {
                        ClearCache(portalId);
                    }

                }
            }
            return returnValue;
        }

        /// <summary>
        /// Reverts page culture back to Neutral (Null), to ensure a non localized site
        /// clears the tab cache optionally
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void EnsureNeutralLanguage(int portalId, string cultureCode, bool clearCache)
        {
            Provider.EnsureNeutralLanguage(portalId, cultureCode);
            if (clearCache)
            {
                ClearCache(portalId);
            }
        }

        /// <summary>
        /// Converts one single tab to a neutral culture
        /// clears the tab cache optionally
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void ConvertTabToNeutralLanguage(int portalId, int tabId, string cultureCode, bool clearCache)
        {
            //parent tabs can not be deleted
            if (GetTabsByPortal(portalId).WithParentId(tabId).Count == 0)
            {
                // delete all translated / localized tabs for this tab
                var tab = GetTab(tabId, portalId, true);
                foreach (var localizedTab in tab.LocalizedTabs.Values)
                {
                    HardDeleteTabInternal(localizedTab.TabID);
                }

                // reset culture of current tab back to neutral
                Provider.ConvertTabToNeutralLanguage(portalId, tabId, cultureCode);
                if (clearCache)
                {
                    ClearCache(portalId);
                }
            }

        }

        /// <summary>
        /// Returns True if a page is missing a translated version in at least one other language
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <returns></returns>
        public bool HasMissingLanguages(int portalId, int tabId)
        {
            var currentTab = GetTab(tabId, portalId, false);
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

            return ((LocaleCount - localizedCount) != 0);

        }

        /// <summary>
        /// Adds localized copies of the page in all missing languages
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        public void AddMissingLanguages(int portalId, int tabId)
        {
            var currentTab = GetTab(tabId, portalId, false);
            if (currentTab.CultureCode != null)
            {
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(portalId);
                var workingTab = currentTab;
                if (workingTab.CultureCode != defaultLocale.Code && workingTab.DefaultLanguageTab == null)
                {
                    // we are adding missing languages to a single culture page that is not in the default language
                    // so we must first add a page in the default culture

                    CreateLocalizedCopy(workingTab, defaultLocale, false);
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
                            CreateLocalizedCopy(workingTab, locale, false);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Gets all tabs.
        /// </summary>
        /// <returns>tab collection</returns>
        public ArrayList GetAllTabs()
        {
            return CBO.FillCollection(Provider.GetAllTabs(), typeof(TabInfo));
        }

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="ignoreCache">if set to <c>true</c> will get tab info directly from database.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTab(int tabId, int portalId, bool ignoreCache)
        {
            TabInfo tab;

            //if we are using the cache
            if (ignoreCache || Host.Host.PerformanceSetting == Globals.PerformanceSettings.NoCaching)
            {
                tab = CBO.FillObject<TabInfo>(Provider.GetTab(tabId));
            }
            else
            {
                //if we do not know the PortalId then try to find it in the Portals Dictionary using the TabId
                portalId = GetPortalId(tabId, portalId);

                //if we have the PortalId then try to get the TabInfo object
                tab = GetTabsByPortal(portalId).WithTabId(tabId) ??
                      GetTabsByPortal(GetPortalId(tabId, Null.NullInteger)).WithTabId(tabId);

                if (tab == null)
                {
                    //recheck the info directly from database to make sure we can avoid error if the cache doesn't update
                    // correctly, this may occurred when install is set up in web farm.
                    tab = CBO.FillObject<TabInfo>(Provider.GetTab(tabId));

                    // if tab is not null means that the cache doesn't update correctly, we need clear the cache
                    // and let it rebuild cache when request next time.
                    if (tab != null)
                    {
                        ClearCache(tab.PortalID);
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
        /// Gets the tab by unique ID.
        /// </summary>
        /// <param name="uniqueID">The unique ID.</param>
        /// <returns>tab info.</returns>
        public TabInfo GetTabByUniqueID(Guid uniqueID)
        {
            return CBO.FillObject<TabInfo>(Provider.GetTabByUniqueID(uniqueID));
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
            TabCollection tabs = GetTabsByPortal(portalId);

            //Get Tab specified by Id
            TabInfo originalTab = tabs.WithTabId(tabId);

            if (locale != null && originalTab != null)
            {
                //Check if tab is in the requested culture
                if (string.IsNullOrEmpty(originalTab.CultureCode) || originalTab.CultureCode == locale.Code)
                {
                    localizedTab = originalTab;
                }
                else
                {
                    //See if tab exists for culture
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
                                    !originalTab.DefaultLanguageTab.LocalizedTabs.TryGetValue(locale.Code,
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
            return GetTabsByPortal(portalId).WithTabName(tabName);
        }

        /// <summary>
        /// Gets the name of the tab by name and parent id.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns>tab info</returns>
        public TabInfo GetTabByName(string tabName, int portalId, int parentId)
        {
            return GetTabsByPortal(portalId).WithTabNameAndParentId(tabName, parentId);
        }

        /// <summary>
        /// Gets the tab count in portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab's count.</returns>
        public int GetTabCount(int portalId)
        {
            return GetTabsByPortal(portalId).Count;
        }

        /// <summary>
        /// Gets the tabs by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab collection</returns>
        public TabCollection GetTabsByPortal(int portalId)
        {
            string cacheKey = string.Format(DataCache.TabCacheKey, portalId);
            return CBO.GetCachedObject<TabCollection>(new CacheItemArgs(cacheKey,
                                                                    DataCache.TabCacheTimeOut,
                                                                    DataCache.TabCachePriority,
                                                                    portalId),
                                                            GetTabsByPortalCallBack);
        }

        /// <summary>
        /// read all settings for a tab from TabSettings table
        /// </summary>
        /// <param name="tabId">ID of the Tab to query</param>
        /// <returns>
        /// (cached) hashtable containing all settings
        /// </returns>
        /// <history>
        /// [jlucarino] 2009-08-31 Created
        ///   </history>
        public Hashtable GetTabSettings(int tabId)
        {
            string cacheKey = "GetTabSettings" + tabId;
            var tabSettings = (Hashtable)DataCache.GetCache(cacheKey);
            if (tabSettings == null)
            {
                tabSettings = new Hashtable();
                IDataReader dr = Provider.GetTabSettings(tabId);
                while (dr.Read())
                {
                    if (!dr.IsDBNull(1))
                    {
                        tabSettings[dr.GetString(0)] = dr.GetString(1);
                    }
                    else
                    {
                        tabSettings[dr.GetString(0)] = "";
                    }
                }
                dr.Close();

                //cache data
                int intCacheTimeout = 20 * Convert.ToInt32(Host.Host.PerformanceSetting);
                DataCache.SetCache(cacheKey, tabSettings, TimeSpan.FromMinutes(intCacheTimeout));
            }
            return tabSettings;
        }

        /// <summary>
        /// Gets the tabs which use the module.
        /// </summary>
        /// <param name="moduleID">The module ID.</param>
        /// <returns>tab collection</returns>
        public IDictionary<int, TabInfo> GetTabsByModuleID(int moduleID)
        {
            return CBO.FillDictionary<int, TabInfo>("TabID", Provider.GetTabsByModuleID(moduleID));
        }

        /// <summary>
        /// Gets the tabs which use the package.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="packageID">The package ID.</param>
        /// <param name="forHost">if set to <c>true</c> [for host].</param>
        /// <returns>tab collection</returns>
        public IDictionary<int, TabInfo> GetTabsByPackageID(int portalID, int packageID, bool forHost)
        {
            return CBO.FillDictionary<int, TabInfo>("TabID", Provider.GetTabsByPackageID(portalID, packageID, forHost));
        }

        /// <summary>
        /// Moves the tab by the tab move type.
        /// </summary>
        /// <param name="tab">The obj tab.</param>
        /// <param name="type">The type.</param>
        /// <seealso cref="TabMoveType"/>
        /// <example>
        /// <code lang="C#">
        /// TabController tabCtrl = new TabController();
        /// tabCtrl.MoveTab(tab, TabMoveType.Up);
        /// </code>
        /// </example>
        public void MoveTab(TabInfo tab, TabMoveType type)
        {
            //Get the List of tabs with the same parent
            IOrderedEnumerable<TabInfo> siblingTabs = GetSiblingTabs(tab).OrderBy(t => t.TabOrder);
            int tabIndex = GetIndexOfTab(tab, siblingTabs);
            switch (type)
            {
                case TabMoveType.Top:
                    MoveTabBefore(tab, siblingTabs.First().TabID);
                    break;
                case TabMoveType.Bottom:
                    MoveTabAfter(tab, siblingTabs.Last().TabID);
                    break;
                case TabMoveType.Up:
                    MoveTabBefore(tab, siblingTabs.ElementAt(tabIndex - 1).TabID);
                    break;
                case TabMoveType.Down:
                    MoveTabAfter(tab, siblingTabs.ElementAt(tabIndex + 1).TabID);
                    break;
                case TabMoveType.Promote:
                    MoveTabAfter(tab, tab.ParentId);
                    break;
                case TabMoveType.Demote:
                    MoveTabToParent(tab, siblingTabs.ElementAt(tabIndex - 1).TabID);
                    break;
            }
            ClearCache(tab.PortalID);
        }

        /// <summary>
        /// Moves the tab after a specific tab.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="afterTabId">will move objTab after this tab.</param>
        public void MoveTabAfter(TabInfo tab, int afterTabId)
        {
            //Get AfterTab
            var afterTab = GetTab(afterTabId, tab.PortalID, false);

            //Create Tab Redirects
            if (afterTab.ParentId != tab.ParentId)
            {
                CreateTabRedirects(tab);
            }

            //Move Tab
            Provider.MoveTabAfter(tab.TabID, afterTabId, UserController.GetCurrentUserInfo().UserID);

            //Clear the Cache
            ClearCache(tab.PortalID);
        }

        /// <summary>
        /// Moves the tab before a specific tab.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="beforeTabId">will move objTab before this tab.</param>
        public void MoveTabBefore(TabInfo tab, int beforeTabId)
        {
            //Get AfterTab
            var beforeTab = GetTab(beforeTabId, tab.PortalID, false);

            //Create Tab Redirects
            if (beforeTab.ParentId != tab.ParentId)
            {
                CreateTabRedirects(tab);
            }

            //Move Tab
            Provider.MoveTabBefore(tab.TabID, beforeTabId, UserController.GetCurrentUserInfo().UserID);

            //Clear the Cache
            ClearCache(tab.PortalID);
        }

        public void MoveTabToParent(TabInfo tab, int parentId)
        {
            //Create Tab Redirects
            if (parentId != tab.ParentId)
            {
                CreateTabRedirects(tab);
            }

            //Move Tab
            Provider.MoveTabToParent(tab.TabID, parentId, UserController.GetCurrentUserInfo().UserID);

            //Clear the Cache
            ClearCache(tab.PortalID);
        }

        /// <summary>
        /// Populates the bread crumbs.
        /// </summary>
        /// <param name="tab">The tab.</param>
        public void PopulateBreadCrumbs(ref TabInfo tab)
        {
            if ((tab.BreadCrumbs == null))
            {
                var crumbs = new ArrayList();
                PopulateBreadCrumbs(tab.PortalID, ref crumbs, tab.TabID);
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
            //find the tab in the tabs collection
            TabInfo tab;
            var tabController = new TabController();
            TabCollection portalTabs = tabController.GetTabsByPortal(portalID);
            TabCollection hostTabs = tabController.GetTabsByPortal(Null.NullInteger);
            bool found = portalTabs.TryGetValue(tabID, out tab);
            if (!found)
            {
                found = hostTabs.TryGetValue(tabID, out tab);
            }
            //if tab was found
            if (found)
            {
                breadCrumbs.Insert(0, tab.Clone());

                //get the tab parent
                if (!Null.IsNull(tab.ParentId))
                {
                    PopulateBreadCrumbs(portalID, ref breadCrumbs, tab.ParentId);
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
                //We are trying to restore the child, so recall this function with the master language's tab id
                RestoreTab(tab.DefaultLanguageTab, portalSettings);
                return;
            }

            tab.IsDeleted = false;
            UpdateTab(tab);

            //Restore any localized children
            foreach (TabInfo localizedtab in tab.LocalizedTabs.Values)
            {
                localizedtab.IsDeleted = false;
                UpdateTab(localizedtab);
            }

            var eventLogController = new EventLogController();
            eventLogController.AddLog(tab, portalSettings, portalSettings.UserId, "",
                                      EventLogController.EventLogType.TAB_RESTORED);

            var moduleController = new ModuleController();
            ArrayList allTabsModules = moduleController.GetAllTabsModules(tab.PortalID, true);
            foreach (ModuleInfo objModule in allTabsModules)
            {
                moduleController.CopyModule(objModule, tab, Null.NullString, true);
            }

            ClearCache(tab.PortalID);
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
            TabInfo tab = GetTab(tabId, portalSettings.PortalId, false);
            if (tab != null)
            {
                if (tab.DefaultLanguageTab != null &&
                    LocaleController.Instance.GetLocales(portalSettings.PortalId).ContainsKey(tab.CultureCode))
                {
                    //We are trying to delete the child, so recall this function with the master language's tab id
                    return SoftDeleteTab(tab.DefaultLanguageTab.TabID, portalSettings);
                }

                //Delete the Tab
                deleted = SoftDeleteTabInternal(tab, portalSettings);

                //Delete any localized children
                if (deleted)
                {
                    foreach (TabInfo localizedtab in tab.LocalizedTabs.Values)
                    {
                        SoftDeleteTabInternal(localizedtab, portalSettings);
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
            TabInfo originalTab = GetTab(updatedTab.TabID, updatedTab.PortalID, true);

            //Update ContentItem If neccessary
            if (updatedTab.ContentItemId == Null.NullInteger && updatedTab.TabID != Null.NullInteger)
            {
                CreateContentItem(updatedTab);
            }

            //Create Tab Redirects
            if (originalTab.ParentId != updatedTab.ParentId || originalTab.TabName != updatedTab.TabName)
            {
                CreateTabRedirects(updatedTab);
            }

            //Update Tab to DataStore
            Provider.UpdateTab(updatedTab.TabID,
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
                               UserController.GetCurrentUserInfo().UserID,
                               updatedTab.CultureCode);

            //Update Tags
            List<Term> terms = updatedTab.Terms;
            ITermController termController = Util.GetTermController();
            termController.RemoveTermsFromContent(updatedTab);
            foreach (Term term in terms)
            {
                termController.AddTermToContent(term, updatedTab);
            }

            var eventLogController = new EventLogController();
            eventLogController.AddLog(updatedTab, PortalController.GetCurrentPortalSettings(),
                                      UserController.GetCurrentUserInfo().UserID, "",
                                      EventLogController.EventLogType.TAB_UPDATED);

            //Update Tab permissions
            TabPermissionController.SaveTabPermissions(updatedTab);

            //Update TabSettings - use Try/catch as tabs are added during upgrade ptocess and the sproc may not exist
            try
            {
                UpdateTabSettings(ref updatedTab);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            //Update Tab Version
            UpdateTabVersion(updatedTab.TabID);

            //Clear Tab Caches
            ClearCache(updatedTab.PortalID);
            if (updatedTab.PortalID != originalTab.PortalID)
            {
                ClearCache(originalTab.PortalID);
            }
        }

        /// <summary>
        /// Updates the tab settings.
        /// </summary>
        /// <param name="updatedTab">The updated tab.</param>
        private void UpdateTabSettings(ref TabInfo updatedTab)
        {
            foreach (string sKeyLoopVariable in updatedTab.TabSettings.Keys)
            {
                string sKey = sKeyLoopVariable;
                UpdateTabSetting(updatedTab.TabID, sKey, Convert.ToString(updatedTab.TabSettings[sKey]));
            }
        }

        /// <summary>
        /// Adds or updates a tab's setting value
        /// </summary>
        /// <param name="tabId">ID of the tab to update</param>
        /// <param name="settingName">name of the setting property</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will remove the setting, if not preserveIfEmpty is true</remarks>
        /// <history>
        ///    [jlucarino] 2009-10-01 Created
        /// </history>
        public void UpdateTabSetting(int tabId, string settingName, string settingValue)
        {
            IDataReader dr = Provider.GetTabSetting(tabId, settingName);
            if (dr.Read())
            {
                if (dr.GetString(0) != settingValue)
                {
                    Provider.UpdateTabSetting(tabId, settingName, settingValue,
                                              UserController.GetCurrentUserInfo().UserID);
                    EventLogController.AddSettingLog(EventLogController.EventLogType.TAB_SETTING_UPDATED,
                                                     "TabId", tabId, settingName, settingValue,
                                                     UserController.GetCurrentUserInfo().UserID);
                }
            }
            else
            {
                Provider.AddTabSetting(tabId, settingName, settingValue, UserController.GetCurrentUserInfo().UserID);
                EventLogController.AddSettingLog(EventLogController.EventLogType.TAB_SETTING_CREATED,
                                                 "TabId", tabId, settingName, settingValue,
                                                 UserController.GetCurrentUserInfo().UserID);
            }
            dr.Close();

            UpdateTabVersion(tabId);
            DataCache.RemoveCache("GetTabSettings" + tabId);
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
                                                    UserController.GetCurrentUserInfo().UserID);

            //Clear Tab Caches
            ClearCache(localizedTab.PortalID);
        }

        #region Static Methods

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
        public static void CopyDesignToChildren(TabInfo parentTab, string skinSrc, string containerSrc,
                                                string cultureCode)
        {
            bool clearCache = Null.NullBoolean;
            var tabController = new TabController();
            List<TabInfo> childTabs = tabController.GetTabsByPortal(parentTab.PortalID).DescendentsOf(parentTab.TabID);
            foreach (TabInfo tab in childTabs)
            {
                if (TabPermissionController.CanAdminPage(tab))
                {
                    //Update ContentItem If neccessary
                    if (tab.ContentItemId == Null.NullInteger && tab.TabID != Null.NullInteger)
                    {
                        tabController.CreateContentItem(tab);
                    }

                    Provider.UpdateTab(tab.TabID,
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
                                       UserController.GetCurrentUserInfo().UserID,
                                       tab.CultureCode);

                    UpdateTabVersion(tab.TabID);

                    var eventLogController = new EventLogController();
                    eventLogController.AddLog(tab, PortalController.GetCurrentPortalSettings(),
                                              UserController.GetCurrentUserInfo().UserID, "",
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
            List<TabInfo> childTabs =
                new TabController().GetTabsByPortal(parentTab.PortalID).DescendentsOf(parentTab.TabID);
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
        /// Processes all panes and modules in the template file
        /// </summary>
        /// <param name="nodePanes">Template file node for the panes is current tab</param>
        /// <param name="portalId">PortalId of the new portal</param>
        /// <param name="tabId">Tab being processed</param>
        /// <param name="mergeTabs">Tabs need to merge.</param>
        /// <param name="hModules">Modules Hashtable.</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	03/09/2004	Created
        /// 	[VMasanas]	15/10/2004	Modified for new skin structure
        ///		[cnurse]	15/10/2004	Modified to allow for merging template
        ///								with existing pages
        ///     [cnurse]    10/02/2007  Moved from PortalController
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeserializePanes(XmlNode nodePanes, int portalId, int tabId,
                                            PortalTemplateModuleAction mergeTabs, Hashtable hModules)
        {
            var moduleController = new ModuleController();

            Dictionary<int, ModuleInfo> dicModules = moduleController.GetTabModules(tabId);

            //If Mode is Replace remove all the modules already on this Tab
            if (mergeTabs == PortalTemplateModuleAction.Replace)
            {
                foreach (KeyValuePair<int, ModuleInfo> kvp in dicModules.Where(kvp => !kvp.Value.AllTabs))
                {
                    moduleController.DeleteTabModule(tabId, kvp.Value.ModuleID, false);
                }
            }

            //iterate through the panes
            foreach (XmlNode nodePane in nodePanes.ChildNodes)
            {
                //iterate through the modules
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
            var tabController = new TabController();
            string tabName = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "name");
            if (!String.IsNullOrEmpty(tabName))
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
                tab.IconFileLarge = Globals.ImportFile(portalId,
                                                       XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "iconfilelarge"));
                tab.Url = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "url");
                tab.StartDate = XmlUtils.GetNodeValueDate(tabNode, "startdate", Null.NullDate);
                tab.EndDate = XmlUtils.GetNodeValueDate(tabNode, "enddate", Null.NullDate);
                tab.RefreshInterval = XmlUtils.GetNodeValueInt(tabNode, "refreshinterval", Null.NullInteger);
                tab.PageHeadText = XmlUtils.GetNodeValue(tabNode, "pageheadtext", Null.NullString);
                tab.IsSecure = XmlUtils.GetNodeValueBoolean(tabNode, "issecure", false);
                tab.SiteMapPriority = XmlUtils.GetNodeValueSingle(tabNode, "sitemappriority", (float)0.5);
                tab.CultureCode = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "cultureCode");
                //objTab.UniqueId = New Guid(XmlUtils.GetNodeValue(nodeTab, "guid", Guid.NewGuid.ToString()));
                //objTab.VersionGuid = New Guid(XmlUtils.GetNodeValue(nodeTab, "versionGuid", Guid.NewGuid.ToString()));
                tab.UseBaseFriendlyUrls = XmlUtils.GetNodeValueBoolean(tabNode, "UseBaseFriendlyUrls", false);

                tab.TabPermissions.Clear();
                DeserializeTabPermissions(tabNode.SelectNodes("tabpermissions/permission"), tab, isAdminTemplate);

                DeserializeTabSettings(tabNode.SelectNodes("tabsettings/tabsetting"), tab);

                //set tab skin and container
                if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "skinsrc", "")))
                {
                    tab.SkinSrc = XmlUtils.GetNodeValue(tabNode, "skinsrc", "");
                }
                if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "containersrc", "")))
                {
                    tab.ContainerSrc = XmlUtils.GetNodeValue(tabNode, "containersrc", "");
                }

                tabName = tab.TabName;
                if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent")))
                {
                    if (tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent")] != null)
                    {
                        //parent node specifies the path (tab1/tab2/tab3), use saved tabid
                        tab.ParentId = Convert.ToInt32(tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent")]);
                        tabName = XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent") + "/" + tab.TabName;
                    }
                    else
                    {
                        //Parent node doesn't spcecify the path, search by name.
                        //Possible incoherence if tabname not unique
                        TabInfo objParent =
                            tabController.GetTabByName(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "parent"),
                                                       portalId);
                        if (objParent != null)
                        {
                            tab.ParentId = objParent.TabID;
                            tabName = objParent.TabName + "/" + tab.TabName;
                        }
                        else
                        {
                            //parent tab not found!
                            tab.ParentId = Null.NullInteger;
                            tabName = tab.TabName;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab")))
                {
                    if (tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab")] != null)
                    {
                        //parent node specifies the path (tab1/tab2/tab3), use saved tabid
                        int defaultLanguageTabId =
                            Convert.ToInt32(tabs[XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab")]);
                        TabInfo defaultLanguageTab = tabController.GetTab(defaultLanguageTabId, portalId, false);
                        if (defaultLanguageTab != null)
                        {
                            tab.DefaultLanguageGuid = defaultLanguageTab.UniqueId;
                        }
                    }
                    else
                    {
                        //Parent node doesn't spcecify the path, search by name.
                        //Possible incoherence if tabname not unique
                        TabInfo defaultLanguageTab =
                            tabController.GetTabByName(
                                XmlUtils.GetNodeValue(tabNode.CreateNavigator(), "defaultLanguageTab"), portalId);
                        if (defaultLanguageTab != null)
                        {
                            tab.DefaultLanguageGuid = defaultLanguageTab.UniqueId;
                        }
                    }
                }

                //create/update tab
                if (tab.TabID == Null.NullInteger)
                {
                    tab.TabID = tabController.AddTab(tab);
                }
                else
                {
                    tabController.UpdateTab(tab);
                }

                //extra check for duplicate tabs in same level
                if (tabs[tabName] == null)
                {
                    tabs.Add(tabName, tab.TabID);
                }
            }

            //Parse Panes
            if (tabNode.SelectSingleNode("panes") != null)
            {
                DeserializePanes(tabNode.SelectSingleNode("panes"), portalId, tab.TabID, mergeTabs, modules);
            }

            //Finally add "tabid" to node
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
            return GetPortalTabs(GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true),
                                 excludeTabId,
                                 includeNoneSpecified,
                                 "<" + Localization.GetString("None_Specified") + ">",
                                 includeHidden,
                                 false,
                                 false,
                                 false,
                                 false);
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
            return GetPortalTabs(GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true),
                                 excludeTabId,
                                 includeNoneSpecified,
                                 "<" + Localization.GetString("None_Specified") + ">",
                                 includeHidden,
                                 includeDeleted,
                                 includeURL,
                                 false,
                                 false);
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
                                                  string noneSpecifiedText, bool includeHidden, bool includeDeleted,
                                                  bool includeURL,
                                                  bool checkViewPermisison, bool checkEditPermission)
        {
            return GetPortalTabs(GetTabsBySortOrder(portalId, PortalController.GetActivePortalLanguage(portalId), true),
                                 excludeTabId,
                                 includeNoneSpecified,
                                 noneSpecifiedText,
                                 includeHidden,
                                 includeDeleted,
                                 includeURL,
                                 checkViewPermisison,
                                 checkEditPermission);
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
                                                  string noneSpecifiedText, bool includeHidden, bool includeDeleted,
                                                  bool includeURL,
                                                  bool checkViewPermisison, bool checkEditPermission)
        {
            var listTabs = new List<TabInfo>();
            if (includeNoneSpecified)
            {
                var tab = new TabInfo { TabID = -1, TabName = noneSpecifiedText, TabOrder = 0, ParentId = -2 };
                listTabs.Add(tab);
            }
            foreach (TabInfo objTab in tabs)
            {
                UserInfo objUserInfo = UserController.GetCurrentUserInfo();
                if (((excludeTabId < 0) || (objTab.TabID != excludeTabId)) &&
                    (!objTab.IsSuperTab || objUserInfo.IsSuperUser))
                {
                    if ((objTab.IsVisible || includeHidden) && (objTab.IsDeleted == false || includeDeleted) &&
                        (objTab.TabType == TabType.Normal || includeURL))
                    {
                        //Check if User has View/Edit Permission for this tab
                        if (checkEditPermission || checkViewPermisison)
                        {
                            const string permissionList = "ADD,COPY,EDIT,MANAGE";
                            if (checkEditPermission &&
                                TabPermissionController.HasTabPermission(objTab.TabPermissions, permissionList))
                            {
                                listTabs.Add(objTab);
                            }
                            else if (checkViewPermisison && TabPermissionController.CanViewPage(objTab))
                            {
                                listTabs.Add(objTab);
                            }
                        }
                        else
                        {
                            //Add Tab to List
                            listTabs.Add(objTab);
                        }
                    }
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
            return new TabController().GetTabsByPortal(portalId).WithParentId(parentId);
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
            return new TabController().GetTabsByPortal(portalId).WithCulture(cultureCode, includeNeutral).AsList();
        }

        /// <summary>
        /// Get all TabInfo for the current culture in SortOrder
        /// </summary>
        /// <param name="portalId">The portalid to load tabs for</param>
        /// <returns>
        /// List of TabInfo oredered by default SortOrder
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
            var portalController = new PortalController();
            bool isSpecial = false;
            foreach (Locale locale in locales.Values)
            {
                PortalInfo portal = portalController.GetPortal(portalId, locale.Code);
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
        /// SerializeTab
        /// </summary>
        /// <param name="tabXml">The Xml Document to use for the Tab</param>
        /// <param name="objTab">The TabInfo object to serialize</param>
        /// <param name="includeContent">A flag used to determine if the Module content is included</param>
        public static XmlNode SerializeTab(XmlDocument tabXml, TabInfo objTab, bool includeContent)
        {
            return SerializeTab(tabXml, null, objTab, null, includeContent);
        }

        /// <summary>
        /// SerializeTab
        /// </summary>
        /// <param name="tabXml">The Xml Document to use for the Tab</param>
        /// <param name="tabs">A Hashtable used to store the names of the tabs</param>
        /// <param name="tab">The TabInfo object to serialize</param>
        /// <param name="portal">The Portal object to which the tab belongs</param>
        /// <param name="includeContent">A flag used to determine if the Module content is included</param>
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

                //remove unwanted elements
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
                    XmlDocument tabPermissions = new XmlDocument();
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

            //Manage Url
            XmlNode urlNode = tabXml.SelectSingleNode("tab/url");
            switch (tab.TabType)
            {
                case TabType.Normal:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "Normal"));
                    break;
                case TabType.Tab:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "Tab"));
                    //Get the tab being linked to
                    TabInfo tempTab = new TabController().GetTab(Int32.Parse(tab.Url), tab.PortalID, false);
                    urlNode.InnerXml = tempTab.TabPath;
                    break;
                case TabType.File:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "File"));
                    IFileInfo file = FileManager.Instance.GetFile(Int32.Parse(tab.Url.Substring(7)));
                    urlNode.InnerXml = file.RelativePath;
                    break;
                case TabType.Url:
                    urlNode.Attributes.Append(XmlUtils.CreateAttribute(tabXml, "type", "Url"));
                    break;
            }

            
            //serialize TabSettings
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
            }
            if (tabs != null)
            {
                //Manage Parent Tab
                if (!Null.IsNull(tab.ParentId))
                {
                    newnode = tabXml.CreateElement("parent");
                    newnode.InnerXml = HttpContext.Current.Server.HtmlEncode(tabs[tab.ParentId].ToString());
                    tabNode.AppendChild(newnode);

                    //save tab as: ParentTabName/CurrentTabName
                    tabs.Add(tab.TabID, tabs[tab.ParentId] + "/" + tab.TabName);
                }
                else
                {
                    //save tab as: CurrentTabName
                    tabs.Add(tab.TabID, tab.TabName);
                }
            }

            //Manage Content Localization
            if (tab.DefaultLanguageTab != null)
            {

                try
                {
                    newnode = tabXml.CreateElement("defaultLanguageTab");
                    newnode.InnerXml = HttpContext.Current.Server.HtmlEncode(tabs[tab.DefaultLanguageTab.TabID].ToString());
                    tabNode.AppendChild(newnode);
                }
                catch { }
            }

            XmlNode panesNode;
            XmlNode paneNode;
            XmlNode nameNode;
            XmlNode modulesNode;
            XmlNode moduleNode;
            XmlDocument moduleXml;
            ModuleInfo module;
            var moduleController = new ModuleController();

            //Serialize modules
            panesNode = tabNode.AppendChild(tabXml.CreateElement("panes"));
            foreach (KeyValuePair<int, ModuleInfo> kvp in moduleController.GetTabModules(tab.TabID))
            {
                module = kvp.Value;
                if (!module.IsDeleted)
                {
                    moduleXml = new XmlDocument();
                    moduleNode = ModuleController.SerializeModule(moduleXml, module, includeContent);
                    if (panesNode.SelectSingleNode("descendant::pane[name='" + module.PaneName + "']") == null)
                    {
                        //new pane found
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
            IEnumerable<PortalAliasInfo> aliasLookup = TestablePortalAliasController.Instance.GetPortalAliases().Values;

            foreach (PortalAliasInfo alias in TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalId))
            {
                string checkAlias = string.Format("{0}{1}", alias.HTTPAlias, tabPath.Replace("//", "/"));
                if (aliasLookup.Any(a => a.HTTPAlias.Equals(checkAlias, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidTabName(string tabName, out string invalidType)
        {
            var valid = true;
            invalidType = string.Empty;

            if (string.IsNullOrEmpty(tabName.Trim()))
            {
                invalidType = "EmptyTabName";
                valid = false;
            }
            else if ((Regex.IsMatch(tabName, "^LPT[1-9]$|^COM[1-9]$", RegexOptions.IgnoreCase)))
            {
                invalidType = "InvalidTabName";
                valid = false;
            }
            else if ((Regex.IsMatch(HtmlUtils.StripNonWord(tabName, false), "^AUX$|^CON$|^NUL$|^SITEMAP$|^LINKCLICK$|^KEEPALIVE$|^DEFAULT$|^ERRORPAGE$|^LOGIN$|^REGISTER$", RegexOptions.IgnoreCase)))
            {
                invalidType = "InvalidTabName";
                valid = false;
            }

            return valid;
        }

        #endregion

        #region Content Localization

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
                    CreateLocalizedCopy(originalTab, subLocale);
                }
            }
        }

        /// <summary>
        /// Creates the localized copy.
        /// </summary>
        /// <param name="tabs">The tabs.</param>
        /// <param name="locale">The locale.</param>
        public void CreateLocalizedCopy(List<TabInfo> tabs, Locale locale)
        {
            foreach (TabInfo t in tabs)
            {
                CreateLocalizedCopy(t, locale);
            }
        }

        /// <summary>
        /// Creates the localized copy.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        public void CreateLocalizedCopy(TabInfo originalTab, Locale locale)
        {
            CreateLocalizedCopy(originalTab,locale,true);
        }

        /// <summary>
        /// Creates the localized copy.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="clearCache">Clear the cache?</param>
        public void CreateLocalizedCopy(TabInfo originalTab, Locale locale, bool clearCache)
        {
            try
            {
                Logger.TraceFormat("Localizing TabId: {0}, TabPath: {1}, Locale: {2}",originalTab.TabID,originalTab.TabPath,locale.Code);
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(originalTab.PortalID);

                //First Clone the Tab
                TabInfo localizedCopy = originalTab.Clone();
                localizedCopy.TabID = Null.NullInteger;

                //Set Guids and Culture Code
                localizedCopy.UniqueId = Guid.NewGuid();
                localizedCopy.VersionGuid = Guid.NewGuid();
                localizedCopy.LocalizedVersionGuid = Guid.NewGuid();
                localizedCopy.CultureCode = locale.Code;
                localizedCopy.TabName = localizedCopy.TabName + " (" + locale.Code + ")";                    
                if (locale == defaultLocale)
                {
                    originalTab.DefaultLanguageGuid = localizedCopy.UniqueId;
                    UpdateTab(originalTab);
                }
                else
                {
                    localizedCopy.DefaultLanguageGuid = originalTab.UniqueId;                   
                }

                //Copy Permissions from original Tab for Admins only
                var portalCtrl = new PortalController();
                PortalInfo portal = portalCtrl.GetPortal(originalTab.PortalID);
                localizedCopy.TabPermissions.AddRange(
                    originalTab.TabPermissions.Where(p => p.RoleID == portal.AdministratorRoleId));

                //Get the original Tabs Parent
                //check the original whether have parent.
                if (!Null.IsNull(originalTab.ParentId))
                {
                    TabInfo originalParent = GetTab(originalTab.ParentId, originalTab.PortalID, false);

                    if (originalParent != null)
                    {
                        //Get the localized parent
                        TabInfo localizedParent = GetTabByCulture(originalParent.TabID, originalParent.PortalID, locale);

                        localizedCopy.ParentId = localizedParent.TabID;
                    }
                }

                //Save Tab
                AddTabInternal(localizedCopy, -1, -1, true);

                //Make shallow copies of all modules
                var moduleCtrl = new ModuleController();
                moduleCtrl.CopyModules(originalTab, localizedCopy, true);

                //Convert these shallow copies to deep copies
                foreach (KeyValuePair<int, ModuleInfo> kvp in moduleCtrl.GetTabModules(localizedCopy.TabID))
                {
                    moduleCtrl.LocalizeModule(kvp.Value, locale);
                }

                //Add Translator Role
                GiveTranslatorRoleEditRights(localizedCopy, null);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }

            //Clear the Cache
            if (clearCache)
                ClearCache(originalTab.PortalID);
        }

        /// <summary>
        /// Gets the default culture tab list.
        /// </summary>
        /// <param name="portalid">The portalid.</param>
        /// <returns></returns>
        public List<TabInfo> GetDefaultCultureTabList(int portalid)
        {
            return (from kvp in GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        /// <summary>
        /// Gets the culture tab list.
        /// </summary>
        /// <param name="portalid">The portalid.</param>
        /// <returns></returns>
        public List<TabInfo> GetCultureTabList(int portalid)
        {
            return (from kvp in GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && kvp.Value.CultureCode == PortalController.GetCurrentPortalSettings().DefaultLanguage
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        /// <summary>
        /// Gives the translator role edit rights.
        /// </summary>
        /// <param name="localizedTab">The localized tab.</param>
        /// <param name="users">The users.</param>
        public void GiveTranslatorRoleEditRights(TabInfo localizedTab, Dictionary<int, UserInfo> users)
        {
            var roleCtrl = new RoleController();
            var permissionCtrl = new PermissionController();
            ArrayList permissionsList = permissionCtrl.GetPermissionByCodeAndKey("SYSTEM_TAB", "EDIT");

            string translatorRoles =
                PortalController.GetPortalSetting(
                    string.Format("DefaultTranslatorRoles-{0}", localizedTab.CultureCode), localizedTab.PortalID, "");
            foreach (string translatorRole in translatorRoles.Split(';'))
            {
                if (users != null)
                {
                    foreach (UserInfo translator in roleCtrl.GetUsersByRoleName(localizedTab.PortalID, translatorRole))
                    {
                        users[translator.UserID] = translator;
                    }
                }

                if (permissionsList != null && permissionsList.Count > 0)
                {
                    var translatePermisison = (PermissionInfo)permissionsList[0];
                    string roleName = translatorRole;
                    RoleInfo role = TestableRoleController.Instance.GetRole(localizedTab.PortalID,
                                                                            r => r.RoleName == roleName);
                    if (role != null)
                    {
                        TabPermissionInfo perm =
                            localizedTab.TabPermissions.Where(
                                tp => tp.RoleID == role.RoleID && tp.PermissionKey == "EDIT").SingleOrDefault();
                        if (perm == null)
                        {
                            //Create Permission
                            var tabTranslatePermission = new TabPermissionInfo(translatePermisison)
                                {
                                    RoleID = role.RoleID,
                                    AllowAccess = true,
                                    RoleName = roleName
                                };
                            localizedTab.TabPermissions.Add(tabTranslatePermission);
                            UpdateTab(localizedTab);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Localizes the tab.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        public void LocalizeTab(TabInfo originalTab, Locale locale)
        {
            LocalizeTab(originalTab,locale,true);
        }

        /// <summary>
        /// Localizes the tab, with optional clear cache
        /// </summary>
        /// <param name="originalTab"></param>
        /// <param name="locale"></param>
        /// <param name="clearCache"></param>
        public void LocalizeTab(TabInfo originalTab, Locale locale, bool clearCache)
        {
            Provider.LocalizeTab(originalTab.TabID, locale.Code, UserController.GetCurrentUserInfo().UserID);
            if (clearCache)
                DataCache.ClearModuleCache(originalTab.TabID);
        }

        /// <summary>
        /// Publishes the tab.
        /// </summary>
        /// <param name="publishTab">The publish tab.</param>
        public void PublishTab(TabInfo publishTab)
        {
            //To publish a subsidiary language tab we need to enable the View Permissions
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
        /// Checks whether the tab is published. Published means: view permissions of tab are identical to the DefaultLanguageTab
        /// </summary>
        /// <param name="publishTab">The tab that is checked</param>
        /// <returns>true if tab is published</returns>
        public bool IsTabPublished(TabInfo publishTab)
        {
            bool returnValue = true;
            //To publish a subsidiary language tab we need to enable the View Permissions
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
        /// Publishes the tabs.
        /// </summary>
        /// <param name="tabs">The tabs.</param>
        public void PublishTabs(List<TabInfo> tabs)
        {
            foreach (TabInfo t in tabs)
            {
                if (t.IsTranslated)
                {
                    PublishTab(t);
                }
            }
        }

        #endregion

        #region Obsolete

        [Obsolete("This method has replaced in DotNetNuke 5.0 by CopyDesignToChildren(TabInfo,String, String)")]
        public void CopyDesignToChildren(ArrayList tabs, string skinSrc, string containerSrc)
        {
            foreach (TabInfo tab in tabs)
            {
                Provider.UpdateTab(tab.TabID,
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
                                   UserController.GetCurrentUserInfo().UserID,
                                   tab.CultureCode);
                var eventLog = new EventLogController();
                eventLog.AddLog(tab, PortalController.GetCurrentPortalSettings(),
                                UserController.GetCurrentUserInfo().UserID, "",
                                EventLogController.EventLogType.TAB_UPDATED);
            }
            if (tabs.Count > 0)
            {
                DataCache.ClearTabsCache(((TabInfo)tabs[0]).PortalID);
            }
        }

        [Obsolete("Deprecated in DotNetNuke 5.0. Replaced by CopyPermissionsToChildren(TabInfo, TabPermissionCollection)")]
        public void CopyPermissionsToChildren(ArrayList tabs, TabPermissionCollection newPermissions)
        {
            foreach (TabInfo tab in tabs)
            {
                tab.TabPermissions.Clear();
                tab.TabPermissions.AddRange(newPermissions);
                TabPermissionController.SaveTabPermissions(tab);
            }
            if (tabs.Count > 0)
            {
                DataCache.ClearTabsCache(((TabInfo)tabs[0]).PortalID);
            }
        }

        [Obsolete("Deprecated in DotNetNuke 5.5.Replaced by ModuleController.CopyModules")]
        public void CopyTab(int portalId, int fromTabId, int toTabId, bool asReference)
        {
            var modules = new ModuleController();
            TabInfo sourceTab = GetTab(fromTabId, portalId, false);
            TabInfo destinationTab = GetTab(fromTabId, toTabId, false);

            if (sourceTab != null && destinationTab != null)
            {
                modules.CopyModules(sourceTab, destinationTab, asReference);
            }
        }

        [Obsolete("Deprecated in DNN 6.2. Replaced by SoftDeleteTab(tabId, portalSettings)")]
        public static bool DeleteTab(int tabId, PortalSettings portalSettings, int userId)
        {
            return new TabController().SoftDeleteTab(tabId, portalSettings);
        }

        [Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction)")]
        public static TabInfo DeserializeTab(string tabName, XmlNode nodeTab, int portalId)
        {
            return DeserializeTab(nodeTab, null, new Hashtable(), portalId, false, PortalTemplateModuleAction.Ignore,
                                  new Hashtable());
        }

        [Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction)")]
        public static TabInfo DeserializeTab(XmlNode tabNode, TabInfo tab, int portalId)
        {
            return DeserializeTab(tabNode, tab, new Hashtable(), portalId, false, PortalTemplateModuleAction.Ignore,
                                  new Hashtable());
        }

        [Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal hTabs As Hashtable, ByVal PortalId As Integer, ByVal IsAdminTemplate As Boolean, ByVal mergeTabs As PortalTemplateModuleAction, ByVal hModules As Hashtable)")]
        public static TabInfo DeserializeTab(string tabName, XmlNode nodeTab, TabInfo objTab, Hashtable hTabs,
                                             int portalId, bool isAdminTemplate, PortalTemplateModuleAction mergeTabs,
                                             Hashtable hModules)
        {
            return DeserializeTab(nodeTab, objTab, hTabs, portalId, isAdminTemplate, mergeTabs, hModules);
        }

        [Obsolete("Deprecated in DNN 6.2. Method is redundant. Replaced by GetAllTabs()")]
        public ArrayList GetAllTabs(bool checkLegacyFields)
        {
            return GetAllTabs();
        }

        [Obsolete("This method is obsolete.  It has been replaced by GetTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal ignoreCache As Boolean) ")]
        public TabInfo GetTab(int tabId)
        {
            return GetTab(tabId, GetPortalId(tabId, Null.NullInteger), false);
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by GetTabByTabPath(portalId, tabPath, cultureCode) ")]
        public static int GetTabByTabPath(int portalId, string tabPath)
        {
            return GetTabByTabPath(portalId, tabPath, Null.NullString);
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by GetTabPathDictionary(portalId, cultureCode) ")]
        public static Dictionary<string, int> GetTabPathDictionary(int portalId)
        {
            return GetTabPathDictionary(portalId, Null.NullString);
        }

        [Obsolete("This method has been replaced in 5.0 by GetTabPathDictionary(ByVal portalId As Integer) As Dictionary(Of String, Integer) ")]
        public static Dictionary<string, int> GetTabPathDictionary()
        {
            var tabpathDic = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            IDataReader dr = DataProvider.Instance().GetTabPaths(Null.NullInteger, Null.NullString);
            try
            {
                while (dr.Read())
                {
                    string strKey = "//" + Null.SetNullInteger(dr["PortalID"]) + Null.SetNullString(dr["TabPath"]);
                    tabpathDic[strKey] = Null.SetNullInteger(dr["TabID"]);
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

        [Obsolete("This method has replaced in DotNetNuke 5.0 by GetTabsByPortal()")]
        public ArrayList GetTabs(int portalId)
        {
            return GetTabsByPortal(portalId).ToArrayList();
        }

        [Obsolete("This method is obsolete.  It has been replaced by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer) ")]
        public ArrayList GetTabsByParentId(int parentId)
        {
            return new ArrayList(GetTabsByParent(parentId, GetPortalId(parentId, Null.NullInteger)));
        }

        [Obsolete("This method has replaced in DotNetNuke 5.0 by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer)")]
        public ArrayList GetTabsByParentId(int parentId, int portalId)
        {
            var arrTabs = new ArrayList();
            foreach (TabInfo objTab in GetTabsByParent(parentId, portalId))
            {
                arrTabs.Add(objTab);
            }
            return arrTabs;
        }

        [Obsolete("Deprecated in DNN 6.2. Replaced by RestoreTab(tabId, portalSettings)")]
        public static void RestoreTab(TabInfo tab, PortalSettings portalSettings, int userId)
        {
            new TabController().RestoreTab(tab, portalSettings);
        }

        [Obsolete("Deprecated in DNN 5.5. Replaced by UpdateTab(updatedTab)")]
        public void UpdateTab(TabInfo updatedTab, string cultureCode)
        {
            updatedTab.CultureCode = cultureCode;
            UpdateTab(updatedTab);
        }

        [Obsolete("Deprecated in DNN 6.2.  Tab Ordering is handled in the DB ")]
        public void UpdateTabOrder(int portalID, int tabId, int tabOrder, int level, int parentId)
        {
            TabInfo objTab = GetTab(tabId, portalID, false);
            objTab.TabOrder = tabOrder;
            objTab.Level = level;
            objTab.ParentId = parentId;
            UpdateTabOrder(objTab);
        }

        [Obsolete("Deprecated in DNN 6.2.  Tab Ordering is handled in the DB ")]
        public void UpdateTabOrder(TabInfo objTab)
        {
            Provider.UpdateTabOrder(objTab.TabID, objTab.TabOrder, objTab.ParentId,
                                    UserController.GetCurrentUserInfo().UserID);
            UpdateTabVersion(objTab.TabID);
            var eventLogController = new EventLogController();
            eventLogController.AddLog(objTab, PortalController.GetCurrentPortalSettings(),
                                      UserController.GetCurrentUserInfo().UserID, "",
                                      EventLogController.EventLogType.TAB_ORDER_UPDATED);
            ClearCache(objTab.PortalID);
        }

        #endregion
    }
}