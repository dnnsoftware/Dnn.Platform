﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Text(this HtmlHelper<PageModel> helper, string showText = "", string cssClass = "", string resourceKey = "", bool replaceTokens = false)
        {
            var portalSettings = PortalSettings.Current;
            var text = showText;

            if (!string.IsNullOrEmpty(resourceKey))
            {
                var file = Path.GetFileName(helper.ViewContext.HttpContext.Server.MapPath(portalSettings.ActiveTab.SkinSrc));
                file = portalSettings.ActiveTab.SkinPath + Localization.LocalResourceDirectory + "/" + file;
                var localization = Localization.GetString(resourceKey, file);
                if (!string.IsNullOrEmpty(localization))
                {
                    text = localization;
                }
            }

            if (replaceTokens)
            {
                var tr = new TokenReplace { AccessingUser = portalSettings.UserInfo };
                text = tr.ReplaceEnvironmentTokens(text);
            }

            var label = new TagBuilder("span");
            label.SetInnerText(text);
            if (!string.IsNullOrEmpty(cssClass))
            {
                label.AddCssClass(cssClass);
            }

            return new MvcHtmlString(label.ToString());
        }
    }
}
