// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;
using Microsoft.Extensions.DependencyInjection;

[DnnDeprecated(9, 8, 1, "Use dependency injection to resolve IEventLogger, IEventLogService or IEventLogConfigService instead")]
#pragma warning disable SA1601 // Partial elements should be documented, not documenting, the whole class is deprecated.
#pragma warning disable SA1600 // Elements should be documented, not documenting, the whole class is deprecated.
public partial class EventLogController : ServiceLocator<IEventLogController, EventLogController>, IEventLogController
{
    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.EventLogType' instead. Scheduled removal in v11.0.0.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1602:Enumeration items should be documented",
        Justification = "Not documenting since the whole class is deprecated.")]
    public enum EventLogType
    {
        USER_CREATED = 0,
        USER_DELETED = 1,
        LOGIN_SUPERUSER = 2,
        LOGIN_SUCCESS = 3,
        LOGIN_FAILURE = 4,
        LOGIN_USERLOCKEDOUT = 5,
        LOGIN_USERNOTAPPROVED = 6,
        CACHE_REFRESHED = 7,
        PASSWORD_SENT_SUCCESS = 8,
        PASSWORD_SENT_FAILURE = 9,
        LOG_NOTIFICATION_FAILURE = 10,
        PORTAL_CREATED = 11,
        PORTAL_DELETED = 12,
        PORTALGROUP_CREATED = 13,
        PORTALGROUP_DELETED = 14,
        PORTAL_ADDEDTOPORTALGROUP = 15,
        PORTAL_REMOVEDFROMPORTALGROUP = 16,
        TAB_CREATED = 17,
        TAB_UPDATED = 18,
        TAB_DELETED = 19,
        TAB_SENT_TO_RECYCLE_BIN = 20,
        TAB_RESTORED = 21,
        USER_ROLE_CREATED = 22,
        USER_ROLE_DELETED = 23,
        USER_ROLE_UPDATED = 24,
        ROLE_CREATED = 25,
        ROLE_UPDATED = 26,
        ROLE_DELETED = 27,
        MODULE_CREATED = 28,
        MODULE_UPDATED = 29,
        MODULE_DELETED = 30,
        MODULE_SENT_TO_RECYCLE_BIN = 31,
        MODULE_RESTORED = 32,
        SCHEDULER_EVENT_STARTED = 33,
        SCHEDULER_EVENT_PROGRESSING = 34,
        SCHEDULER_EVENT_COMPLETED = 35,
        APPLICATION_START = 36,
        APPLICATION_END = 37,
        APPLICATION_SHUTTING_DOWN = 38,
        SCHEDULER_STARTED = 39,
        SCHEDULER_SHUTTING_DOWN = 40,
        SCHEDULER_STOPPED = 41,
        ADMIN_ALERT = 42,
        HOST_ALERT = 43,
        CACHE_REMOVED = 44,
        CACHE_EXPIRED = 45,
        CACHE_UNDERUSED = 46,
        CACHE_DEPENDENCYCHANGED = 47,
        CACHE_OVERFLOW = 48,
        CACHE_REFRESH = 49,
        LISTENTRY_CREATED = 50,
        LISTENTRY_UPDATED = 51,
        LISTENTRY_DELETED = 52,
        DESKTOPMODULE_CREATED = 53,
        DESKTOPMODULE_UPDATED = 54,
        DESKTOPMODULE_DELETED = 55,
        SKINCONTROL_CREATED = 56,
        SKINCONTROL_UPDATED = 57,
        SKINCONTROL_DELETED = 58,
        PORTALALIAS_CREATED = 59,
        PORTALALIAS_UPDATED = 60,
        PORTALALIAS_DELETED = 61,
        PROFILEPROPERTY_CREATED = 62,
        PROFILEPROPERTY_UPDATED = 63,
        PROFILEPROPERTY_DELETED = 64,
        USER_UPDATED = 65,
        DESKTOPMODULEPERMISSION_CREATED = 66,
        DESKTOPMODULEPERMISSION_UPDATED = 67,
        DESKTOPMODULEPERMISSION_DELETED = 68,
        PERMISSION_CREATED = 69,
        PERMISSION_UPDATED = 70,
        PERMISSION_DELETED = 71,
        TABPERMISSION_CREATED = 72,
        TABPERMISSION_UPDATED = 73,
        TABPERMISSION_DELETED = 74,
        AUTHENTICATION_CREATED = 75,
        AUTHENTICATION_UPDATED = 76,
        AUTHENTICATION_DELETED = 77,
        FILE_ADDED = 78,
        FILE_CHANGED = 79,
        FILE_DELETED = 80,
        FILE_DOWNLOADED = 81,
        FILE_MOVED = 82,
        FILE_OVERWRITTEN = 83,
        FILE_RENAMED = 84,
        FILE_METADATACHANGED = 85,
        FOLDER_CREATED = 86,
        FOLDER_UPDATED = 87,
        FOLDER_DELETED = 88,
        PACKAGE_CREATED = 89,
        PACKAGE_UPDATED = 90,
        PACKAGE_DELETED = 91,
        LANGUAGEPACK_CREATED = 92,
        LANGUAGEPACK_UPDATED = 93,
        LANGUAGEPACK_DELETED = 94,
        LANGUAGE_CREATED = 95,
        LANGUAGE_UPDATED = 96,
        LANGUAGE_DELETED = 97,
        LIBRARY_UPDATED = 98,
        SKINPACKAGE_CREATED = 99,
        SKINPACKAGE_UPDATED = 100,
        SKINPACKAGE_DELETED = 101,
        SCHEDULE_CREATED = 102,
        SCHEDULE_UPDATED = 103,
        SCHEDULE_DELETED = 104,
        HOST_SETTING_CREATED = 105,
        HOST_SETTING_UPDATED = 106,
        HOST_SETTING_DELETED = 107,
        PORTALDESKTOPMODULE_CREATED = 108,
        PORTALDESKTOPMODULE_UPDATED = 109,
        PORTALDESKTOPMODULE_DELETED = 110,
        TABMODULE_CREATED = 111,
        TABMODULE_UPDATED = 112,
        TABMODULE_DELETED = 113,
        TABMODULE_SETTING_CREATED = 114,
        TABMODULE_SETTING_UPDATED = 115,
        TABMODULE_SETTING_DELETED = 116,
        MODULE_SETTING_CREATED = 117,
        MODULE_SETTING_UPDATED = 118,
        MODULE_SETTING_DELETED = 119,
        PORTAL_SETTING_CREATED = 120,
        PORTAL_SETTING_UPDATED = 121,
        PORTAL_SETTING_DELETED = 122,
        PORTALINFO_CREATED = 123,
        PORTALINFO_UPDATED = 124,
        PORTALINFO_DELETED = 125,
        AUTHENTICATION_USER_CREATED = 126,
        AUTHENTICATION_USER_UPDATED = 127,
        AUTHENTICATION_USER_DELETED = 128,
        LANGUAGETOPORTAL_CREATED = 129,
        LANGUAGETOPORTAL_UPDATED = 130,
        LANGUAGETOPORTAL_DELETED = 131,
        TAB_ORDER_UPDATED = 132,
        TAB_SETTING_CREATED = 133,
        TAB_SETTING_UPDATED = 134,
        TAB_SETTING_DELETED = 135,
        HOST_SQL_EXECUTED = 136,
        USER_RESTORED = 137,
        USER_REMOVED = 138,
        USER_IMPERSONATED = 139,
        USERNAME_UPDATED = 140,
        IP_LOGIN_BANNED = 141,
        PAGE_NOT_FOUND_404 = 142,
        TABURL_CREATED = 143,
        TABURL_UPDATED = 144,
        TABURL_DELETED = 145,
        SCRIPT_COLLISION = 146,
        POTENTIAL_PAYPAL_PAYMENT_FRAUD = 147,
        WEBSERVER_CREATED = 148,
        WEBSERVER_UPDATED = 149,
        WEBSERVER_DISABLED = 150,
        WEBSERVER_ENABLED = 151,
        WEBSERVER_PINGFAILED = 152,
        FOLDER_MOVED = 153,
        PORTALPERMISSION_DELETED = 154,
        PORTALPERMISSION_CREATED = 155,
        PORTALPERMISSION_UPDATED = 156,
    }

    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public static partial void AddSettingLog(EventLogType logTypeKey, string idFieldName, int idValue, string settingName, string settingValue, int userId) =>
        Globals.DependencyProvider.GetRequiredService<IEventLogger>()
            .AddSettingLog((Abstractions.Logging.EventLogType)logTypeKey, idFieldName, idValue, settingName, settingValue, userId);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(string propertyName, string propertyValue, EventLogType logType) =>
        this.EventLogger.AddLog(propertyName, propertyValue, (Abstractions.Logging.EventLogType)logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 7, 0, "It has been replaced by the overload taking IPortalSettings")]
    public partial void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, EventLogType logType) =>
        this.AddLog(propertyName, propertyValue, (IPortalSettings)portalSettings, userID, logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(string propertyName, string propertyValue, IPortalSettings portalSettings, int userID, EventLogType logType) =>
        this.EventLogger.AddLog(propertyName, propertyValue, portalSettings, userID, (Abstractions.Logging.EventLogType)logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 7, 0, "It has been replaced by the overload taking IPortalSettings")]
    public partial void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, string logType) =>
        this.AddLog(propertyName, propertyValue, (IPortalSettings)portalSettings, userID, logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(string propertyName, string propertyValue, IPortalSettings portalSettings, int userID, string logType) =>
        this.EventLogger.AddLog(propertyName, propertyValue, portalSettings, userID, logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 7, 0, "It has been replaced by the overload taking IPortalSettings")]
    public partial void AddLog(LogProperties properties, PortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering) =>
        this.AddLog(properties, (IPortalSettings)portalSettings, userID, logTypeKey, bypassBuffering);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(LogProperties properties, IPortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering) =>
        this.EventLogger.AddLog(properties, portalSettings, userID, logTypeKey, bypassBuffering);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 7, 0, "It has been replaced by the overload taking IPortalSettings")]
    public partial void AddLog(PortalSettings portalSettings, int userID, EventLogType logType) =>
        this.AddLog((IPortalSettings)portalSettings, userID, logType);

    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(IPortalSettings portalSettings, int userID, EventLogType logType) =>
        this.EventLogger.AddLog(portalSettings, userID, (Abstractions.Logging.EventLogType)logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 7, 0, "It has been replaced by the overload taking IPortalSettings")]
    public partial void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, EventLogType logType) =>
        this.AddLog(businessObject, (IPortalSettings)portalSettings, userID, userName, logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, EventLogType logType) =>
        this.EventLogger.AddLog(businessObject, portalSettings, userID, userName, (Abstractions.Logging.EventLogType)logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 7, 0, "It has been replaced by the overload taking IPortalSettings")]
    public partial void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, string logType) =>
        this.AddLog(businessObject, (IPortalSettings)portalSettings, userID, userName, logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, string logType) =>
        this.EventLogger.AddLog(businessObject, portalSettings, userID, userName, logType);

    /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
    public partial void AddLog(LogInfo logInfo) =>
        this.EventLogger.AddLog(logInfo);

    /// <inheritdoc cref="IEventLogConfigService.AddLogType(string,string)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial void AddLogType(string configFile, string fallbackConfigFile) =>
        this.EventLogConfigService.AddLogType(configFile, fallbackConfigFile);

    /// <inheritdoc cref="IEventLogConfigService.AddLogType(string,string)"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial void AddLogType(LogTypeInfo logType) =>
        this.EventLogConfigService.AddLogType(logType);

    /// <inheritdoc cref="IEventLogConfigService.AddLogTypeConfigInfo"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial void AddLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig) =>
        this.EventLogConfigService.AddLogTypeConfigInfo(logTypeConfig);

    /// <inheritdoc cref="IEventLogService.ClearLog"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService' instead")]
    public partial void ClearLog() =>
        this.EventLogService.ClearLog();

    /// <inheritdoc cref="IEventLogService.DeleteLog"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService' instead")]
    public partial void DeleteLog(LogInfo logInfo) =>
        this.EventLogService.DeleteLog(logInfo);

    /// <inheritdoc cref="IEventLogConfigService.DeleteLogType"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial void DeleteLogType(LogTypeInfo logType) =>
        this.EventLogConfigService.DeleteLogType(logType);

    /// <inheritdoc cref="IEventLogConfigService.DeleteLogTypeConfigInfo"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial void DeleteLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig) =>
        this.EventLogConfigService.DeleteLogTypeConfigInfo(logTypeConfig);

    /// <inheritdoc cref="IEventLogService.GetLogs"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService' instead")]
    public partial List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords) =>
        this.EventLogService.GetLogs(portalID, logType, pageSize, pageIndex, ref totalRecords).Cast<LogInfo>().ToList();

    /// <inheritdoc cref="ILogController.GetLogTypeConfigInfo"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial ArrayList GetLogTypeConfigInfo() =>
        LogController.Instance.GetLogTypeConfigInfo();

    /// <inheritdoc cref="IEventLogConfigService.GetLogTypeConfigInfoByID"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial LogTypeConfigInfo GetLogTypeConfigInfoByID(string id) =>
        (LogTypeConfigInfo)this.EventLogConfigService.GetLogTypeConfigInfoByID(id);

    /// <inheritdoc cref="IEventLogConfigService.GetLogTypeInfoDictionary"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public partial Dictionary<string, LogTypeInfo> GetLogTypeInfoDictionary() =>
        this.EventLogConfigService
            .GetLogTypeInfoDictionary()
            .ToDictionary(key => key.Key, value => (LogTypeInfo)value.Value);

    /// <inheritdoc cref="ILogController.GetSingleLog"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService.GetLog()' instead")]
    public partial object GetSingleLog(LogInfo log, LoggingProvider.ReturnType returnType) =>
        LogController.Instance.GetSingleLog(log, returnType);

    /// <inheritdoc cref="IEventLogService.PurgeLogBuffer"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogService' instead")]
    public partial void PurgeLogBuffer() =>
        this.EventLogService.PurgeLogBuffer();

    /// <inheritdoc cref="IEventLogConfigService.UpdateLogTypeConfigInfo"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public virtual partial void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig) =>
        this.EventLogConfigService.UpdateLogTypeConfigInfo(logTypeConfig);

    /// <inheritdoc cref="IEventLogConfigService.UpdateLogType"/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogConfigService' instead")]
    public virtual partial void UpdateLogType(LogTypeInfo logType) =>
        this.EventLogConfigService.UpdateLogType(logType);

    /// <inheritdoc/>
    [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    protected override partial Func<IEventLogController> GetFactory() =>
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        () => new EventLogController();
}
#pragma warning restore SA1601 // Partial elements should be documented
#pragma warning restore SA1600 // Elements should be documented
