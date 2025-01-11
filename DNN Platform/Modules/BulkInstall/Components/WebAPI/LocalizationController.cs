using System.Web.Http;
using DotNetNuke.BulkInstall.Components.WebAPI.ActionFilters;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    /// <summary>
    /// Provides REST APIs for localization.
    /// </summary>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class LocalizationController : DnnApiController
    {
        private readonly ILocalizationProvider localizationProvider;

        public LocalizationController(ILocalizationProvider localizationProvider)
        {
            this.localizationProvider = localizationProvider;
        }

        [HttpGet]
        public IHttpActionResult GetResources()
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
            var resourceFile = "~/DesktopModules/BulkInstall/App_LocalResources/BulkInstall.resx";
            var resources = localizationProvider.GetCompiledResourceFile(this.PortalSettings, resourceFile, culture);
            return Ok(resources);
        }
    }
}
