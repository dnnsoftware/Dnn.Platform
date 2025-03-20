// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString JQuery(this HtmlHelper<PageModel> helper, bool dnnjQueryPlugins = false, bool jQueryHoverIntent = false, bool jQueryUI = false)
        {
            var script = new TagBuilder("script");
            script.Attributes.Add("src", "~/Resources/Shared/Scripts/jquery/jquery.js");
            script.Attributes.Add("type", "text/javascript");

            if (dnnjQueryPlugins)
            {
                script.InnerHtml += "<script src=\"~/Resources/Shared/Scripts/dnn.jquery.js\" type=\"text/javascript\"></script>";
            }

            if (jQueryHoverIntent)
            {
                script.InnerHtml += "<script src=\"~/Resources/Shared/Scripts/jquery/jquery.hoverIntent.js\" type=\"text/javascript\"></script>";
            }

            if (jQueryUI)
            {
                script.InnerHtml += "<script src=\"~/Resources/Shared/Scripts/jquery/jquery-ui.js\" type=\"text/javascript\"></script>";
            }

            return new MvcHtmlString(script.ToString());
        }
    }
}
