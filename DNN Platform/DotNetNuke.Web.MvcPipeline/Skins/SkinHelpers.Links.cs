// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.UI;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        private static readonly Regex SrcRegex = new Regex("src=[']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IHtmlString Links(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string separator = " ", string level = "same", string alignment = "", bool showDisabled = false, bool forceLinks = true, bool includeActiveTab = true)
        {
            var portalSettings = PortalSettings.Current;
            
            // Separator processing
            if (!string.IsNullOrEmpty(separator) && separator != " ")
            {
                if (separator.IndexOf("src=", StringComparison.Ordinal) != -1)
                {
                    separator = SrcRegex.Replace(separator, "$&" + portalSettings.ActiveTab.SkinPath);
                }
                separator = string.Format("<span class=\"{0}\">{1}</span>", cssClass, separator);
            }
            else
            {
                separator = " ";
            }

            string strLinks = BuildLinks(portalSettings, level, separator, cssClass, alignment, showDisabled, includeActiveTab);

            if (string.IsNullOrEmpty(strLinks) && forceLinks)
            {
                strLinks = BuildLinks(portalSettings, string.Empty, separator, cssClass, alignment, showDisabled, includeActiveTab);
            }

            return new MvcHtmlString(strLinks);
        }

        private static string BuildLinks(PortalSettings portalSettings, string level, string separator, string cssClass, string alignment, bool showDisabled, bool includeActiveTab)
        {
            var sbLinks = new StringBuilder();
            var portalTabs = TabController.GetTabsBySortOrder(portalSettings.PortalId);
            var hostTabs = TabController.GetTabsBySortOrder(Null.NullInteger);

            foreach (TabInfo objTab in portalTabs)
            {
                sbLinks.Append(ProcessLink(ProcessTab(objTab, portalSettings, level, cssClass, includeActiveTab, showDisabled), sbLinks.Length, separator, alignment));
            }

            foreach (TabInfo objTab in hostTabs)
            {
                sbLinks.Append(ProcessLink(ProcessTab(objTab, portalSettings, level, cssClass, includeActiveTab, showDisabled), sbLinks.Length, separator, alignment));
            }

            return sbLinks.ToString();
        }

        private static string ProcessTab(TabInfo objTab, PortalSettings portalSettings, string level, string cssClass, bool includeActiveTab, bool showDisabled)
        {
            if (Navigation.CanShowTab(objTab, false, showDisabled)) // Assuming AdminMode is false for now as it wasn't passed, or check permissions
            {
                switch (level)
                {
                    case "same":
                    case "":
                        if (objTab.ParentId == portalSettings.ActiveTab.ParentId)
                        {
                            if (includeActiveTab || objTab.TabID != portalSettings.ActiveTab.TabID)
                            {
                                return AddLink(objTab.TabName, objTab.FullUrl, cssClass);
                            }
                        }
                        break;
                    case "child":
                        if (objTab.ParentId == portalSettings.ActiveTab.TabID)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, cssClass);
                        }
                        break;
                    case "parent":
                        if (objTab.TabID == portalSettings.ActiveTab.ParentId)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, cssClass);
                        }
                        break;
                    case "root":
                        if (objTab.Level == 0)
                        {
                            return AddLink(objTab.TabName, objTab.FullUrl, cssClass);
                        }
                        break;
                }
            }
            return string.Empty;
        }

        private static string ProcessLink(string sLink, int currentLength, string separator, string alignment)
        {
            if (string.IsNullOrEmpty(sLink))
            {
                return string.Empty;
            }

            if (alignment == "vertical")
            {
                return string.Concat("<div>", separator, sLink, "</div>");
            }
            else if (!string.IsNullOrEmpty(separator) && currentLength > 0)
            {
                return string.Concat(separator, sLink);
            }

            return sLink;
        }

        private static string AddLink(string strTabName, string strURL, string strCssClass)
        {
            return string.Format("<a class=\"{0}\" href=\"{1}\">{2}</a>", strCssClass, strURL, strTabName);
        }
    }
}
