// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// The event log config service provides APIs for managing the
    /// Event Log configuration and individual logs itself.
    /// </summary>
    public interface IEventLogConfigService
    {
        /// <summary>
        /// Adds the log type.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <param name="fallbackConfigFile">The fallback configuration file.</param>
        void AddLogType(string configFile, string fallbackConfigFile);

        /// <summary>
        /// Adds the log type config info.
        /// </summary>
        /// <param name="logTypeConfig">The log type configuration.</param>
        void AddLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="logType">The log type.</param>
        void AddLogType(ILogTypeInfo logType);

        /// <summary>
        /// Clears the log.
        /// </summary>
        void ClearLog();

        /// <summary>
        /// Deletes the log.
        /// </summary>
        /// <param name="logInfo">the log info.</param>
        void DeleteLog(ILogInfo logInfo);

        /// <summary>
        /// Deletes the log type.
        /// </summary>
        /// <param name="logType">The log type.</param>
        void DeleteLogType(ILogTypeInfo logType);

        /// <summary>
        /// Delete the log type config info.
        /// </summary>
        /// <param name="logTypeConfig">The log type config.</param>
        void DeleteLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

        /// <summary>
        /// Get the logs.
        /// </summary>
        /// <param name="portalID">The portal id.</param>
        /// <param name="logType">The log type.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>The logs.</returns>
        IEnumerable<ILogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords);

        /// <summary>
        /// Gets the log type config info.
        /// </summary>
        /// <returns>The logs.</returns>
        ArrayList GetLogTypeConfigInfo();

        /// <summary>
        /// Get the log type config info by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The log type config info.</returns>
        ILogTypeConfigInfo GetLogTypeConfigInfoByID(string id);

        /// <summary>
        /// Gets the log type info dictionary.
        /// </summary>
        /// <returns>The dictionary.</returns>
        IDictionary<string, ILogTypeInfo> GetLogTypeInfoDictionary();

        /// <summary>
        /// Get a single log.
        /// </summary>
        /// <param name="log">The log to retrieve.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>The single log.</returns>
        object GetSingleLog(ILogInfo log, LoggingProviderReturnType returnType);

        /// <summary>
        /// Purge the log buffer.
        /// </summary>
        void PurgeLogBuffer();

        /// <summary>
        /// UPdate the log type config info.
        /// </summary>
        /// <param name="logTypeConfig">The log type config.</param>
        void UpdateLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

        /// <summary>
        /// Update the log type.
        /// </summary>
        /// <param name="logType">The log type info.</param>
        void UpdateLogType(ILogTypeInfo logType);
    }
}
