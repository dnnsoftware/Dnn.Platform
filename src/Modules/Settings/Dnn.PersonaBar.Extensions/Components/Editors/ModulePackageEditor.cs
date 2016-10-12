using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Dnn.PersonaBar.Extensions.Components.Dto;
using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
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

        public PackageDetailDto GetPackageDetail(int portalId, PackageInfo package)
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(package.PackageID);
            if (portalId == Null.NullInteger)
            {
                return new ModulePackageDetailDto(portalId, package, desktopModule);
            }
            else
            {
                var detail = new ModulePackagePermissionsDto(portalId, package)
                {
                    DesktopModuleId = desktopModule.DesktopModuleID,
                    Permissions = GetPermissionsData(portalId, desktopModule.DesktopModuleID)
                };

                return detail;
            }
        }

        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(packageSettings.PackageId);
                if (packageSettings.PortalId != Null.NullInteger)
                {
                    UpdatePermissions(desktopModule, packageSettings);
                }
                else
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
                            case "permissions":
                                desktopModule.Permissions = settingValue;
                                break;
                            case "shareable":
                                desktopModule.Shareable = (ModuleSharing) Convert.ToInt32(settingValue);
                                break;
                            case "assignportal":
                                AssignPortal(desktopModule, Convert.ToInt32(settingValue));
                                break;
                            case "unassignportal":
                                UnassignPortal(desktopModule, Convert.ToInt32(settingValue));
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
                        desktopModulePermissions.Add(new DesktopModulePermissionInfo()
                        {
                            PermissionID = permission.PermissionId,
                            RoleID = int.Parse(Globals.glbRoleNothing),
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

        private void UnassignPortal(DesktopModuleInfo desktopModule, int portalId)
        {
            DesktopModuleController.AddDesktopModuleToPortal(portalId, desktopModule.DesktopModuleID, true, true);
        }

        private void AssignPortal(DesktopModuleInfo desktopModule, int portalId)
        {
            DesktopModuleController.RemoveDesktopModuleFromPortal(portalId, desktopModule.DesktopModuleID, true);
        }

        private void SaveModuleDefinition(ModuleDefinitionDto definitionDto)
        {
            var moduleDefinition = new ModuleDefinitionInfo
            {
                ModuleDefID = definitionDto.Id,
                DesktopModuleID = definitionDto.DesktopModuleId,
                DefinitionName = definitionDto.Name,
                FriendlyName = definitionDto.FriendlyName,
                DefaultCacheTime = definitionDto.CacheTime
            };

            ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);
        }

        private void DeleteModuleDefinition(int defId)
        {
            new ModuleDefinitionController().DeleteModuleDefinition(defId);
        }

        private void SaveModuleControl(ModuleControlDto moduleControlDto)
        {
            var moduleControl = new ModuleControlInfo
            {
                ModuleControlID = moduleControlDto.Id,
                ModuleDefID = moduleControlDto.DefinitionId,
                ControlKey = moduleControlDto.Key,
                ControlTitle = moduleControlDto.Title,
                ControlSrc = moduleControlDto.Source,
                ControlType = moduleControlDto.Type,
                ViewOrder = moduleControlDto.Order,
                IconFile = moduleControlDto.Icon,
                HelpURL = moduleControlDto.HelpUrl,
                SupportsPopUps = moduleControlDto.SupportPopups,
                SupportsPartialRendering = moduleControlDto.SupportPartialRendering
            };

            ModuleControlController.SaveModuleControl(moduleControl, true);
        }

        private void DeleteModuleControl(int controlId)
        {
            ModuleControlController.DeleteModuleControl(controlId);
        }

        #endregion

    }
}