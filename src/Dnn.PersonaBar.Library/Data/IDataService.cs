// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2016, DNN Corp.
// All rights reserved.

using System.Data;

namespace Dnn.PersonaBar.Library.Data
{
    public interface IDataService
    {
        int SavePersonaBarMenu(string identifier, string moduleName, string controller, string resourceKey, string path,
            string link, string cssClass, bool mobileSupport, int parentId, int order, bool allowHost, bool enabled, int currentUserId);

        IDataReader GetPersonaBarMenu();

        void DeletePersonaBarMenuByIdentifier(string identifier);


        int SavePersonaBarExtension(int extensionId, string identifier, int menuId, string controller, string container, string path,
            int order, bool enabled, int currentUserId);

        void DeletePersonaBarExtension(string identifier);

        IDataReader GetPersonaBarExtensions();


        int SavePersonaBarMenuDefaultRoles(int menuId, string roleNames);

        string GetPersonaBarMenuDefaultRoles(int menuId);


        int SavePersonaBarPermission(int menuPermissionId, int portalId, int menuId, int permissionId,
            int roleId, int userId, bool allowAccees, int currentUserId);

        IDataReader GetPersonbaBarPermissionsByPortal(int portalId);

        void DeletePersonbaBarPermissionsByMenuId(int portalId, int menuId);

        void DeletePersonbaBarPermissionsById(int menuPermissionId);
    }
}
