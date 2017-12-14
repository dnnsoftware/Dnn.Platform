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
using System.IO;
using System.Xml.Serialization;
using System.Xml.XPath;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Urls.Config
{
    [Serializable, XmlRoot("RewriterConfig")]
    public class RewriterConfiguration
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (RewriterConfiguration));
		private static readonly object _threadLocker = new object();
        private RewriterRuleCollection _rules;

        public RewriterRuleCollection Rules
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

        public static RewriterConfiguration GetConfig()
        {
            var config = new RewriterConfiguration {Rules = new RewriterRuleCollection()};
            FileStream fileReader = null;
            string filePath = "";
            try
            {
                config = (RewriterConfiguration) DataCache.GetCache("RewriterConfig");
                if (config == null)
                {
	                lock (_threadLocker)
	                {
						config = (RewriterConfiguration) DataCache.GetCache("RewriterConfig");
		                if (config == null)
		                {
			                filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteUrls);

			                //Create a FileStream for the Config file
			                fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			                var doc = new XPathDocument(fileReader);
			                config = new RewriterConfiguration {Rules = new RewriterRuleCollection()};
			                foreach (XPathNavigator nav in doc.CreateNavigator().Select("RewriterConfig/Rules/RewriterRule"))
			                {
				                var rule = new RewriterRule
				                           {
					                           LookFor = nav.SelectSingleNode("LookFor").Value,
					                           SendTo = nav.SelectSingleNode("SendTo").Value
				                           };
				                config.Rules.Add(rule);
			                }
			                if (File.Exists(filePath))
			                {
				                //Set back into Cache
				                DataCache.SetCache("RewriterConfig", config, new DNNCacheDependency(filePath));
			                }
		                }
	                }
                }
            }
            catch (Exception ex)
            {
				//log it
                var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()};
                log.AddProperty("UrlRewriter.RewriterConfiguration", "GetConfig Failed");
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

        public static void SaveConfig(RewriterRuleCollection rules)
        {
            if (rules != null)
            {
                var config = new RewriterConfiguration {Rules = rules};
                
				//Create a new Xml Serializer
				var ser = new XmlSerializer(typeof (RewriterConfiguration));
                
				//Create a FileStream for the Config file
                var filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteUrls);
                if (File.Exists(filePath))
                {
					//make sure file is not read-only
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }
                using (var fileWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(fileWriter))
                {
                    //Serialize the RewriterConfiguration
                    ser.Serialize(writer, config);

                    //Close the Writers
                    writer.Close();
                    fileWriter.Close();
                }

                //Set Cache
                DataCache.SetCache("RewriterConfig", config, new DNNCacheDependency(filePath));
            }
        }
    }
}