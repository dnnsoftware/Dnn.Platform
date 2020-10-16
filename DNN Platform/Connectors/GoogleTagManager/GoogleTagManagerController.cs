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
    public class GoogleTagManagerController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GoogleTagManagerController));

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
