#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : ModulePermissionController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModulePermissionController provides the Business Layer for Module Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ModulePermissionController
    {
        #region Private Members

        private static readonly PermissionProvider _provider = PermissionProvider.Instance();

        #endregion

        #region Private Methods

        private static void ClearPermissionCache(int moduleId)
        {
            ModuleInfo objModule = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
            DataCache.ClearModulePermissionsCache(objModule.TabID);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a list with all roles with implicit permissions on Modules
        /// </summary>
        /// <param name="portalId">The Portal Id where the Roles are</param>
        /// <returns>A List with the implicit roles</returns>
        public static IEnumerable<RoleInfo> ImplicitRoles(int portalId)
        {
            return _provider.ImplicitRolesForPages(portalId);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can administer a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanAdminModule(ModuleInfo module)
        {
            return _provider.CanAdminModule(module);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can delete a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanDeleteModule(ModuleInfo module)
        {
            return _provider.CanDeleteModule(module);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can edit module content
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanEditModuleContent(ModuleInfo module)
        {
            return _provider.CanEditModuleContent(module);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can export a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanExportModule(ModuleInfo module)
        {
            return _provider.CanExportModule(module);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can import a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanImportModule(ModuleInfo module)
        {
            return _provider.CanImportModule(module);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can manage a module's settings
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanManageModule(ModuleInfo module)
        {
            return _provider.CanManageModule(module);
        }

        /// <summary>
        /// Returns a flag indicating whether the current user can view a module
        /// </summary>
        /// <param name="module">The page</param>
        /// <returns>A flag indicating whether the user has permission</returns>
        public static bool CanViewModule(ModuleInfo module)
        {
            return _provider.CanViewModule(module);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteModulePermissionsByUser deletes a user's Module Permission in the Database
        /// </summary>
        /// <param name="user">The user</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteModulePermissionsByUser(UserInfo user)
        {
            _provider.DeleteModulePermissionsByUser(user);
            DataCache.ClearModulePermissionsCachesByPortal(user.PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetModulePermissions gets a ModulePermissionCollection
        /// </summary>
        /// <param name="moduleId">The ID of the module</param>
        /// <param name="tabId">The ID of the tab</param>
        /// -----------------------------------------------------------------------------
        public static ModulePermissionCollection GetModulePermissions(int moduleId, int tabId)
        {
            return _provider.GetModulePermissions(moduleId, tabId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// HasModulePermission checks whether the current user has a specific Module Permission
        /// </summary>
        /// <remarks>If you pass in a comma delimited list of permissions (eg "ADD,DELETE", this will return
        /// true if the user has any one of the permissions.</remarks>
        /// <param name="modulePermissions">The Permissions for the Module</param>
        /// <param name="permissionKey">The Permission to check</param>
        /// -----------------------------------------------------------------------------
        public static bool HasModulePermission(ModulePermissionCollection modulePermissions, string permissionKey)
        {
            return _provider.HasModulePermission(modulePermissions, permissionKey);
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Determines if user has the necessary permissions to access an item with the
        /// designated AccessLevel.
        /// </summary>
        /// <param name="accessLevel">The SecurityAccessLevel required to access a portal module or module action.</param>
        /// <param name="permissionKey">If Security Access is Edit the permissionKey is the actual "edit" permisison required.</param>
        /// <param name="moduleConfiguration">The ModuleInfo object for the associated module.</param>
        /// <returns>A boolean value indicating if the user has the necessary permissions</returns>
        /// <remarks>Every module control and module action has an associated permission level.  This
        /// function determines whether the user represented by UserName has sufficient permissions, as
        /// determined by the PortalSettings and ModuleSettings, to access a resource with the
        /// designated AccessLevel.</remarks>
        ///-----------------------------------------------------------------------------
        public static bool HasModuleAccess(SecurityAccessLevel accessLevel, string permissionKey, ModuleInfo moduleConfiguration)
        {
            return _provider.HasModuleAccess(accessLevel, permissionKey, moduleConfiguration);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveModulePermissions updates a Module's permissions
        /// </summary>
        /// <param name="module">The Module to update</param>
        /// -----------------------------------------------------------------------------
        public static void SaveModulePermissions(ModuleInfo module)
        {
            _provider.SaveModulePermissions(module);
            DataCache.ClearModulePermissionsCache(module.TabID);
        }

        #endregion
    }
}
