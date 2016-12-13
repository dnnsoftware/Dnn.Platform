#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Services.Analytics.Config
{
    [Serializable, XmlRoot("AnalyticsConfig")]
    public class AnalyticsConfiguration
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AnalyticsConfiguration));
		#region "Private Members"

        private AnalyticsRuleCollection _rules;
        private AnalyticsSettingCollection _settings;

		#endregion

		#region "Public Properties"

        public AnalyticsSettingCollection Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }

        public AnalyticsRuleCollection Rules
        {
            get
            {
                return _rules;
            }
            set
            {
                _rules = value;
            }
        }
		
		#endregion

		#region "Shared Methods"

        public static AnalyticsConfiguration GetConfig(string analyticsEngineName)
        {
            string cacheKey = analyticsEngineName + "." + PortalSettings.Current.PortalId;

            var Config = new AnalyticsConfiguration();
            Config.Rules = new AnalyticsRuleCollection();
            Config.Settings = new AnalyticsSettingCollection();

            FileStream fileReader = null;
            string filePath = "";
            try
            {
                Config = (AnalyticsConfiguration) DataCache.GetCache(cacheKey);
                if ((Config == null))
                {
                    filePath = PortalSettings.Current.HomeDirectoryMapPath + "\\" + analyticsEngineName + ".config";

                    if (!File.Exists(filePath))
                    {
                        return null;
                    }
					
                    //Create a FileStream for the Config file
                    fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    var doc = new XPathDocument(fileReader);
                    Config = new AnalyticsConfiguration();
                    Config.Rules = new AnalyticsRuleCollection();
                    Config.Settings = new AnalyticsSettingCollection();

                    var allSettings = new Hashtable();
                    foreach (XPathNavigator nav in doc.CreateNavigator().Select("AnalyticsConfig/Settings/AnalyticsSetting"))
                    {
                        var setting = new AnalyticsSetting();
                        setting.SettingName = nav.SelectSingleNode("SettingName").Value;
                        setting.SettingValue = nav.SelectSingleNode("SettingValue").Value;
                        Config.Settings.Add(setting);
                    }
                    foreach (XPathNavigator nav in doc.CreateNavigator().Select("AnalyticsConfig/Rules/AnalyticsRule"))
                    {
                        var rule = new AnalyticsRule();
                        rule.RoleId = Convert.ToInt32(nav.SelectSingleNode("RoleId").Value);
                        rule.TabId = Convert.ToInt32(nav.SelectSingleNode("TabId").Value);
                        rule.Label = nav.SelectSingleNode("Label").Value;
                        var valueNode = nav.SelectSingleNode("Value");
                        if (valueNode != null)
                        {
                            rule.Value = valueNode.Value;
                        }
                        Config.Rules.Add(rule);
                    }
                    if (File.Exists(filePath))
                    {
						//Set back into Cache
                        DataCache.SetCache(cacheKey, Config, new DNNCacheDependency(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
				//log it
                var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString()};
                log.AddProperty("Analytics.AnalyticsConfiguration", "GetConfig Failed");
                log.AddProperty("FilePath", filePath);
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(ex);
            }
            finally
            {
                if (fileReader != null)
                {
					//Close the Reader
                    fileReader.Close();
                }
            }
            return Config;
        }

        public static void SaveConfig(string analyticsEngineName, AnalyticsConfiguration config)
        {
            string cacheKey = analyticsEngineName + "." + PortalSettings.Current.PortalId;
            if (config.Settings != null)
            {
                //Create a new Xml Serializer
                var ser = new XmlSerializer(typeof (AnalyticsConfiguration));
                string filePath = "";

                //Create a FileStream for the Config file
                filePath = PortalSettings.Current.HomeDirectoryMapPath + "\\" + analyticsEngineName + ".config";
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }
                using (var fileWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(fileWriter))
                {
                    //Serialize the AnalyticsConfiguration
                    ser.Serialize(writer, config);

                    //Close the Writers
                    writer.Close();
                    fileWriter.Close();
                }
                DataCache.SetCache(cacheKey, config, new DNNCacheDependency(filePath));
            }
        }
		
		#endregion
    }
}