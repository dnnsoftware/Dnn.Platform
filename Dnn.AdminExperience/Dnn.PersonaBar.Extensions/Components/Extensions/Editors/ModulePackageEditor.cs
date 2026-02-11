// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using Dnn.PersonaBar.Library.Helper;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>An <see cref="IPackageEditor"/> implementation for modules.</summary>
    /// <param name="permissionDefinitionService">The permission definition service.</param>
    /// <param name="eventLogger">The event logger.</param>
    public class ModulePackageEditor(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger) : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePackageEditor));
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

        /// <summary>Initializes a new instance of the <see cref="ModulePackageEditor"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ModulePackageEditor()
            : this(null, null)
        {
        }

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);

            if (desktopModule == null)
            {
                return new PackageInfoDto(portalId, package);
            }

            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            var detail = isHostUser ? new ModulePackageDetailDto(portalId, package, desktopModule)
                                        : new ModulePackagePermissionsDto(portalId, package);

            detail.DesktopModuleId = desktopModule.DesktopModuleID;
            detail.Permissions = GetPermissionsData(this.permissionDefinitionService, portalId, desktopModule.DesktopModuleID);

            return detail;
        }

        /// <inheritdoc />
        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(packageSettings.PackageId);

                if (desktopModule == null)
                {
                    return false;
                }

                var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

                UpdatePermissions(this.permissionDefinitionService, this.eventLogger, desktopModule, packageSettings);

                if (isHostUser)
                {
                    UpdateModuleProperties(this.permissionDefinitionService, this.eventLogger, desktopModule, packageSettings.Settings);
                    UpdateModuleProperties(this.permissionDefinitionService, this.eventLogger, desktopModule, packageSettings.EditorActions);

                    DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        private static void UnassignPortals(IEventLogger eventLogger, DesktopModuleInfo desktopModule, IList<ListItemDto> portals)
        {
            foreach (var portal in portals)
            {
                DesktopModuleController.RemoveDesktopModuleFromPortal(eventLogger, portal.Id, desktopModule.DesktopModuleID, true);
            }
        }

        private static void AssignPortals(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, DesktopModuleInfo desktopModule, IList<ListItemDto> portals)
        {
            foreach (var portal in portals)
            {
                DesktopModuleController.AddDesktopModuleToPortal(eventLogger, permissionDefinitionService, portal.Id, desktopModule.DesktopModuleID, true, true);
            }
        }

        private static void SaveModuleDefinition(IPermissionDefinitionService permissionDefinitionService, ModuleDefinitionDto definitionDto)
        {
            var moduleDefinition = definitionDto.ToModuleDefinitionInfo();
            ModuleDefinitionController.SaveModuleDefinition(permissionDefinitionService, moduleDefinition, false, true);
        }

        private static void DeleteModuleDefinition(IPermissionDefinitionService permissionDefinitionService, int defId)
        {
            new ModuleDefinitionController(permissionDefinitionService).DeleteModuleDefinition(defId);
        }

        private static void SaveModuleControl(ModuleControlDto moduleControlDto)
        {
            var moduleControl = moduleControlDto.ToModuleControlInfo();
            ModuleControlController.SaveModuleControl(moduleControl, true);
        }

        private static void DeleteModuleControl(int controlId)
        {
            ModuleControlController.DeleteModuleControl(controlId);
        }

        private static void UpdateModuleProperties(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, DesktopModuleInfo desktopModule, IDictionary<string, string> settings)
        {
            foreach (var setting in settings)
            {
                var settingName = setting.Key;
                var settingValue = setting.Value;

                switch (settingName.ToLowerInvariant())
                {
                    case "foldername":
                        desktopModule.FolderName = settingValue;
                        break;
                    case "category":
                        desktopModule.Category = settingValue;
                        break;
                    case "businesscontroller":
                        desktopModule.BusinessControllerClass = settingValue;
                        break;
                    case "dependencies":
                        desktopModule.Dependencies = settingValue;
                        break;
                    case "hostpermissions":
                        desktopModule.Permissions = settingValue;
                        break;
                    case "premiummodule":
                        desktopModule.IsPremium = Convert.ToBoolean(settingValue);
                        break;
                    case "shareable":
                        desktopModule.Shareable = (ModuleSharing)Convert.ToInt32(settingValue, CultureInfo.InvariantCulture);
                        break;
                    case "assignportal":
                        AssignPortals(permissionDefinitionService, eventLogger, desktopModule, JsonConvert.DeserializeObject<IList<ListItemDto>>(settingValue));
                        break;
                    case "unassignportal":
                        UnassignPortals(eventLogger, desktopModule, JsonConvert.DeserializeObject<IList<ListItemDto>>(settingValue));
                        break;
                    case "savedefinition":
                        var definition = JsonConvert.DeserializeObject<ModuleDefinitionDto>(settingValue);
                        SaveModuleDefinition(permissionDefinitionService, definition);
                        break;
                    case "deletedefinition":
                        DeleteModuleDefinition(permissionDefinitionService, Convert.ToInt32(settingValue, CultureInfo.InvariantCulture));
                        break;
                    case "savemodulecontrol":
                        var moduleControl = JsonConvert.DeserializeObject<ModuleControlDto>(settingValue);
                        SaveModuleControl(moduleControl);
                        break;
                    case "deletemodulecontrol":
                        DeleteModuleControl(Convert.ToInt32(settingValue, CultureInfo.InvariantCulture));
                        break;
                    case "friendlyname":
                        desktopModule.FriendlyName = settingValue;
                        break;
                }
            }
        }

        private static PermissionsDto GetPermissionsData(IPermissionDefinitionService permissionDefinitionService, int portalId, int desktopModuleId)
        {
            var permissions = new PermissionsDto(permissionDefinitionService, true);
            if (desktopModuleId > 0)
            {
                var portalModule = DesktopModuleController.GetPortalDesktopModule(portalId, desktopModuleId);
                if (portalModule != null)
                {
                    permissions.DesktopModuleId = desktopModuleId;

                    var modulePermissions = DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID);
                    foreach (IPermissionInfo permission in modulePermissions)
                    {
                        if (permission.UserId != Null.NullInteger)
                        {
                            permissions.AddUserPermission(permission);
                        }
                        else
                        {
                            permissions.AddRolePermission(permission);
                        }
                    }

                    permissions.RolePermissions =
                        permissions.RolePermissions.OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                    permissions.UserPermissions = permissions.UserPermissions.OrderBy(p => p.DisplayName).ToList();
                }
            }

            return permissions;
        }

        private static void UpdatePermissions(IPermissionDefinitionService permissionDefinitionService, IEventLogger eventLogger, DesktopModuleInfo desktopModule, PackageSettingsDto packageSettings)
        {
            if (!packageSettings.EditorActions.TryGetValue("permissions", out var permissionsJson) || string.IsNullOrEmpty(permissionsJson))
            {
                return;
            }

            var portalModule = DesktopModuleController.GetPortalDesktopModule(packageSettings.PortalId, desktopModule.DesktopModuleID);
            if (portalModule == null)
            {
                return;
            }

            var portalSettings = new PortalSettings(packageSettings.PortalId);
            var permissions = JsonConvert.DeserializeObject<PermissionsDto>(permissionsJson);
            var hasAdmin = permissions.RolePermissions?.Any(permission => permission.RoleId == portalSettings.AdministratorRoleId) ?? false;

            var desktopModulePermissions = new DesktopModulePermissionCollection();

            // add default permissions for administrators
            if (!hasAdmin || (permissions.RolePermissions.Count == 0 && permissions.UserPermissions.Count == 0))
            {
                // add default permissions
                var permissionsList = permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_DESKTOPMODULE", "DEPLOY");
                foreach (var permissionInfo in permissionsList)
                {
                    var permission = new DesktopModulePermissionInfo(permissionInfo)
                    {
                        PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                        AllowAccess = true,
                        RoleName = portalSettings.AdministratorRoleName,
                    };
                    ((IPermissionInfo)permission).RoleId = portalSettings.AdministratorRoleId;
                    ((IPermissionInfo)permission).UserId = Null.NullInteger;
                    desktopModulePermissions.Add(permission);
                }
            }

            // add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions)
                {
                    foreach (var permission in rolePermission.Permissions)
                    {
                        var info = new DesktopModulePermissionInfo
                        {
                            PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                            AllowAccess = permission.AllowAccess,
                        };
                        ((IPermissionInfo)info).PermissionId = permission.PermissionId;
                        ((IPermissionInfo)info).RoleId = rolePermission.RoleId;
                        ((IPermissionInfo)info).UserId = Null.NullInteger;
                        desktopModulePermissions.Add(info);
                    }
                }
            }

            // add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    foreach (var permission in userPermission.Permissions)
                    {
                        if (!int.TryParse(Globals.glbRoleNothing, out var roleId))
                        {
                            roleId = -4;
                        }

                        var desktopModulePermissionInfo = new DesktopModulePermissionInfo
                        {
                            PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                            AllowAccess = permission.AllowAccess,
                        };
                        ((IPermissionInfo)desktopModulePermissionInfo).PermissionId = permission.PermissionId;
                        ((IPermissionInfo)desktopModulePermissionInfo).RoleId = roleId;
                        ((IPermissionInfo)desktopModulePermissionInfo).UserId = userPermission.UserId;
                        desktopModulePermissions.Add(desktopModulePermissionInfo);
                    }
                }
            }

            // Update DesktopModule Permissions
            var currentPermissions = DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID);
            if (!currentPermissions.CompareTo(desktopModulePermissions))
            {
                DesktopModulePermissionController.DeleteDesktopModulePermissionsByPortalDesktopModuleID(eventLogger, portalModule.PortalDesktopModuleID);
                foreach (DesktopModulePermissionInfo objPermission in desktopModulePermissions)
                {
                    DesktopModulePermissionController.AddDesktopModulePermission(eventLogger, objPermission);
                }
            }

            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.PortalDesktopModuleCacheKey, portalSettings.PortalId));
        }
    }
}
