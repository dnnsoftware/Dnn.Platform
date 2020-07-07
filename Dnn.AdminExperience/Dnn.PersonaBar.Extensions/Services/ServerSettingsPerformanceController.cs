// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Components.PerformanceSettings;
    using Dnn.PersonaBar.Servers.Services.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Client;

    using static System.Boolean;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsPerformanceController : PersonaBarApiController
    {
        private const string UseSSLKey = "UseSSLForCacheSync";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsPerformanceController));
        private readonly PerformanceController _performanceController = new PerformanceController();

        /// GET: api/Servers/GetPerformanceSettings
        /// <summary>
        /// Gets performance settings.
        /// </summary>
        /// <param></param>
        /// <returns>performance settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetPerformanceSettings()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                var perfSettings = new
                {
                    PortalName = PortalSettings.Current.PortalName,

                    CachingProvider = this._performanceController.GetCachingProvider(),
                    PageStatePersistence = Host.PageStatePersister,
                    ModuleCacheProvider = Host.ModuleCachingMethod,
                    PageCacheProvider = Host.PageCachingMethod,
                    CacheSetting = Host.PerformanceSetting,
                    AuthCacheability = Host.AuthenticatedCacheability,
                    UnauthCacheability = Host.UnauthenticatedCacheability,
                    SslForCacheSynchronization = HostController.Instance.GetBoolean(UseSSLKey, false),
                    ClientResourcesManagementMode = PortalController.GetPortalSetting("ClientResourcesManagementMode", portalId, "h"),

                    CurrentHostVersion = Host.CrmVersion.ToString(CultureInfo.InvariantCulture),
                    HostEnableCompositeFiles = Host.CrmEnableCompositeFiles,
                    HostMinifyCss = Host.CrmMinifyCss,
                    HostMinifyJs = Host.CrmMinifyJs,
                    CurrentPortalVersion = this.GetPortalVersion(portalId),
                    PortalEnableCompositeFiles = Parse(PortalController.GetPortalSetting(ClientResourceSettings.EnableCompositeFilesKey, portalId, "false")),
                    PortalMinifyCss = Parse(PortalController.GetPortalSetting(ClientResourceSettings.MinifyCssKey, portalId, "false")),
                    PortalMinifyJs = Parse(PortalController.GetPortalSetting(ClientResourceSettings.MinifyJsKey, portalId, "false")),

                    // Options
                    CachingProviderOptions = this._performanceController.GetCachingProviderOptions(),
                    PageStatePersistenceOptions = this._performanceController.GetPageStatePersistenceOptions(),
                    ModuleCacheProviders = this._performanceController.GetModuleCacheProviders(),
                    PageCacheProviders = this._performanceController.GetPageCacheProviders(),
                    CacheSettingOptions = this._performanceController.GetCacheSettingOptions(),
                    AuthCacheabilityOptions = this._performanceController.GetCacheabilityOptions(),
                    UnauthCacheabilityOptions = this._performanceController.GetCacheabilityOptions()
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, perfSettings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Servers/IncrementPortalVersion
        /// <summary>
        /// Increment portal resources management version.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage IncrementPortalVersion()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                PortalController.IncrementCrmVersion(portalId);
                PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.OverrideDefaultSettingsKey, TrueString, false);
                PortalController.UpdatePortalSetting(portalId, "ClientResourcesManagementMode", "p", false);
                DataCache.ClearCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Servers/IncrementHostVersion
        /// <summary>
        /// Increment host resources management version.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage IncrementHostVersion()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                HostController.Instance.IncrementCrmVersion(false);
                PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.OverrideDefaultSettingsKey, FalseString, false);
                PortalController.UpdatePortalSetting(portalId, "ClientResourcesManagementMode", "h", false);
                DataCache.ClearCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Servers/UpdatePerformanceSettings
        /// <summary>
        /// Updates performance settings.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdatePerformanceSettings(UpdatePerfSettingsRequest request)
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                this.SaveCachingProvider(request.CachingProvider);
                HostController.Instance.Update("PageStatePersister", request.PageStatePersistence);
                HostController.Instance.Update("ModuleCaching", request.ModuleCacheProvider, false);
                if (this._performanceController.GetPageCacheProviders().Any())
                {
                    HostController.Instance.Update("PageCaching", request.PageCacheProvider, false);
                }
                HostController.Instance.Update("PerformanceSetting", request.CacheSetting, false);

                Globals.PerformanceSettings perfSetting;
                Enum.TryParse(request.CacheSetting, false, out perfSetting);
                Host.PerformanceSetting = perfSetting;

                HostController.Instance.Update("AuthenticatedCacheability", request.AuthCacheability, false);
                HostController.Instance.Update("UnauthenticatedCacheability", request.UnauthCacheability, false);

                HostController.Instance.Update(UseSSLKey, request.SslForCacheSynchronization.ToString(), true);

                PortalController.UpdatePortalSetting(portalId, "ClientResourcesManagementMode", request.ClientResourcesManagementMode, false);

                if (request.ClientResourcesManagementMode == "h")
                {
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.OverrideDefaultSettingsKey, FalseString, false);
                    HostController.Instance.Update(ClientResourceSettings.EnableCompositeFilesKey, request.HostEnableCompositeFiles.ToString(CultureInfo.InvariantCulture));
                    HostController.Instance.Update(ClientResourceSettings.MinifyCssKey, request.HostMinifyCss.ToString(CultureInfo.InvariantCulture));
                    HostController.Instance.Update(ClientResourceSettings.MinifyJsKey, request.HostMinifyJs.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.OverrideDefaultSettingsKey, TrueString, false);
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.EnableCompositeFilesKey, request.PortalEnableCompositeFiles.ToString(CultureInfo.InvariantCulture), false);
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.MinifyCssKey, request.PortalMinifyCss.ToString(CultureInfo.InvariantCulture), false);
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.MinifyJsKey, request.PortalMinifyJs.ToString(CultureInfo.InvariantCulture), false);
                }

                DataCache.ClearCache();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private int GetPortalVersion(int portalId)
        {
            var settingValue = PortalController.GetPortalSetting(ClientResourceSettings.VersionKey, portalId, "0");
            int version;
            if (int.TryParse(settingValue, out version))
            {
                if (version == 0)
                {
                    version = 1;
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.VersionKey, "1", true);
                }
            }

            return version;
        }

        private void SaveCachingProvider(string cachingProvider)
        {
            if (!string.IsNullOrEmpty(cachingProvider))
            {
                var xmlConfig = Config.Load();

                var xmlCaching = xmlConfig.SelectSingleNode("configuration/dotnetnuke/caching");
                XmlUtils.UpdateAttribute(xmlCaching, "defaultProvider", cachingProvider);

                Config.Save(xmlConfig);
            }
        }
    }
}
