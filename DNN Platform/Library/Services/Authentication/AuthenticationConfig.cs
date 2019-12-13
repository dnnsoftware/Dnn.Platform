#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;

#endregion

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
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AuthenticationConfig));
        private const string CACHEKEY = "Authentication.DNN";

        protected AuthenticationConfig(int portalID) : base(portalID)
        {
            UseCaptcha = Null.NullBoolean;
            Enabled = true;
            try
            {
                string setting = Null.NullString;
                if (PortalController.Instance.GetPortalSettings(portalID).TryGetValue("DNN_Enabled", out setting))
                {
                    Enabled = bool.Parse(setting);
                }
                setting = Null.NullString;
                if (PortalController.Instance.GetPortalSettings(portalID).TryGetValue("DNN_UseCaptcha", out setting))
                {
                    UseCaptcha = bool.Parse(setting);
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
            var config = (AuthenticationConfig) DataCache.GetCache(key);
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
