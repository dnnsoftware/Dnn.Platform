// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

/// <summary>System Event Log Types.</summary>
public enum EventLogType
{
    /// <summary>User Created.</summary>
    USER_CREATED = 0,

    /// <summary>User Deleted.</summary>
    USER_DELETED = 1,

    /// <summary>Login Super User.</summary>
    LOGIN_SUPERUSER = 2,

    /// <summary>Log Success.</summary>
    LOGIN_SUCCESS = 3,

    /// <summary>Login failure.</summary>
    LOGIN_FAILURE = 4,

    /// <summary>Login user locked out.</summary>
    LOGIN_USERLOCKEDOUT = 5,

    /// <summary>Login user not approved.</summary>
    LOGIN_USERNOTAPPROVED = 6,

    /// <summary>Cache refreshed.</summary>
    CACHE_REFRESHED = 7,

    /// <summary>Password sent success.</summary>
    PASSWORD_SENT_SUCCESS = 8,

    /// <summary>Password sent failure.</summary>
    PASSWORD_SENT_FAILURE = 9,

    /// <summary>log notification failure.</summary>
    LOG_NOTIFICATION_FAILURE = 10,

    /// <summary>Portal created.</summary>
    PORTAL_CREATED = 11,

    /// <summary>Portal deleted.</summary>
    PORTAL_DELETED = 12,

    /// <summary>Portal group created.</summary>
    PORTALGROUP_CREATED = 13,

    /// <summary>Portal group deleted.</summary>
    PORTALGROUP_DELETED = 14,

    /// <summary>Portal added to portal group.</summary>
    PORTAL_ADDEDTOPORTALGROUP = 15,

    /// <summary>Portal removed from portal group.</summary>
    PORTAL_REMOVEDFROMPORTALGROUP = 16,

    /// <summary>Tab created.</summary>
    TAB_CREATED = 17,

    /// <summary>Tab updated.</summary>
    TAB_UPDATED = 18,

    /// <summary>Tab deleted.</summary>
    TAB_DELETED = 19,

    /// <summary>Tab sent to recycle bin.</summary>
    TAB_SENT_TO_RECYCLE_BIN = 20,

    /// <summary>Tab restored.</summary>
    TAB_RESTORED = 21,

    /// <summary>User role created.</summary>
    USER_ROLE_CREATED = 22,

    /// <summary>User role deleted.</summary>
    USER_ROLE_DELETED = 23,

    /// <summary>User role updated.</summary>
    USER_ROLE_UPDATED = 24,

    /// <summary>Role created.</summary>
    ROLE_CREATED = 25,

    /// <summary>Role updated.</summary>
    ROLE_UPDATED = 26,

    /// <summary>Role deleted.</summary>
    ROLE_DELETED = 27,

    /// <summary>Module created.</summary>
    MODULE_CREATED = 28,

    /// <summary>Module updated.</summary>
    MODULE_UPDATED = 29,

    /// <summary>Module deleted.</summary>
    MODULE_DELETED = 30,

    /// <summary>Module sent to recycle bin.</summary>
    MODULE_SENT_TO_RECYCLE_BIN = 31,

    /// <summary>Module restored.</summary>
    MODULE_RESTORED = 32,

    /// <summary>scheduler event started.</summary>
    SCHEDULER_EVENT_STARTED = 33,

    /// <summary>Scheduler event progressing.</summary>
    SCHEDULER_EVENT_PROGRESSING = 34,

    /// <summary>Scheduler event completed.</summary>
    SCHEDULER_EVENT_COMPLETED = 35,

    /// <summary>Application start.</summary>
    APPLICATION_START = 36,

    /// <summary>Application end.</summary>
    APPLICATION_END = 37,

    /// <summary>Application shutting down.</summary>
    APPLICATION_SHUTTING_DOWN = 38,

    /// <summary>Scheduler started.</summary>
    SCHEDULER_STARTED = 39,

    /// <summary>Scheduler shutting down.</summary>
    SCHEDULER_SHUTTING_DOWN = 40,

    /// <summary>Scheduler stopped.</summary>
    SCHEDULER_STOPPED = 41,

    /// <summary>Admin alert.</summary>
    ADMIN_ALERT = 42,

    /// <summary>Host alert.</summary>
    HOST_ALERT = 43,

    /// <summary>Cache removed.</summary>
    CACHE_REMOVED = 44,

    /// <summary>Cache expired.</summary>
    CACHE_EXPIRED = 45,

    /// <summary>Cache under used.</summary>
    CACHE_UNDERUSED = 46,

    /// <summary>Cache dependency changed.</summary>
    CACHE_DEPENDENCYCHANGED = 47,

    /// <summary>Cache overflow.</summary>
    CACHE_OVERFLOW = 48,

    /// <summary>Cache refresh.</summary>
    CACHE_REFRESH = 49,

    /// <summary>Lisentry created.</summary>
    LISTENTRY_CREATED = 50,

    /// <summary>Lisentry updated.</summary>
    LISTENTRY_UPDATED = 51,

