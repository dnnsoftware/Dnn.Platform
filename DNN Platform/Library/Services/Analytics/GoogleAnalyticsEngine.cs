// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics
{
    using System;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Analytics.Config;

    public class GoogleAnalyticsEngine : AnalyticsEngineBase
    {
        public override string EngineName
        {
            get
            {
                return "GoogleAnalytics";
            }
        }

        public override string RenderScript(string scriptTemplate)
        {
            AnalyticsConfiguration config = this.GetConfig();

            if (config == null)
            {
                return string.Empty;
            }

            var trackingId = string.Empty;
            var urlParameter = string.Empty;
            var trackForAdmin = true;

            foreach (AnalyticsSetting setting in config.Settings)
            {
                switch (setting.SettingName.ToLowerInvariant())
                {
                    case "trackingid":
                        trackingId = setting.SettingValue;
                        break;
                    case "urlparameter":
                        urlParameter = setting.SettingValue;
                        break;
                    case "trackforadmin":
                        if (!bool.TryParse(setting.SettingValue, out trackForAdmin))
                        {
                            trackForAdmin = true;
                        }

                        break;
                }
            }

            if (string.IsNullOrEmpty(trackingId))
            {
                return string.Empty;
            }

            // check whether setting to not track traffic if current user is host user or website administrator.
            if (!trackForAdmin &&
                (UserController.Instance.GetCurrentUserInfo().IsSuperUser
                 ||
                 (PortalSettings.Current != null &&
                  UserController.Instance.GetCurrentUserInfo().IsInRole(PortalSettings.Current.AdministratorRoleName))))
            {
                return string.Empty;
            }

            scriptTemplate = scriptTemplate.Replace("[TRACKING_ID]", trackingId);
            if (!string.IsNullOrEmpty(urlParameter))
            {
                scriptTemplate = scriptTemplate.Replace("[PAGE_URL]", urlParameter);
            }
            else
            {
                scriptTemplate = scriptTemplate.Replace("[PAGE_URL]", string.Empty);
            }

            scriptTemplate = scriptTemplate.Replace("[CUSTOM_SCRIPT]", this.RenderCustomScript(config));

            return scriptTemplate;
        }

        public override string RenderCustomScript(AnalyticsConfiguration config)
        {
            try
            {
                var anonymize = false;
                var trackingUserId = false;

                foreach (AnalyticsSetting setting in config.Settings)
                {
                    switch (setting.SettingName.ToLowerInvariant())
                    {
                        case "anonymizeip":
                            {
                                bool.TryParse(setting.SettingValue, out anonymize);
                                break;
                            }

                        case "trackinguser":
                            {
                                bool.TryParse(setting.SettingValue, out trackingUserId);
                                break;
                            }
                    }
                }

                var customScripts = new System.Text.StringBuilder();

                if (anonymize || PortalSettings.Current.DataConsentActive)
                {
                    customScripts.Append("ga('set', 'anonymizeIp', true);");
                }

                if (trackingUserId)
                {
                    customScripts.AppendFormat("ga('set', 'userId', {0});", UserController.Instance.GetCurrentUserInfo().UserID);
                }

                return customScripts.ToString();
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);

                return string.Empty;
            }
        }
    }
}
