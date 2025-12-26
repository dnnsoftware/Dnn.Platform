// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Pages
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.Models;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using DotNetNuke.Web.MvcPipeline.Utils;

    /// <summary>
    /// HTML helper extensions for rendering MVC pipeline page-level elements.
    /// </summary>
    public static partial class HtmlHelpers
    {
        /// <summary>
        /// Renders all head tags (scripts, styles, meta tags) registered for the current page.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <returns>An HTML string containing the rendered head tags.</returns>
        public static IHtmlString RenderHeadTags(this HtmlHelper<PageModel> helper)
        {
            var pageService = helper.ViewData.Model.PageService;
            var headTags = new StringBuilder();
            foreach (var item in pageService.GetHeadTags())
            {
                headTags.Append(item.Value);
            }

            foreach (var item in pageService.GetMetaTags())
            {
                var tag = new TagBuilder("meta");
                tag.Attributes.Add("name", item.Name);
                tag.Attributes.Add("content", item.Content);
                headTags.Append(tag.ToString());
            }

            return new MvcHtmlString(headTags.ToString());
        }

        /// <summary>
        /// Renders page-level messages (errors, warnings, info, success) using DNN message styles.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <returns>An HTML string containing the rendered messages, or an empty string if there are no messages.</returns>
        public static IHtmlString RenderPageMessages(this HtmlHelper<PageModel> helper)
        {
            var pageService = helper.ViewData.Model.PageService;
            var messages = pageService.GetMessages();
            if (messages.Any())
            {
                var outer = new TagBuilder("div");
                outer.Attributes["id"] = "dnnSkinMessage";

                foreach (var msg in pageService.GetMessages())
                {
                    var wrapper = new TagBuilder("div");
                    var panel = new TagBuilder("div");
                    switch (msg.MessageType)
                    {
                        case PageMessageType.Error:
                            panel.AddCssClass("dnnFormError");
                            break;
                        case PageMessageType.Warning:
                            panel.AddCssClass("dnnFormWarning");
                            break;
                        case PageMessageType.Success:
                            panel.AddCssClass("dnnFormSuccess");
                            break;
                        case PageMessageType.Info:
                            panel.AddCssClass("dnnFormInfo");
                            break;
                    }

                    panel.AddCssClass("dnnFormMessage");
                    if (!string.IsNullOrEmpty(msg.Heading))
                    {
                        var headingSpan = new TagBuilder("span");
                        headingSpan.SetInnerText(msg.Heading);
                        headingSpan.AddCssClass("dnnModMessageHeading");
                        panel.InnerHtml += headingSpan.ToString();
                    }

                    var messageDiv = new TagBuilder("span");
                    messageDiv.InnerHtml = msg.Message;
                    panel.InnerHtml += messageDiv.ToString();

                    wrapper.InnerHtml = panel.ToString();
                    outer.InnerHtml += wrapper.ToString();
                }

                var script =
                    "jQuery(document).ready(function ($) {" +
                    "var $body = window.opera ? (document.compatMode == \"CSS1Compat\" ? $('html') : $('body')) : $('html,body');" +
                    "var scrollTop = $('#dnnSkinMessage').offset().top - parseInt($(document.body).css(\"margin-top\"));" +
                    "$body.animate({ scrollTop: scrollTop }, 'fast');" +
                    "});";

                MvcClientAPI.RegisterScript("dnnSkinMessage", script);

                return new MvcHtmlString(outer.ToString());
            }
            else
            {
                return new MvcHtmlString(string.Empty);
            }
        }
    }
}
