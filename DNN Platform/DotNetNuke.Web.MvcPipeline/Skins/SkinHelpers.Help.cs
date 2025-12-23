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
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Skin helper methods for rendering help links to host or portal contacts.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders a mailto link for requesting support, directing to either host or portal email.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="cssClass">Optional CSS class applied to the anchor.</param>
        /// <returns>An HTML string representing the help link, or empty if the user is not authenticated.</returns>
        public static IHtmlString Help(this HtmlHelper<PageModel> helper, string cssClass = "")
        {
            if (!helper.ViewContext.HttpContext.Request.IsAuthenticated)
            {
                return MvcHtmlString.Empty;
            }

            var portalSettings = PortalSettings.Current;
            var hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            
            var link = new TagBuilder("a");
            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }

            string email;
            if (TabPermissionController.CanAdminPage())
            {
                email = hostSettings.HostEmail;
            }
            else
            {
                email = portalSettings.Email;
            }

            link.Attributes.Add("href", "mailto:" + email + "?subject=" + portalSettings.PortalName + " Support Request");
            link.SetInnerText(Localization.GetString("Help"));

            return new MvcHtmlString(link.ToString());
        }
    }
}
