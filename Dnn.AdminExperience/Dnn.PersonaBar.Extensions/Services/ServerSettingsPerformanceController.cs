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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Client;

    using Microsoft.Extensions.DependencyInjection;

    using static System.Boolean;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsPerformanceController : PersonaBarApiController
    {
        private const string UseSSLKey = "UseSSLForCacheSync";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsPerformanceController));
        private readonly PerformanceController performanceController = new PerformanceController();
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="ServerSettingsPerformanceController"/> class.</summary>
        public ServerSettingsPerformanceController()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServerSettingsPerformanceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="appStatus">The application status.</param>
        public ServerSettingsPerformanceController(IHostSettings hostSettings, IHostSettingsService hostSettingsService, IApplicationStatusInfo appStatus)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        }

        /// GET: api/Servers/GetPerformanceSettings
        /// <summary>Gets performance settings.</summary>
        /// <returns>performance settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetPerformanceSettings()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                var perfSettings = new
                {
                    PortalSettings.Current.PortalName,

                    CachingProvider = this.performanceController.GetCachingProvider(),
                    PageStatePersistence = this.hostSettings.PageStatePersister,
                    ModuleCacheProvider = this.hostSettings.ModuleCachingMethod,
                    PageCacheProvider = this.hostSettings.PageCachingMethod,
                    CacheSetting = this.hostSettings.PerformanceSetting,
                    AuthCacheability = this.hostSettings.AuthenticatedCacheability,
                    UnauthCacheability = this.hostSettings.UnauthenticatedCacheability,
                    SslForCacheSynchronization = this.hostSettingsService.GetBoolean(UseSSLKey, false),
                    ClientResourcesManagementMode = PortalController.GetPortalSetting("ClientResourcesManagementMode", portalId, "h"),

                    CurrentHostVersion = this.hostSettings.CrmVersion.ToString(CultureInfo.InvariantCulture),
                    HostEnableCompositeFiles = this.hostSettings.CrmEnableCompositeFiles,
                    HostMinifyCss = this.hostSettings.CrmMinifyCss,
                    HostMinifyJs = this.hostSettings.CrmMinifyJs,
                    CurrentPortalVersion = GetPortalVersion(portalId),

                    // Options
                    CachingProviderOptions = this.performanceController.GetCachingProviderOptions(),
                    PageStatePersistenceOptions = this.performanceController.GetPageStatePersistenceOptions(),
                    ModuleCacheProviders = this.performanceController.GetModuleCacheProviders(),
                    PageCacheProviders = this.performanceController.GetPageCacheProviders(),
                    CacheSettingOptions = this.performanceController.GetCacheSettingOptions(),
                    AuthCacheabilityOptions = this.performanceController.GetCacheabilityOptions(),
                    UnauthCacheabilityOptions = this.performanceController.GetCacheabilityOptions(),
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
        /// <summary>Increment portal resources management version.</summary>
        /// <returns>A response indicating success.</returns>
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
        /// <summary>Increment host resources management version.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage IncrementHostVersion()
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                this.hostSettingsService.IncrementCrmVersion(false);
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
        /// <summary>Updates performance settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdatePerformanceSettings(UpdatePerfSettingsRequest request)
        {
            try
            {
                var portalId = PortalSettings.Current.PortalId;
                this.SaveCachingProvider(request.CachingProvider);
                this.hostSettingsService.Update("PageStatePersister", request.PageStatePersistence);
                this.hostSettingsService.Update("ModuleCaching", request.ModuleCacheProvider, false);
                if (this.performanceController.GetPageCacheProviders().Any())
                {
                    this.hostSettingsService.Update("PageCaching", request.PageCacheProvider, false);
                }

                this.hostSettingsService.Update("PerformanceSetting", request.CacheSetting, false);

                Enum.TryParse(request.CacheSetting, false, out PerformanceSettings perfSetting);
                this.hostSettings.PerformanceSetting = perfSetting;

                Enum.TryParse(request.AuthCacheability, false, out CacheControlHeader authCacheability);
                this.hostSettingsService.Update("AuthenticatedCacheability", authCacheability.ToString(), false);

                Enum.TryParse(request.UnauthCacheability, false, out CacheControlHeader unAuthCacheability);
                this.hostSettingsService.Update("UnauthenticatedCacheability", unAuthCacheability.ToString(), false);

                this.hostSettingsService.Update(UseSSLKey, request.SslForCacheSynchronization.ToString(), true);

                PortalController.UpdatePortalSetting(portalId, "ClientResourcesManagementMode", request.ClientResourcesManagementMode, false);

                if (request.ClientResourcesManagementMode == "h")
                {
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.OverrideDefaultSettingsKey, FalseString, false);
                }
                else
                {
                    PortalController.UpdatePortalSetting(portalId, ClientResourceSettings.OverrideDefaultSettingsKey, TrueString, false);
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

        private static int GetPortalVersion(int portalId)
        {
            var settingValue = PortalController.GetPortalSetting(ClientResourceSettings.VersionKey, portalId, "0");
            if (int.TryParse(settingValue, out var version))
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
                var xmlConfig = Config.Load(this.appStatus);

                var xmlCaching = xmlConfig.SelectSingleNode("configuration/dotnetnuke/caching");
                XmlUtils.UpdateAttribute(xmlCaching, "defaultProvider", cachingProvider);

                Config.Save(this.appStatus, xmlConfig);
            }
        }
    }
}
