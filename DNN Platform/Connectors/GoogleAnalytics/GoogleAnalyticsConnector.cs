// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Connectors.GoogleAnalytics
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Analytics.Config;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    public class GoogleAnalyticsConnector : IConnector
    {
        private const string DefaultDisplayName = "Google Analytics";

        private string _displayName;

        public string Name
        {
            get { return "Core Google Analytics Connector"; }
        }

        public string IconUrl
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics/Images/GoogleAnalytics_32X32_Standard.png"; }
        }

        public string PluginFolder
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics/"; }
        }

        public bool IsEngageConnector
        {
            get
            {
                return false;
            }
        }

        public ConnectorCategories Type => ConnectorCategories.Analytics;

        // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
        public bool SupportsMultiple => false;

        public string DisplayName
        {
            get
            {
                return
                    string.IsNullOrEmpty(this._displayName) ? DefaultDisplayName : this._displayName;
            }

            set { this._displayName = value; }
        }

        public string Id { get; set; }

        public IEnumerable<IConnector> GetConnectors(int portalId)
        {
            return new List<IConnector> { this };
        }

        public void DeleteConnector(int portalId)
        {
        }

        public bool HasConfig(int portalId)
        {
            IDictionary<string, string> config = this.GetConfig(portalId);

            return config.ContainsKey("TrackingID") && !string.IsNullOrEmpty(config["TrackingID"]);
        }

        public IDictionary<string, string> GetConfig(int portalId)
        {
            var analyticsConfig = AnalyticsConfiguration.GetConfig("GoogleAnalytics");
            var portalSettings = new PortalSettings(portalId);

            // Important, knockout handles empty strings as false and any other string as true
            // so we need to pass empty strings when we mean false, however it passes us back the string "false"
            // when saving the settings in the SaveConfig method, so we need to handle that case too
            var trackingId = string.Empty;
            var urlParameter = string.Empty;
            var trackForAdmin = string.Empty;
            var anonymizeIp = string.Empty;
            var trackUserId = string.Empty;

            if (analyticsConfig != null)
            {
                foreach (AnalyticsSetting setting in analyticsConfig.Settings)
                {
                    switch (setting.SettingName.ToLower())
                    {
                        case "trackingid":
                            trackingId = setting.SettingValue;
                            break;
                        case "urlparameter":
                            urlParameter = setting.SettingValue;
                            break;
                        case "trackforadmin":
                            trackForAdmin = this.HandleCustomBoolean(setting.SettingValue);
                            break;
                        case "anonymizeip":
                            anonymizeIp = this.HandleCustomBoolean(setting.SettingValue);
                            break;
                        case "trackuserid":
                            trackUserId = this.HandleCustomBoolean(setting.SettingValue);
                            break;
                    }
                }
            }

            if (portalSettings.DataConsentActive)
            {
                anonymizeIp = "true";
            }

            var configItems = new Dictionary<string, string>
            {
                { "TrackingID", trackingId },
                { "UrlParameter", urlParameter },
                { "TrackAdministrators", trackForAdmin },
                { "AnonymizeIp", anonymizeIp },
                { "TrackUserId", trackUserId },
                { "DataConsent", this.HandleCustomBoolean(portalSettings.DataConsentActive.ToString()) },
                { "isDeactivating", this.HandleCustomBoolean("false") },
            };

            return configItems;
        }

        public bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage)
        {
            // Delete / Deactivation functionality added into SaveConfig because
            // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
            customErrorMessage = string.Empty;
            bool isValid;

            try
            {
                var isDeactivating = false;

                bool.TryParse(values["isDeactivating"].ToLowerInvariant(), out isDeactivating);

                string trackingID;
                string urlParameter;
                string trackForAdmin;
                string anonymizeIp;
                string trackUserId;

                isValid = true;

                if (isDeactivating)
                {
                    trackingID = null;
                    urlParameter = null;
                    trackForAdmin = null;
                    anonymizeIp = null;
                    trackUserId = null;
                }
                else
                {
                    trackingID = values["TrackingID"] != null ? values["TrackingID"].ToUpperInvariant().Trim() : string.Empty;
                    urlParameter = values["UrlParameter"]?.Trim() ?? string.Empty;
                    trackForAdmin = values["TrackAdministrators"] != null ? values["TrackAdministrators"].ToLowerInvariant().Trim() : string.Empty;
                    anonymizeIp = values["AnonymizeIp"] != null ? values["AnonymizeIp"].ToLowerInvariant().Trim() : string.Empty;
                    trackUserId = values["TrackUserId"] != null ? values["TrackUserId"].ToLowerInvariant().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(trackingID))
                    {
                        isValid = false;
                        customErrorMessage = Localization.GetString("TrackingCodeFormat.ErrorMessage", Constants.LocalResourceFile);
                    }
                }

                if (isValid)
                {
                    var config = new AnalyticsConfiguration
                    {
                        Settings = new AnalyticsSettingCollection(),
                    };

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackingId",
                        SettingValue = trackingID,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "UrlParameter",
                        SettingValue = urlParameter,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackForAdmin",
                        SettingValue = trackForAdmin,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "AnonymizeIp",
                        SettingValue = anonymizeIp,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackUserId",
                        SettingValue = trackUserId,
                    });

                    AnalyticsConfiguration.SaveConfig("GoogleAnalytics", config);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Handles custom conversion from "true" => "true"
        /// Anything else to "" to support the strange knockout handling of string as booleans.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string HandleCustomBoolean(string value)
        {
            if (value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return "true";
            }

            return string.Empty;
        }
    }
}
