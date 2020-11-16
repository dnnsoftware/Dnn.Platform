// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Framework;

    public class PageManagementController : ServiceLocator<IPageManagementController, PageManagementController>, IPageManagementController
    {
        public static string PageDateTimeFormat = "yyyy-MM-dd hh:mm tt";
        private readonly ITabController _tabController;

        public PageManagementController()
        {
            this._tabController = TabController.Instance;
        }

        public string GetCreatedInfo(TabInfo tab)
        {
            var createdBy = tab.CreatedByUser(PortalSettings.Current.PortalId);
            var displayName = Localization.GetString("System");
            if (createdBy != null)
            {
                displayName = createdBy.DisplayName;
            }

            return displayName;
        }

        public bool TabHasChildren(TabInfo tabInfo)
        {
            var children = TabController.GetTabsByParent(tabInfo.TabID, tabInfo.PortalID);
            return children != null && children.Count >= 1;
        }

        public string GetTabHierarchy(TabInfo tab)
        {
            this._tabController.PopulateBreadCrumbs(ref tab);
            return tab.BreadCrumbs.Count == 1 ? string.Empty : string.Join(" > ", from t in tab.BreadCrumbs.Cast<TabInfo>().Take(tab.BreadCrumbs.Count - 1) select t.LocalizedTabName);
        }

        public string GetTabUrl(TabInfo tab)
        {
            var url = string.Empty;

            if (tab.IsSuperTab || (Config.GetFriendlyUrlProvider() != "advanced"))
            {
                return url;
            }

            if (tab.TabUrls.Count > 0)
            {
                var tabUrl = tab.TabUrls.SingleOrDefault(t => t.IsSystem && t.HttpStatus == "200" && t.SeqNum == 0);

                if (tabUrl != null)
                {
                    url = tabUrl.Url;
                }
            }

            if (string.IsNullOrEmpty(url) && tab.TabID > -1 && !tab.IsSuperTab)
            {
                var friendlyUrlSettings = new FriendlyUrlSettings(PortalSettings.Current.PortalId);
                var baseUrl = Globals.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias) + "/Default.aspx?TabId=" + tab.TabID;
                var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                                                                            baseUrl,
                                                                            Globals.glbDefaultPage,
                                                                            PortalSettings.Current.PortalAlias.HTTPAlias,
                                                                            false, //dnndev-27493 :we want any custom Urls that apply
                                                                            friendlyUrlSettings,
                                                                            Guid.Empty);

                url = path.Replace(Globals.AddHTTP(PortalSettings.Current.PortalAlias.HTTPAlias), "");
            }

            return url;
        }

        protected override Func<IPageManagementController> GetFactory()
        {
            return () => new PageManagementController();
        }
    }
}
