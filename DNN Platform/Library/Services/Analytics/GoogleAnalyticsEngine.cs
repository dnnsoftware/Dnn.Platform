#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Analytics.Config;

#endregion

namespace DotNetNuke.Services.Analytics
{
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
            AnalyticsConfiguration config = GetConfig();

            if (config == null)
            {
                return "";
            }

            var trackingId = "";
            var urlParameter = "";
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

            if (String.IsNullOrEmpty(trackingId))
            {
                return "";
            }

            //check whether setting to not track traffic if current user is host user or website administrator.
            if (!trackForAdmin &&
                (UserController.Instance.GetCurrentUserInfo().IsSuperUser
                 ||
                 (PortalSettings.Current != null &&
                  UserController.Instance.GetCurrentUserInfo().IsInRole(PortalSettings.Current.AdministratorRoleName))))
            {
                return "";
            }

            scriptTemplate = scriptTemplate.Replace("[TRACKING_ID]", trackingId);
            if ((!String.IsNullOrEmpty(urlParameter)))
            {
                scriptTemplate = scriptTemplate.Replace("[PAGE_URL]", urlParameter);
            }
            else
            {
                scriptTemplate = scriptTemplate.Replace("[PAGE_URL]", "");
            }

            scriptTemplate = scriptTemplate.Replace("[CUSTOM_SCRIPT]", RenderCustomScript(config));

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