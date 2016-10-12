#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Library.Data;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using Dnn.PersonaBar.Library.PersonaBar.Repository;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;

#endregion

namespace Dnn.PersonaBar.Library.PersonaBar.Permissions
{
    public class MenuPermissionController
    {
        #region Private Members

        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(MenuPermissionController));

        private static readonly PermissionProvider _provider = PermissionProvider.Instance();
        private static IDataService _dataService = new DataService();
        private static object _threadLocker = new object();

        private const string PersonaBarMenuPermissionsCacheKey = "PersonaBarMenuPermissions{0}";
        private const string PermissionInitializedKey = "PersonaBarMenuPermissionsInitialized";

        private const string PersonaBarMenuViewPermissionKey = "VIEW";
        private const string PersonaBarMenuEditPermissionKey = "EDIT";
        private const string PersonaBarMenuAdminPermissionKey = "ADMIN";
        private const string PersonaBarMenuPermissionCode = "PERSONABAR_MENU";

        #endregion

        #region Public Methods

        public static bool CanAdmin(int portalId, MenuItem menu)
        {
            return HasMenuPermission(GetMenuPermissions(portalId, menu.MenuId), PersonaBarMenuAdminPermissionKey);
        }

        public static bool CanEdit(int portalId, MenuItem menu)
        {
            return HasMenuPermission(GetMenuPermissions(portalId, menu.MenuId), PersonaBarMenuEditPermissionKey);
        }

        public static bool CanView(int portalId, MenuItem menu)
        {
            return HasMenuPermission(GetMenuPermissions(portalId, menu.MenuId), PersonaBarMenuViewPermissionKey);
        }

        public static void DeleteMenuPermissions(int portalId, MenuItem menu)
        {
            _dataService.DeletePersonbaBarPermissionsByMenuId(portalId, menu.MenuId);
            ClearCache(portalId);
        }

        public static MenuPermissionCollection GetMenuPermissions(int portalId)
        {
            var cacheKey = GetCacheKey(portalId);
            var permissions = DataCache.GetCache<MenuPermissionCollection>(cacheKey);
            if (permissions == null)
            {
                lock (_threadLocker)
                {
                    permissions = DataCache.GetCache<MenuPermissionCollection>(cacheKey);
                    if (permissions == null)
                    {
                        permissions = new MenuPermissionCollection();
                        EnsureMenuDefaultPermissions(portalId);
                        var reader = _dataService.GetPersonbaBarPermissionsByPortal(portalId);
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

        public static MenuPermissionCollection GetMenuPermissions(int portalId, int menuId)
        {
            var permissions = GetMenuPermissions(portalId)
                    .Cast<MenuPermissionInfo>()
                    .Where(p => p.MenuId == menuId && (p.PortalId == Null.NullInteger || p.PortalId == portalId)).ToList();
            return new MenuPermissionCollection(permissions);
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

            permissionInfo.MenuPermissionId = _dataService.SavePersonaBarPermission(permissionInfo.MenuPermissionId, portalId, menu.MenuId, permissionInfo.PermissionID,
                permissionInfo.RoleID, permissionInfo.UserID, permissionInfo.AllowAccess, user.UserID);

            ClearCache(portalId);
        }

        public static IList<PermissionInfo> GetPermissions()
        {
            var permissions = new List<PermissionInfo>();
            var permissionController = new PermissionController();
            permissions.AddRange(permissionController.GetPermissionByCodeAndKey(PersonaBarMenuPermissionCode, PersonaBarMenuViewPermissionKey).Cast<PermissionInfo>());
            permissions.AddRange(permissionController.GetPermissionByCodeAndKey(PersonaBarMenuPermissionCode, PersonaBarMenuEditPermissionKey).Cast<PermissionInfo>());
            permissions.AddRange(permissionController.GetPermissionByCodeAndKey(PersonaBarMenuPermissionCode, PersonaBarMenuAdminPermissionKey).Cast<PermissionInfo>());

            return permissions;
        }

        public static void SaveMenuDefaultPermissions(int portalId, MenuItem menuItem, string roleName)
        {
            SaveMenuDefaultPermissions(portalId, menuItem, roleName, false);
        }

        #endregion

        #region Private Methods

        private static void EnsureMenuDefaultPermissions(int portalId)
        {
            try
            {
                var permissionInitialized = PortalController.Instance.GetPortalSettings(portalId).ContainsKey(PermissionInitializedKey);
                if (!permissionInitialized)
                {
                    var menuItems = PersonaBarRepository.Instance.GetMenu().AllItems;
                    foreach (var menuItem in menuItems)
                    {
                        var defaultRoles = PersonaBarRepository.Instance.GetMenuDefaultRoles(menuItem.MenuId);
                        if (!string.IsNullOrEmpty(defaultRoles))
                        {
                            foreach (var roleName in defaultRoles.Split(','))
                            {
                                if (!string.IsNullOrEmpty(roleName.Trim()))
                                {
                                    SaveMenuDefaultPermissions(portalId, menuItem, roleName.Trim(), true);
                                }
                            }
                        }
                    }

                    PortalController.UpdatePortalSetting(portalId, PermissionInitializedKey, "Y");
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
                var permissions = GetPermissions();
                var role = RoleController.Instance.GetRoleByName(portalId, roleName);
                if (role != null && role.IsSystemRole && (ignoreExists || GetMenuPermissions(portalId, menuItem.MenuId).ToList().All(p => p.RoleID != role.RoleID)))
                {
                    foreach (var permission in permissions)
                    {
                        var menuPermissionInfo = new MenuPermissionInfo
                        {
                            MenuPermissionId = Null.NullInteger,
                            MenuId = menuItem.MenuId,
                            PermissionID = permission.PermissionID,
                            RoleID = role.RoleID,
                            UserID = Null.NullInteger,
                            AllowAccess = true
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
            var cacheKey = GetCacheKey(portalId);
            DataCache.RemoveCache(cacheKey);
        }

        private static string GetCacheKey(int portalId)
        {
            return string.Format(PersonaBarMenuPermissionsCacheKey, portalId);
        }
        #endregion
    }
}
