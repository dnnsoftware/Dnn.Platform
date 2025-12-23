// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin helper methods for rendering DNN application branding.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders a link to the DNN application site using the legal copyright text.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="cssClass">Optional CSS class applied to the anchor.</param>
        /// <returns>An HTML string for the DNN copyright link, or empty when disabled.</returns>
        public static IHtmlString DotNetNuke(this HtmlHelper<PageModel> helper, string cssClass = "Normal")
        {
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            if (!hostSettingsService.GetBoolean("Copyright", true))
            {
                return MvcHtmlString.Empty;
            }

            var link = new TagBuilder("a");
            link.Attributes.Add("href", DotNetNukeContext.Current.Application.Url);
            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }

            link.SetInnerText(DotNetNukeContext.Current.Application.LegalCopyright);

            return new MvcHtmlString(link.ToString());
        }
    }
}
