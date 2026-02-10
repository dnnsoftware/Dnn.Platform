// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Logging;

    public interface ILogController
    {
        void AddLog(LogInfo logInfo);

        void AddLogType(string configFile, string fallbackConfigFile);

        void AddLogType(LogTypeInfo logType);

        void AddLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig);

        void ClearLog();

        void DeleteLog(LogInfo logInfo);

        void DeleteLogType(LogTypeInfo logType);

        void DeleteLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig);

        List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords);

        ArrayList GetLogTypeConfigInfo();

        LogTypeConfigInfo GetLogTypeConfigInfoByID(string id);

        Dictionary<string, LogTypeInfo> GetLogTypeInfoDictionary();

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService.GetLog()' instead. Scheduled for removal in v11.0.0.")]
        object GetSingleLog(LogInfo log, LoggingProvider.ReturnType returnType);

        /// <summary>Retrieves a single event log via the Log GUID.</summary>
        /// <param name="logGuid">A string representation of the log GUID.</param>
        /// <returns>The <see cref="ILogInfo"/>.</returns>
        ILogInfo GetLog(string logGuid);

        void PurgeLogBuffer();

        void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig);

        void UpdateLogType(LogTypeInfo logType);
    }
}
