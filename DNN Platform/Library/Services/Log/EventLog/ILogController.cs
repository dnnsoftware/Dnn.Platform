// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections;
using System.Collections.Generic;

namespace DotNetNuke.Services.Log.EventLog
{
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

        void PurgeLogBuffer();

        void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig);

        void UpdateLogType(LogTypeInfo logType);
    }
}
