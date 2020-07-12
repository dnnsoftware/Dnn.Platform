// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Caching;

    using Dnn.PersonaBar.Library.Data;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Repository;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;

    using PermissionInfo = Dnn.PersonaBar.Library.Model.PermissionInfo;

    public class MenuPermissionController
    {
        private const string PersonaBarMenuPermissionsCacheKey = "PersonaBarMenuPermissions{0}";
        private const string PersonaBarPermissionsCacheKey = "PersonaBarPermissions";
        private const string PermissionInitializedKey = "PersonaBarMenuPermissionsInitialized";

        private const string ViewPermissionKey = "VIEW";

        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(MenuPermissionController));

        private static readonly IDataService DataService = new DataService();
        private static readonly object ThreadLocker = new object();
        private static readonly object DefaultPermissionLocker = new object();

        public static bool CanView(int portalId, MenuItem menu)
        {
            return HasMenuPermission(GetMenuPermissions(portalId, menu.MenuId), ViewPermissionKey);
        }

        public static void DeleteMenuPermissions(int portalId, MenuItem menu)
        {
            DataService.DeletePersonbaBarMenuPermissionsByMenuId(portalId, menu.MenuId);
            ClearCache(portalId);
        }

        public static MenuPermissionCollection GetMenuPermissions(int portalId)
        {
            var cacheKey = GetCacheKey(portalId);
            var permissions = DataCache.GetCache<MenuPermissionCollection>(cacheKey);
            if (permissions == null)
            {
                lock (ThreadLocker)
                {
                    permissions = DataCache.GetCache<MenuPermissionCollection>(cacheKey);
                    if (permissions == null)
                    {
                        permissions = new MenuPermissionCollection();
                        EnsureMenuDefaultPermissions(portalId);
                        var reader = DataService.GetPersonbaBarMenuPermissionsByPortal(portalId);
                        try
                        {
                            while (reader.Read())
                            {
                                var permissionInfo = CBO.FillObject<MenuPermissionInfo>(reader, false);
                                permissions.Add(permissionInfo, true);
                            }

                            DataCache.SetCache(cacheKey, permissions);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                        finally
                        {
                            CBO.CloseDataReader(reader, true);
                        }
                    }
                }
            }

            return permissions;
        }

        public static MenuPermissionCollection GetMenuPermissions(int portalId, string identifier)
        {
            var menu = PersonaBarRepository.Instance.GetMenuItem(identifier);
            if (menu == null)
            {
                return null;
            }

            return GetMenuPermissions(portalId, menu.MenuId);
        }

        public static MenuPermissionCollection GetMenuPermissions(int portalId, int menuId)
        {
            var permissions = GetMenuPermissions(portalId)
                    .Cast<MenuPermissionInfo>()
                    .Where(p => p.MenuId == menuId && (p.PortalId == Null.NullInteger || p.PortalId == portalId)).ToList();
            return new MenuPermissionCollection(permissions);
        }

        public static bool HasMenuPermission(int portalId, MenuItem menu, string permissionKey)
        {
            return HasMenuPermission(GetMenuPermissions(portalId, menu.MenuId), permissionKey);
        }

        public static bool HasMenuPermission(MenuPermissionCollection menuPermissions, string permissionKey)
        {
            bool hasPermission = Null.NullBoolean;
            if (permissionKey.Contains(","))
            {
                foreach (string permission in permissionKey.Split(','))
                {
                    if (PortalSecurity.IsInRoles(menuPermissions.ToString(permission)))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }
            else
            {
                hasPermission = PortalSecurity.IsInRoles(menuPermissions.ToString(permissionKey));
            }

            return hasPermission;
        }

        public static void SaveMenuPermissions(int portalId, MenuItem menu, MenuPermissionInfo permissionInfo)
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            permissionInfo.MenuPermissionId = DataService.SavePersonaBarMenuPermission(
                portalId,
                menu.MenuId,
                permissionInfo.PermissionID,
                permissionInfo.RoleID,
                permissionInfo.UserID,
                permissionInfo.AllowAccess,
                user.UserID);

            ClearCache(portalId);
        }

        public static IList<PermissionInfo> GetPermissions(int menuId)
        {
            return GetAllPermissions()
                .Where(p => p.MenuId == Null.NullInteger || p.MenuId == menuId)
                .ToList();
        }

        public static void SaveMenuDefaultPermissions(int portalId, MenuItem menuItem, string roleName)
        {
            SaveMenuDefaultPermissions(portalId, menuItem, roleName, false);
        }

        public static void SavePersonaBarPermission(string menuIdentifier, string permissionKey, string permissionName)
        {
            var menu = PersonaBarRepository.Instance.GetMenuItem(menuIdentifier);
            if (menu == null)
            {
                return;
            }

            SavePersonaBarPermission(menu.MenuId, permissionKey, permissionName);
        }

        public static void SavePersonaBarPermission(int menuId, string permissionKey, string permissionName)
        {
            var user = UserController.Instance.GetCurrentUserInfo();

            DataService.SavePersonaBarPermission(menuId, permissionKey, permissionName, user.UserID);

            ClearCache(Null.NullInteger);
        }

        public static void DeletePersonaBarPermission(int menuId, string permissionKey)
        {
            var permission = GetAllPermissions().FirstOrDefault(p => p.MenuId == menuId && p.PermissionKey == permissionKey);

            if (permission != null)
            {
                DataService.DeletePersonaBarPermission(permission.PermissionId);
            }

            ClearCache(Null.NullInteger);
        }

        public static bool PermissionAlreadyInitialized(int portalId)
        {
            return PortalController.Instance.GetPortalSettings(portalId).ContainsKey(PermissionInitializedKey);
        }

        private static void SetPermissionIntialized(int portalId)
        {
            PortalController.UpdatePortalSetting(portalId, PermissionInitializedKey, "Y");
        }

        private static void EnsureMenuDefaultPermissions(int portalId)
        {
            try
            {
                var permissionInitialized = PermissionAlreadyInitialized(portalId);
                if (!permissionInitialized)
                {
                    lock (DefaultPermissionLocker)
                    {
                        permissionInitialized = PermissionAlreadyInitialized(portalId);
                        if (!permissionInitialized)
                        {
                            var menuItems = PersonaBarRepository.Instance.GetMenu().AllItems;
                            foreach (var menuItem in menuItems)
                            {
                                var defaultPermissions = PersonaBarRepository.Instance.GetMenuDefaultPermissions(menuItem.MenuId);
                                if (!string.IsNullOrEmpty(defaultPermissions))
                                {
                                    foreach (var roleName in defaultPermissions.Split(','))
                                    {
                                        if (!string.IsNullOrEmpty(roleName.Trim()))
                                        {
                                            SaveMenuDefaultPermissions(portalId, menuItem, roleName.Trim(), true);
                                        }
                                    }
                                }
                            }

                            SetPermissionIntialized(portalId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void SaveMenuDefaultPermissions(int portalId, MenuItem menuItem, string roleName, bool ignoreExists)
        {
            try
            {
                var defaultPermissions = roleName.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (defaultPermissions.Count > 1)
                {
                    roleName = defaultPermissions[0];
                }

                defaultPermissions.RemoveAt(0);
                var administratorRole = PortalController.Instance.GetPortal(portalId).AdministratorRoleName;

                var nullRoleId = Convert.ToInt32(Globals.glbRoleNothing);
                var permissions = GetPermissions(menuItem.MenuId)
                    .Where(p => p.MenuId == Null.NullInteger
                                    || (roleName == administratorRole && defaultPermissions.Count == 0) // Administrator gets all granular permissions only if no permission specified explicity.
                                    || defaultPermissions.Contains(p.PermissionKey));

                var roleId = nullRoleId;
                switch (roleName)
                {
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser);
                        break;
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers);
                        break;
                    default:
                        var role = RoleController.Instance.GetRoleByName(portalId, roleName);
                        if (role != null && role.IsSystemRole)
                        {
                            roleId = role.RoleID;
                        }
                        else if (role != null)
                        {
                            Logger.Error($"Role \"{roleName}\" in portal \"{portalId}\" doesn't marked as system role, will ignore add this default permission to {menuItem.Identifier}.");
                        }

                        break;
                }

                if (roleId > nullRoleId)
                {
                    if (!ignoreExists)
                    {
                        var menuPermissions = GetMenuPermissions(portalId, menuItem.MenuId);
                        permissions =
                            permissions.Where(
                                x =>
                                    !menuPermissions.ToList()
                                        .Exists(y => y.PermissionID == x.PermissionId && y.RoleID == roleId));
                    }

                    foreach (var permission in permissions)
                    {
                        var menuPermissionInfo = new MenuPermissionInfo
                        {
                            MenuPermissionId = Null.NullInteger,
                            MenuId = menuItem.MenuId,
                            PermissionID = permission.PermissionId,
                            RoleID = roleId,
                            UserID = Null.NullInteger,
                            AllowAccess = true,
                        };

                        SaveMenuPermissions(portalId, menuItem, menuPermissionInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void ClearCache(int portalId)
        {
            if (portalId > Null.NullInteger)
            {
                var cacheKey = GetCacheKey(portalId);
                DataCache.RemoveCache(cacheKey);
            }
            else
            {
                DataCache.RemoveCache(PersonaBarPermissionsCacheKey);
            }
        }

        private static string GetCacheKey(int portalId)
        {
            return string.Format(PersonaBarMenuPermissionsCacheKey, portalId);
        }

        private static IList<PermissionInfo> GetAllPermissions()
        {
            var cacheItemArgs = new CacheItemArgs(PersonaBarPermissionsCacheKey, 20, CacheItemPriority.AboveNormal);
            return CBO.GetCachedObject<IList<PermissionInfo>>(cacheItemArgs, c =>
                CBO.FillCollection<PermissionInfo>(DataService.GetPersonaBarPermissions()));
        }
    }
}
