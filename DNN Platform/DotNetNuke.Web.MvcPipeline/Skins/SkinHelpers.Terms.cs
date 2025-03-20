// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Terms(this HtmlHelper<PageModel> helper, string text = "", string cssClass = "SkinObject", string rel = "nofollow")
        {
            var navigationManager = helper.ViewData.Model.NavigationManager;
            var portalSettings = PortalSettings.Current;
            var link = new TagBuilder("a");

            // Add Css Class
            link.Attributes.Add("class", cssClass);

            // Add Text
            if (string.IsNullOrWhiteSpace(text))
            {
                text = Localization.GetString("Terms.Text", GetSkinsResourceFile("Terms.ascx"));
            }

            link.SetInnerText(text);

            // Add Link
            var href = portalSettings.TermsTabId == Null.NullInteger ? navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Terms") : navigationManager.NavigateURL(portalSettings.TermsTabId);
            link.Attributes.Add("href", href);

            // Add Rel
            if (!string.IsNullOrWhiteSpace(rel))
            {
                link.Attributes.Add("rel", rel);
            }

            return new MvcHtmlString(link.ToString());
        }
    }
}