    /// <summary>Lisentry deleted.</summary>
    LISTENTRY_DELETED = 52,

    /// <summary>Desktop Module created.</summary>
    DESKTOPMODULE_CREATED = 53,

    /// <summary>Desktop Module updated.</summary>
    DESKTOPMODULE_UPDATED = 54,

    /// <summary>Desktop Module deleted.</summary>
    DESKTOPMODULE_DELETED = 55,

    /// <summary>Skin control created.</summary>
    SKINCONTROL_CREATED = 56,

    /// <summary>Skin control updated.</summary>
    SKINCONTROL_UPDATED = 57,

    /// <summary>Skin control deleted.</summary>
    SKINCONTROL_DELETED = 58,

    /// <summary>Portal alias created.</summary>
    PORTALALIAS_CREATED = 59,

    /// <summary>Portal alias updated.</summary>
    PORTALALIAS_UPDATED = 60,

    /// <summary>Portal alias deleted.</summary>
    PORTALALIAS_DELETED = 61,

    /// <summary>Profile property created.</summary>
    PROFILEPROPERTY_CREATED = 62,

    /// <summary>Profile property updated.</summary>
    PROFILEPROPERTY_UPDATED = 63,

    /// <summary>Profile property deleted.</summary>
    PROFILEPROPERTY_DELETED = 64,

    /// <summary>User updated.</summary>
    USER_UPDATED = 65,

    /// <summary>Desktop module permission created.</summary>
    DESKTOPMODULEPERMISSION_CREATED = 66,

    /// <summary>Desktop module permission updated.</summary>
    DESKTOPMODULEPERMISSION_UPDATED = 67,

    /// <summary>Desktop module permission deleted.</summary>
    DESKTOPMODULEPERMISSION_DELETED = 68,

    /// <summary>Permission created.</summary>
    PERMISSION_CREATED = 69,

    /// <summary>Permission updated.</summary>
    PERMISSION_UPDATED = 70,

    /// <summary>Permission deleted.</summary>
    PERMISSION_DELETED = 71,

    /// <summary>Tab permission created.</summary>
    TABPERMISSION_CREATED = 72,

    /// <summary>Tab permission updated.</summary>
    TABPERMISSION_UPDATED = 73,

    /// <summary>Tab permission deleted.</summary>
    TABPERMISSION_DELETED = 74,

    /// <summary>Authentication created.</summary>
    AUTHENTICATION_CREATED = 75,

    /// <summary>Authentication updated.</summary>
    AUTHENTICATION_UPDATED = 76,

    /// <summary>Authentication deleted.</summary>
    AUTHENTICATION_DELETED = 77,

    /// <summary>File added.</summary>
    FILE_ADDED = 78,

    /// <summary>File changed.</summary>
    FILE_CHANGED = 79,

    /// <summary>File deleted.</summary>
    FILE_DELETED = 80,

    /// <summary>File downloaded.</summary>
    FILE_DOWNLOADED = 81,

    /// <summary>File moved.</summary>
    FILE_MOVED = 82,

    /// <summary>File overwritten.</summary>
    FILE_OVERWRITTEN = 83,

    /// <summary>File renamed.</summary>
    FILE_RENAMED = 84,

    /// <summary>File metadata changed.</summary>
    FILE_METADATACHANGED = 85,

    /// <summary>Folder created.</summary>
    FOLDER_CREATED = 86,

    /// <summary>Folder updated.</summary>
    FOLDER_UPDATED = 87,

    /// <summary>Folder deleted.</summary>
    FOLDER_DELETED = 88,

    /// <summary>Package created.</summary>
    PACKAGE_CREATED = 89,

    /// <summary>Package updated.</summary>
    PACKAGE_UPDATED = 90,

    /// <summary>Package deleted.</summary>
    PACKAGE_DELETED = 91,

    /// <summary>Language pack created.</summary>
    LANGUAGEPACK_CREATED = 92,

    /// <summary>Language pack updated.</summary>
    LANGUAGEPACK_UPDATED = 93,

    /// <summary>Langauge pack deleted.</summary>
    LANGUAGEPACK_DELETED = 94,

    /// <summary>Language created.</summary>
    LANGUAGE_CREATED = 95,

    /// <summary>Language updated.</summary>
    LANGUAGE_UPDATED = 96,

    /// <summary>Language deleted.</summary>
    LANGUAGE_DELETED = 97,

    /// <summary>Library updated.</summary>
    LIBRARY_UPDATED = 98,

    /// <summary>Skin package created.</summary>
    SKINPACKAGE_CREATED = 99,

    /// <summary>Skin package updated.</summary>
    SKINPACKAGE_UPDATED = 100,

    /// <summary>Skin package deleted.</summary>
    SKINPACKAGE_DELETED = 101,

    /// <summary>Schedule created.</summary>
    SCHEDULE_CREATED = 102,

    /// <summary>Schedule updated.</summary>
    SCHEDULE_UPDATED = 103,

