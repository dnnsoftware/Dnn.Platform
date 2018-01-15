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