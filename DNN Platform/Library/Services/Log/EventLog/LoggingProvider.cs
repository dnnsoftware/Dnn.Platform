// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using DotNetNuke.ComponentModel;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    public abstract class LoggingProvider
    {
        #region ReturnType enum

        public enum ReturnType
        {
            LogInfoObjects,
            XML
        }

        #endregion
		
		#region "Shared/Static Methods"

        //return the provider
		public static LoggingProvider Instance()
        {
            return ComponentFactory.GetComponent<LoggingProvider>();
        }
		
		#endregion

		#region "Abstract Methods"

        public abstract void AddLog(LogInfo logInfo);

        public abstract void AddLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner);

        public abstract void AddLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string notificationThresholdTime, string notificationThresholdTimeType, string mailFromAddress, string mailToAddress);

        public abstract void ClearLog();

        public abstract void DeleteLog(LogInfo logInfo);

        public abstract void DeleteLogType(string logTypeKey);

        public abstract void DeleteLogTypeConfigInfo(string id);

        public virtual List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            return new List<LogInfo>();
        }

        public abstract ArrayList GetLogTypeConfigInfo();

        public abstract ArrayList GetLogTypeInfo();

        public abstract LogTypeConfigInfo GetLogTypeConfigInfoByID(string id);

        public abstract object GetSingleLog(LogInfo logInfo, ReturnType returnType);

        public abstract bool LoggingIsEnabled(string logType, int portalID);

        public abstract void PurgeLogBuffer();

        public abstract void SendLogNotifications();

        public abstract bool SupportsEmailNotification();

        public abstract bool SupportsInternalViewer();

        public abstract bool SupportsSendToCoreTeam();

        public abstract bool SupportsSendViaEmail();

        public abstract void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner);

        public abstract void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string notificationThresholdTime, string notificationThresholdTimeType, string mailFromAddress, string mailToAddress);

		#endregion
    }
}
