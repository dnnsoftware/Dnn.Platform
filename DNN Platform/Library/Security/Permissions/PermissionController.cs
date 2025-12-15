// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>The default <see cref="IPermissionDefinitionService"/> implementation.</summary>
    public partial class PermissionController : IPermissionDefinitionService
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        public static string BuildPermissions(IList permissions, string permissionKey)
        {
            var permissionsBuilder = new StringBuilder();
            foreach (PermissionInfoBase permission in permissions)
            {
                if (permissionKey.Equals(permission.PermissionKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Deny permissions are prefixed with a "!"
                    string prefix = !permission.AllowAccess ? "!" : string.Empty;

                    // encode permission
                    string permissionString;
                    if (Null.IsNull(permission.UserID))
                    {
                        permissionString = prefix + permission.RoleName + ";";
                    }
                    else
                    {
                        permissionString = prefix + "[" + permission.UserID + "];";
                    }

                    // build permissions string ensuring that Deny permissions are inserted at the beginning and Grant permissions at the end
                    if (prefix == "!")
                    {
                        permissionsBuilder.Insert(0, permissionString);
                    }
                    else
                    {
                        permissionsBuilder.Append(permissionString);
                    }
                }
            }

            // get string
            string permissionsString = permissionsBuilder.ToString();

            // ensure leading delimiter
            if (!permissionsString.StartsWith(";"))
            {
                permissionsString = permissionsString.Insert(0, ";");
            }

            return permissionsString;
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByFolder" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByFolder)} instead.")]
        public static partial ArrayList GetPermissionsByFolder()
        {
            return new ArrayList(GetPermissionsByFolderEnumerable().ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByPortalDesktopModule" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByPortalDesktopModule)} instead.")]
        public static partial ArrayList GetPermissionsByPortalDesktopModule()
        {
            return new ArrayList(GetPermissionsByPortalDesktopModuleEnumerable().ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByTab" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByTab)} instead.")]
        public static partial ArrayList GetPermissionsByTab()
        {
            return new ArrayList(GetPermissionsByTabEnumerable().ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.AddDefinition" />
        public int AddPermission(PermissionInfo permission)
        {
            return this.AddPermission((IPermissionDefinitionInfo)permission);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.AddDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int AddPermission(IPermissionDefinitionInfo permission)
        {
            EventLogController.Instance.AddLog(permission, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PERMISSION_CREATED);
            var permissionId = Convert.ToInt32(Provider.AddPermission(
                permission.PermissionCode,
                permission.ModuleDefId,
                permission.PermissionKey,
                permission.PermissionName,
                UserController.Instance.GetCurrentUserInfo().UserID));

            ClearCache();
            return permissionId;
        }

        /// <inheritdoc cref="IPermissionDefinitionService.DeleteDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void DeletePermission(int permissionID)
        {
            EventLogController.Instance.AddLog(
                "PermissionID",
                permissionID.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.PERMISSION_DELETED);
            Provider.DeletePermission(permissionID);
            ClearCache();
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PermissionInfo GetPermission(int permissionID)
        {
            return GetPermissions().SingleOrDefault(p => p.PermissionID == permissionID);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByCodeAndKey" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByCodeAndKey)} instead.")]
        public partial ArrayList GetPermissionByCodeAndKey(string permissionCode, string permissionKey)
        {
            return new ArrayList(GetPermissionByCodeAndKeyEnumerable(permissionCode, permissionKey).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByModuleDefId" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByModuleDefId)} instead.")]
        public partial ArrayList GetPermissionsByModuleDefID(int moduleDefID)
        {
            return new ArrayList(GetPermissionsByModuleDefIdEnumerable(moduleDefID).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.GetDefinitionsByModule" />
        [DnnDeprecated(9, 13, 1, $"Use {nameof(IPermissionDefinitionService)}.{nameof(IPermissionDefinitionService.GetDefinitionsByModule)} instead.")]
        public partial ArrayList GetPermissionsByModule(int moduleId, int tabId)
        {
            return new ArrayList(GetPermissionsByModuleEnumerable(moduleId, tabId).ToArray());
        }

        /// <inheritdoc cref="IPermissionDefinitionService.UpdateDefinition" />
        public void UpdatePermission(PermissionInfo permission)
        {
            this.UpdatePermission((IPermissionDefinitionInfo)permission);
        }

        /// <inheritdoc cref="IPermissionDefinitionService.UpdateDefinition" />
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void UpdatePermission(IPermissionDefinitionInfo permission)
        {
            EventLogController.Instance.AddLog(permission, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PERMISSION_UPDATED);
            Provider.UpdatePermission(
                permission.PermissionId,
                permission.PermissionCode,
                permission.ModuleDefId,
                permission.PermissionKey,
                permission.PermissionName,
                UserController.Instance.GetCurrentUserInfo().UserID);
            ClearCache();
        }

        public T RemapPermission<T>(T permission, int portalId)
            where T : PermissionInfoBase
        {
            PermissionInfo permissionInfo = this.GetPermissionByCodeAndKey(permission.PermissionCode, permission.PermissionKey).ToArray().Cast<PermissionInfo>().FirstOrDefault();
            T result = null;

            if (permissionInfo != null)
            {
                int roleID = int.MinValue;
                int userID = int.MinValue;

                if (string.IsNullOrEmpty(permission.RoleName))
                {
                    UserInfo user = UserController.GetUserByName(portalId, permission.Username);
                    if (user != null)
                    {
                        userID = user.UserID;
                    }
                }
                else
                {
                    switch (permission.RoleName)
                    {
                        case Globals.glbRoleAllUsersName:
                            roleID = Convert.ToInt32(Globals.glbRoleAllUsers);
                            break;
                        case Globals.glbRoleUnauthUserName:
                            roleID = Convert.ToInt32(Globals.glbRoleUnauthUser);
                            break;
                        default:
                            RoleInfo role = RoleController.Instance.GetRole(portalId, r => r.RoleName == permission.RoleName);
                            if (role != null)
                            {
                                roleID = role.RoleID;
                            }

                            break;
                    }
                }

                // if role was found add, otherwise ignore
                if (roleID != int.MinValue || userID != int.MinValue)
                {
                    permission.PermissionID = permissionInfo.PermissionID;
                    if (roleID != int.MinValue)
                    {
                        permission.RoleID = roleID;
                    }

                    if (userID != int.MinValue)
                    {
                        permission.UserID = userID;
                    }

                    result = permission;
                }
            }

            return result;
        }

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitions() => GetPermissions();

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByFolder() => GetPermissionsByFolderEnumerable();

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByPortalDesktopModule() => GetPermissionsByPortalDesktopModuleEnumerable();

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByTab() => GetPermissionsByTabEnumerable();

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByCodeAndKey(string permissionCode, string permissionKey) => GetPermissionByCodeAndKeyEnumerable(permissionCode, permissionKey);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByModuleDefId(int moduleDefId) => GetPermissionsByModuleDefIdEnumerable(moduleDefId);

        /// <inheritdoc />
        IEnumerable<IPermissionDefinitionInfo> IPermissionDefinitionService.GetDefinitionsByModule(int moduleId, int tabId) => GetPermissionsByModuleEnumerable(moduleId, tabId);

        /// <inheritdoc />
        int IPermissionDefinitionService.AddDefinition(IPermissionDefinitionInfo permission) => this.AddPermission(permission);

        /// <inheritdoc />
        void IPermissionDefinitionService.DeleteDefinition(IPermissionDefinitionInfo permission) => this.DeletePermission(permission.PermissionId);

        /// <inheritdoc />
        IPermissionDefinitionInfo IPermissionDefinitionService.GetDefinition(int permissionDefinitionId) => this.GetPermission(permissionDefinitionId);

        /// <inheritdoc />
        void IPermissionDefinitionService.UpdateDefinition(IPermissionDefinitionInfo permission) => this.UpdatePermission(permission);

        /// <inheritdoc />
        void IPermissionDefinitionService.ClearCache() => ClearCache();

        private static IEnumerable<PermissionInfo> GetPermissions()
        {
            return CBO.GetCachedObject<IEnumerable<PermissionInfo>>(
                new CacheItemArgs(
                DataCache.PermissionsCacheKey,
                DataCache.PermissionsCacheTimeout,
                DataCache.PermissionsCachePriority),
                c => CBO.FillCollection<PermissionInfo>(Provider.ExecuteReader("GetPermissions")));
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByFolderEnumerable()
        {
            return GetPermissions().Where(p => p.PermissionCode == "SYSTEM_FOLDER");
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByPortalDesktopModuleEnumerable()
        {
            return GetPermissions().Where(p => p.PermissionCode == "SYSTEM_DESKTOPMODULE");
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByTabEnumerable()
        {
            return GetPermissions().Where(p => p.PermissionCode == "SYSTEM_TAB");
        }

        private static IEnumerable<PermissionInfo> GetPermissionByCodeAndKeyEnumerable(string permissionCode, string permissionKey)
        {
            return GetPermissions().Where(p => p.PermissionCode.Equals(permissionCode, StringComparison.InvariantCultureIgnoreCase)
                                               && p.PermissionKey.Equals(permissionKey, StringComparison.InvariantCultureIgnoreCase));
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByModuleDefIdEnumerable(int moduleDefId)
        {
            return GetPermissions().Where(p => p.ModuleDefID == moduleDefId);
        }

        private static IEnumerable<PermissionInfo> GetPermissionsByModuleEnumerable(int moduleId, int tabId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, tabId, false);
            var moduleDefId = module.ModuleDefID;

            return GetPermissions().Where(p => p.ModuleDefID == moduleDefId || p.PermissionCode == "SYSTEM_MODULE_DEFINITION");
        }

        private static void ClearCache()
        {
            DataCache.RemoveCache(DataCache.PermissionsCacheKey);
        }
    }
}
