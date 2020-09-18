// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.HttpModules.Analytics
    /// Project:    HttpModules
    /// Module:     AnalyticsEngineConfiguration
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Class definition for AnalyticsEngineConfiguration which is used to create
    /// an AnalyticsEngineCollection.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    [XmlRoot("AnalyticsEngineConfig")]
    public class AnalyticsEngineConfiguration
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AnalyticsEngineConfiguration));
        private AnalyticsEngineCollection _analyticsEngines;

        public AnalyticsEngineCollection AnalyticsEngines
        {
            get
            {
                return this._analyticsEngines;
            }

            set
            {
                this._analyticsEngines = value;
            }
        }

        public static AnalyticsEngineConfiguration GetConfig()
        {
            var config = new AnalyticsEngineConfiguration { AnalyticsEngines = new AnalyticsEngineCollection() };
            FileStream fileReader = null;
            string filePath = null;
            try
            {
                config = (AnalyticsEngineConfiguration)DataCache.GetCache("AnalyticsEngineConfig");
                if (config == null)
                {
                    filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteAnalytics);

                    // Create a FileStream for the Config file
                    fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var doc = new XPathDocument(fileReader);
                    config = new AnalyticsEngineConfiguration { AnalyticsEngines = new AnalyticsEngineCollection() };
                    foreach (XPathNavigator nav in
                        doc.CreateNavigator().Select("AnalyticsEngineConfig/Engines/AnalyticsEngine"))
                    {
                        var analyticsEngine = new AnalyticsEngine
                        {
                            EngineType = nav.SelectSingleNode("EngineType").Value,
                            ElementId = nav.SelectSingleNode("ElementId").Value,
                            InjectTop = Convert.ToBoolean(nav.SelectSingleNode("InjectTop").Value),
                            ScriptTemplate = nav.SelectSingleNode("ScriptTemplate").Value,
                        };
                        config.AnalyticsEngines.Add(analyticsEngine);
                    }

                    if (File.Exists(filePath))
                    {
                        // Set back into Cache
                        DataCache.SetCache("AnalyticsEngineConfig", config, new DNNCacheDependency(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                // log it
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("Analytics.AnalyticsEngineConfiguration", "GetConfig Failed");
                if (!string.IsNullOrEmpty(filePath))
                {
                    log.AddProperty("FilePath", filePath);
                }

                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(log);
            }
            finally
            {
                if (fileReader != null)
                {
                    // Close the Reader
                    fileReader.Close();
                }
            }

            return config;
        }
    }
}
