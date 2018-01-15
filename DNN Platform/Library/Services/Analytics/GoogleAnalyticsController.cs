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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Services.Analytics
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Analytics
    /// Module:     GoogleAnalytics
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Controller class definition for GoogleAnalytics which handles upgrades
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class GoogleAnalyticsController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (GoogleAnalyticsController));
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Handles module upgrades includes a new Google Analytics Asychronous script.
        /// </summary>
        /// <param name = "Version"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public void UpgradeModule(string Version)
        {
            // MD5 Hash value of the old synchronous script config file (from previous module versions)
            string[] TRADITIONAL_FILEHASHES = {"aRUf9NsElvrpiASJHHlmZg==", "+R2k5mvFvVhWsCm4WinyAA=="};

            switch (Version)
            {
                case "05.06.00":
                    //previous module versions
                    using (StreamReader fileReader = GetConfigFile())
                    {
                        if (fileReader != null)
                        {
                            var fileEncoding = new ASCIIEncoding();
                            using (var md5 = new MD5CryptoServiceProvider())
                            {
                                string currFileHashValue = "";

                                //calculate md5 hash of existing file
                                currFileHashValue = Convert.ToBase64String(md5.ComputeHash(fileEncoding.GetBytes(fileReader.ReadToEnd())));
                                fileReader.Close();

                                IEnumerable<string> result = (from h in TRADITIONAL_FILEHASHES where h == currFileHashValue select h);

                                //compare md5 hash
                                if (result.Any())
                                {
                                    //Copy new config file from \Config
                                    //True causes .config to be overwritten
                                    Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteAnalytics, true);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Retrieves the Google Analytics config file, "SiteAnalytics.config".
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private StreamReader GetConfigFile()
        {
            StreamReader fileReader = null;
            string filePath = "";
            try
            {
                filePath = Globals.ApplicationMapPath + "\\SiteAnalytics.config";

                if (File.Exists(filePath))
                {
                    fileReader = new StreamReader(filePath);
                }
            }
            catch (Exception ex)
            {
                //log it
                var log = new LogInfo {LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()};
                log.AddProperty("GoogleAnalytics.UpgradeModule", "GetConfigFile Failed");
                log.AddProperty("FilePath", filePath);
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                if (fileReader != null)
                {
                    fileReader.Close();
                }
                Logger.Error(ex);

            }

            return fileReader;
        }
    }
}