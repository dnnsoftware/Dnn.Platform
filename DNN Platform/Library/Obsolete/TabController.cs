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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
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
    public partial class TabController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has replaced in DotNetNuke 5.0 by CopyDesignToChildren(TabInfo,String, String)")]
        public void CopyDesignToChildren(ArrayList tabs, string skinSrc, string containerSrc)
        {
            foreach (TabInfo tab in tabs)
            {
                _dataProvider.UpdateTab(tab.TabID,
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
                EventLogController.Instance.AddLog(tab, PortalController.Instance.GetCurrentPortalSettings(),
                                UserController.Instance.GetCurrentUserInfo().UserID, "",
                                EventLogController.EventLogType.TAB_UPDATED);
            }
            if (tabs.Count > 0)
            {
                DataCache.ClearTabsCache(((TabInfo)tabs[0]).PortalID);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5.Replaced by ModuleController.CopyModules")]
        public void CopyTab(int portalId, int fromTabId, int toTabId, bool asReference)
        {
            TabInfo sourceTab = GetTab(fromTabId, portalId, false);
            TabInfo destinationTab = GetTab(fromTabId, toTabId, false);

            if (sourceTab != null && destinationTab != null)
            {
                ModuleController.Instance.CopyModules(sourceTab, destinationTab, asReference);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. RUse alternate overload")]
        public void CreateLocalizedCopy(List<TabInfo> tabs, Locale locale)
        {
            foreach (TabInfo t in tabs)
            {
                CreateLocalizedCopy(t, locale, true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. RUse alternate overload")]
        public void CreateLocalizedCopy(TabInfo originalTab, Locale locale)
        {
            CreateLocalizedCopy(originalTab, locale, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.2. Replaced by SoftDeleteTab(tabId, portalSettings)")]
        public static bool DeleteTab(int tabId, PortalSettings portalSettings, int userId)
        {
            return Instance.SoftDeleteTab(tabId, portalSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction)")]
        public static TabInfo DeserializeTab(string tabName, XmlNode nodeTab, int portalId)
        {
            return DeserializeTab(nodeTab, null, new Hashtable(), portalId, false, PortalTemplateModuleAction.Ignore,
                                  new Hashtable());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal PortalId As Integer, ByVal mergeTabs As PortalTemplateModuleAction)")]
        public static TabInfo DeserializeTab(XmlNode tabNode, TabInfo tab, int portalId)
        {
            return DeserializeTab(tabNode, tab, new Hashtable(), portalId, false, PortalTemplateModuleAction.Ignore,
                                  new Hashtable());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has replaced in DotNetNuke 5.0 by DeserializeTab(ByVal nodeTab As XmlNode, ByVal objTab As TabInfo, ByVal hTabs As Hashtable, ByVal PortalId As Integer, ByVal IsAdminTemplate As Boolean, ByVal mergeTabs As PortalTemplateModuleAction, ByVal hModules As Hashtable)")]
        public static TabInfo DeserializeTab(string tabName, XmlNode nodeTab, TabInfo objTab, Hashtable hTabs,
                                             int portalId, bool isAdminTemplate, PortalTemplateModuleAction mergeTabs,
                                             Hashtable hModules)
        {
            return DeserializeTab(nodeTab, objTab, hTabs, portalId, isAdminTemplate, mergeTabs, hModules);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not scalable. Use GetTabsByPortal")]
        public ArrayList GetAllTabs()
        {
            return CBO.FillCollection(_dataProvider.GetAllTabs(), typeof(TabInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.2. Method is not scalable. Use GetTabsByPortal")]
        public ArrayList GetAllTabs(bool checkLegacyFields)
        {
            return CBO.FillCollection(_dataProvider.GetAllTabs(), typeof(TabInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not neccessary.  Use LINQ and GetPortalTabs()")]
        public List<TabInfo> GetCultureTabList(int portalid)
        {
            return (from kvp in GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && kvp.Value.CultureCode == PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not neccessary.  Use LINQ and GetPortalTabs()")]
        public List<TabInfo> GetDefaultCultureTabList(int portalid)
        {
            return (from kvp in GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete.  It has been replaced by GetTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal ignoreCache As Boolean) ")]
        public TabInfo GetTab(int tabId)
        {
            return GetTab(tabId, GetPortalId(tabId, Null.NullInteger), false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.5. Replaced by GetTabByTabPath(portalId, tabPath, cultureCode) ")]
        public static int GetTabByTabPath(int portalId, string tabPath)
        {
            return GetTabByTabPath(portalId, tabPath, Null.NullString);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use LINQ queries on tab collections thata re cached")]
        public TabInfo GetTabByUniqueID(Guid uniqueID)
        {
            return CBO.FillObject<TabInfo>(_dataProvider.GetTabByUniqueID(uniqueID));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use GetTabsByPortal(portalId).Count")]
        public int GetTabCount(int portalId)
        {
            return GetTabsByPortal(portalId).Count;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.5. Replaced by GetTabPathDictionary(portalId, cultureCode) ")]
        public static Dictionary<string, int> GetTabPathDictionary(int portalId)
        {
            return GetTabPathDictionary(portalId, Null.NullString);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has replaced in DotNetNuke 5.0 by GetTabsByPortal()")]
        public ArrayList GetTabs(int portalId)
        {
            return GetTabsByPortal(portalId).ToArrayList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete.  It has been replaced by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer) ")]
        public ArrayList GetTabsByParentId(int parentId)
        {
            return new ArrayList(GetTabsByParent(parentId, GetPortalId(parentId, Null.NullInteger)));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use one of the alternate MoveTabxxx methods)")]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.2. Replaced by RestoreTab(tabId, portalSettings)")]
        public static void RestoreTab(TabInfo tab, PortalSettings portalSettings, int userId)
        {
            Instance.RestoreTab(tab, portalSettings);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.5. Replaced by UpdateTab(updatedTab)")]
        public void UpdateTab(TabInfo updatedTab, string cultureCode)
        {
            updatedTab.CultureCode = cultureCode;
            UpdateTab(updatedTab);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.2.  Tab Ordering is handled in the DB ")]
        public void UpdateTabOrder(int portalID, int tabId, int tabOrder, int level, int parentId)
        {
            TabInfo objTab = GetTab(tabId, portalID, false);
            objTab.TabOrder = tabOrder;
            objTab.Level = level;
            objTab.ParentId = parentId;
            UpdateTabOrder(objTab);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.2.  Tab Ordering is handled in the DB ")]
        public void UpdateTabOrder(TabInfo objTab)
        {
            _dataProvider.UpdateTabOrder(objTab.TabID, objTab.TabOrder, objTab.ParentId,
                                    UserController.Instance.GetCurrentUserInfo().UserID);
            UpdateTabVersion(objTab.TabID);
            EventLogController.Instance.AddLog(objTab, PortalController.Instance.GetCurrentPortalSettings(),
                                      UserController.Instance.GetCurrentUserInfo().UserID, "",
                                      EventLogController.EventLogType.TAB_ORDER_UPDATED);
            ClearCache(objTab.PortalID);
        }
    }
}