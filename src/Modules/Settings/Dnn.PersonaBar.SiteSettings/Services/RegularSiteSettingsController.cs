#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.UI.Internals;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Api;
using FileInfo = System.IO.FileInfo;

namespace Dnn.PersonaBar.SiteSettings.Services
{
    [MenuPermission(MenuName = "Dnn.SiteSettings")]
    public class RegularSiteSettingsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RegularSiteSettingsController));
        private const string LocalResourcesFile = "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.SiteSettings/App_LocalResources/SiteSettings.resx";
        private const string AuthFailureMessage = "Authorization has been denied for this request.";

        #region Site Info API

        /// GET: api/SiteSettings/GetPortalSettings
        /// <summary>
        /// Gets site settings
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns>site settings</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalSettings(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : cultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        string.Format(Localization.GetString("InvalidLocale.ErrorMessage", LocalResourcesFile), cultureCode));
                }

                var portal = PortalController.Instance.GetPortal(pid, cultureCode);
                var portalSettings = new PortalSettings(portal);
                var logoFile = string.IsNullOrEmpty(portal.LogoFile) ? null : FileManager.Instance.GetFile(pid, portal.LogoFile);
                var favIcon = string.IsNullOrEmpty(new FavIcon(portal.PortalID).GetSettingPath()) ? null : FileManager.Instance.GetFile(pid, new FavIcon(portal.PortalID).GetSettingPath());

                var settings = new
                {
                    PortalId = portal.PortalID,
                    portal.CultureCode,
                    portal.PortalName,
                    portal.Description,
                    portal.KeyWords,
                    GUID = portal.GUID.ToString().ToUpper(),
                    portal.FooterText,
                    TimeZone = portalSettings.TimeZone.Id,
                    portal.HomeDirectory,
                    LogoFile = logoFile != null ? new FileDto()
                    {
                        fileName = logoFile.FileName,
                        folderPath = logoFile.Folder,
                        fileId = logoFile.FileId,
                        folderId = logoFile.FolderId
                    } : null,
                    FavIcon = favIcon != null ? new FileDto()
                    {
                        fileName = favIcon.FileName,
                        folderPath = favIcon.Folder,
                        fileId = favIcon.FileId,
                        folderId = favIcon.FolderId
                    } : null,
                    IconSet = PortalController.GetPortalSetting("DefaultIconLocation", pid, "Sigma", cultureCode).Replace("icons/", "")
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

        /// GET: api/SiteSettings/GetCultureList
        /// <summary>
        /// Gets culture list
        /// </summary>
        /// <param name="portalId"></param>
        /// <returns>Culture List</returns>
        [HttpGet]
        public HttpResponseMessage GetCultureList(int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);

                string viewType = GetLanguageDisplayMode(pid);

                var locals = LocaleController.Instance.GetLocales(pid).Values;
                var cultureCodeList = locals.Select(local => new
                {
                    Name = viewType == "NATIVE" ? local.NativeName : local.EnglishName,
                    local.Code,
                    Icon = Globals.ResolveUrl(string.IsNullOrEmpty(local.Code) ? "~/images/Flags/none.gif" :
                        $"~/images/Flags/{local.Code}.gif"),
                    IsDefault = local.Code == portalSettings.DefaultLanguage
                }).ToList();

                var response = new
                {
                    Success = true,
                    Cultures = cultureCodeList
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Private Methods

        private string GetLanguageDisplayMode(int portalId)
        {
            string viewTypePersonalizationKey = "LanguageDisplayMode:ViewType" + portalId;
            var personalizationController = new PersonalizationController();
            var personalization = personalizationController.LoadProfile(UserInfo.UserID, portalId);

            string viewType = Convert.ToString(personalization.Profile[viewTypePersonalizationKey]);
            return string.IsNullOrEmpty(viewType) ? "NATIVE" : viewType;
        }

        #endregion
    }
}
