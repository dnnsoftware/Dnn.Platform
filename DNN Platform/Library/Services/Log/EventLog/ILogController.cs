﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Log.EventLog
{
    using DotNetNuke.Abstractions.Logging;

    using System.Collections;
    using System.Collections.Generic;

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

        object GetSingleLog(LogInfo log, LoggingProvider.ReturnType returnType);

        /// <summary>
        /// Retrieves a single event log via the Log Guid.
        /// </summary>
        /// <param name="logGuid">A string reprenstation of the log Guid.</param>
        /// <returns>The <see cref="ILogInfo"/>.</returns>
        ILogInfo GetLog(string logGuid);

        void PurgeLogBuffer();

        void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig);

        void UpdateLogType(LogTypeInfo logType);
    }
}
