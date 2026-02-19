// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    /// <summary>Provides REST APIs for localization.</summary>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class LocalizationController(ILocalizationProvider localizationProvider) : DnnApiController
    {
        private readonly ILocalizationProvider localizationProvider = localizationProvider;

        /// <summary>Gets the localized resources for the current culture.</summary>
        /// <returns>A response wrapping a dictionary.</returns>
        [HttpGet]
        public IHttpActionResult GetResources()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            const string resourceFile = "~/DesktopModules/BulkInstall/App_LocalResources/BulkInstall.resx";
            var resources = this.localizationProvider.GetCompiledResourceFile(this.PortalSettings, resourceFile, culture);
            return this.Ok(resources);
        }
    }
}
