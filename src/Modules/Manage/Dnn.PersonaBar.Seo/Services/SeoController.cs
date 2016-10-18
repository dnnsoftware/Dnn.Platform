#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.Seo.Services
{
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class SeoController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SeoController));
        private readonly Components.SeoController _controller = new Components.SeoController();
        private static readonly string LocalResourcesFile = Path.Combine("~/admin/Dnn.PersonaBar/App_LocalResources/Seo.resx");

        /// GET: api/SEO/GetGeneralSettings
        /// <summary>
        /// Gets general SEO settings
        /// </summary>
        /// <returns>General SEO settings</returns>
        [HttpGet]
        public HttpResponseMessage GetGeneralSettings()
        {
            try
            {
                var urlSettings = new FriendlyUrlSettings(PortalId);

                var replacementCharacterList = new List<KeyValuePair<string, string>>();
                replacementCharacterList.Add(new KeyValuePair<string, string>(Localization.GetString("minusCharacter", LocalResourcesFile), "-"));
                replacementCharacterList.Add(new KeyValuePair<string, string>(Localization.GetString("underscoreCharacter", LocalResourcesFile), "_"));

                var deletedPageHandlingTypes = new List<KeyValuePair<string, string>>();
                deletedPageHandlingTypes.Add(new KeyValuePair<string, string>(Localization.GetString("Do404Error", LocalResourcesFile), "Do404Error"));
                deletedPageHandlingTypes.Add(new KeyValuePair<string, string>(Localization.GetString("Do301RedirectToPortalHome", LocalResourcesFile), "Do301RedirectToPortalHome"));

                var response = new
                {
                    Success = true,
                    Settings = new
                    {
                        EnableSystemGeneratedUrls = urlSettings.ReplaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing,
                        urlSettings.ReplaceSpaceWith,
                        urlSettings.ForceLowerCase,
                        urlSettings.AutoAsciiConvert,
                        urlSettings.ForcePortalDefaultLanguage,
                        DeletedTabHandlingType = urlSettings.DeletedTabHandlingType.ToString(),
                        urlSettings.RedirectUnfriendly,
                        urlSettings.RedirectWrongCase
                    },
                    ReplacementCharacterList = replacementCharacterList,
                    DeletedPageHandlingTypes = deletedPageHandlingTypes
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
