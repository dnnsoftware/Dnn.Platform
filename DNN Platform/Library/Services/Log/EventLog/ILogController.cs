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
