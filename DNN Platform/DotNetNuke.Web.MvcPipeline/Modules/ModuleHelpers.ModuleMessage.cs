// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;

    public static partial class ModuleHelpers
    {
        /// <summary>
        /// Creates a DNN skin message panel with optional heading and automatic scrolling.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper instance.</param>
        /// <param name="message">The message text to display.</param>
        /// <param name="messageType">The type of message (affects CSS classes).</param>
        /// <param name="heading">Optional heading text.</param>
        /// <param name="autoScroll">Whether to automatically scroll to the message.</param>
        /// <param name="htmlAttributes">Additional HTML attributes for the panel.</param>
        /// <returns>HTML string for the skin message panel.</returns>
        public static IHtmlString ModuleMessage(
            this HtmlHelper htmlHelper,
            string message,
            ModuleMessageType messageType = ModuleMessageType.Info,
            string heading = null,
            bool autoScroll = true,
            object htmlAttributes = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                return MvcHtmlString.Empty;
            }

            var cssClass = GetMessageCssClass(messageType);
            
            // Create the main panel
            var panelBuilder = new TagBuilder("div");
            panelBuilder.GenerateId("dnnSkinMessage");
            panelBuilder.AddCssClass("dnnFormMessage");
            panelBuilder.AddCssClass(cssClass);

            // Merge additional HTML attributes
            if (htmlAttributes != null)
            {
                var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
                panelBuilder.MergeAttributes(attributes);
            }

            var innerHtml = string.Empty;

            // Add heading if provided
            if (!string.IsNullOrEmpty(heading))
            {
                var headingBuilder = new TagBuilder("h6");
                headingBuilder.SetInnerText(heading);
                innerHtml += headingBuilder.ToString();
            }

            // Add message
            var messageBuilder = new TagBuilder("div");
            messageBuilder.SetInnerText(message);
            innerHtml += messageBuilder.ToString();

            panelBuilder.InnerHtml = innerHtml;

            var result = panelBuilder.ToString();

            // Add auto-scroll script if requested
            if (autoScroll)
            {
                MvcClientAPI.RegisterScript("scrollScript", GenerateScrollScript());
            }

            return MvcHtmlString.Create(result);
        }

        /// <summary>
        /// Convenience method for creating an info message.
        /// </summary>
        public static IHtmlString ModuleInfoMessage(this HtmlHelper htmlHelper, string message, string heading = null, bool autoScroll = true)
        {
            return htmlHelper.ModuleMessage(message, ModuleMessageType.Info, heading, autoScroll);
        }

        /// <summary>
        /// Convenience method for creating a success message.
        /// </summary>
        public static IHtmlString ModuleSuccessMessage(this HtmlHelper htmlHelper, string message, string heading = null, bool autoScroll = true)
        {
            return htmlHelper.ModuleMessage(message, ModuleMessageType.Success, heading, autoScroll);
        }

        /// <summary>
        /// Convenience method for creating a warning message.
        /// </summary>
        public static IHtmlString ModuleWarningMessage(this HtmlHelper htmlHelper, string message, string heading = null, bool autoScroll = true)
        {
            return htmlHelper.ModuleMessage(message, ModuleMessageType.Warning, heading, autoScroll);
        }

        /// <summary>
        /// Convenience method for creating an error message.
        /// </summary>
        public static IHtmlString ModuleErrorMessage(this HtmlHelper htmlHelper, string message, string heading = null, bool autoScroll = true)
        {
            return htmlHelper.ModuleMessage(message, ModuleMessageType.Error, heading, autoScroll);
        }

        private static string GetMessageCssClass(ModuleMessageType messageType)
        {
            switch (messageType)
            {
                case ModuleMessageType.Success:
                    return "dnnFormSuccess";
                case ModuleMessageType.Warning:
                    return "dnnFormWarning";
                case ModuleMessageType.Error:
                    return "dnnFormError";
                case ModuleMessageType.Info:
                default:
                    return "dnnFormInfo";
            }
        }

        private static string GenerateScrollScript()
        {
            return $@"
                    jQuery(document).ready(function ($) {{
                        var $body = window.opera ? (document.compatMode == ""CSS1Compat"" ? $('html') : $('body')) : $('html,body');
                        var $message = $('.dnnFormMessage');
                        if ($message.length > 0) {{
                            var scrollTop = $message.offset().top - parseInt($(document.body).css(""margin-top""));
                            $body.animate({{ scrollTop: scrollTop }}, 'fast');
                        }}
                    }});";
        }
    }
}
