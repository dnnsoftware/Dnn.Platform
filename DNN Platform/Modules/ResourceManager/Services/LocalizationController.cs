using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using Dnn.Modules.ResourceManager.Components;

namespace Dnn.Modules.ResourceManager.Services
{
    [DnnAuthorize]
    [DnnExceptionFilter]
    public class LocalizationController : DnnApiController
    {
        private readonly ILocalizationController _localizationController;

        public LocalizationController()
        {
            _localizationController = Components.LocalizationController.Instance;
        }

        [HttpGet]
        [AllowAnonymous]
        [DnnPageEditor]
        public HttpResponseMessage GetResources(string culture)
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                _localizationController.GetLocalizedDictionary(Constants.ViewResourceFileName, culture, Constants.ResourceManagerLocalization));
        }
    }
}