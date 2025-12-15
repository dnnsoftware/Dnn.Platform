// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Connectors.GoogleTagManager
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Analytics.Config;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>Connector to provide configuration for Google Tag Manager support.</summary>
    public class GoogleTagManagerConnector : IConnector
    {
        private const string DefaultDisplayName = "Google Tag Manager";

        private string displayName;

        /// <inheritdoc/>
        public string Name
        {
            get { return "Core Google Tag Manager Connector"; }
        }

        /// <inheritdoc/>
        public string IconUrl
        {
            get { return "~/DesktopModules/Connectors/GoogleTagManager/Images/GoogleTagManager_32X32_Standard.png"; }
        }

        /// <inheritdoc/>
        public string PluginFolder
        {
            get { return "~/DesktopModules/Connectors/GoogleTagManager/"; }
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

            return config.ContainsKey("GtmID") && !string.IsNullOrEmpty(config["GtmID"]);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetConfig(int portalId)
        {
            var gtmConfig = AnalyticsConfiguration.GetConfig("GoogleTagManager");
            var portalSettings = new PortalSettings(portalId);

            // Important, knockout handles empty strings as false and any other string as true
            // so we need to pass empty strings when we mean false, however it passes us back the string "false"
            // when saving the settings in the SaveConfig method, so we need to handle that case too
            var gtmId = string.Empty;
            var trackForAdmin = string.Empty;

            if (gtmConfig != null)
            {
                foreach (AnalyticsSetting setting in gtmConfig.Settings)
                {
                    switch (setting.SettingName.ToLower())
                    {
                        case "gtmid":
                            gtmId = setting.SettingValue;
                            break;
                        case "trackforadmin":
                            trackForAdmin = HandleCustomBoolean(setting.SettingValue);
                            break;
                    }
                }
            }

            var configItems = new Dictionary<string, string>
            {
                { "GtmID", gtmId },
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

                string gtmId;
                string trackForAdmin;

                var isValid = true;

                if (isDeactivating)
                {
                    gtmId = null;
                    trackForAdmin = null;
                }
                else
                {
                    gtmId = values["GtmID"] != null ? values["GtmID"].ToUpperInvariant().Trim() : string.Empty;
                    trackForAdmin = values["TrackAdministrators"] != null ? values["TrackAdministrators"].ToLowerInvariant().Trim() : string.Empty;

                    if (string.IsNullOrEmpty(gtmId))
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
                        SettingName = "GtmId",
                        SettingValue = gtmId,
                    });

                    config.Settings.Add(new AnalyticsSetting
                    {
                        SettingName = "TrackForAdmin",
                        SettingValue = trackForAdmin,
                    });

                    AnalyticsConfiguration.SaveConfig("GoogleTagManager", config);

                    if (!isDeactivating)
                    {
                        EnsureScriptInConfig();
                    }
                }

                return isValid;
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
                if (engineTypeNode.InnerText.Contains("DotNetNuke.Services.Analytics.GoogleTagManagerEngine"))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var fileGtm = applicationMapPath + @"\DesktopModules\Connectors\GoogleTagManager\GoogleTagManager.config";
                var xdocGtm = new XmlDocument();
                xdocGtm.Load(fileGtm);

                var enginesElement = xdoc.SelectSingleNode("/AnalyticsEngineConfig/Engines");
                foreach (XmlNode engineNode in xdocGtm.SelectNodes("/AnalyticsEngineConfig/Engines/AnalyticsEngine"))
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
