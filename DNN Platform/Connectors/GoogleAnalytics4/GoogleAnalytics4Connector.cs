// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Connectors.GoogleAnalytics4
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Services.Analytics.Config;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>Connector to provide configuration for Google Tag Manager support.</summary>
    public class GoogleAnalytics4Connector : IConnector
    {
        private const string DefaultDisplayName = "Google Analytics 4";

        private string displayName;

        /// <inheritdoc/>
        public string Name
        {
            get { return "Core Google Analytics 4 Connector"; }
        }

        /// <inheritdoc/>
        public string IconUrl
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics4/Images/GoogleAnalytics4_32X32_Standard.png"; }
        }

        /// <inheritdoc/>
        public string PluginFolder
        {
            get { return "~/DesktopModules/Connectors/GoogleAnalytics4/"; }
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
            var config = this.GetConfig(portalId);

            return config.ContainsKey("Ga4ID") && !string.IsNullOrEmpty(config["Ga4ID"]);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetConfig(int portalId)
        {
            var ga4Config = AnalyticsConfiguration.GetConfig("GoogleAnalytics4");

            // Important, knockout handles empty strings as false and any other string as true
            // so we need to pass empty strings when we mean false, however it passes us back the string "false"
            // when saving the settings in the SaveConfig method, so we need to handle that case too
            var ga4Id = string.Empty;
            var trackForAdmin = string.Empty;

            if (ga4Config != null)
            {
                foreach (AnalyticsSetting setting in ga4Config.Settings)
                {
                    switch (setting.SettingName.ToUpperInvariant())
                    {
                        case "GA4ID":
                            ga4Id = setting.SettingValue;
                            break;
                        case "TRACKFORADMIN":
                            trackForAdmin = HandleCustomBoolean(setting.SettingValue);
                            break;
                    }
                }
            }

            var configItems = new Dictionary<string, string>
            {
                { "Ga4ID", ga4Id },
                { "TrackAdministrators", trackForAdmin },
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

                string ga4Id;
                string trackForAdmin;

                var isValid = true;

                if (isDeactivating)
                {
                    ga4Id = null;
                    trackForAdmin = null;
                }
                else
                {
                    ga4Id = values["Ga4ID"] != null ? values["Ga4ID"].ToUpperInvariant().Trim() : string.Empty;
                    trackForAdmin = values["TrackAdministrators"] != null ? values["TrackAdministrators"].ToLowerInvariant().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(ga4Id))
                    {
                        isValid = false;
                        customErrorMessage = Localization.GetString("TrackingCodeFormat.ErrorMessage", Constants.LocalResourceFile);
                    }
                }

                if (!isValid)
                {
                    return false;
                }

                var config = new AnalyticsConfiguration
                {
                    Settings = new AnalyticsSettingCollection
                    {
                        new AnalyticsSetting
                        {
                            SettingName = "Ga4Id",
                            SettingValue = ga4Id,
                        },
                        new AnalyticsSetting
                        {
                            SettingName = "TrackForAdmin",
                            SettingValue = trackForAdmin,
                        },
                    },
                };

                AnalyticsConfiguration.SaveConfig("GoogleAnalytics4", config);

                if (!isDeactivating)
                {
                    EnsureScriptInConfig();
                }

                return true;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        /// <summary>Check if there's an AnalyticsEngine element in siteanalytics.config for this connector. If not, adds the default one.</summary>
        private static void EnsureScriptInConfig()
        {
            var applicationMapPath = HttpContext.Current.Server.MapPath(@"\");
            var file = applicationMapPath + @"\SiteAnalytics.config";
            var xdoc = new XmlDocument();
            xdoc.Load(file);
            var found = false;
            foreach (XmlNode engineTypeNode in xdoc.SelectNodes("/AnalyticsEngineConfig/Engines/AnalyticsEngine/EngineType"))
            {
                if (engineTypeNode.InnerText.Contains("DotNetNuke.Services.Analytics.GoogleAnalytics4Engine"))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var fileGa4 = applicationMapPath + @"\DesktopModules\Connectors\GoogleAnalytics4\GoogleAnalytics4.config";
                var xdocGa4 = new XmlDocument();
                xdocGa4.Load(fileGa4);

                var enginesElement = xdoc.SelectSingleNode("/AnalyticsEngineConfig/Engines");
                foreach (XmlNode engineNode in xdocGa4.SelectNodes("/AnalyticsEngineConfig/Engines/AnalyticsEngine"))
                {
                    var engineFrag = xdoc.CreateDocumentFragment();
                    engineFrag.InnerXml = engineNode.OuterXml;
                    enginesElement.AppendChild(engineFrag);
                }

                xdoc.Save(file);
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
