#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
        [Obsolete("Deprecated in DNN 7.3. Method is not scalable. Use GetTabsByPortal")]
        public ArrayList GetAllTabs()
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
        [Obsolete("This method is obsolete.  It has been replaced by GetTabsByParent(ByVal ParentId As Integer, ByVal PortalId As Integer) ")]
        public ArrayList GetTabsByParentId(int parentId)
        {
            return new ArrayList(GetTabsByParent(parentId, GetPortalId(parentId, Null.NullInteger)));
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
    }
}