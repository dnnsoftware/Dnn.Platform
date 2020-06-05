// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Globalization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.Authentication.OAuth
{

    /// <summary>
    /// The Config class provides a central area for management of Module Configuration Settings.
    /// </summary>
    [Serializable]
    public class OAuthConfigBase : AuthenticationConfigBase
    {
        private const string _cacheKey = "Authentication";

        protected OAuthConfigBase(string service, int portalId)
            : base(portalId)
        {
            this.Service = service;
            
            var portalApiKey = PortalController.GetPortalSetting(this.Service + "_APIKey", portalId, "");
            var hostApiKey = "";

            if (string.IsNullOrEmpty(portalApiKey))
            {
                hostApiKey = HostController.Instance.GetString(this.Service + "_APIKey", "");
                this.HostConfig = !string.IsNullOrEmpty(hostApiKey);
            }
            else
            {
                this.HostConfig = false;
            }

            if (this.HostConfig)
            {
                this.APIKey = hostApiKey;
                this.APISecret = HostController.Instance.GetString(this.Service + "_APISecret", "");
                this.Enabled = HostController.Instance.GetBoolean(this.Service + "_Enabled", false);
            }
            else
            {
                this.APIKey = portalApiKey;
                this.APISecret = PortalController.GetPortalSetting(this.Service + "_APISecret", portalId, "");
                this.Enabled = PortalController.GetPortalSettingAsBoolean(this.Service + "_Enabled", portalId, false);
            }
        }

        protected string Service { get; set; }

        public string APIKey { get; set; }

        public string APISecret { get; set; }

        public bool Enabled { get; set; }

        public bool HostConfig { get; set; }

        private static string GetCacheKey(string service, int portalId)
        {
            return _cacheKey + "." + service + "_" + portalId;
        }

        public static void ClearConfig(string service, int portalId)
        {
            DataCache.RemoveCache(GetCacheKey(service, portalId));
        }

        public static OAuthConfigBase GetConfig(string service, int portalId)
        {
            string key = GetCacheKey(service, portalId);
            var config = (OAuthConfigBase)DataCache.GetCache(key);
            if (config == null)
            {
                config = new OAuthConfigBase(service, portalId);
                DataCache.SetCache(key, config);
            }
            return config;
        }

        public static void UpdateConfig(OAuthConfigBase config)
        {
            if (config.HostConfig)
            {
                HostController.Instance.Update(config.Service + "_APIKey", config.APIKey, true);
                HostController.Instance.Update(config.Service + "_APISecret", config.APISecret, true);
                HostController.Instance.Update(config.Service + "_Enabled", config.Enabled.ToString(CultureInfo.InvariantCulture), true);
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_APIKey");
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_APISecret");
                PortalController.DeletePortalSetting(config.PortalID, config.Service + "_Enabled");
            }
            else
            {
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_APIKey", config.APIKey);
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_APISecret", config.APISecret);
                PortalController.UpdatePortalSetting(config.PortalID, config.Service + "_Enabled", config.Enabled.ToString(CultureInfo.InvariantCulture));
            }

            ClearConfig(config.Service, config.PortalID);
        }
    }
}
