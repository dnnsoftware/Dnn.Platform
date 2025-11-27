// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class SkinHelpers
    {
        private const string UrlRegex = "(href|src)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)";

        public static IHtmlString BreadCrumb(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string separator = "<img alt=\"breadcrumb separator\" src=\"/images/breadcrumb.gif\">", int rootLevel = 0, bool useTitle = false, bool hideWithNoBreadCrumb = false, bool cleanerMarkup = false)
        {
            var portalSettings = PortalSettings.Current;
            var navigationManager = helper.ViewData.Model.NavigationManager;
            var breadcrumb = new StringBuilder("<span itemscope itemtype=\"http://schema.org/BreadcrumbList\">");
            var position = 1;
            var showRoot = rootLevel < 0;
            var homeUrl = string.Empty;
            var homeTabName = "Root";

            // Resolve separator paths
            separator = ResolveSeparatorPaths(separator, portalSettings);

            // Get UserId and GroupId from request
            var request = helper.ViewContext.HttpContext.Request;
            int profileUserId = Null.NullInteger;
            if (!string.IsNullOrEmpty(request.Params["UserId"]))
            {
                int.TryParse(request.Params["UserId"], out profileUserId);
            }

            int groupId = Null.NullInteger;
            if (!string.IsNullOrEmpty(request.Params["GroupId"]))
            {
                int.TryParse(request.Params["GroupId"], out groupId);
            }

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

                if (profileUserId > -1)
                {
                    tabUrl = navigationManager.NavigateURL(tab.TabID, string.Empty, "UserId=" + profileUserId);
                }

                if (groupId > -1)
                {
                    tabUrl = navigationManager.NavigateURL(tab.TabID, string.Empty, "GroupId=" + groupId);
                }

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

            // Wrap in the outer span to match the original .ascx structure
            var outerHtml = new StringBuilder();
            outerHtml.Append("<span itemprop=\"breadcrumb\" itemscope itemtype=\"https://schema.org/breadcrumb\">");
            outerHtml.Append(breadcrumb.ToString());
            outerHtml.Append("</span>");

            return new MvcHtmlString(outerHtml.ToString());
        }

        private static string ResolveSeparatorPaths(string separator, PortalSettings portalSettings)
        {
            if (string.IsNullOrEmpty(separator))
            {
                return separator;
            }

            var urlMatches = Regex.Matches(separator, UrlRegex, RegexOptions.IgnoreCase);
            if (urlMatches.Count > 0)
            {
                foreach (Match match in urlMatches)
                {
                    var url = match.Groups[3].Value;
                    var changed = false;

                    if (url.StartsWith("/"))
                    {
                        if (!string.IsNullOrEmpty(Globals.ApplicationPath))
                        {
                            url = string.Format("{0}{1}", Globals.ApplicationPath, url);
                            changed = true;
                        }
                    }
                    else if (url.StartsWith("~/"))
                    {
                        url = Globals.ResolveUrl(url);
                        changed = true;
                    }
                    else
                    {
                        url = string.Format("{0}{1}", portalSettings.ActiveTab.SkinPath, url);
                        changed = true;
                    }

                    if (changed)
                    {
                        var newMatch = string.Format(
                            "{0}={1}{2}{3}",
                            match.Groups[1].Value,
                            match.Groups[2].Value,
                            url,
                            match.Groups[4].Value);

                        separator = separator.Replace(match.Value, newMatch);
                    }
                }
            }

            return separator;
        }
    }
}
