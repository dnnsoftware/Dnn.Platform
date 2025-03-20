// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class SkinHelpers
    {
        public static IHtmlString BreadCrumb(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string separator = "<img alt=\"breadcrumb separator\" src=\"/images/breadcrumb.gif\">", int rootLevel = 0, bool useTitle = false, bool hideWithNoBreadCrumb = false, bool cleanerMarkup = false)
        {
            var portalSettings = PortalSettings.Current;
            var navigationManager = helper.ViewData.Model.NavigationManager;
            var breadcrumb = new StringBuilder("<span itemscope itemtype=\"http://schema.org/BreadcrumbList\">");
            var position = 1;
            var showRoot = rootLevel < 0;
            var homeUrl = string.Empty;
            var homeTabName = "Root";

            if (showRoot)
            {
                rootLevel = 0;
            }

            if (hideWithNoBreadCrumb && portalSettings.ActiveTab.BreadCrumbs.Count == (rootLevel + 1))
            {
                return MvcHtmlString.Empty;
            }

            if (showRoot && portalSettings.ActiveTab.TabID != portalSettings.HomeTabId)
            {
                homeUrl = Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias);

                if (portalSettings.HomeTabId != -1)
                {
                    homeUrl = navigationManager.NavigateURL(portalSettings.HomeTabId);

                    var tc = new TabController();
                    var homeTab = tc.GetTab(portalSettings.HomeTabId, portalSettings.PortalId, false);
                    homeTabName = homeTab.LocalizedTabName;

                    if (useTitle && !string.IsNullOrEmpty(homeTab.Title))
                    {
                        homeTabName = homeTab.Title;
                    }
                }

                breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                breadcrumb.Append("<a href=\"" + homeUrl + "\" class=\"" + cssClass + "\" itemprop=\"item\" ><span itemprop=\"name\">" + homeTabName + "</span></a>");
                breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />");
                breadcrumb.Append("</span>");
                breadcrumb.Append(separator);
            }

            for (var i = rootLevel; i < portalSettings.ActiveTab.BreadCrumbs.Count; ++i)
            {
                if (i > rootLevel)
                {
                    breadcrumb.Append(separator);
                }

                var tab = (TabInfo)portalSettings.ActiveTab.BreadCrumbs[i];
                var tabName = tab.LocalizedTabName;

                if (useTitle && !string.IsNullOrEmpty(tab.Title))
                {
                    tabName = tab.Title;
                }

                var tabUrl = tab.FullUrl;

                if (tab.DisableLink)
                {
                    if (cleanerMarkup)
                    {
                        breadcrumb.Append("<span class=\"" + cssClass + "\">" + tabName + "</span>");
                    }
                    else
                    {
                        breadcrumb.Append("<span><span class=\"" + cssClass + "\">" + tabName + "</span></span>");
                    }
                }
                else
                {
                    breadcrumb.Append("<span itemprop=\"itemListElement\" itemscope itemtype=\"http://schema.org/ListItem\">");
                    breadcrumb.Append("<a href=\"" + tabUrl + "\" class=\"" + cssClass + "\" itemprop=\"item\"><span itemprop=\"name\">" + tabName + "</span></a>");
                    breadcrumb.Append("<meta itemprop=\"position\" content=\"" + position++ + "\" />");
                    breadcrumb.Append("</span>");
                }
            }

            breadcrumb.Append("</span>");

            return new MvcHtmlString(breadcrumb.ToString());
        }
    }
}
