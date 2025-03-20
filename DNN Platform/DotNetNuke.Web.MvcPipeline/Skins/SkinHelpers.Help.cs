﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Help(this HtmlHelper<PageModel> helper, string cssClass = "")
        {
            var portalSettings = PortalSettings.Current;
            var link = new TagBuilder("a");
            link.Attributes.Add("href", "mailto:" + portalSettings.Email + "?subject=" + portalSettings.PortalName + " Support Request");
            link.Attributes.Add("class", cssClass);
            link.SetInnerText("Help");

            return new MvcHtmlString(link.ToString());
        }
    }
}
