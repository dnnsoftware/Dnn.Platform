// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Caching;

    using Dnn.PersonaBar.Library.Data;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    using PermissionInfo = Dnn.PersonaBar.Library.Model.PermissionInfo;

    /// <summary>Controls menu permissions.</summary>
    public partial class MenuPermissionController
    {
        private const string PersonaBarMenuPermissionsCacheKey = "PersonaBarMenuPermissions{0}";
        private const string PersonaBarPermissionsCacheKey = "PersonaBarPermissions";
        private const string PermissionInitializedKey = "PersonaBarMenuPermissionsInitialized";

        private const string ViewPermissionKey = "VIEW";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MenuPermissionController));

        private static readonly DataService DataService = new DataService();
        private static readonly object ThreadLocker = new object();
        private static readonly object DefaultPermissionLocker = new object();
        private static readonly char[] RoleSeparator = ['|',];

        /// <summary>Gets a value indicating whether the current user can view the specified <paramref name="menu"/>.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menu">The menu.</param>
        /// <returns><see langword="true"/> if the user can view the menu, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial bool CanView(int portalId, MenuItem menu)
            => CanView(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, menu);

        /// <summary>Gets a value indicating whether the current user can view the specified <paramref name="menu"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menu">The menu.</param>
        /// <returns><see langword="true"/> if the user can view the menu, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload taking IPortalController")]
        public static partial bool CanView(IHostSettings hostSettings, int portalId, MenuItem menu)
            => CanView(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId, menu);

        /// <summary>Gets a value indicating whether the current user can view the specified <paramref name="menu"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menu">The menu.</param>
        /// <returns><see langword="true"/> if the user can view the menu, otherwise <see langword="false"/>.</returns>
        public static bool CanView(IHostSettings hostSettings, IPortalController portalController, int portalId, MenuItem menu)
        {
            return HasMenuPermission(GetMenuPermissions(hostSettings, portalController, portalId, menu.MenuId), ViewPermissionKey);
        }

        public static void DeleteMenuPermissions(int portalId, MenuItem menu)
        {
            DataService.DeletePersonbaBarMenuPermissionsByMenuId(portalId, menu.MenuId);
            ClearCache(portalId);
        }

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial MenuPermissionCollection GetMenuPermissions(int portalId)
            => GetMenuPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId);

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload taking IPortalController")]
        public static partial MenuPermissionCollection GetMenuPermissions(IHostSettings hostSettings, int portalId)
            => GetMenuPermissions(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId);

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        public static MenuPermissionCollection GetMenuPermissions(IHostSettings hostSettings, IPortalController portalController, int portalId)
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
                        EnsureMenuDefaultPermissions(hostSettings, portalController, portalId);
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

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="identifier">The menu identifier.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial MenuPermissionCollection GetMenuPermissions(int portalId, string identifier)
            => GetMenuPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, identifier);

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="identifier">The menu identifier.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload taking IPortalController")]
        public static partial MenuPermissionCollection GetMenuPermissions(IHostSettings hostSettings, int portalId, string identifier)
            => GetMenuPermissions(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId, identifier);

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="identifier">The menu identifier.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        public static MenuPermissionCollection GetMenuPermissions(IHostSettings hostSettings, IPortalController portalController, int portalId, string identifier)
        {
            var menu = PersonaBarRepository.Instance.GetMenuItem(identifier);
            if (menu == null)
            {
                return null;
            }

            return GetMenuPermissions(hostSettings, portalController, portalId, menu.MenuId);
        }

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menuId">The menu ID.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial MenuPermissionCollection GetMenuPermissions(int portalId, int menuId)
            => GetMenuPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, menuId);

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menuId">The menu ID.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload taking IPortalController")]
        public static partial MenuPermissionCollection GetMenuPermissions(IHostSettings hostSettings, int portalId, int menuId)
            => GetMenuPermissions(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId, menuId);

        /// <summary>Gets the menu permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menuId">The menu ID.</param>
        /// <returns>A <see cref="MenuPermissionCollection"/>.</returns>
        public static MenuPermissionCollection GetMenuPermissions(IHostSettings hostSettings, IPortalController portalController, int portalId, int menuId)
        {
            var permissions = GetMenuPermissions(hostSettings, portalController, portalId)
                    .Cast<MenuPermissionInfo>()
                    .Where(p => p.MenuId == menuId && (p.PortalId == Null.NullInteger || p.PortalId == portalId)).ToList();
            return new MenuPermissionCollection(permissions);
        }

        /// <summary>Gets a value indicating whether the current user has the specified menu permission.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menu">The menu.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <returns><see langword="true"/> if the user has permission, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial bool HasMenuPermission(int portalId, MenuItem menu, string permissionKey)
            => HasMenuPermission(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, menu, permissionKey);

        /// <summary>Gets a value indicating whether the current user has the specified menu permission.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menu">The menu.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <returns><see langword="true"/> if the user has permission, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload taking IPortalController")]
        public static partial bool HasMenuPermission(IHostSettings hostSettings, int portalId, MenuItem menu, string permissionKey)
            => HasMenuPermission(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId, menu, permissionKey);

        /// <summary>Gets a value indicating whether the current user has the specified menu permission.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menu">The menu.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <returns><see langword="true"/> if the user has permission, otherwise <see langword="false"/>.</returns>
        public static bool HasMenuPermission(IHostSettings hostSettings, IPortalController portalController, int portalId, MenuItem menu, string permissionKey)
            => HasMenuPermission(GetMenuPermissions(hostSettings, portalController, portalId, menu.MenuId), permissionKey);

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
                ((IPermissionInfo)permissionInfo).PermissionId,
                ((IPermissionInfo)permissionInfo).RoleId,
                ((IPermissionInfo)permissionInfo).UserId,
                permissionInfo.AllowAccess,
                user.UserID);

            ClearCache(portalId);
        }

        /// <summary>Gets the permissions for the menu.</summary>
        /// <param name="menuId">The menu ID.</param>
        /// <returns>A list of permissions.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IHostSettings")]
        public static partial IList<PermissionInfo> GetPermissions(int menuId)
            => GetPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), menuId);

        /// <summary>Gets the permissions for the menu.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="menuId">The menu ID.</param>
        /// <returns>A list of permissions.</returns>
        public static IList<PermissionInfo> GetPermissions(IHostSettings hostSettings, int menuId)
        {
            return GetAllPermissions(hostSettings)
                .Where(p => p.MenuId == Null.NullInteger || p.MenuId == menuId)
                .ToList();
        }

        /// <summary>Saves the menu's default permissions.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <param name="roleName">The role name.</param>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial void SaveMenuDefaultPermissions(int portalId, MenuItem menuItem, string roleName)
            => SaveMenuDefaultPermissions(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalId, menuItem, roleName);

        /// <summary>Saves the menu's default permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <param name="roleName">The role name.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload taking IPortalController")]
        public static partial void SaveMenuDefaultPermissions(IHostSettings hostSettings, int portalId, MenuItem menuItem, string roleName)
            => SaveMenuDefaultPermissions(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId, menuItem, roleName);

        /// <summary>Saves the menu's default permissions.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <param name="roleName">The role name.</param>
        public static void SaveMenuDefaultPermissions(IHostSettings hostSettings, IPortalController portalController, int portalId, MenuItem menuItem, string roleName)
            => SaveMenuDefaultPermissions(hostSettings, portalController, portalId, menuItem, roleName, false);

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

        /// <summary>Deletes a menu permission.</summary>
        /// <param name="menuId">The menu ID.</param>
        /// <param name="permissionKey">The permission key.</param>
        [DnnDeprecated(10, 2, 2, "Please use overload with IHostSettings")]
        public static partial void DeletePersonaBarPermission(int menuId, string permissionKey)
            => DeletePersonaBarPermission(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), menuId, permissionKey);

        /// <summary>Deletes a menu permission.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="menuId">The menu ID.</param>
        /// <param name="permissionKey">The permission key.</param>
        public static void DeletePersonaBarPermission(IHostSettings hostSettings, int menuId, string permissionKey)
        {
            var permission = GetAllPermissions(hostSettings).FirstOrDefault(p => p.MenuId == menuId && p.PermissionKey == permissionKey);

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

        private static void SetPermissionInitialized(IPortalController portalController, int portalId)
        {
            PortalController.UpdatePortalSetting(portalController, portalId, PermissionInitializedKey, "Y");
        }

        private static void EnsureMenuDefaultPermissions(IHostSettings hostSettings, IPortalController portalController, int portalId)
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
                                            SaveMenuDefaultPermissions(hostSettings, portalController, portalId, menuItem, roleName.Trim(), true);
                                        }
                                    }
                                }
                            }

                            SetPermissionInitialized(portalController, portalId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static void SaveMenuDefaultPermissions(IHostSettings hostSettings, IPortalController portalController, int portalId, MenuItem menuItem, string roleName, bool ignoreExists)
        {
            try
            {
                var defaultPermissions = roleName.Split(RoleSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (defaultPermissions.Count > 1)
                {
                    roleName = defaultPermissions[0];
                }

                defaultPermissions.RemoveAt(0);
                var administratorRole = PortalController.Instance.GetPortal(portalId).AdministratorRoleName;

                var nullRoleId = Convert.ToInt32(Globals.glbRoleNothing, CultureInfo.InvariantCulture);
                var permissions = GetPermissions(hostSettings, menuItem.MenuId)
                    .Where(p => p.MenuId == Null.NullInteger
                                    || (roleName == administratorRole && defaultPermissions.Count == 0) // Administrator gets all granular permissions only if no permission specified explicity.
                                    || defaultPermissions.Contains(p.PermissionKey));

                var roleId = nullRoleId;
                switch (roleName)
                {
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
                        break;
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
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
                        var menuPermissions = GetMenuPermissions(hostSettings, portalController, portalId, menuItem.MenuId);
                        permissions = permissions.Where(x => !menuPermissions.Any((IPermissionInfo y) => y.PermissionId == x.PermissionId && y.RoleId == roleId));
                    }

                    foreach (var permission in permissions)
                    {
                        var menuPermissionInfo = new MenuPermissionInfo
                        {
                            MenuPermissionId = Null.NullInteger,
                            MenuId = menuItem.MenuId,
                            AllowAccess = true,
                        };
                        ((IPermissionInfo)menuPermissionInfo).PermissionId = permission.PermissionId;
                        ((IPermissionInfo)menuPermissionInfo).RoleId = roleId;
                        ((IPermissionInfo)menuPermissionInfo).UserId = Null.NullInteger;

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
            return string.Format(CultureInfo.InvariantCulture, PersonaBarMenuPermissionsCacheKey, portalId);
        }

        private static IList<PermissionInfo> GetAllPermissions(IHostSettings hostSettings)
        {
            var cacheItemArgs = new CacheItemArgs(PersonaBarPermissionsCacheKey, 20, CacheItemPriority.AboveNormal);
            return CBO.GetCachedObject<IList<PermissionInfo>>(
                hostSettings,
                cacheItemArgs,
                static _ => CBO.FillCollection<PermissionInfo>(DataService.GetPersonaBarPermissions()));
        }
    }
}
