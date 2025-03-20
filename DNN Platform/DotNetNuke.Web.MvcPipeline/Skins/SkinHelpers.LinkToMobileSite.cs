// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mobile;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString LinkToMobileSite(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject")
        {
            var redirectionController = new RedirectionController();
            var redirectUrl = redirectionController.GetMobileSiteUrl();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var portalSettings = PortalSettings.Current;
                var link = new TagBuilder("a");

                link.Attributes.Add("href", portalSettings.PortalAlias.HTTPAlias);
                link.SetInnerText(Localization.GetString("lnkPortal.Text", GetSkinsResourceFile("LinkToMobileSite.ascx")));
                return new MvcHtmlString(link.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
        }
    }
}
