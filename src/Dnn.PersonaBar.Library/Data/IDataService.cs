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

using System.Data;

namespace Dnn.PersonaBar.Library.Data
{
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
