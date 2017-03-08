#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
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

using System;
using Dnn.ExportImport.Components.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Log.EventLog;

namespace Dnn.ExportImport.Components.Controllers
{
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
                        AddNewLogTypes();
                        break;
                }
                return "Success";
            }
            catch (Exception)
            {
                return "Failed";
            }
        }

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
    }
}
