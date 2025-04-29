// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Localization;

/// <summary>TabController provides all operation to tabinfo.</summary>
/// <remarks>
/// Tab is equal to page in DotNetNuke.
/// Tabs will be a sitemap for a poatal, and every request at first need to check whether there is valid tab information
/// include in the url, if not it will use default tab to display information.
/// </remarks>
public partial class TabController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use alternate overload", RemovalVersion = 10)]
    public partial void CreateLocalizedCopy(List<TabInfo> tabs, Locale locale)
    {
        foreach (TabInfo t in tabs)
        {
            this.CreateLocalizedCopy(t, locale, true);
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use alternate overload", RemovalVersion = 10)]
    public partial void CreateLocalizedCopy(TabInfo originalTab, Locale locale)
    {
        this.CreateLocalizedCopy(originalTab, locale, true);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Method is not scalable. Use GetTabsByPortal", RemovalVersion = 10)]
    public partial ArrayList GetAllTabs()
    {
        return CBO.FillCollection(this.dataProvider.GetAllTabs(), typeof(TabInfo));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Method is not necessary. Use LINQ and GetPortalTabs()", RemovalVersion = 10)]
    public partial List<TabInfo> GetCultureTabList(int portalid)
    {
        return (from kvp in this.GetTabsByPortal(portalid)
            where !kvp.Value.TabPath.StartsWith("//Admin")
                  && kvp.Value.CultureCode == PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage
                  && !kvp.Value.IsDeleted
            select kvp.Value).ToList();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Method is not necessary. Use LINQ and GetPortalTabs()", RemovalVersion = 10)]
    public partial List<TabInfo> GetDefaultCultureTabList(int portalid)
    {
        return (from kvp in this.GetTabsByPortal(portalid)
            where !kvp.Value.TabPath.StartsWith("//Admin")
                  && !kvp.Value.IsDeleted
            select kvp.Value).ToList();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 0, 0, "Replaced by GetTab(int tabId, int portalId, bool ignoreCache)", RemovalVersion = 10)]
    public partial TabInfo GetTab(int tabId)
    {
        return this.GetTab(tabId, GetPortalId(tabId, Null.NullInteger), false);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use LINQ queries on tab collections that are cached", RemovalVersion = 10)]
    public partial TabInfo GetTabByUniqueID(Guid uniqueID)
    {
        return CBO.FillObject<TabInfo>(this.dataProvider.GetTabByUniqueID(uniqueID));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use GetTabsByPortal(portalId).Count", RemovalVersion = 10)]
    public partial int GetTabCount(int portalId)
    {
        return this.GetTabsByPortal(portalId).Count;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 0, 0, "Replaced by GetTabsByParent(int parentId, int portalId)", RemovalVersion = 10)]
    public partial ArrayList GetTabsByParentId(int parentId)
    {
        return new ArrayList(GetTabsByParent(parentId, GetPortalId(parentId, Null.NullInteger)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use one of the alternate MoveTabxxx methods", RemovalVersion = 10)]
    public partial void MoveTab(TabInfo tab, TabMoveType type)
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
