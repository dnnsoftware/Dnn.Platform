// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Xml;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Themes.Components;
    using Dnn.PersonaBar.Themes.Components.DTO;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class ThemesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));
        private IThemesController _controller = Components.ThemesController.Instance;

        [HttpGet]
        public HttpResponseMessage GetCurrentTheme(string language)
        {
            try
            {

                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetThemes(ThemeLevel level)
        {
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Layouts = this._controller.GetLayouts(this.PortalSettings, level),
                    Containers = this._controller.GetContainers(this.PortalSettings, level)
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName, ThemeType type, ThemeLevel level)
        {
            try
            {
                var theme = (type == ThemeType.Skin ? this._controller.GetLayouts(this.PortalSettings, level)
                                                    : this._controller.GetContainers(this.PortalSettings, level)
                            ).FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase));

                if (theme == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, this._controller.GetThemeFiles(this.PortalSettings, theme));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage ApplyTheme(ApplyThemeInfo applyTheme, string language)
        {
            try
            {
                this._controller.ApplyTheme(this.PortalId, applyTheme.ThemeFile, applyTheme.Scope);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage ApplyDefaultTheme(ApplyDefaultThemeInfo defaultTheme, string language)
        {
            try
            {
                var themeInfo = this._controller.GetLayouts(this.PortalSettings, ThemeLevel.All)
                                    .FirstOrDefault(
                                        t => t.PackageName.Equals(defaultTheme.ThemeName, StringComparison.OrdinalIgnoreCase)
                                                && t.Level == defaultTheme.Level);

                if (themeInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
                }

                var themeFiles = this._controller.GetThemeFiles(this.PortalSettings, themeInfo);
                if (themeFiles.Count == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "NoThemeFile");
                }

                this._controller.ApplyDefaultTheme(this.PortalSettings, defaultTheme.ThemeName, defaultTheme.Level);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage DeleteThemePackage(ThemeInfo theme)
        {
            try
            {
                if (theme.Level == ThemeLevel.Global && !this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "NoPermission");
                }

                this._controller.DeleteThemePackage(this.PortalSettings, theme);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (InvalidOperationException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetEditableTokens()
        {
            try
            {
                var tokens = SkinControlController.GetSkinControls().Values
                    .Where(c => !string.IsNullOrEmpty(c.ControlKey) && !string.IsNullOrEmpty(c.ControlSrc))
                    .Select(c => new ListItemInfo { Text = c.ControlKey, Value = c.ControlSrc });

                return this.Request.CreateResponse(HttpStatusCode.OK, tokens);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEditableSettings(string token)
        {
            try
            {
                var strFile = Globals.ApplicationMapPath + "\\" + token.ToLowerInvariant().Replace("/", "\\").Replace(".ascx", ".xml");
                var settings = new List<ListItemInfo>();
                if (File.Exists(strFile))
                {
                    var xmlDoc = new XmlDocument { XmlResolver = null };
                    xmlDoc.Load(strFile);
                    foreach (XmlNode xmlSetting in xmlDoc.SelectNodes("//Settings/Setting"))
                    {
                        settings.Add(new ListItemInfo(xmlSetting.SelectSingleNode("Name").InnerText, xmlSetting.SelectSingleNode("Name").InnerText));
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, settings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEditableValues(string token, string setting)
        {
            try
            {
                var strFile = Globals.ApplicationMapPath + "\\" + token.ToLowerInvariant().Replace("/", "\\").Replace(".ascx", ".xml");
                var value = string.Empty;
                if (File.Exists(strFile))
                {
                    var xmlDoc = new XmlDocument { XmlResolver = null };
                    xmlDoc.Load(strFile);
                    foreach (XmlNode xmlSetting in xmlDoc.SelectNodes("//Settings/Setting"))
                    {
                        if (xmlSetting.SelectSingleNode("Name").InnerText == setting)
                        {
                            string strValue = xmlSetting.SelectSingleNode("Value").InnerText;
                            switch (strValue)
                            {
                                case "":
                                    break;
                                case "[TABID]":
                                    value = "Pages";
                                    break;
                                default:
                                    value = strValue;
                                    break;
                            }
                        }
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Value = value });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage UpdateTheme(UpdateThemeInfo updateTheme)
        {
            try
            {
                var token = SkinControlController.GetSkinControls().Values.FirstOrDefault(t => t.ControlSrc == updateTheme.Token);
                if (token == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidParameter");
                }

                var themeFilePath = updateTheme.Path.ToLowerInvariant();
                if ((!themeFilePath.StartsWith("[g]") && !themeFilePath.StartsWith("[l]") && !themeFilePath.StartsWith("[s]"))
                    || (themeFilePath.StartsWith("[g]") && !this.UserInfo.IsSuperUser))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPermission");
                }

                updateTheme.Token = token.ControlKey;
                this._controller.UpdateTheme(this.PortalSettings, updateTheme);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage ParseTheme(ParseThemeInfo parseTheme)
        {
            try
            {
                var themeName = parseTheme.ThemeName;

                var layout = this._controller.GetLayouts(this.PortalSettings, ThemeLevel.All)
                                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == parseTheme.Level);

                if (layout != null)
                {
                    this._controller.ParseTheme(this.PortalSettings, layout, parseTheme.ParseType);
                }

                var container = this._controller.GetContainers(this.PortalSettings, ThemeLevel.All)
                                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == parseTheme.Level);

                if (container != null)
                {
                    this._controller.ParseTheme(this.PortalSettings, container, parseTheme.ParseType);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage RestoreTheme(string language)
        {
            try
            {
                SkinController.SetSkin(SkinController.RootSkin, this.PortalId, SkinType.Portal, "");
                SkinController.SetSkin(SkinController.RootContainer, this.PortalId, SkinType.Portal, "");
                SkinController.SetSkin(SkinController.RootSkin, this.PortalId, SkinType.Admin, "");
                SkinController.SetSkin(SkinController.RootContainer, this.PortalId, SkinType.Admin, "");
                DataCache.ClearPortalCache(this.PortalId, true);

                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private object GetCurrentThemeObject()
        {
            var cultureCode = LocaleController.Instance.GetCurrentLocale(this.PortalId).Code;
            var siteLayout = PortalController.GetPortalSetting("DefaultPortalSkin", this.PortalId, Host.DefaultPortalSkin, cultureCode);
            var siteContainer = PortalController.GetPortalSetting("DefaultPortalContainer", this.PortalId, Host.DefaultPortalContainer, cultureCode);
            var editLayout = PortalController.GetPortalSetting("DefaultAdminSkin", this.PortalId, Host.DefaultAdminSkin, cultureCode);
            var editContainer = PortalController.GetPortalSetting("DefaultAdminContainer", this.PortalId, Host.DefaultAdminContainer, cultureCode);

            var currentTheme = new
            {
                SiteLayout = this._controller.GetThemeFile(this.PortalSettings, siteLayout, ThemeType.Skin),
                SiteContainer = this._controller.GetThemeFile(this.PortalSettings, siteContainer, ThemeType.Container),
                EditLayout = this._controller.GetThemeFile(this.PortalSettings, editLayout, ThemeType.Skin),
                EditContainer = this._controller.GetThemeFile(this.PortalSettings, editContainer, ThemeType.Container)

            };

            return currentTheme;
        }
    }
}
