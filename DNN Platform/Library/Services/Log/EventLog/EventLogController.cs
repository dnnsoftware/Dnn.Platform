// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    public class EventLogController : ServiceLocator<IEventLogController, EventLogController>, IEventLogController
    {
        public enum EventLogType
        {
            USER_CREATED,
            USER_DELETED,
            LOGIN_SUPERUSER,
            LOGIN_SUCCESS,
            LOGIN_FAILURE,
            LOGIN_USERLOCKEDOUT,
            LOGIN_USERNOTAPPROVED,
            CACHE_REFRESHED,
            PASSWORD_SENT_SUCCESS,
            PASSWORD_SENT_FAILURE,
            LOG_NOTIFICATION_FAILURE,
            PORTAL_CREATED,
            PORTAL_DELETED,
            PORTALGROUP_CREATED,
            PORTALGROUP_DELETED,
            PORTAL_ADDEDTOPORTALGROUP,
            PORTAL_REMOVEDFROMPORTALGROUP,
            TAB_CREATED,
            TAB_UPDATED,
            TAB_DELETED,
            TAB_SENT_TO_RECYCLE_BIN,
            TAB_RESTORED,
            USER_ROLE_CREATED,
            USER_ROLE_DELETED,
            USER_ROLE_UPDATED,
            ROLE_CREATED,
            ROLE_UPDATED,
            ROLE_DELETED,
            MODULE_CREATED,
            MODULE_UPDATED,
            MODULE_DELETED,
            MODULE_SENT_TO_RECYCLE_BIN,
            MODULE_RESTORED,
            SCHEDULER_EVENT_STARTED,
            SCHEDULER_EVENT_PROGRESSING,
            SCHEDULER_EVENT_COMPLETED,
            APPLICATION_START,
            APPLICATION_END,
            APPLICATION_SHUTTING_DOWN,
            SCHEDULER_STARTED,
            SCHEDULER_SHUTTING_DOWN,
            SCHEDULER_STOPPED,
            ADMIN_ALERT,
            HOST_ALERT,
            CACHE_REMOVED,
            CACHE_EXPIRED,
            CACHE_UNDERUSED,
            CACHE_DEPENDENCYCHANGED,
            CACHE_OVERFLOW,
            CACHE_REFRESH,
            LISTENTRY_CREATED,
            LISTENTRY_UPDATED,
            LISTENTRY_DELETED,
            DESKTOPMODULE_CREATED,
            DESKTOPMODULE_UPDATED,
            DESKTOPMODULE_DELETED,
            SKINCONTROL_CREATED,
            SKINCONTROL_UPDATED,
            SKINCONTROL_DELETED,
            PORTALALIAS_CREATED,
            PORTALALIAS_UPDATED,
            PORTALALIAS_DELETED,
            PROFILEPROPERTY_CREATED,
            PROFILEPROPERTY_UPDATED,
            PROFILEPROPERTY_DELETED,
            USER_UPDATED,
            DESKTOPMODULEPERMISSION_CREATED,
            DESKTOPMODULEPERMISSION_UPDATED,
            DESKTOPMODULEPERMISSION_DELETED,
            PERMISSION_CREATED,
            PERMISSION_UPDATED,
            PERMISSION_DELETED,
            TABPERMISSION_CREATED,
            TABPERMISSION_UPDATED,
            TABPERMISSION_DELETED,
            AUTHENTICATION_CREATED,
            AUTHENTICATION_UPDATED,
            AUTHENTICATION_DELETED,
            FILE_ADDED,
            FILE_CHANGED,
            FILE_DELETED,
            FILE_DOWNLOADED,
            FILE_MOVED,
            FILE_OVERWRITTEN,
            FILE_RENAMED,
            FILE_METADATACHANGED,
            FOLDER_CREATED,
            FOLDER_UPDATED,
            FOLDER_DELETED,
            PACKAGE_CREATED,
            PACKAGE_UPDATED,
            PACKAGE_DELETED,
            LANGUAGEPACK_CREATED,
            LANGUAGEPACK_UPDATED,
            LANGUAGEPACK_DELETED,
            LANGUAGE_CREATED,
            LANGUAGE_UPDATED,
            LANGUAGE_DELETED,
            LIBRARY_UPDATED,
            SKINPACKAGE_CREATED,
            SKINPACKAGE_UPDATED,
            SKINPACKAGE_DELETED,
            SCHEDULE_CREATED,
            SCHEDULE_UPDATED,
            SCHEDULE_DELETED,
            HOST_SETTING_CREATED,
            HOST_SETTING_UPDATED,
            HOST_SETTING_DELETED,
            PORTALDESKTOPMODULE_CREATED,
            PORTALDESKTOPMODULE_UPDATED,
            PORTALDESKTOPMODULE_DELETED,
            TABMODULE_CREATED,
            TABMODULE_UPDATED,
            TABMODULE_DELETED,
            TABMODULE_SETTING_CREATED,
            TABMODULE_SETTING_UPDATED,
            TABMODULE_SETTING_DELETED,
            MODULE_SETTING_CREATED,
            MODULE_SETTING_UPDATED,
            MODULE_SETTING_DELETED,
            PORTAL_SETTING_CREATED,
            PORTAL_SETTING_UPDATED,
            PORTAL_SETTING_DELETED,
            PORTALINFO_CREATED,
            PORTALINFO_UPDATED,
            PORTALINFO_DELETED,
            AUTHENTICATION_USER_CREATED,
            AUTHENTICATION_USER_UPDATED,
            AUTHENTICATION_USER_DELETED,
            LANGUAGETOPORTAL_CREATED,
            LANGUAGETOPORTAL_UPDATED,
            LANGUAGETOPORTAL_DELETED,
            TAB_ORDER_UPDATED,
            TAB_SETTING_CREATED,
            TAB_SETTING_UPDATED,
            TAB_SETTING_DELETED,
            HOST_SQL_EXECUTED,
            USER_RESTORED,
            USER_REMOVED,
            USER_IMPERSONATED,
            USERNAME_UPDATED,
            IP_LOGIN_BANNED,
            PAGE_NOT_FOUND_404,
            TABURL_CREATED,
            TABURL_UPDATED,
            TABURL_DELETED,
            SCRIPT_COLLISION,
            POTENTIAL_PAYPAL_PAYMENT_FRAUD,
            WEBSERVER_CREATED,
            WEBSERVER_UPDATED,
            WEBSERVER_DISABLED,
            WEBSERVER_ENABLED,
            WEBSERVER_PINGFAILED,
            FOLDER_MOVED,
        }

        public static void AddSettingLog(EventLogType logTypeKey, string idFieldName, int idValue, string settingName, string settingValue, int userId)
        {
            var log = new LogInfo() { LogUserID = userId, LogTypeKey = logTypeKey.ToString() };
            log.LogProperties.Add(new LogDetailInfo(idFieldName, idValue.ToString()));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            log.LogProperties.Add(new LogDetailInfo("SettingValue", settingValue));

            LogController.Instance.AddLog(log);
        }

        public void AddLog(string propertyName, string propertyValue, EventLogType logType)
        {
            this.AddLog(propertyName, propertyValue, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, logType);
        }

        public void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, EventLogType logType)
        {
            this.AddLog(propertyName, propertyValue, portalSettings, userID, logType.ToString());
        }

        public void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID, string logType)
        {
            var properties = new LogProperties();
            var logDetailInfo = new LogDetailInfo { PropertyName = propertyName, PropertyValue = propertyValue };
            properties.Add(logDetailInfo);
            this.AddLog(properties, portalSettings, userID, logType, false);
        }

        public void AddLog(LogProperties properties, PortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering)
        {
            // supports adding a custom string for LogType
            var log = new LogInfo
            {
                LogUserID = userID,
                LogTypeKey = logTypeKey,
                LogProperties = properties,
                BypassBuffering = bypassBuffering,
            };
            if (portalSettings != null)
            {
                log.LogPortalID = portalSettings.PortalId;
                log.LogPortalName = portalSettings.PortalName;
            }

            LogController.Instance.AddLog(log);
        }

        public void AddLog(PortalSettings portalSettings, int userID, EventLogType logType)
        {
            this.AddLog(new LogProperties(), portalSettings, userID, logType.ToString(), false);
        }

        public void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, EventLogType logType)
        {
            this.AddLog(businessObject, portalSettings, userID, userName, logType.ToString());
        }

        public void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName, string logType)
        {
            var log = new LogInfo { LogUserID = userID, LogTypeKey = logType };
            if (portalSettings != null)
            {
                log.LogPortalID = portalSettings.PortalId;
                log.LogPortalName = portalSettings.PortalName;
            }

            if (businessObject is PortalInfo)
            {
                var portal = (PortalInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("PortalID", portal.PortalID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("PortalName", portal.PortalName));
                log.LogProperties.Add(new LogDetailInfo("Description", portal.Description));
                log.LogProperties.Add(new LogDetailInfo("KeyWords", portal.KeyWords));
                log.LogProperties.Add(new LogDetailInfo("LogoFile", portal.LogoFile));
            }
            else if (businessObject is TabInfo)
            {
                var tab = (TabInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("TabID", tab.TabID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("PortalID", tab.PortalID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("TabName", tab.TabName));
                log.LogProperties.Add(new LogDetailInfo("Title", tab.Title));
                log.LogProperties.Add(new LogDetailInfo("Description", tab.Description));
                log.LogProperties.Add(new LogDetailInfo("KeyWords", tab.KeyWords));
                log.LogProperties.Add(new LogDetailInfo("Url", tab.Url));
                log.LogProperties.Add(new LogDetailInfo("ParentId", tab.ParentId.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("IconFile", tab.IconFile));
                log.LogProperties.Add(new LogDetailInfo("IsVisible", tab.IsVisible.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("SkinSrc", tab.SkinSrc));
                log.LogProperties.Add(new LogDetailInfo("ContainerSrc", tab.ContainerSrc));
            }
            else if (businessObject is ModuleInfo)
            {
                var module = (ModuleInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("ModuleId", module.ModuleID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("ModuleTitle", module.ModuleTitle));
                log.LogProperties.Add(new LogDetailInfo("TabModuleID", module.TabModuleID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("TabID", module.TabID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("PortalID", module.PortalID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("ModuleDefId", module.ModuleDefID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("FriendlyName", module.DesktopModule.FriendlyName));
                log.LogProperties.Add(new LogDetailInfo("IconFile", module.IconFile));
                log.LogProperties.Add(new LogDetailInfo("Visibility", module.Visibility.ToString()));
                log.LogProperties.Add(new LogDetailInfo("ContainerSrc", module.ContainerSrc));
            }
            else if (businessObject is UserInfo)
            {
                var user = (UserInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("UserID", user.UserID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("FirstName", user.Profile.FirstName));
                log.LogProperties.Add(new LogDetailInfo("LastName", user.Profile.LastName));
                log.LogProperties.Add(new LogDetailInfo("UserName", user.Username));
                log.LogProperties.Add(new LogDetailInfo("Email", user.Email));
            }
            else if (businessObject is RoleInfo)
            {
                var role = (RoleInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("RoleID", role.RoleID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("RoleName", role.RoleName));
                log.LogProperties.Add(new LogDetailInfo("PortalID", role.PortalID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("Description", role.Description));
                log.LogProperties.Add(new LogDetailInfo("IsPublic", role.IsPublic.ToString(CultureInfo.InvariantCulture)));
            }
            else if (businessObject is DesktopModuleInfo)
            {
                var desktopModule = (DesktopModuleInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("DesktopModuleID", desktopModule.DesktopModuleID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("ModuleName", desktopModule.ModuleName));
                log.LogProperties.Add(new LogDetailInfo("FriendlyName", desktopModule.FriendlyName));
                log.LogProperties.Add(new LogDetailInfo("FolderName", desktopModule.FolderName));
                log.LogProperties.Add(new LogDetailInfo("Description", desktopModule.Description));
            }
            else if (businessObject is FolderInfo)
            {
                var folderInfo = (FolderInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("FolderID", folderInfo.FolderID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("PortalID", folderInfo.PortalID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("FolderName", folderInfo.FolderName));
                log.LogProperties.Add(new LogDetailInfo("FolderPath", folderInfo.FolderPath));
                log.LogProperties.Add(new LogDetailInfo("FolderMappingID", folderInfo.FolderMappingID.ToString(CultureInfo.InvariantCulture)));
            }
            else if (businessObject is FileInfo)
            {
                var fileInfo = (FileInfo)businessObject;
                log.LogProperties.Add(new LogDetailInfo("FileID", fileInfo.FileId.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("PortalID", fileInfo.PortalId.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("FolderID", fileInfo.FolderId.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("FolderMappingID", fileInfo.FolderMappingID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("ContentType", fileInfo.ContentType));
                log.LogProperties.Add(new LogDetailInfo("FileName", fileInfo.FileName));
                log.LogProperties.Add(new LogDetailInfo("FolderName", fileInfo.Folder));
                log.LogProperties.Add(new LogDetailInfo("PhysicalPath", fileInfo.PhysicalPath));
                log.LogProperties.Add(new LogDetailInfo("VersionGuid", fileInfo.VersionGuid.ToString()));
            }
            else
            {
                // Serialise using XmlSerializer
                log.LogProperties.Add(new LogDetailInfo("logdetail", XmlUtils.Serialize(businessObject)));
            }

            LogController.Instance.AddLog(log);
        }

        public void AddLog(LogInfo logInfo)
        {
            LogController.Instance.AddLog(logInfo);
        }

        public void AddLogType(string configFile, string fallbackConfigFile)
        {
            LogController.Instance.AddLogType(configFile, fallbackConfigFile);
        }

        public void AddLogType(LogTypeInfo logType)
        {
            LogController.Instance.AddLogType(logType);
        }

        public void AddLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            LogController.Instance.AddLogTypeConfigInfo(logTypeConfig);
        }

        public void ClearLog()
        {
            LogController.Instance.ClearLog();
        }

        public void DeleteLog(LogInfo logInfo)
        {
            LogController.Instance.DeleteLog(logInfo);
        }

        public void DeleteLogType(LogTypeInfo logType)
        {
            LogController.Instance.DeleteLogType(logType);
        }

        public void DeleteLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            LogController.Instance.DeleteLogTypeConfigInfo(logTypeConfig);
        }

        public List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            return LogController.Instance.GetLogs(portalID, logType, pageSize, pageIndex, ref totalRecords);
        }

        public ArrayList GetLogTypeConfigInfo()
        {
            return LogController.Instance.GetLogTypeConfigInfo();
        }

        public LogTypeConfigInfo GetLogTypeConfigInfoByID(string id)
        {
            return LogController.Instance.GetLogTypeConfigInfoByID(id);
        }

        public Dictionary<string, LogTypeInfo> GetLogTypeInfoDictionary()
        {
            return LogController.Instance.GetLogTypeInfoDictionary();
        }

        public object GetSingleLog(LogInfo log, LoggingProvider.ReturnType returnType)
        {
            return LogController.Instance.GetSingleLog(log, returnType);
        }

        public void PurgeLogBuffer()
        {
            LogController.Instance.PurgeLogBuffer();
        }

        public virtual void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            LogController.Instance.UpdateLogTypeConfigInfo(logTypeConfig);
        }

        public virtual void UpdateLogType(LogTypeInfo logType)
        {
            LogController.Instance.UpdateLogType(logType);
        }

        protected override Func<IEventLogController> GetFactory()
        {
            return () => new EventLogController();
        }
    }
}
