// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;

/// <inheritdoc />
public partial class EventLogController : IEventLogger, IEventLogConfigService, IEventLogService
{
    private IEventLogger EventLogger => this;

    private IEventLogConfigService EventLogConfigService => this;

    private IEventLogService EventLogService => this;

    /// <inheritdoc />
    void IEventLogger.AddLog(string name, string value, Abstractions.Logging.EventLogType logType)
    {
        this.EventLogger.AddLog(name, value, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, logType);
    }

    /// <inheritdoc />
    void IEventLogger.AddLog(string name, string value, IPortalSettings portalSettings, int userID, Abstractions.Logging.EventLogType logType)
    {
        this.EventLogger.AddLog(name, value, portalSettings, userID, logType.ToString());
    }

    /// <inheritdoc />
    void IEventLogger.AddLog(string name, string value, IPortalSettings portalSettings, int userID, string logType)
    {
        var properties = new LogProperties();
        var logDetailInfo = new LogDetailInfo { PropertyName = name, PropertyValue = value };
        properties.Add(logDetailInfo);
        this.AddLog(properties, portalSettings, userID, logType, false);
    }

    /// <inheritdoc />
    void IEventLogger.AddLog(ILogProperties properties, IPortalSettings portalSettings, int userID, string logTypeKey, bool bypassBuffering)
    {
        // supports adding a custom string for LogType
        var log = new LogInfo
        {
            LogUserID = userID,
            LogTypeKey = logTypeKey,
            LogProperties = (LogProperties)properties,
            BypassBuffering = bypassBuffering,
        };

        if (portalSettings != null)
        {
            log.LogPortalID = portalSettings.PortalId;
            log.LogPortalName = portalSettings.PortalName;
        }

        LogController.Instance.AddLog(log);
    }

    /// <inheritdoc />
    void IEventLogger.AddLog(IPortalSettings portalSettings, int userID, Abstractions.Logging.EventLogType logType)
    {
        this.EventLogger.AddLog(new LogProperties(), portalSettings, userID, logType.ToString(), false);
    }

    /// <inheritdoc />
    void IEventLogger.AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, Abstractions.Logging.EventLogType logType)
    {
        this.AddLog(businessObject, portalSettings, userID, userName, logType.ToString());
    }

    /// <inheritdoc />
    void IEventLogger.AddLog(object businessObject, IPortalSettings portalSettings, int userID, string userName, string logType)
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

    /// <inheritdoc />
    void IEventLogger.AddLog(ILogInfo logInfo)
    {
        LogController.Instance.AddLog((LogInfo)logInfo);
    }

    /// <inheritdoc />
    void IEventLogConfigService.AddLogType(string configFile, string fallbackConfigFile)
    {
        LogController.Instance.AddLogType(configFile, fallbackConfigFile);
    }

    /// <inheritdoc />
    void IEventLogConfigService.AddLogType(ILogTypeInfo logType)
    {
        LogController.Instance.AddLogType((LogTypeInfo)logType);
    }

    /// <inheritdoc />
    void IEventLogConfigService.AddLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig)
    {
        LogController.Instance.AddLogTypeConfigInfo((LogTypeConfigInfo)logTypeConfig);
    }

    /// <inheritdoc />
    void IEventLogService.ClearLog()
    {
        LogController.Instance.ClearLog();
    }

    /// <inheritdoc />
    void IEventLogService.DeleteLog(ILogInfo logInfo)
    {
        LogController.Instance.DeleteLog((LogInfo)logInfo);
    }

    /// <inheritdoc />
    void IEventLogConfigService.DeleteLogType(ILogTypeInfo logType)
    {
        LogController.Instance.DeleteLogType((LogTypeInfo)logType);
    }

    /// <inheritdoc />
    void IEventLogConfigService.DeleteLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig)
    {
        LogController.Instance.DeleteLogTypeConfigInfo((LogTypeConfigInfo)logTypeConfig);
    }

    /// <inheritdoc />
    IEnumerable<ILogInfo> IEventLogService.GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
    {
        return LogController.Instance.GetLogs(portalID, logType, pageSize, pageIndex, ref totalRecords);
    }

    /// <inheritdoc />
    IEnumerable<ILogTypeConfigInfo> IEventLogConfigService.GetLogTypeConfigInfo()
    {
        return LogController.Instance.GetLogTypeConfigInfo().Cast<ILogTypeConfigInfo>();
    }

    /// <inheritdoc />
    ILogTypeConfigInfo IEventLogConfigService.GetLogTypeConfigInfoByID(string id)
    {
        return LogController.Instance.GetLogTypeConfigInfoByID(id);
    }

    /// <inheritdoc />
    IDictionary<string, ILogTypeInfo> IEventLogConfigService.GetLogTypeInfoDictionary()
    {
        return LogController.Instance.GetLogTypeInfoDictionary()
            .ToDictionary(key => key.Key, value => (ILogTypeInfo)value.Value);
    }

    /// <inheritdoc />
    public ILogInfo GetLog(string logGuid)
    {
        return LogController.Instance.GetLog(logGuid);
    }

    /// <inheritdoc />
    void IEventLogService.PurgeLogBuffer()
    {
        LogController.Instance.PurgeLogBuffer();
    }

    /// <inheritdoc />
    void IEventLogConfigService.UpdateLogTypeConfigInfo(ILogTypeConfigInfo logTypeConfig)
    {
        LogController.Instance.UpdateLogTypeConfigInfo((LogTypeConfigInfo)logTypeConfig);
    }

    /// <inheritdoc />
    void IEventLogConfigService.UpdateLogType(ILogTypeInfo logType)
    {
        LogController.Instance.UpdateLogType((LogTypeInfo)logType);
    }
}
