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
    ///   Controller class definition for GoogleTagManager which handles upgrades.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class GoogleTagManagerController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GoogleTagManagerController));

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Handles module upgrades includes a new Google Tag Manager Asychronous script.
        /// </summary>
        /// <param name = "version">Target Version number for the upgrade.</param>
        /// -----------------------------------------------------------------------------
        public void UpgradeModule(string version)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Retrieves the Google Tag Manager config file, "GoogleTagManager.config".
        /// </summary>
        /// <returns>Streamreader for the Google Tag Manager config file.</returns>
        /// -----------------------------------------------------------------------------
        private StreamReader GetConfigFile()
        {
            StreamReader fileReader = null;
            string filePath = string.Empty;
            try
            {
                filePath = Globals.ApplicationMapPath + "\\DesktopModules\\Connectors\\GoogleTagManager\\GoogleTagManager.config";

                if (File.Exists(filePath))
                {
                    fileReader = new StreamReader(filePath);
                }
            }
            catch (Exception ex)
            {
                // log it
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("GoogleTagManager", "GetConfigFile Failed");
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
