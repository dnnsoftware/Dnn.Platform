// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin helper methods for rendering links to the host site.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders a link to the host site using host settings.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="cssClass">Optional CSS class applied to the anchor.</param>
        /// <returns>An HTML string for the host name link.</returns>
        public static IHtmlString HostName(this HtmlHelper<PageModel> helper, string cssClass = "")
        {
            var hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

            var link = new TagBuilder("a");
            link.Attributes.Add("href", Globals.AddHTTP(hostSettings.HostUrl));
            
            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }

            link.SetInnerText(hostSettings.HostTitle);

            return new MvcHtmlString(link.ToString());
        }
    }
}
