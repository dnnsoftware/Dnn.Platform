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

using System;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework;

namespace Dnn.PersonaBar.Pages.Components
{
    public class PageManagementController : ServiceLocator<IPageManagementController, PageManagementController>, IPageManagementController
    {
        public static string PageDateTimeFormat = "yyyy-MM-dd hh:mm tt";
        private readonly ITabController _tabController;

        public PageManagementController()
        {
            _tabController = TabController.Instance;
        }
        
        
        #region Public Methods

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
            _tabController.PopulateBreadCrumbs(ref tab);
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
        #endregion

        protected override Func<IPageManagementController> GetFactory()
        {
            return () => new PageManagementController();
        }
    }
}
