// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Localization
{
    using System;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    /// <summary>Sets up localization for all requests.</summary>
    public class LocalizationModule : IHttpModule
    {
        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // intentionally left empty
        }

        private static void Context_BeginRequest(object sender, EventArgs e)
        {
            var isInstallPage = HttpContext.Current.Request.Url.LocalPath.ToLowerInvariant().Contains("installwizard.aspx");
            if (isInstallPage)
            {
                return;
            }

            var portalSettings = PortalController.Instance.GetCurrentSettings();
            Localization.SetThreadCultures(Localization.GetPageLocale(portalSettings), portalSettings);
        }
    }
}
