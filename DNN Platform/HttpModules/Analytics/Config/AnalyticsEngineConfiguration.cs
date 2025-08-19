// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Config
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Class definition for AnalyticsEngineConfiguration which is used to create
    /// an AnalyticsEngineCollection.
    /// </summary>
    [Serializable]
    [XmlRoot("AnalyticsEngineConfig")]
    public class AnalyticsEngineConfiguration
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AnalyticsEngineConfiguration));

        /// <summary>Gets or sets the collection of analytics engines.</summary>
        public AnalyticsEngineCollection AnalyticsEngines { get; set; }

        /// <summary>Gets the site analytics config.</summary>
        /// <returns>An <see cref="AnalyticsEngineConfiguration"/> instance.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public static AnalyticsEngineConfiguration GetConfig()
            => GetConfig(Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>());

        /// <summary>Gets the site analytics config.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <returns>An <see cref="AnalyticsEngineConfiguration"/> instance.</returns>
        public static AnalyticsEngineConfiguration GetConfig(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            var config = new AnalyticsEngineConfiguration { AnalyticsEngines = new AnalyticsEngineCollection() };
            FileStream fileReader = null;
            string filePath = null;
            try
            {
                config = (AnalyticsEngineConfiguration)DataCache.GetCache("AnalyticsEngineConfig");
                if (config == null)
                {
                    filePath = Config.GetPathToFile(appStatus, Config.ConfigFileType.SiteAnalytics);

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
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT), };
                log.AddProperty("Analytics.AnalyticsEngineConfiguration", "GetConfig Failed");
                if (!string.IsNullOrEmpty(filePath))
                {
                    log.AddProperty("FilePath", filePath);
                }

                log.AddProperty("ExceptionMessage", ex.Message);
                eventLogger.AddLog(log);
                Logger.Error(log);
            }
            finally
            {
                // Close the Reader
                fileReader?.Close();
            }

            return config;
        }
    }
}
