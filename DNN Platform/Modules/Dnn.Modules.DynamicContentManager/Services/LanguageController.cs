// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// LanguageController provides the Web Services to manage Languages
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class LanguageController : DnnApiController
    {
        /// <summary>
        /// GetEnabledLanguages retrieves the enabled languages for this portal
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetEnabledLanguages()
        {
            var locales = LocaleController.Instance.GetLocales(PortalSettings.PortalId).Values;
            var languages = locales
                                .Select(language => new { code = language.Code, language = language.NativeName })
                                .ToList();

            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            results = languages,
                                        }
                            };

            return Request.CreateResponse(response);
        }
    }
}