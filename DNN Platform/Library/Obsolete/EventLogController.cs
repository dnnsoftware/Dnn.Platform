// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
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
        /// <inheritdoc cref="DotNetNuke.Abstractions.Logging.EventLogType" />
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.EventLogType' instead. Scheduled removal in v11.0.0.")]
        public enum EventLogType
        {
#pragma warning disable CA1707 // Identifiers should not contain underscores
            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_CREATED" />
            USER_CREATED = 0,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_DELETED" />
            USER_DELETED = 1,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LOGIN_SUPERUSER" />
            LOGIN_SUPERUSER = 2,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LOGIN_SUCCESS" />
            LOGIN_SUCCESS = 3,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LOGIN_FAILURE" />
            LOGIN_FAILURE = 4,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LOGIN_USERLOCKEDOUT" />
            LOGIN_USERLOCKEDOUT = 5,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LOGIN_USERNOTAPPROVED" />
            LOGIN_USERNOTAPPROVED = 6,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_REFRESHED" />
            CACHE_REFRESHED = 7,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PASSWORD_SENT_SUCCESS" />
            PASSWORD_SENT_SUCCESS = 8,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PASSWORD_SENT_FAILURE" />
            PASSWORD_SENT_FAILURE = 9,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LOG_NOTIFICATION_FAILURE" />
            LOG_NOTIFICATION_FAILURE = 10,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_CREATED" />
            PORTAL_CREATED = 11,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_DELETED" />
            PORTAL_DELETED = 12,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALGROUP_CREATED" />
            PORTALGROUP_CREATED = 13,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALGROUP_DELETED" />
            PORTALGROUP_DELETED = 14,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_ADDEDTOPORTALGROUP" />
            PORTAL_ADDEDTOPORTALGROUP = 15,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_REMOVEDFROMPORTALGROUP" />
            PORTAL_REMOVEDFROMPORTALGROUP = 16,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_CREATED" />
            TAB_CREATED = 17,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_UPDATED" />
            TAB_UPDATED = 18,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_DELETED" />
            TAB_DELETED = 19,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_SENT_TO_RECYCLE_BIN" />
            TAB_SENT_TO_RECYCLE_BIN = 20,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_RESTORED" />
            TAB_RESTORED = 21,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_ROLE_CREATED" />
            USER_ROLE_CREATED = 22,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_ROLE_DELETED" />
            USER_ROLE_DELETED = 23,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_ROLE_UPDATED" />
            USER_ROLE_UPDATED = 24,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.ROLE_CREATED" />
            ROLE_CREATED = 25,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.ROLE_UPDATED" />
            ROLE_UPDATED = 26,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.ROLE_DELETED" />
            ROLE_DELETED = 27,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_CREATED" />
            MODULE_CREATED = 28,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_UPDATED" />
            MODULE_UPDATED = 29,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_DELETED" />
            MODULE_DELETED = 30,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_SENT_TO_RECYCLE_BIN" />
            MODULE_SENT_TO_RECYCLE_BIN = 31,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_RESTORED" />
            MODULE_RESTORED = 32,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULER_EVENT_STARTED" />
            SCHEDULER_EVENT_STARTED = 33,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULER_EVENT_PROGRESSING" />
            SCHEDULER_EVENT_PROGRESSING = 34,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULER_EVENT_COMPLETED" />
            SCHEDULER_EVENT_COMPLETED = 35,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.APPLICATION_START" />
            APPLICATION_START = 36,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.APPLICATION_END" />
            APPLICATION_END = 37,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.APPLICATION_SHUTTING_DOWN" />
            APPLICATION_SHUTTING_DOWN = 38,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULER_STARTED" />
            SCHEDULER_STARTED = 39,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULER_SHUTTING_DOWN" />
            SCHEDULER_SHUTTING_DOWN = 40,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULER_STOPPED" />
            SCHEDULER_STOPPED = 41,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.ADMIN_ALERT" />
            ADMIN_ALERT = 42,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.HOST_ALERT" />
            HOST_ALERT = 43,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_REMOVED" />
            CACHE_REMOVED = 44,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_EXPIRED" />
            CACHE_EXPIRED = 45,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_UNDERUSED" />
            CACHE_UNDERUSED = 46,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_DEPENDENCYCHANGED" />
            CACHE_DEPENDENCYCHANGED = 47,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_OVERFLOW" />
            CACHE_OVERFLOW = 48,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.CACHE_REFRESH" />
            CACHE_REFRESH = 49,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LISTENTRY_CREATED" />
            LISTENTRY_CREATED = 50,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LISTENTRY_UPDATED" />
            LISTENTRY_UPDATED = 51,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LISTENTRY_DELETED" />
            LISTENTRY_DELETED = 52,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.DESKTOPMODULE_CREATED" />
            DESKTOPMODULE_CREATED = 53,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.DESKTOPMODULE_UPDATED" />
            DESKTOPMODULE_UPDATED = 54,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.DESKTOPMODULE_DELETED" />
            DESKTOPMODULE_DELETED = 55,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SKINCONTROL_CREATED" />
            SKINCONTROL_CREATED = 56,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SKINCONTROL_UPDATED" />
            SKINCONTROL_UPDATED = 57,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SKINCONTROL_DELETED" />
            SKINCONTROL_DELETED = 58,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALALIAS_CREATED" />
            PORTALALIAS_CREATED = 59,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALALIAS_UPDATED" />
            PORTALALIAS_UPDATED = 60,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALALIAS_DELETED" />
            PORTALALIAS_DELETED = 61,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PROFILEPROPERTY_CREATED" />
            PROFILEPROPERTY_CREATED = 62,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PROFILEPROPERTY_UPDATED" />
            PROFILEPROPERTY_UPDATED = 63,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PROFILEPROPERTY_DELETED" />
            PROFILEPROPERTY_DELETED = 64,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_UPDATED" />
            USER_UPDATED = 65,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.DESKTOPMODULEPERMISSION_CREATED" />
            DESKTOPMODULEPERMISSION_CREATED = 66,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.DESKTOPMODULEPERMISSION_UPDATED" />
            DESKTOPMODULEPERMISSION_UPDATED = 67,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.DESKTOPMODULEPERMISSION_DELETED" />
            DESKTOPMODULEPERMISSION_DELETED = 68,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PERMISSION_CREATED" />
            PERMISSION_CREATED = 69,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PERMISSION_UPDATED" />
            PERMISSION_UPDATED = 70,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PERMISSION_DELETED" />
            PERMISSION_DELETED = 71,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABPERMISSION_CREATED" />
            TABPERMISSION_CREATED = 72,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABPERMISSION_UPDATED" />
            TABPERMISSION_UPDATED = 73,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABPERMISSION_DELETED" />
            TABPERMISSION_DELETED = 74,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.AUTHENTICATION_CREATED" />
            AUTHENTICATION_CREATED = 75,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.AUTHENTICATION_UPDATED" />
            AUTHENTICATION_UPDATED = 76,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.AUTHENTICATION_DELETED" />
            AUTHENTICATION_DELETED = 77,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_ADDED" />
            FILE_ADDED = 78,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_CHANGED" />
            FILE_CHANGED = 79,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_DELETED" />
            FILE_DELETED = 80,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_DOWNLOADED" />
            FILE_DOWNLOADED = 81,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_MOVED" />
            FILE_MOVED = 82,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_OVERWRITTEN" />
            FILE_OVERWRITTEN = 83,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_RENAMED" />
            FILE_RENAMED = 84,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FILE_METADATACHANGED" />
            FILE_METADATACHANGED = 85,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FOLDER_CREATED" />
            FOLDER_CREATED = 86,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FOLDER_UPDATED" />
            FOLDER_UPDATED = 87,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FOLDER_DELETED" />
            FOLDER_DELETED = 88,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PACKAGE_CREATED" />
            PACKAGE_CREATED = 89,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PACKAGE_UPDATED" />
            PACKAGE_UPDATED = 90,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PACKAGE_DELETED" />
            PACKAGE_DELETED = 91,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGEPACK_CREATED" />
            LANGUAGEPACK_CREATED = 92,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGEPACK_UPDATED" />
            LANGUAGEPACK_UPDATED = 93,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGEPACK_DELETED" />
            LANGUAGEPACK_DELETED = 94,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGE_CREATED" />
            LANGUAGE_CREATED = 95,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGE_UPDATED" />
            LANGUAGE_UPDATED = 96,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGE_DELETED" />
            LANGUAGE_DELETED = 97,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LIBRARY_UPDATED" />
            LIBRARY_UPDATED = 98,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SKINPACKAGE_CREATED" />
            SKINPACKAGE_CREATED = 99,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SKINPACKAGE_UPDATED" />
            SKINPACKAGE_UPDATED = 100,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SKINPACKAGE_DELETED" />
            SKINPACKAGE_DELETED = 101,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULE_CREATED" />
            SCHEDULE_CREATED = 102,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULE_UPDATED" />
            SCHEDULE_UPDATED = 103,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCHEDULE_DELETED" />
            SCHEDULE_DELETED = 104,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.HOST_SETTING_CREATED" />
            HOST_SETTING_CREATED = 105,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.HOST_SETTING_UPDATED" />
            HOST_SETTING_UPDATED = 106,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.HOST_SETTING_DELETED" />
            HOST_SETTING_DELETED = 107,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALDESKTOPMODULE_CREATED" />
            PORTALDESKTOPMODULE_CREATED = 108,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALDESKTOPMODULE_UPDATED" />
            PORTALDESKTOPMODULE_UPDATED = 109,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALDESKTOPMODULE_DELETED" />
            PORTALDESKTOPMODULE_DELETED = 110,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABMODULE_CREATED" />
            TABMODULE_CREATED = 111,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABMODULE_UPDATED" />
            TABMODULE_UPDATED = 112,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABMODULE_DELETED" />
            TABMODULE_DELETED = 113,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABMODULE_SETTING_CREATED" />
            TABMODULE_SETTING_CREATED = 114,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABMODULE_SETTING_UPDATED" />
            TABMODULE_SETTING_UPDATED = 115,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABMODULE_SETTING_DELETED" />
            TABMODULE_SETTING_DELETED = 116,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_SETTING_CREATED" />
            MODULE_SETTING_CREATED = 117,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_SETTING_UPDATED" />
            MODULE_SETTING_UPDATED = 118,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.MODULE_SETTING_DELETED" />
            MODULE_SETTING_DELETED = 119,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_SETTING_CREATED" />
            PORTAL_SETTING_CREATED = 120,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_SETTING_UPDATED" />
            PORTAL_SETTING_UPDATED = 121,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTAL_SETTING_DELETED" />
            PORTAL_SETTING_DELETED = 122,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALINFO_CREATED" />
            PORTALINFO_CREATED = 123,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALINFO_UPDATED" />
            PORTALINFO_UPDATED = 124,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALINFO_DELETED" />
            PORTALINFO_DELETED = 125,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.AUTHENTICATION_USER_CREATED" />
            AUTHENTICATION_USER_CREATED = 126,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.AUTHENTICATION_USER_UPDATED" />
            AUTHENTICATION_USER_UPDATED = 127,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.AUTHENTICATION_USER_DELETED" />
            AUTHENTICATION_USER_DELETED = 128,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGETOPORTAL_CREATED" />
            LANGUAGETOPORTAL_CREATED = 129,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGETOPORTAL_UPDATED" />
            LANGUAGETOPORTAL_UPDATED = 130,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.LANGUAGETOPORTAL_DELETED" />
            LANGUAGETOPORTAL_DELETED = 131,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_ORDER_UPDATED" />
            TAB_ORDER_UPDATED = 132,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_SETTING_CREATED" />
            TAB_SETTING_CREATED = 133,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_SETTING_UPDATED" />
            TAB_SETTING_UPDATED = 134,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TAB_SETTING_DELETED" />
            TAB_SETTING_DELETED = 135,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.HOST_SQL_EXECUTED" />
            HOST_SQL_EXECUTED = 136,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_RESTORED" />
            USER_RESTORED = 137,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_REMOVED" />
            USER_REMOVED = 138,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USER_IMPERSONATED" />
            USER_IMPERSONATED = 139,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.USERNAME_UPDATED" />
            USERNAME_UPDATED = 140,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.IP_LOGIN_BANNED" />
            IP_LOGIN_BANNED = 141,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PAGE_NOT_FOUND_404" />
            PAGE_NOT_FOUND_404 = 142,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABURL_CREATED" />
            TABURL_CREATED = 143,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABURL_UPDATED" />
            TABURL_UPDATED = 144,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.TABURL_DELETED" />
            TABURL_DELETED = 145,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.SCRIPT_COLLISION" />
            SCRIPT_COLLISION = 146,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD" />
            POTENTIAL_PAYPAL_PAYMENT_FRAUD = 147,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.WEBSERVER_CREATED" />
            WEBSERVER_CREATED = 148,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.WEBSERVER_UPDATED" />
            WEBSERVER_UPDATED = 149,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.WEBSERVER_DISABLED" />
            WEBSERVER_DISABLED = 150,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.WEBSERVER_ENABLED" />
            WEBSERVER_ENABLED = 151,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.WEBSERVER_PINGFAILED" />
            WEBSERVER_PINGFAILED = 152,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.FOLDER_MOVED" />
            FOLDER_MOVED = 153,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALPERMISSION_DELETED" />
            PORTALPERMISSION_DELETED = 154,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALPERMISSION_CREATED" />
            PORTALPERMISSION_CREATED = 155,

            /// <inheritdoc cref="Abstractions.Logging.EventLogType.PORTALPERMISSION_UPDATED" />
            PORTALPERMISSION_UPDATED = 156,
#pragma warning restore CA1707
        }

        [DnnDeprecated(9, 8, 0, "Use Dependency Injection to resolve 'DotNetNuke.Abstractions.Logging.IEventLogger' instead")]
        public static partial void AddSettingLog(EventLogType logTypeKey, string idFieldName, int idValue, string settingName, string settingValue, int userId) =>
            Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>()
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

        /// <inheritdoc cref="IEventLogger.AddLog(string,string,DotNetNuke.Abstractions.Logging.EventLogType)"/>
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
        protected override Func<IEventLogController> GetFactory() => () => new EventLogController();
    }
#pragma warning restore SA1601 // Partial elements should be documented
#pragma warning restore SA1600 // Elements should be documented
}
