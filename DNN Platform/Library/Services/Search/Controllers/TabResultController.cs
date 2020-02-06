﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Linq;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    /// <summary>
    /// Search Result Controller for Tab Indexer
    /// </summary>
    /// <remarks></remarks>
    [Serializable]
    public class TabResultController : BaseResultController
    {
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        #region Abstract Class Implmentation

        public override bool HasViewPermission(SearchResult searchResult)
        {
            var viewable = true;

            if (searchResult.TabId > 0)
            {
                var tab = TabController.Instance.GetTab(searchResult.TabId, searchResult.PortalId, false);
                viewable = tab != null && !tab.IsDeleted && !tab.DisableLink && TabPermissionController.CanViewPage(tab);
            }

            return viewable;
        }
        
        public override string GetDocUrl(SearchResult searchResult)
        {
            var url = Localization.Localization.GetString("SEARCH_NoLink");

            var tab = TabController.Instance.GetTab(searchResult.TabId, searchResult.PortalId, false);
            if (TabPermissionController.CanViewPage(tab))
            {
                if (searchResult.PortalId != PortalSettings.Current.PortalId)
                {
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(searchResult.PortalId)
                                    .OrderByDescending(a => a.IsPrimary)
                                    .FirstOrDefault();

                    if (alias != null)
                    {
                        var portalSettings = new PortalSettings(searchResult.PortalId, alias);
                        url = TestableGlobals.Instance.NavigateURL(searchResult.TabId, portalSettings, string.Empty, searchResult.QueryString);
                    }
                }
                else
                {
                    url = TestableGlobals.Instance.NavigateURL(searchResult.TabId, string.Empty, searchResult.QueryString);
                }
            }
            
            return url;
        }

        public override string LocalizedSearchTypeName => Localization.Localization.GetString("Crawler_tab", LocalizedResxFile);

        #endregion
    }
}
