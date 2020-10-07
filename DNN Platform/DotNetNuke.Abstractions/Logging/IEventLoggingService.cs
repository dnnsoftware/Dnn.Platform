// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging
{
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Portals;

    /// <summary>
    /// Event Logging Service.
    /// </summary>
    public interface IEventLoggingService
    {
        /// <summary>
        /// Add setting log.
        /// </summary>
        /// <param name="logTypeKey">The log type key.</param>
        /// <param name="idFieldName">The id field name.</param>
        /// <param name="idValue">The id value.</param>
        /// <param name="settingName">The setting name.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="userId">The user id.</param>
        void AddSettingLog(EventLogType logTypeKey, string idFieldName, int idValue, string settingName, string settingValue, int userId);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="name">The log property name.</param>
        /// <param name="value">The log property value.</param>
        /// <param name="logType">The log type.</param>
        void AddLog(string name, string value, EventLogType logType);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="name">The log property name.</param>
        /// <param name="value">The log property value.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="logType">The log type.</param>
        void AddLog(string name, string value, IPortalSettings portalSettings, int userID, EventLogType logType);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="name">The log property name.</param>
        /// <param name="value">The log property value.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="logType">The log type.</param>
        void AddLog(string name, string value, IPortalSettings portalSettings, int userID, string logType);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="properties">The properties of the log.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="logTypeKey">The log type key.</param>
        /// <param name="bypassBuffering">The bypass buffering.</param>
        void AddLog(ILogProperties properties, IPortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="logType">The log type.</param>
        void AddLog(IPortalSettings portalSettings, int userID, EventLogType logType);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="businessObject">The business object.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="logType">The log type.</param>
        void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, EventLogType logType);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="businessObject">The business object.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="userName">The user name.</param>
        /// <param name="logType">The log type.</param>
        void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, string logType);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="logInfo">The log info.</param>
        void AddLog(ILogInfo logInfo);

        /// <summary>
        /// Adds the log type.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <param name="fallbackConfigFile">The fallback configuration file.</param>
        void AddLogType(string configFile, string fallbackConfigFile);

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="logType">The log type.</param>
        void AddLogType(ILogTypeInfo logType);

        /// <summary>
        /// Adds the log type config info.
        /// </summary>
        /// <param name="logTypeConfig">The log type configuration.</param>
        void AddLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig);

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
