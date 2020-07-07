// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Log.EventLog;

    public class PermissionController
    {
        private static readonly DataProvider provider = DataProvider.Instance();

        public static string BuildPermissions(IList Permissions, string PermissionKey)
        {
            var permissionsBuilder = new StringBuilder();
            foreach (PermissionInfoBase permission in Permissions)
            {
                if (PermissionKey.Equals(permission.PermissionKey, StringComparison.InvariantCultureIgnoreCase))
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
                permissionsString.Insert(0, ";");
            }

            return permissionsString;
        }

        public static ArrayList GetPermissionsByFolder()
        {
            return new ArrayList(GetPermissions().Where(p => p.PermissionCode == "SYSTEM_FOLDER").ToArray());
        }

        public static ArrayList GetPermissionsByPortalDesktopModule()
        {
            return new ArrayList(GetPermissions().Where(p => p.PermissionCode == "SYSTEM_DESKTOPMODULE").ToArray());
        }

        public static ArrayList GetPermissionsByTab()
        {
            return new ArrayList(GetPermissions().Where(p => p.PermissionCode == "SYSTEM_TAB").ToArray());
        }

        public int AddPermission(PermissionInfo permission)
        {
            EventLogController.Instance.AddLog(permission, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PERMISSION_CREATED);
            var permissionId = Convert.ToInt32(provider.AddPermission(
                permission.PermissionCode,
                permission.ModuleDefID,
                permission.PermissionKey,
                permission.PermissionName,
                UserController.Instance.GetCurrentUserInfo().UserID));

            this.ClearCache();
            return permissionId;
        }

        public void DeletePermission(int permissionID)
        {
            EventLogController.Instance.AddLog(
                "PermissionID",
                permissionID.ToString(),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.PERMISSION_DELETED);
            provider.DeletePermission(permissionID);
            this.ClearCache();
        }

        public PermissionInfo GetPermission(int permissionID)
        {
            return GetPermissions().SingleOrDefault(p => p.PermissionID == permissionID);
        }

        public ArrayList GetPermissionByCodeAndKey(string permissionCode, string permissionKey)
        {
            return new ArrayList(GetPermissions().Where(p => p.PermissionCode.Equals(permissionCode, StringComparison.InvariantCultureIgnoreCase)
                                                             && p.PermissionKey.Equals(permissionKey, StringComparison.InvariantCultureIgnoreCase)).ToArray());
        }

        public ArrayList GetPermissionsByModuleDefID(int moduleDefID)
        {
            return new ArrayList(GetPermissions().Where(p => p.ModuleDefID == moduleDefID).ToArray());
        }

        public ArrayList GetPermissionsByModule(int moduleId, int tabId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, tabId, false);

            return new ArrayList(GetPermissions().Where(p => p.ModuleDefID == module.ModuleDefID || p.PermissionCode == "SYSTEM_MODULE_DEFINITION").ToArray());
        }

        public void UpdatePermission(PermissionInfo permission)
        {
            EventLogController.Instance.AddLog(permission, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.PERMISSION_UPDATED);
            provider.UpdatePermission(
                permission.PermissionID,
                permission.PermissionCode,
                permission.ModuleDefID,
                permission.PermissionKey,
                permission.PermissionName,
                UserController.Instance.GetCurrentUserInfo().UserID);
            this.ClearCache();
        }

        public T RemapPermission<T>(T permission, int portalId)
            where T : PermissionInfoBase
        {
            PermissionInfo permissionInfo = this.GetPermissionByCodeAndKey(permission.PermissionCode, permission.PermissionKey).ToArray().Cast<PermissionInfo>().FirstOrDefault();
            T result = null;

            if (permissionInfo != null)
            {
                int RoleID = int.MinValue;
                int UserID = int.MinValue;

                if (string.IsNullOrEmpty(permission.RoleName))
                {
                    UserInfo _user = UserController.GetUserByName(portalId, permission.Username);
                    if (_user != null)
                    {
                        UserID = _user.UserID;
                    }
                }
                else
                {
                    switch (permission.RoleName)
                    {
                        case Globals.glbRoleAllUsersName:
                            RoleID = Convert.ToInt32(Globals.glbRoleAllUsers);
                            break;
                        case Globals.glbRoleUnauthUserName:
                            RoleID = Convert.ToInt32(Globals.glbRoleUnauthUser);
                            break;
                        default:
                            RoleInfo _role = RoleController.Instance.GetRole(portalId, r => r.RoleName == permission.RoleName);
                            if (_role != null)
                            {
                                RoleID = _role.RoleID;
                            }

                            break;
                    }
                }

                // if role was found add, otherwise ignore
                if (RoleID != int.MinValue || UserID != int.MinValue)
                {
                    permission.PermissionID = permissionInfo.PermissionID;
                    if (RoleID != int.MinValue)
                    {
                        permission.RoleID = RoleID;
                    }

                    if (UserID != int.MinValue)
                    {
                        permission.UserID = UserID;
                    }

                    result = permission;
                }
            }

            return result;
        }

        [Obsolete("Deprecated in DNN 7.3.0. Replaced by GetPermissionsByModule(int, int). Scheduled removal in v10.0.0.")]
        public ArrayList GetPermissionsByModuleID(int moduleId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);

            return this.GetPermissionsByModuleDefID(module.ModuleDefID);
        }

        private static IEnumerable<PermissionInfo> GetPermissions()
        {
            return CBO.GetCachedObject<IEnumerable<PermissionInfo>>(
                new CacheItemArgs(
                DataCache.PermissionsCacheKey,
                DataCache.PermissionsCacheTimeout,
                DataCache.PermissionsCachePriority),
                c => CBO.FillCollection<PermissionInfo>(provider.ExecuteReader("GetPermissions")));
        }

        private void ClearCache()
        {
            DataCache.RemoveCache(DataCache.PermissionsCacheKey);
        }
    }
}
