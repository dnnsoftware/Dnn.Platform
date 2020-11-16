// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Data
{
    using System.Data;

    public interface IDataService
    {
        int SavePersonaBarMenu(string identifier, string moduleName, string folderName, string controller, string resourceKey, string path,
            string link, string cssClass, string iconFile, int parentId, int order, bool allowHost, bool enabled, int currentUserId);

        IDataReader GetPersonaBarMenu();

        void DeletePersonaBarMenuByIdentifier(string identifier);

        int SavePersonaBarExtension(string identifier, int menuId, string folderName, string controller, string container, string path,
            int order, bool enabled, int currentUserId);

        void DeletePersonaBarExtension(string identifier);

        IDataReader GetPersonaBarExtensions();

        int SavePersonaBarMenuDefaultPermissions(int menuId, string roleNames);

        string GetPersonaBarMenuDefaultPermissions(int menuId);

        int SavePersonaBarMenuPermission(int portalId, int menuId, int permissionId,
            int roleId, int userId, bool allowAccees, int currentUserId);

        IDataReader GetPersonbaBarMenuPermissionsByPortal(int portalId);

        void DeletePersonbaBarMenuPermissionsByMenuId(int portalId, int menuId);

        void DeletePersonbaBarMenuPermissionsById(int menuPermissionId);

        int SavePersonaBarPermission(int menuId, string permissionKey, string permissionName, int currentUserId);

        void DeletePersonaBarPermission(int permissionId);

        IDataReader GetPersonaBarPermissions();

        void UpdateMenuController(string identifier, string controller, int userId);
    }
}
