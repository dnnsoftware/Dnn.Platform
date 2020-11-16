// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Analytics
    /// Module:     GoogleAnalytics
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Controller class definition for GoogleAnalytics which handles upgrades.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class GoogleAnalyticsController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GoogleAnalyticsController));

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
            string[] TRADITIONAL_FILEHASHES = { "aRUf9NsElvrpiASJHHlmZg==", "+R2k5mvFvVhWsCm4WinyAA==" };

            switch (Version)
            {
                case "05.06.00":
                    // previous module versions
                    using (StreamReader fileReader = this.GetConfigFile())
                    {
                        if (fileReader != null)
                        {
                            var fileEncoding = new ASCIIEncoding();
                            using (var md5 = new MD5CryptoServiceProvider())
                            {
                                string currFileHashValue = string.Empty;

                                // calculate md5 hash of existing file
                                currFileHashValue = Convert.ToBase64String(md5.ComputeHash(fileEncoding.GetBytes(fileReader.ReadToEnd())));
                                fileReader.Close();

                                IEnumerable<string> result = from h in TRADITIONAL_FILEHASHES where h == currFileHashValue select h;

                                // compare md5 hash
                                if (result.Any())
                                {
                                    // Copy new config file from \Config
                                    // True causes .config to be overwritten
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
            string filePath = string.Empty;
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
                // log it
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
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
