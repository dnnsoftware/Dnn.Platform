// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Data
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;

    public class DataService : IDataService
    {
        protected static readonly DotNetNuke.Data.DataProvider DataProvider = DotNetNuke.Data.DataProvider.Instance();

        /// <inheritdoc/>
        public int SavePersonaBarMenu(string identifier, string moduleName, string folderName, string controller, string resourceKey, string path, string link, string cssClass, string iconFile, int parentId, int order, bool allowHost, bool enabled, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarMenu",
                identifier,
                moduleName,
                folderName,
                controller,
                resourceKey,
                path,
                Null.GetNull(link, DBNull.Value),
                Null.GetNull(cssClass, DBNull.Value),
                Null.GetNull(parentId, DBNull.Value),
                order,
                allowHost,
                enabled,
                currentUserId,
                Null.GetNull(iconFile, DBNull.Value));
        }

        /// <inheritdoc/>
        public IDataReader GetPersonaBarMenu()
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarMenu");
        }

        /// <inheritdoc/>
        public void DeletePersonaBarMenuByIdentifier(string identifier)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuByIdentifier", identifier);
        }

        /// <inheritdoc/>
        public int SavePersonaBarExtension(string identifier, int menuId, string folderName, string controller, string container, string path, int order, bool enabled, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarExtension",
                identifier,
                menuId,
                folderName,
                controller,
                container,
                path,
                order,
                enabled,
                currentUserId);
        }

        /// <inheritdoc/>
        public void DeletePersonaBarExtension(string identifier)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarExtension", identifier);
        }

        /// <inheritdoc/>
        public IDataReader GetPersonaBarExtensions()
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarExtensions");
        }

        /// <inheritdoc/>
        public int SavePersonaBarMenuDefaultPermissions(int menuId, string roleNames)
        {
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarMenuDefaultPermissions", menuId, roleNames);
        }

        /// <inheritdoc/>
        public string GetPersonaBarMenuDefaultPermissions(int menuId)
        {
            return DataProvider.ExecuteScalar<string>("PersonaBar_GetPersonaBarMenuDefaultPermissions", menuId);
        }

        /// <inheritdoc/>
        public int SavePersonaBarMenuPermission(int portalId, int menuId, int permissionId, int roleId, int userId, bool allowAccees, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarMenuPermission",
                DataProvider.GetNull(portalId),
                menuId,
                permissionId,
                this.GetRoleNull(roleId),
                DataProvider.GetNull(userId),
                allowAccees,
                currentUserId);
        }

        /// <inheritdoc/>
        public IDataReader GetPersonbaBarMenuPermissionsByPortal(int portalId)
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarMenuPermissionsByPortal", portalId);
        }

        /// <inheritdoc/>
        public void DeletePersonbaBarMenuPermissionsByMenuId(int portalId, int menuId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuPermissionsByMenuId", portalId, menuId);
        }

        /// <inheritdoc/>
        public void DeletePersonbaBarMenuPermissionsById(int menuPermissionId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuPermissionById", menuPermissionId);
        }

        /// <inheritdoc/>
        public int SavePersonaBarPermission(int menuId, string permissionKey, string permissionName, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>(
                "PersonaBar_SavePersonaBarPermission",
                Null.GetNull(menuId, DBNull.Value),
                permissionKey,
                permissionName,
                currentUserId);
        }

        /// <inheritdoc/>
        public void DeletePersonaBarPermission(int permissionId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarPermission", permissionId);
        }

        /// <inheritdoc/>
        public IDataReader GetPersonaBarPermissions()
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarPermissions");
        }

        /// <inheritdoc/>
        public void UpdateMenuController(string identifier, string controller, int userId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_UpdateMenuController", identifier, controller, userId);
        }

        private object GetRoleNull(int roleId)
        {
            if (roleId.ToString() == "-4")
            {
                return DBNull.Value;
            }

            return (object)roleId;
        }
    }
}