    /// <summary>schedule deleted.</summary>
    SCHEDULE_DELETED = 104,

    /// <summary>Host setting created.</summary>
    HOST_SETTING_CREATED = 105,

    /// <summary>Host setting updated.</summary>
    HOST_SETTING_UPDATED = 106,

    /// <summary>Host setting deleted.</summary>
    HOST_SETTING_DELETED = 107,

    /// <summary>Portal desktop module created.</summary>
    PORTALDESKTOPMODULE_CREATED = 108,

    /// <summary>Portal desktop module updated.</summary>
    PORTALDESKTOPMODULE_UPDATED = 109,

    /// <summary>Portal desktop module deleted.</summary>
    PORTALDESKTOPMODULE_DELETED = 110,

    /// <summary>Tab module created.</summary>
    TABMODULE_CREATED = 111,

    /// <summary>Tab module updated.</summary>
    TABMODULE_UPDATED = 112,

    /// <summary>Tab module deleted.</summary>
    TABMODULE_DELETED = 113,

    /// <summary>Tab module setting created.</summary>
    TABMODULE_SETTING_CREATED = 114,

    /// <summary>Tab module setting updated.</summary>
    TABMODULE_SETTING_UPDATED = 115,

    /// <summary>Tab module setting deleted.</summary>
    TABMODULE_SETTING_DELETED = 116,

    /// <summary>Module setting created.</summary>
    MODULE_SETTING_CREATED = 117,

    /// <summary>Module setting updated.</summary>
    MODULE_SETTING_UPDATED = 118,

    /// <summary>Module setting deleted.</summary>
    MODULE_SETTING_DELETED = 119,

    /// <summary>Portal setting created.</summary>
    PORTAL_SETTING_CREATED = 120,

    /// <summary>Portal setting updated.</summary>
    PORTAL_SETTING_UPDATED = 121,

    /// <summary>POrtal setting deleted.</summary>
    PORTAL_SETTING_DELETED = 122,

    /// <summary>Portal info created.</summary>
    PORTALINFO_CREATED = 123,

    /// <summary>Portal info updated.</summary>
    PORTALINFO_UPDATED = 124,

    /// <summary>Portal info deleted.</summary>
    PORTALINFO_DELETED = 125,

    /// <summary>Authentication user created.</summary>
    AUTHENTICATION_USER_CREATED = 126,

    /// <summary>Authentication user updated.</summary>
    AUTHENTICATION_USER_UPDATED = 127,

    /// <summary>Authentication user deleted.</summary>
    AUTHENTICATION_USER_DELETED = 128,

    /// <summary>Language to portal created.</summary>
    LANGUAGETOPORTAL_CREATED = 129,

    /// <summary>Language to portal updated.</summary>
    LANGUAGETOPORTAL_UPDATED = 130,

    /// <summary>Language to portal deleted.</summary>
    LANGUAGETOPORTAL_DELETED = 131,

    /// <summary>Tab order updated.</summary>
    TAB_ORDER_UPDATED = 132,

    /// <summary>Tab setting created.</summary>
    TAB_SETTING_CREATED = 133,

    /// <summary>Tab setting updated.</summary>
    TAB_SETTING_UPDATED = 134,

    /// <summary>Tab setting deleted.</summary>
    TAB_SETTING_DELETED = 135,

    /// <summary>Host SQL executed.</summary>
    HOST_SQL_EXECUTED = 136,

    /// <summary>User restored.</summary>
    USER_RESTORED = 137,

    /// <summary>User removed.</summary>
    USER_REMOVED = 138,

    /// <summary>User impersonated.</summary>
    USER_IMPERSONATED = 139,

    /// <summary>Username updated.</summary>
    USERNAME_UPDATED = 140,

    /// <summary>IP Login banned.</summary>
    IP_LOGIN_BANNED = 141,

    /// <summary>Page Not Found 404.</summary>
    PAGE_NOT_FOUND_404 = 142,

    /// <summary>Tab URL created.</summary>
    TABURL_CREATED = 143,

    /// <summary>Tab URL updated.</summary>
    TABURL_UPDATED = 144,

    /// <summary>Tab URL deleted.</summary>
    TABURL_DELETED = 145,

    /// <summary>Script collision.</summary>
    SCRIPT_COLLISION = 146,

    /// <summary>Potential paypal payment fraud.</summary>
    POTENTIAL_PAYPAL_PAYMENT_FRAUD = 147,

    /// <summary>Webserver created.</summary>
    WEBSERVER_CREATED = 148,

    /// <summary>Webserver updated.</summary>
    WEBSERVER_UPDATED = 149,

    /// <summary>Webserver disabled.</summary>
    WEBSERVER_DISABLED = 150,

    /// <summary>Webserver enabled.</summary>
    WEBSERVER_ENABLED = 151,

    /// <summary>Webserver ping failed.</summary>
    WEBSERVER_PINGFAILED = 152,

    /// <summary>Folder moved.</summary>
    FOLDER_MOVED = 153,
}
