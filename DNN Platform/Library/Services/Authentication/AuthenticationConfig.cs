#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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