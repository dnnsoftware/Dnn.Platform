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

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString LinkToTabletSite(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject")
        {
            var portalSettings = PortalSettings.Current;
            var link = new TagBuilder("a");

            link.Attributes.Add("href", portalSettings.PortalAlias.HTTPAlias);
            link.SetInnerText(Localization.GetString("lnkPortal.Text", GetSkinsResourceFile("LinkToTabletSite.ascx")));

            return new MvcHtmlString(link.ToString());
        }
    }
}
