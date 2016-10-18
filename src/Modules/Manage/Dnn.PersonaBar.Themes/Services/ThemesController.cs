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
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Themes.Components;
using Dnn.PersonaBar.Themes.Components.DTO;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Themes.Services
{
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class ThemesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));
        private IThemesController _controller = Components.ThemesController.Instance;

        [HttpGet]
        public HttpResponseMessage GetCurrentTheme()
        {
            try
            {
                

                return Request.CreateResponse(HttpStatusCode.OK, GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetThemes(ThemeLevel level)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Layouts = _controller.GetLayouts(PortalSettings, level),
                    Containers = _controller.GetContainers(PortalSettings, level)
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName, ThemeType type, ThemeLevel level)
        {
            try
            {
                var theme = (type == ThemeType.Skin ? _controller.GetLayouts(PortalSettings, level)
                                                    : _controller.GetContainers(PortalSettings, level)
                            ).FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));

                if (theme == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
                }

                return Request.CreateResponse(HttpStatusCode.OK, _controller.GetThemeFiles(PortalSettings, theme));
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
                return Request.CreateResponse(HttpStatusCode.OK, GetCurrentThemeObject());
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

        [HttpGet]
        public HttpResponseMessage GetThemeFileTokens(string path)
        {
            try
            {
                var skinSrc = SkinController.FormatSkinSrc(path, PortalSettings);

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

        private object GetCurrentThemeObject()
        {
            var cultureCode = LocaleController.Instance.GetCurrentLocale(PortalId).Code;
            var siteLayout = PortalController.GetPortalSetting("DefaultPortalSkin", PortalId, Host.DefaultPortalSkin, cultureCode);
            var siteContainer = PortalController.GetPortalSetting("DefaultPortalContainer", PortalId, Host.DefaultPortalContainer, cultureCode);
            var editLayout = PortalController.GetPortalSetting("DefaultAdminSkin", PortalId, Host.DefaultAdminSkin, cultureCode);
            var editContainer = PortalController.GetPortalSetting("DefaultAdminContainer", PortalId, Host.DefaultAdminContainer, cultureCode);

            var currentTheme = new
            {
                SiteLayout = _controller.GetThemeFile(PortalSettings, siteLayout, ThemeType.Skin),
                SiteContainer = _controller.GetThemeFile(PortalSettings, siteContainer, ThemeType.Container),
                EditLayout = _controller.GetThemeFile(PortalSettings, editLayout, ThemeType.Skin),
                EditContainer = _controller.GetThemeFile(PortalSettings, editContainer, ThemeType.Container)

            };

            return currentTheme;
        }
    }
}
