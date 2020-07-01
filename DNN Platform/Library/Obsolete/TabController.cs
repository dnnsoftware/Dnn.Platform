// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
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
        [Obsolete("Deprecated in DotNetNuke 7.3. RUse alternate overload. Scheduled removal in v10.0.0.")]
        public void CreateLocalizedCopy(List<TabInfo> tabs, Locale locale)
        {
            foreach (TabInfo t in tabs)
            {
                this.CreateLocalizedCopy(t, locale, true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 7.3. RUse alternate overload. Scheduled removal in v10.0.0.")]
        public void CreateLocalizedCopy(TabInfo originalTab, Locale locale)
        {
            this.CreateLocalizedCopy(originalTab, locale, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not scalable. Use GetTabsByPortal. Scheduled removal in v10.0.0.")]
        public ArrayList GetAllTabs()
        {
            return CBO.FillCollection(this._dataProvider.GetAllTabs(), typeof(TabInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not neccessary.  Use LINQ and GetPortalTabs(). Scheduled removal in v10.0.0.")]
        public List<TabInfo> GetCultureTabList(int portalid)
        {
            return (from kvp in this.GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && kvp.Value.CultureCode == PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Method is not neccessary.  Use LINQ and GetPortalTabs(). Scheduled removal in v10.0.0.")]
        public List<TabInfo> GetDefaultCultureTabList(int portalid)
        {
            return (from kvp in this.GetTabsByPortal(portalid)
                    where !kvp.Value.TabPath.StartsWith("//Admin")
                          && !kvp.Value.IsDeleted
                    select kvp.Value).ToList();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete.  It has been replaced by GetTab(ByVal TabId As Integer, ByVal PortalId As Integer, ByVal ignoreCache As Boolean) . Scheduled removal in v10.0.0.")]
        public TabInfo GetTab(int tabId)
        {
            return this.GetTab(tabId, GetPortalId(tabId, Null.NullInteger), false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use LINQ queries on tab collections thata re cached. Scheduled removal in v10.0.0.")]
        public TabInfo GetTabByUniqueID(Guid uniqueID)
        {
            return CBO.FillObject<TabInfo>(this._dataProvider.GetTabByUniqueID(uniqueID));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use GetTabsByPortal(portalId).Count. Scheduled removal in v10.0.0.")]
        public int GetTabCount(int portalId)
        {
            return this.GetTabsByPortal(portalId).Count;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is obsolete.  It has been replaced by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer) . Scheduled removal in v10.0.0.")]
        public ArrayList GetTabsByParentId(int parentId)
        {
            return new ArrayList(GetTabsByParent(parentId, GetPortalId(parentId, Null.NullInteger)));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Use one of the alternate MoveTabxxx methods). Scheduled removal in v10.0.0.")]
        public void MoveTab(TabInfo tab, TabMoveType type)
        {
            // Get the List of tabs with the same parent
            IOrderedEnumerable<TabInfo> siblingTabs = this.GetSiblingTabs(tab).OrderBy(t => t.TabOrder);
            int tabIndex = GetIndexOfTab(tab, siblingTabs);
            switch (type)
            {
                case TabMoveType.Top:
                    this.MoveTabBefore(tab, siblingTabs.First().TabID);
                    break;
                case TabMoveType.Bottom:
                    this.MoveTabAfter(tab, siblingTabs.Last().TabID);
                    break;
                case TabMoveType.Up:
                    this.MoveTabBefore(tab, siblingTabs.ElementAt(tabIndex - 1).TabID);
                    break;
                case TabMoveType.Down:
                    this.MoveTabAfter(tab, siblingTabs.ElementAt(tabIndex + 1).TabID);
                    break;
                case TabMoveType.Promote:
                    this.MoveTabAfter(tab, tab.ParentId);
                    break;
                case TabMoveType.Demote:
                    this.MoveTabToParent(tab, siblingTabs.ElementAt(tabIndex - 1).TabID);
                    break;
            }

            this.ClearCache(tab.PortalID);
        }
    }
}
