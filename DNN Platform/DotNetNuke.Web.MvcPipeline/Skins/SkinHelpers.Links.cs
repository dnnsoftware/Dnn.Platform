// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.UI;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Links(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string separator = " ", string level = "same", bool showDisabled = false, bool forceLinks = true, bool includeActiveTab = true)
        {
            var portalSettings = PortalSettings.Current;
            var links = new StringBuilder();

            var tabs = TabController.GetTabsBySortOrder(portalSettings.PortalId);
            foreach (var tab in tabs)
            {
                if (Navigation.CanShowTab(tab, false, showDisabled))
                {
                    if (level == "same" && tab.ParentId == portalSettings.ActiveTab.ParentId)
                    {
                        if (includeActiveTab || tab.TabID != portalSettings.ActiveTab.TabID)
                        {
                            links.Append($"<a class=\"{cssClass}\" href=\"{tab.FullUrl}\">{tab.TabName}</a>{separator}");
                        }
                    }
                    else if (level == "child" && tab.ParentId == portalSettings.ActiveTab.TabID)
                    {
                        links.Append($"<a class=\"{cssClass}\" href=\"{tab.FullUrl}\">{tab.TabName}</a>{separator}");
                    }
                    else if (level == "parent" && tab.TabID == portalSettings.ActiveTab.ParentId)
                    {
                        links.Append($"<a class=\"{cssClass}\" href=\"{tab.FullUrl}\">{tab.TabName}</a>{separator}");
                    }
                    else if (level == "root" && tab.Level == 0)
                    {
                        links.Append($"<a class=\"{cssClass}\" href=\"{tab.FullUrl}\">{tab.TabName}</a>{separator}");
                    }
                }
            }

            if (forceLinks && string.IsNullOrEmpty(links.ToString()))
            {
                foreach (var tab in tabs)
                {
                    if (Navigation.CanShowTab(tab, false, showDisabled))
                    {
                        links.Append($"<a class=\"{cssClass}\" href=\"{tab.FullUrl}\">{tab.TabName}</a>{separator}");
                    }
                }
            }

            return new MvcHtmlString(links.ToString().TrimEnd(separator.ToCharArray()));
        }
    }
}
