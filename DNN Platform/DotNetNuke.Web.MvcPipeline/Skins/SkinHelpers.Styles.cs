// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.UI.Skins;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Styles(this HtmlHelper<PageModel> helper, string styleSheet, string condition = "", bool isFirst = false, bool useSkinPath = true, string media = "")
        {
            var skinPath = useSkinPath ? ((Skin)helper.ViewContext.Controller.ViewData["Skin"]).SkinPath : string.Empty;
            var link = new TagBuilder("link");
            link.Attributes.Add("rel", "stylesheet");
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("href", skinPath + styleSheet);
            if (!string.IsNullOrEmpty(media))
            {
                link.Attributes.Add("media", media);
            }

            if (string.IsNullOrEmpty(condition))
            {
                return new MvcHtmlString(link.ToString());
            }
            else
            {
                var openIf = new TagBuilder("span");
                openIf.InnerHtml = $"<!--[if {condition}]>";
                var closeIf = new TagBuilder("span");
                closeIf.InnerHtml = "<![endif]-->";
                return new MvcHtmlString(openIf.ToString() + link.ToString() + closeIf.ToString());
            }
        }
    }
}
