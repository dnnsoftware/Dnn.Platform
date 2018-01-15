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
using System.IO;
using System.Xml.Serialization;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.HttpModules.Config
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.HttpModules.Analytics
    /// Project:    HttpModules
    /// Module:     AnalyticsEngineConfiguration
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Class definition for AnalyticsEngineConfiguration which is used to create
    /// an AnalyticsEngineCollection
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable, XmlRoot("AnalyticsEngineConfig")]
    public class AnalyticsEngineConfiguration
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AnalyticsEngineConfiguration));
        private AnalyticsEngineCollection _analyticsEngines;

        public AnalyticsEngineCollection AnalyticsEngines
        {
            get
            {
                return _analyticsEngines;
            }
            set
            {
                _analyticsEngines = value;
            }
        }

        public static AnalyticsEngineConfiguration GetConfig()
        {
            var config = new AnalyticsEngineConfiguration {AnalyticsEngines = new AnalyticsEngineCollection()};
            FileStream fileReader = null;
            string filePath = null;
            try
            {
                config = (AnalyticsEngineConfiguration) DataCache.GetCache("AnalyticsEngineConfig");
                if ((config == null))
                {
                    filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteAnalytics);

                    //Create a FileStream for the Config file
                    fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var doc = new XPathDocument(fileReader);
                    config = new AnalyticsEngineConfiguration {AnalyticsEngines = new AnalyticsEngineCollection()};
                    foreach (XPathNavigator nav in
                        doc.CreateNavigator().Select("AnalyticsEngineConfig/Engines/AnalyticsEngine"))
                    {
                        var analyticsEngine = new AnalyticsEngine
                                                  {
                                                      EngineType = nav.SelectSingleNode("EngineType").Value,
                                                      ElementId = nav.SelectSingleNode("ElementId").Value,
                                                      InjectTop = Convert.ToBoolean(nav.SelectSingleNode("InjectTop").Value),
                                                      ScriptTemplate = nav.SelectSingleNode("ScriptTemplate").Value
                                                  };
                        config.AnalyticsEngines.Add(analyticsEngine);
                    }
                    if (File.Exists(filePath))
                    {
                        //Set back into Cache
                        DataCache.SetCache("AnalyticsEngineConfig", config, new DNNCacheDependency(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                //log it
                var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()};
                log.AddProperty("Analytics.AnalyticsEngineConfiguration", "GetConfig Failed");
                if (!string.IsNullOrEmpty(filePath))
                    log.AddProperty("FilePath", filePath);
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(log);

            }
            finally
            {
                if (fileReader != null)
                {
                    //Close the Reader
                    fileReader.Close();
                }
            }
            return config;
        }
    }
}