// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Controllers
{
    using System;

    using DotNetNuke.Entities.Modules;

    /// <summary>
    ///
    /// </summary>
    public class BusinessController : IUpgradeable
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string UpgradeModule(string version)
        {
            try
            {
                switch (version)
                {
                    case "09.01.00":
                        // AddNewLogTypes(); // moved to SQL migration script
                        break;
                }

                return "Success";
            }
            catch (Exception)
            {
                return "Failed";
            }
        }

        /*
        private static void AddNewLogTypes()
        {
            var logTypeInfo = new LogTypeInfo
            {
                LogTypeKey = Constants.LogTypeSiteExport,
                LogTypeFriendlyName = "Site Export Request",
                LogTypeDescription = "",
                LogTypeCSSClass = "GeneralAdminOperation",
                LogTypeOwner = "DotNetNuke.Logging.EventLogType"
            };
            LogController.Instance.AddLogType(logTypeInfo);

            logTypeInfo.LogTypeKey = Constants.LogTypeSiteImport;
            logTypeInfo.LogTypeFriendlyName = "Site Import Request";
            LogController.Instance.AddLogType(logTypeInfo);

            var logTypeConf = new LogTypeConfigInfo
            {
                LoggingIsActive = true,
                LogTypeKey = Constants.LogTypeSiteExport,
                KeepMostRecent = "50",
                NotificationThreshold = 1,
                NotificationThresholdTime = 1,
                NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Minutes,
                MailFromAddress = Null.NullString,
                MailToAddress = Null.NullString,
                LogTypePortalID = "*"
            };
            LogController.Instance.AddLogTypeConfigInfo(logTypeConf);

            logTypeConf.LogTypeKey = Constants.LogTypeSiteImport;
            LogController.Instance.AddLogTypeConfigInfo(logTypeConf);
        }
         */
    }
}
