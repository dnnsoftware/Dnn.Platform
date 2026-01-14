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

    /// <summary>Connector to provide configuration for Google Analytics support.</summary>
    public class GoogleAnalyticsConnector : IConnector
    {
        private const string DefaultDisplayName = "Google Analytics (Legacy UA)";

        private string displayName;

        /// <inheritdoc/>
        public string Name
        {
            get { return "Core Google Analytics Connector"; }
        }

        /// <inheritdoc/>
        public string IconUrl
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics/Images/GoogleAnalytics_32X32_Standard.png"; }
        }

        /// <inheritdoc/>
        public string PluginFolder
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics/"; }
        }

        /// <inheritdoc/>
        public bool IsEngageConnector
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public ConnectorCategories Type => ConnectorCategories.Analytics;

        /// <inheritdoc/>
        // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
        public bool SupportsMultiple => false;

        /// <inheritdoc/>
        public string DisplayName
        {
            get => string.IsNullOrEmpty(this.displayName) ? DefaultDisplayName : this.displayName;
            set => this.displayName = value;
        }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IConnector> GetConnectors(int portalId)
        {
            return new List<IConnector> { this };
        }

        /// <inheritdoc/>
        public void DeleteConnector(int portalId)
        {
        }

        /// <inheritdoc/>
        public bool HasConfig(int portalId)
        {
            IDictionary<string, string> config = this.GetConfig(portalId);

            return config.ContainsKey("TrackingID") && !string.IsNullOrEmpty(config["TrackingID"]);
        }

        /// <inheritdoc/>
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
                    switch (setting.SettingName.ToUpperInvariant())
                    {
                        case "TRACKINGID":
                            trackingId = setting.SettingValue;
                            break;
                        case "URLPARAMETER":
                            urlParameter = setting.SettingValue;
                            break;
                        case "TRACKFORADMIN":
                            trackForAdmin = HandleCustomBoolean(setting.SettingValue);
                            break;
                        case "ANONYMIZEIP":
                            anonymizeIp = HandleCustomBoolean(setting.SettingValue);
                            break;
                        case "TRACKUSERID":
                            trackUserId = HandleCustomBoolean(setting.SettingValue);
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
                { "DataConsent", HandleCustomBoolean(portalSettings.DataConsentActive.ToString()) },
                { "isDeactivating", HandleCustomBoolean("false") },
            };

            return configItems;
        }

        /// <inheritdoc/>
        public bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage)
        {
            // Delete / Deactivation functionality added into SaveConfig because
            // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
            customErrorMessage = string.Empty;

            try
            {
                if (!bool.TryParse(values["isDeactivating"].ToLowerInvariant(), out var isDeactivating))
                {
                    isDeactivating = false;
                }

                string trackingId;
                string urlParameter;
                string trackForAdmin;
                string anonymizeIp;
                string trackUserId;

                var isValid = true;

                if (isDeactivating)
                {
                    trackingId = null;
                    urlParameter = null;
                    trackForAdmin = null;
                    anonymizeIp = null;
                    trackUserId = null;
                }
                else
                {
                    trackingId = values["TrackingID"] != null ? values["TrackingID"].ToUpperInvariant().Trim() : string.Empty;
                    urlParameter = values["UrlParameter"]?.Trim() ?? string.Empty;
                    trackForAdmin = values["TrackAdministrators"] != null ? values["TrackAdministrators"].ToLowerInvariant().Trim() : string.Empty;
                    anonymizeIp = values["AnonymizeIp"] != null ? values["AnonymizeIp"].ToLowerInvariant().Trim() : string.Empty;
                    trackUserId = values["TrackUserId"] != null ? values["TrackUserId"].ToLowerInvariant().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(trackingId))
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
                        SettingValue = trackingId,
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
        /// <param name="value">The string representing a boolean.</param>
        /// <returns>The string representing a boolean after the correction.</returns>
        private static string HandleCustomBoolean(string value)
        {
            if ((value ?? string.Empty).Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return "true";
            }

            return string.Empty;
        }
    }
}
