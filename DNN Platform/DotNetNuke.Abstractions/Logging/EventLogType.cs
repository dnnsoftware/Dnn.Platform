// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>System Event Log Types.</summary>
    public enum EventLogType
    {
        /// <summary>User Created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_CREATED = 0,

        /// <summary>User Deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_DELETED = 1,

        /// <summary>Login Super User.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LOGIN_SUPERUSER = 2,

        /// <summary>Log Success.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LOGIN_SUCCESS = 3,

        /// <summary>Login failure.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LOGIN_FAILURE = 4,

        /// <summary>Login user locked out.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LOGIN_USERLOCKEDOUT = 5,

        /// <summary>Login user not approved.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LOGIN_USERNOTAPPROVED = 6,

        /// <summary>Cache refreshed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_REFRESHED = 7,

        /// <summary>Password sent success.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PASSWORD_SENT_SUCCESS = 8,

        /// <summary>Password sent failure.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PASSWORD_SENT_FAILURE = 9,

        /// <summary>log notification failure.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LOG_NOTIFICATION_FAILURE = 10,

        /// <summary>Portal created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_CREATED = 11,

        /// <summary>Portal deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_DELETED = 12,

        /// <summary>Portal group created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALGROUP_CREATED = 13,

        /// <summary>Portal group deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALGROUP_DELETED = 14,

        /// <summary>Portal added to portal group.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_ADDEDTOPORTALGROUP = 15,

        /// <summary>Portal removed from portal group.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_REMOVEDFROMPORTALGROUP = 16,

        /// <summary>Tab created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_CREATED = 17,

        /// <summary>Tab updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_UPDATED = 18,

        /// <summary>Tab deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_DELETED = 19,

        /// <summary>Tab sent to recycle bin.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_SENT_TO_RECYCLE_BIN = 20,

        /// <summary>Tab restored.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_RESTORED = 21,

        /// <summary>User role created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_ROLE_CREATED = 22,

        /// <summary>User role deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_ROLE_DELETED = 23,

        /// <summary>User role updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_ROLE_UPDATED = 24,

        /// <summary>Role created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        ROLE_CREATED = 25,

        /// <summary>Role updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        ROLE_UPDATED = 26,

        /// <summary>Role deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        ROLE_DELETED = 27,

        /// <summary>Module created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_CREATED = 28,

        /// <summary>Module updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_UPDATED = 29,

        /// <summary>Module deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_DELETED = 30,

        /// <summary>Module sent to recycle bin.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_SENT_TO_RECYCLE_BIN = 31,

        /// <summary>Module restored.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_RESTORED = 32,

        /// <summary>scheduler event started.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULER_EVENT_STARTED = 33,

        /// <summary>Scheduler event progressing.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULER_EVENT_PROGRESSING = 34,

        /// <summary>Scheduler event completed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULER_EVENT_COMPLETED = 35,

        /// <summary>Application start.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APPLICATION_START = 36,

        /// <summary>Application end.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APPLICATION_END = 37,

        /// <summary>Application shutting down.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APPLICATION_SHUTTING_DOWN = 38,

        /// <summary>Scheduler started.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULER_STARTED = 39,

        /// <summary>Scheduler shutting down.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULER_SHUTTING_DOWN = 40,

        /// <summary>Scheduler stopped.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULER_STOPPED = 41,

        /// <summary>Admin alert.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        ADMIN_ALERT = 42,

        /// <summary>Host alert.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        HOST_ALERT = 43,

        /// <summary>Cache removed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_REMOVED = 44,

        /// <summary>Cache expired.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_EXPIRED = 45,

        /// <summary>Cache under used.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_UNDERUSED = 46,

        /// <summary>Cache dependency changed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_DEPENDENCYCHANGED = 47,

        /// <summary>Cache overflow.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_OVERFLOW = 48,

        /// <summary>Cache refresh.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        CACHE_REFRESH = 49,

        /// <summary>List entry created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LISTENTRY_CREATED = 50,

        /// <summary>List entry updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LISTENTRY_UPDATED = 51,

        /// <summary>List entry deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LISTENTRY_DELETED = 52,

        /// <summary>Desktop Module created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        DESKTOPMODULE_CREATED = 53,

        /// <summary>Desktop Module updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        DESKTOPMODULE_UPDATED = 54,

        /// <summary>Desktop Module deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        DESKTOPMODULE_DELETED = 55,

        /// <summary>Skin control created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SKINCONTROL_CREATED = 56,

        /// <summary>Skin control updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SKINCONTROL_UPDATED = 57,

        /// <summary>Skin control deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SKINCONTROL_DELETED = 58,

        /// <summary>Portal alias created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALALIAS_CREATED = 59,

        /// <summary>Portal alias updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALALIAS_UPDATED = 60,

        /// <summary>Portal alias deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALALIAS_DELETED = 61,

        /// <summary>Profile property created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PROFILEPROPERTY_CREATED = 62,

        /// <summary>Profile property updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PROFILEPROPERTY_UPDATED = 63,

        /// <summary>Profile property deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PROFILEPROPERTY_DELETED = 64,

        /// <summary>User updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_UPDATED = 65,

        /// <summary>Desktop module permission created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        DESKTOPMODULEPERMISSION_CREATED = 66,

        /// <summary>Desktop module permission updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        DESKTOPMODULEPERMISSION_UPDATED = 67,

        /// <summary>Desktop module permission deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        DESKTOPMODULEPERMISSION_DELETED = 68,

        /// <summary>Permission created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PERMISSION_CREATED = 69,

        /// <summary>Permission updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PERMISSION_UPDATED = 70,

        /// <summary>Permission deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PERMISSION_DELETED = 71,

        /// <summary>Tab permission created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABPERMISSION_CREATED = 72,

        /// <summary>Tab permission updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABPERMISSION_UPDATED = 73,

        /// <summary>Tab permission deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABPERMISSION_DELETED = 74,

        /// <summary>Authentication created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        AUTHENTICATION_CREATED = 75,

        /// <summary>Authentication updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        AUTHENTICATION_UPDATED = 76,

        /// <summary>Authentication deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        AUTHENTICATION_DELETED = 77,

        /// <summary>File added.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_ADDED = 78,

        /// <summary>File changed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_CHANGED = 79,

        /// <summary>File deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_DELETED = 80,

        /// <summary>File downloaded.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_DOWNLOADED = 81,

        /// <summary>File moved.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_MOVED = 82,

        /// <summary>File overwritten.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_OVERWRITTEN = 83,

        /// <summary>File renamed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_RENAMED = 84,

        /// <summary>File metadata changed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FILE_METADATACHANGED = 85,

        /// <summary>Folder created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FOLDER_CREATED = 86,

        /// <summary>Folder updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FOLDER_UPDATED = 87,

        /// <summary>Folder deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FOLDER_DELETED = 88,

        /// <summary>Package created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PACKAGE_CREATED = 89,

        /// <summary>Package updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PACKAGE_UPDATED = 90,

        /// <summary>Package deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PACKAGE_DELETED = 91,

        /// <summary>Language pack created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGEPACK_CREATED = 92,

        /// <summary>Language pack updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGEPACK_UPDATED = 93,

        /// <summary>Language pack deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGEPACK_DELETED = 94,

        /// <summary>Language created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGE_CREATED = 95,

        /// <summary>Language updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGE_UPDATED = 96,

        /// <summary>Language deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGE_DELETED = 97,

        /// <summary>Library updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LIBRARY_UPDATED = 98,

        /// <summary>Skin package created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SKINPACKAGE_CREATED = 99,

        /// <summary>Skin package updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SKINPACKAGE_UPDATED = 100,

        /// <summary>Skin package deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SKINPACKAGE_DELETED = 101,

        /// <summary>Schedule created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULE_CREATED = 102,

        /// <summary>Schedule updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULE_UPDATED = 103,

        /// <summary>schedule deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCHEDULE_DELETED = 104,

        /// <summary>Host setting created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        HOST_SETTING_CREATED = 105,

        /// <summary>Host setting updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        HOST_SETTING_UPDATED = 106,

        /// <summary>Host setting deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        HOST_SETTING_DELETED = 107,

        /// <summary>Portal desktop module created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALDESKTOPMODULE_CREATED = 108,

        /// <summary>Portal desktop module updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALDESKTOPMODULE_UPDATED = 109,

        /// <summary>Portal desktop module deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALDESKTOPMODULE_DELETED = 110,

        /// <summary>Tab module created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABMODULE_CREATED = 111,

        /// <summary>Tab module updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABMODULE_UPDATED = 112,

        /// <summary>Tab module deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABMODULE_DELETED = 113,

        /// <summary>Tab module setting created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABMODULE_SETTING_CREATED = 114,

        /// <summary>Tab module setting updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABMODULE_SETTING_UPDATED = 115,

        /// <summary>Tab module setting deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABMODULE_SETTING_DELETED = 116,

        /// <summary>Module setting created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_SETTING_CREATED = 117,

        /// <summary>Module setting updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_SETTING_UPDATED = 118,

        /// <summary>Module setting deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        MODULE_SETTING_DELETED = 119,

        /// <summary>Portal setting created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_SETTING_CREATED = 120,

        /// <summary>Portal setting updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_SETTING_UPDATED = 121,

        /// <summary>Portal setting deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTAL_SETTING_DELETED = 122,

        /// <summary>Portal info created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALINFO_CREATED = 123,

        /// <summary>Portal info updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALINFO_UPDATED = 124,

        /// <summary>Portal info deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PORTALINFO_DELETED = 125,

        /// <summary>Authentication user created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        AUTHENTICATION_USER_CREATED = 126,

        /// <summary>Authentication user updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        AUTHENTICATION_USER_UPDATED = 127,

        /// <summary>Authentication user deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        AUTHENTICATION_USER_DELETED = 128,

        /// <summary>Language to portal created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGETOPORTAL_CREATED = 129,

        /// <summary>Language to portal updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGETOPORTAL_UPDATED = 130,

        /// <summary>Language to portal deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        LANGUAGETOPORTAL_DELETED = 131,

        /// <summary>Tab order updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_ORDER_UPDATED = 132,

        /// <summary>Tab setting created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_SETTING_CREATED = 133,

        /// <summary>Tab setting updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_SETTING_UPDATED = 134,

        /// <summary>Tab setting deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TAB_SETTING_DELETED = 135,

        /// <summary>Host SQL executed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        HOST_SQL_EXECUTED = 136,

        /// <summary>User restored.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_RESTORED = 137,

        /// <summary>User removed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_REMOVED = 138,

        /// <summary>User impersonated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USER_IMPERSONATED = 139,

        /// <summary>Username updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        USERNAME_UPDATED = 140,

        /// <summary>IP Login banned.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        IP_LOGIN_BANNED = 141,

        /// <summary>Page Not Found 404.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        PAGE_NOT_FOUND_404 = 142,

        /// <summary>Tab URL created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABURL_CREATED = 143,

        /// <summary>Tab URL updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABURL_UPDATED = 144,

        /// <summary>Tab URL deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        TABURL_DELETED = 145,

        /// <summary>Script collision.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        SCRIPT_COLLISION = 146,

        /// <summary>Potential paypal payment fraud.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        POTENTIAL_PAYPAL_PAYMENT_FRAUD = 147,

        /// <summary>Webserver created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        WEBSERVER_CREATED = 148,

        /// <summary>Webserver updated.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        WEBSERVER_UPDATED = 149,

        /// <summary>Webserver disabled.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        WEBSERVER_DISABLED = 150,

        /// <summary>Webserver enabled.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        WEBSERVER_ENABLED = 151,

        /// <summary>Webserver ping failed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        WEBSERVER_PINGFAILED = 152,

        /// <summary>Folder moved.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        FOLDER_MOVED = 153,

        /// <summary>API Token Authentication Failed.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APITOKEN_AUTHENTICATION_FAILED = 154,

        /// <summary>API Token Created.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APITOKEN_CREATED = 155,

        /// <summary>API Token Deleted.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APITOKEN_DELETED = 156,

        /// <summary>API Token Revoked.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APITOKEN_REVOKED = 157,
    }
}
