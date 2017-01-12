using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    public class ModulePackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePackageEditor));

        #region IPackageEditor Implementation

        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);

            if(desktopModule == null)
            {
                return new PackageInfoDto(portalId, package);
            }
        
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;

            var detail = isHostUser ? new ModulePackageDetailDto(portalId, package, desktopModule)
                                        : new ModulePackagePermissionsDto(portalId, package);

            detail.DesktopModuleId = desktopModule.DesktopModuleID;
            detail.Permissions = GetPermissionsData(portalId, desktopModule.DesktopModuleID);

            return detail;
        }

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

                UpdatePermissions(desktopModule, packageSettings);

                if(isHostUser)
                {
                    foreach (var settingName in packageSettings.EditorActions.Keys)
                    {
                        var settingValue = packageSettings.EditorActions[settingName];

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
                                desktopModule.Shareable = (ModuleSharing) Convert.ToInt32(settingValue);
                                break;
                            case "assignportal":
                                AssignPortals(desktopModule, JsonConvert.DeserializeObject<IList<ListItemDto>>(settingValue));
                                break;
                            case "unassignportal":
                                UnassignPortals(desktopModule, JsonConvert.DeserializeObject<IList<ListItemDto>>(settingValue));
                                break;
                            case "savedefinition":
                                var definition = JsonConvert.DeserializeObject<ModuleDefinitionDto>(settingValue);
                                SaveModuleDefinition(definition);
                                break;
                            case "deletedefinition":
                                DeleteModuleDefinition(Convert.ToInt32(settingValue));
                                break;
                            case "savemodulecontrol":
                                var moduleControl = JsonConvert.DeserializeObject<ModuleControlDto>(settingValue);
                                SaveModuleControl(moduleControl);
                                break;
                            case "deletemodulecontrol":
                                DeleteModuleControl(Convert.ToInt32(settingValue));
                                break;
                        }
                    }

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

        #endregion

        #region Private Methods

        private PermissionsDto GetPermissionsData(int portalId, int desktopModuleId)
        {
            var permissions = new PermissionsDto(true);
            if (desktopModuleId > 0)
            {
                var portalModule = DesktopModuleController.GetPortalDesktopModule(portalId, desktopModuleId);
                if (portalModule != null)
                {
                    permissions.DesktopModuleId = desktopModuleId;

                    var modulePermissions = DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID);
                    foreach (DesktopModulePermissionInfo permission in modulePermissions)
                    {
                        if (permission.UserID != Null.NullInteger)
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

        private void UpdatePermissions(DesktopModuleInfo desktopModule, PackageSettingsDto packageSettings)
        {
            if (!packageSettings.EditorActions.ContainsKey("permissions") || string.IsNullOrEmpty(packageSettings.EditorActions["permissions"]))
            {
                return;
            }

            var portalModule = DesktopModuleController.GetPortalDesktopModule(packageSettings.PortalId, desktopModule.DesktopModuleID);
            if (portalModule == null)
            {
                return;
            }

            var portalSettings = new PortalSettings(packageSettings.PortalId);
            var permissions = JsonConvert.DeserializeObject<PermissionsDto>(packageSettings.EditorActions["permissions"]);
            var hasAdmin = permissions.RolePermissions == null ? false : permissions.RolePermissions.Any(permission => permission.RoleId == portalSettings.AdministratorRoleId);

            var desktopModulePermissions = new DesktopModulePermissionCollection();
            //add default permissions for administrators
            if (!hasAdmin || (permissions.RolePermissions.Count == 0 && permissions.UserPermissions.Count == 0))
            {
                //add default permissions
                var permissionController = new PermissionController();
                var permissionsList = permissionController.GetPermissionByCodeAndKey("SYSTEM_DESKTOPMODULE", "DEPLOY");
                foreach (PermissionInfo permissionInfo in permissionsList)
                {
                    var permission = new DesktopModulePermissionInfo(permissionInfo)
                    {
                        RoleID = portalSettings.AdministratorRoleId,
                        UserID = Null.NullInteger,
                        PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                        AllowAccess = true,
                        RoleName = portalSettings.AdministratorRoleName
                    };
                    desktopModulePermissions.Add(permission);

                }
            }

            //add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions)
                {
                    foreach (var permission in rolePermission.Permissions)
                    {
                        desktopModulePermissions.Add(new DesktopModulePermissionInfo()
                        {
                            PermissionID = permission.PermissionId,
                            RoleID = rolePermission.RoleId,
                            UserID = Null.NullInteger,
                            PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                            AllowAccess = permission.AllowAccess
                        });
                    }
                }
            }


            //add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    foreach (var permission in userPermission.Permissions)
                    {
                        int roleId;
                        int.TryParse(Globals.glbRoleNothing, out roleId);
                        desktopModulePermissions.Add(new DesktopModulePermissionInfo()
                        {
                            PermissionID = permission.PermissionId,
                            RoleID = roleId,
                            UserID = userPermission.UserId,
                            PortalDesktopModuleID = portalModule.PortalDesktopModuleID,
                            AllowAccess = permission.AllowAccess
                        });
                    }
                }
            }

            //Update DesktopModule Permissions
            var currentPermissions = DesktopModulePermissionController.GetDesktopModulePermissions(portalModule.PortalDesktopModuleID);
            if (!currentPermissions.CompareTo(desktopModulePermissions))
            {
                DesktopModulePermissionController.DeleteDesktopModulePermissionsByPortalDesktopModuleID(portalModule.PortalDesktopModuleID);
                foreach (DesktopModulePermissionInfo objPermission in desktopModulePermissions)
                {
                    DesktopModulePermissionController.AddDesktopModulePermission(objPermission);
                }
            }
            DataCache.RemoveCache(string.Format(DataCache.PortalDesktopModuleCacheKey, portalSettings.PortalId));
        }

        private static void UnassignPortals(DesktopModuleInfo desktopModule, IList<ListItemDto> portals)
        {
            foreach (var portal in portals)
            {
                DesktopModuleController.RemoveDesktopModuleFromPortal(portal.Id, desktopModule.DesktopModuleID, true);
            }
        }

        private static void AssignPortals(DesktopModuleInfo desktopModule, IList<ListItemDto> portals)
        {
            foreach(var portal in portals)
            {
                DesktopModuleController.AddDesktopModuleToPortal(portal.Id, desktopModule.DesktopModuleID, true, true);
            }
        }

        private static void SaveModuleDefinition(ModuleDefinitionDto definitionDto)
        {
            var moduleDefinition = definitionDto.ToModuleDefinitionInfo();
            ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);
        }

        private static void DeleteModuleDefinition(int defId)
        {
            new ModuleDefinitionController().DeleteModuleDefinition(defId);
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


        #endregion

    }
}