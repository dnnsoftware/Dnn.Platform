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
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.SiteInfo.Services.Dto;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Internals;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.SiteInfo.Services
{
    [ServiceScope(Scope = ServiceScope.Admin, Identifier = "SiteInfo")]
    public class SiteInfoController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SiteInfoController));

        /// GET: api/SiteInfo/GetPortalSettings
        /// <summary>
        /// Gets site settings
        /// </summary>
        /// <param></param>
        /// <returns>site settings</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalSettings()
        {
            try
            {
                var portal = PortalController.Instance.GetPortal(PortalId);
                var cultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
                var settings = new
                {
                    portal.PortalName,
                    portal.Description,
                    portal.KeyWords,
                    GUID = portal.GUID.ToString().ToUpper(),
                    portal.FooterText,
                    TimeZone = PortalSettings.TimeZone.Id,
                    portal.HomeDirectory,
                    portal.LogoFile,
                    FavIcon = new FavIcon(portal.PortalID).GetSettingPath(),
                    IconSet = PortalController.GetPortalSetting("DefaultIconLocation", PortalId, "Sigma", cultureCode).Replace("icons/", "")
                };
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = settings,
                    TimeZones = TimeZoneInfo.GetSystemTimeZones().Select(z => new
                    {
                        z.Id,
                        z.DisplayName
                    }),
                    IconSets = IconController.GetIconSets()
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteInfo/UpdatePortalSettings
        /// <summary>
        /// Updates site settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdatePortalSettings(UpdateSiteSettingsRequest request)
        {
            try
            {
                var cultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
                var portal = new PortalInfo
                {
                    PortalID = PortalId,
                    PortalName = request.PortalName,
                    LogoFile = request.LogoFile,
                    FooterText = request.FooterText,
                    Description = request.Description,
                    KeyWords = request.KeyWords
                };

                PortalController.Instance.UpdatePortalInfo(portal);
                PortalController.UpdatePortalSetting(PortalId, "TimeZone", request.TimeZone, false);
                new FavIcon(PortalId).Update(request.FavIcon);
                PortalController.UpdatePortalSetting(PortalId, "DefaultIconLocation", "icons/" + request.IconSet, false, cultureCode);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
