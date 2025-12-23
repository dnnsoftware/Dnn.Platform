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

    /// <summary>
    /// Skin helper methods for rendering links to the portal privacy page.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders a link to the portal privacy page, using a localized default label when none is provided.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="text">Optional link text; when empty, a localized default is used.</param>
        /// <param name="cssClass">Optional CSS class applied to the anchor.</param>
        /// <param name="rel">Optional <c>rel</c> attribute value (for example, <c>nofollow</c>).</param>
        /// <returns>An HTML string for the privacy link.</returns>
        public static IHtmlString Privacy(this HtmlHelper<PageModel> helper, string text = "", string cssClass = "SkinObject", string rel = "nofollow")
        {
            var navigationManager = helper.ViewData.Model.NavigationManager;
            var portalSettings = PortalSettings.Current;
            var link = new TagBuilder("a");

             // Add Css Class
            link.Attributes.Add("class", cssClass);

            // Add Text
            if (string.IsNullOrWhiteSpace(text))
            {
                text = Localization.GetString("Privacy.Text", GetSkinsResourceFile("Privacy.ascx"));
            }

            link.SetInnerText(text);

            // Add Link
            var href = portalSettings.PrivacyTabId == Null.NullInteger ? navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, "Privacy") : navigationManager.NavigateURL(portalSettings.PrivacyTabId);
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
