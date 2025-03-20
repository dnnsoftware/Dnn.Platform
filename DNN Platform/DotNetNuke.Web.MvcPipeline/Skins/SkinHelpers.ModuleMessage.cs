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
        public static IHtmlString ModuleMessage(this HtmlHelper<PageModel> helper, string heading = "", string message = "", string cssClass = "dnnModuleMessage", string headingCssClass = "dnnModMessageHeading")
        {
            var panel = new TagBuilder("div");
            panel.AddCssClass(cssClass);

            if (!string.IsNullOrEmpty(heading))
            {
                var headingLabel = new TagBuilder("span");
                headingLabel.AddCssClass(headingCssClass);
                headingLabel.SetInnerText(heading);
                panel.InnerHtml += headingLabel.ToString();
            }

            var messageLabel = new TagBuilder("span");
            messageLabel.SetInnerText(message);
            panel.InnerHtml += messageLabel.ToString();

            var script = new TagBuilder("script");
            script.Attributes.Add("type", "text/javascript");
            script.InnerHtml = @"
                jQuery(document).ready(function ($) {
                    var $body = window.opera ? (document.compatMode == 'CSS1Compat' ? $('html') : $('body')) : $('html,body');
                    var scrollTop = $('#" + panel.Attributes["id"] + @"').offset().top - parseInt($(document.body).css('margin-top'));
                    $body.animate({ scrollTop: scrollTop }, 'fast');
                });
            ";

            return new MvcHtmlString(panel.ToString() + script.ToString());
        }
    }
}
