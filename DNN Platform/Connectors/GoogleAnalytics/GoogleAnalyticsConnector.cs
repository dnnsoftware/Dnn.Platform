using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Services.Analytics.Config;
using DotNetNuke.Services.Connections;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace DNN.Connectors.GoogleAnalytics
{
    public class GoogleAnalyticsConnector : IConnector
    {
        #region Properties

        private const string DefaultDisplayName = "Google Analytics";

        public string Name
        {
            get { return "Core Google Analytics Connector"; }
        }

        private string _displayName;

        public string DisplayName
        {
            get
            {
                return
                    string.IsNullOrEmpty(_displayName) ? DefaultDisplayName : _displayName;
            }

            set { _displayName = value; }
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

        public string Id { get; set; }

        public ConnectorCategories Type => ConnectorCategories.Analytics;

        // As of DNN 9.2.2 you need to support multiple to get access to the Delete Connection functionality
        public bool SupportsMultiple => false;

        #endregion

        #region Public Methods
        public IEnumerable<IConnector> GetConnectors(int portalId)
        {

            return new List<IConnector> { this };

        }

        public void DeleteConnector(int portalId)
        {


        }

        public bool HasConfig(int portalId)
        {
            IDictionary<string, string> config = GetConfig(portalId);

            return (config.ContainsKey("TrackingID") && !String.IsNullOrEmpty(config["TrackingID"]));

        }

        public IDictionary<string, string> GetConfig(int portalId)
        {

            var analyticsConfig = AnalyticsConfiguration.GetConfig("GoogleAnalytics");
            var trackingId = String.Empty;
            var urlParameter = String.Empty;
            var trackForAdmin = false;

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
                            if (!bool.TryParse(setting.SettingValue, out trackForAdmin))
                            {
                                trackForAdmin = true;
                            }
                            break;
                    }
                }
            }

            var configItems = new Dictionary<string, string>
            {
                { "TrackingID", trackingId },
                { "UrlParameter", urlParameter},
                { "TrackAdministrators", trackForAdmin.ToString()},
                { "isDeactivating", false.ToString()}
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

                bool isDeactivating = false;
                bool.TryParse(values["isDeactivating"].ToLowerInvariant(), out isDeactivating);

                string trackingID;
                string urlParameter;
                bool trackForAdmin;

                isValid = true;


                if (isDeactivating)
                {

                    trackingID = null;
                    urlParameter = null;
                    trackForAdmin = false;


                }
                else
                {

                    trackingID = values["TrackingID"] != null ? values["TrackingID"].ToString().Trim() : String.Empty;
                    urlParameter = values["UrlParameter"] != null ? values["UrlParameter"].ToString().Trim() : String.Empty;
                    trackForAdmin = bool.TryParse(values["TrackAdministrators"].ToLowerInvariant(), out trackForAdmin);

                    if (String.IsNullOrEmpty(trackingID))
                    {

                        isValid = false;
                        customErrorMessage = Localization.GetString("TrackingCodeFormat.ErrorMessage", Constants.LocalResourceFile);

                    }

                }

                if (isValid)
                {

                    var config = new AnalyticsConfiguration
                    {
                        Settings = new AnalyticsSettingCollection()
                    };


                    var setting = new AnalyticsSetting
                    {
                        SettingName = "TrackingId",
                        SettingValue = trackingID
                    };

                    config.Settings.Add(setting);

                    setting = new AnalyticsSetting
                    {
                        SettingName = "UrlParameter",
                        SettingValue = urlParameter
                    };

                    config.Settings.Add(setting);

                    setting = new AnalyticsSetting
                    {
                        SettingName = "TrackForAdmin",
                        SettingValue = trackForAdmin.ToString().ToLowerInvariant()
                    };

                    config.Settings.Add(setting);

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

        #endregion

    }
}