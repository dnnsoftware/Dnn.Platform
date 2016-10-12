#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Themes.Components;
using Dnn.PersonaBar.Themes.Components.DTO;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Themes.Services
{
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class ThemesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));
        private IThemesController _controller = Components.ThemesController.Instance;

        [HttpGet]
        public HttpResponseMessage GetThemes(ThemeLevel level)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    layouts = _controller.GetLayouts(PortalSettings, level),
                    containers = _controller.GetContainers(PortalSettings, level)
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApplyTheme(ApplyThemeInfo applyTheme)
        {
            try
            {
                _controller.ApplyTheme(PortalId, applyTheme.ThemeFile, applyTheme.Scope);
                return Request.CreateResponse(HttpStatusCode.OK, new {});
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteTheme(ThemeFileInfo themeFile)
        {
            try
            {
                _controller.DeleteTheme(PortalSettings, themeFile);
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteThemePackage(ThemeInfo theme)
        {
            try
            {
                _controller.DeleteThemePackage(PortalSettings, theme);
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSkin(UpdateThemeInfo updateTheme)
        {
            try
            {
                _controller.UpdateTheme(PortalSettings, updateTheme);
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ParseTheme(ThemeInfo theme, [FromUri] ParseType type)
        {
            try
            {
                _controller.ParseTheme(PortalSettings, theme, type);
                return Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
