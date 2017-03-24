#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;

namespace Dnn.PersonaBar.SiteImportExport.Services
{
    [MenuPermission(Scope = ServiceScope.Admin)]
    public class SiteImportExportController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SiteImportExportController));
        private const string AuthFailureMessage = "Authorization has been denied for this request.";

        /// GET: api/SiteImportExport/GetPortalLocales
        /// <summary>
        /// Gets list of portal locales
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>List of portal locales</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalLocales(int portalId)
        {
            try
            {
                if (!UserInfo.IsSuperUser && PortalId != portalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var locales = LocaleController.Instance.GetLocales(portalId).Values;
                var response = new
                {
                    Success = true,
                    Results = locales.Select(l => new
                    {
                        l.Code,
                        l.EnglishName,
                        l.LanguageId
                    }),
                    PortalSettings.DefaultLanguage,
                    PortalSettings.ContentLocalizationEnabled,
                    TotalResults = locales.Count
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