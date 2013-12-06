#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

#region Usings

using System.Globalization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    public class EventLogController : LogController, IEventLogController
    {
        #region EventLogType enum

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
            SCRIPT_COLLISION
        }

        #endregion

        #region IEventLogController Members

        public void AddLog(string propertyName, string propertyValue, EventLogType logType)
        {
            AddLog(propertyName, propertyValue, PortalController.GetCurrentPortalSettings(),
                   UserController.GetCurrentUserInfo().UserID, logType);
        }

        public void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID,
                           EventLogType logType)
        {
            AddLog(propertyName, propertyValue, portalSettings, userID, logType.ToString());
        }

        public void AddLog(string propertyName, string propertyValue, PortalSettings portalSettings, int userID,
                           string logType)
        {
            var properties = new LogProperties();
            var logDetailInfo = new LogDetailInfo {PropertyName = propertyName, PropertyValue = propertyValue};
            properties.Add(logDetailInfo);
            AddLog(properties, portalSettings, userID, logType, false);
        }

        public void AddLog(LogProperties properties, PortalSettings portalSettings, int userID, string logTypeKey,
                           bool bypassBuffering)
        {
            //supports adding a custom string for LogType
            var logInfo = new LogInfo
                {
                    LogUserID = userID,
                    LogTypeKey = logTypeKey,
                    LogProperties = properties,
                    BypassBuffering = bypassBuffering
                };
            if (portalSettings != null)
            {
                logInfo.LogPortalID = portalSettings.PortalId;
                logInfo.LogPortalName = portalSettings.PortalName;
            }
            AddLog(logInfo);
        }

        public void AddLog(PortalSettings portalSettings, int userID, EventLogType logType)
        {
            AddLog(new LogProperties(), portalSettings, userID, logType.ToString(), false);
        }

        public void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName,
                           EventLogType logType)
        {
            AddLog(businessObject, portalSettings, userID, userName, logType.ToString());
        }

        public void AddLog(object businessObject, PortalSettings portalSettings, int userID, string userName,
                           string logType)
        {
            var logInfo = new LogInfo {LogUserID = userID, LogTypeKey = logType};
            if (portalSettings != null)
            {
                logInfo.LogPortalID = portalSettings.PortalId;
                logInfo.LogPortalName = portalSettings.PortalName;
            }
            switch (businessObject.GetType().FullName)
            {
                case "DotNetNuke.Entities.Portals.PortalInfo":
                    var portal = (PortalInfo) businessObject;
                    logInfo.LogProperties.Add(new LogDetailInfo("PortalID",
                                                                portal.PortalID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("PortalName", portal.PortalName));
                    logInfo.LogProperties.Add(new LogDetailInfo("Description", portal.Description));
                    logInfo.LogProperties.Add(new LogDetailInfo("KeyWords", portal.KeyWords));
                    logInfo.LogProperties.Add(new LogDetailInfo("LogoFile", portal.LogoFile));
                    break;
                case "DotNetNuke.Entities.Tabs.TabInfo":
                    var tab = (TabInfo) businessObject;
                    logInfo.LogProperties.Add(new LogDetailInfo("TabID",
                                                                tab.TabID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("PortalID",
                                                                tab.PortalID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("TabName", tab.TabName));
                    logInfo.LogProperties.Add(new LogDetailInfo("Title", tab.Title));
                    logInfo.LogProperties.Add(new LogDetailInfo("Description", tab.Description));
                    logInfo.LogProperties.Add(new LogDetailInfo("KeyWords", tab.KeyWords));
                    logInfo.LogProperties.Add(new LogDetailInfo("Url", tab.Url));
                    logInfo.LogProperties.Add(new LogDetailInfo("ParentId",
                                                                tab.ParentId.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("IconFile", tab.IconFile));
                    logInfo.LogProperties.Add(new LogDetailInfo("IsVisible",
                                                                tab.IsVisible.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("SkinSrc", tab.SkinSrc));
                    logInfo.LogProperties.Add(new LogDetailInfo("ContainerSrc", tab.ContainerSrc));
                    break;
                case "DotNetNuke.Entities.Modules.ModuleInfo":
                    var module = (ModuleInfo) businessObject;
                    logInfo.LogProperties.Add(new LogDetailInfo("ModuleId",
                                                                module.ModuleID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("ModuleTitle", module.ModuleTitle));
                    logInfo.LogProperties.Add(new LogDetailInfo("TabModuleID",
                                                                module.TabModuleID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("TabID",
                                                                module.TabID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("PortalID",
                                                                module.PortalID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("ModuleDefId",
                                                                module.ModuleDefID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("FriendlyName", module.DesktopModule.FriendlyName));
                    logInfo.LogProperties.Add(new LogDetailInfo("IconFile", module.IconFile));
                    logInfo.LogProperties.Add(new LogDetailInfo("Visibility", module.Visibility.ToString()));
                    logInfo.LogProperties.Add(new LogDetailInfo("ContainerSrc", module.ContainerSrc));
                    break;
                case "DotNetNuke.Entities.Users.UserInfo":
                    var user = (UserInfo) businessObject;
                    logInfo.LogProperties.Add(new LogDetailInfo("UserID",
                                                                user.UserID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("FirstName", user.Profile.FirstName));
                    logInfo.LogProperties.Add(new LogDetailInfo("LastName", user.Profile.LastName));
                    logInfo.LogProperties.Add(new LogDetailInfo("UserName", user.Username));
                    logInfo.LogProperties.Add(new LogDetailInfo("Email", user.Email));
                    break;
                case "DotNetNuke.Security.Roles.RoleInfo":
                    var role = (RoleInfo) businessObject;
                    logInfo.LogProperties.Add(new LogDetailInfo("RoleID",
                                                                role.RoleID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("RoleName", role.RoleName));
                    logInfo.LogProperties.Add(new LogDetailInfo("PortalID",
                                                                role.PortalID.ToString(CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("Description", role.Description));
                    logInfo.LogProperties.Add(new LogDetailInfo("IsPublic",
                                                                role.IsPublic.ToString(CultureInfo.InvariantCulture)));
                    break;
                case "DotNetNuke.Entities.Modules.DesktopModuleInfo":
                    var desktopModule = (DesktopModuleInfo) businessObject;
                    logInfo.LogProperties.Add(new LogDetailInfo("DesktopModuleID",
                                                                desktopModule.DesktopModuleID.ToString(
                                                                    CultureInfo.InvariantCulture)));
                    logInfo.LogProperties.Add(new LogDetailInfo("ModuleName", desktopModule.ModuleName));
                    logInfo.LogProperties.Add(new LogDetailInfo("FriendlyName", desktopModule.FriendlyName));
                    logInfo.LogProperties.Add(new LogDetailInfo("FolderName", desktopModule.FolderName));
                    logInfo.LogProperties.Add(new LogDetailInfo("Description", desktopModule.Description));
                    break;
                default: //Serialise using XmlSerializer
                    logInfo.LogProperties.Add(new LogDetailInfo("logdetail", XmlUtils.Serialize(businessObject)));
                    break;
            }
            base.AddLog(logInfo);
        }

        #endregion

        #region Helper Methods

        public static void AddSettingLog(EventLogType logTypeKey, string idFieldName, int idValue, string settingName,
                                            string settingValue, int userId)
        {
            var eventLogController = new EventLogController();
            var eventLogInfo = new LogInfo() { LogUserID = userId, LogTypeKey = logTypeKey.ToString() };
            eventLogInfo.LogProperties.Add(new LogDetailInfo(idFieldName, idValue.ToString()));
            eventLogInfo.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            eventLogInfo.LogProperties.Add(new LogDetailInfo("SettingValue", settingValue));

            eventLogController.AddLog(eventLogInfo);
        }

        #endregion
    }
}