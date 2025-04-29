// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.ComponentModel;

public abstract class LoggingProvider
{
    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService.GetLog()' instead. Scheduled for removal in v11.0.0.")]
    public enum ReturnType
    {
        LogInfoObjects = 0,
        XML = 1,
    }

    // return the provider
    public static LoggingProvider Instance()
    {
        return ComponentFactory.GetComponent<LoggingProvider>();
    }

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

    /// <summary>Retrieves a single event log via the Log Guid.</summary>
    /// <param name="logGuid">A string reprenstation of the log Guid.</param>
    /// <returns>The <see cref="ILogInfo"/>.</returns>
    public abstract ILogInfo GetLog(string logGuid);

    public abstract bool LoggingIsEnabled(string logType, int portalID);

    public abstract void PurgeLogBuffer();

    public abstract void SendLogNotifications();

    public abstract bool SupportsEmailNotification();

    public abstract bool SupportsInternalViewer();

    public abstract bool SupportsSendToCoreTeam();

    public abstract bool SupportsSendViaEmail();

    public abstract void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner);

    public abstract void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string notificationThresholdTime, string notificationThresholdTimeType, string mailFromAddress, string mailToAddress);
}
