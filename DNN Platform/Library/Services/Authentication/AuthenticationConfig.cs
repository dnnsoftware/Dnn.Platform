
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.Authentication
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AuthenticationConfig class providesa configuration class for the DNN
    /// Authentication provider
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class AuthenticationConfig : AuthenticationConfigBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthenticationConfig));
        private const string CACHEKEY = "Authentication.DNN";

        protected AuthenticationConfig(int portalID)
            : base(portalID)
        {
            this.UseCaptcha = Null.NullBoolean;
            this.Enabled = true;
            try
            {
                string setting = Null.NullString;
                if (PortalController.Instance.GetPortalSettings(portalID).TryGetValue("DNN_Enabled", out setting))
                {
                    this.Enabled = bool.Parse(setting);
                }
                setting = Null.NullString;
                if (PortalController.Instance.GetPortalSettings(portalID).TryGetValue("DNN_UseCaptcha", out setting))
                {
                    this.UseCaptcha = bool.Parse(setting);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool Enabled { get; set; }

        public bool UseCaptcha { get; set; }

        public static void ClearConfig(int portalId)
        {
            string key = CACHEKEY + "_" + portalId;
            DataCache.RemoveCache(key);
        }

        public static AuthenticationConfig GetConfig(int portalId)
        {
            string key = CACHEKEY + "_" + portalId;
            var config = (AuthenticationConfig)DataCache.GetCache(key);
            if (config == null)
            {
                config = new AuthenticationConfig(portalId);
                DataCache.SetCache(key, config);
            }
            return config;
        }

        public static void UpdateConfig(AuthenticationConfig config)
        {
            PortalController.UpdatePortalSetting(config.PortalID, "DNN_Enabled", config.Enabled.ToString());
            PortalController.UpdatePortalSetting(config.PortalID, "DNN_UseCaptcha", config.UseCaptcha.ToString());
            ClearConfig(config.PortalID);
        }
    }
}
