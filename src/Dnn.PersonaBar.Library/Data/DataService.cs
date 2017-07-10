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

using System;
using System.Data;
using DotNetNuke.Common.Utilities;

namespace Dnn.PersonaBar.Library.Data
{
    public class DataService : IDataService
    {
        protected static readonly DotNetNuke.Data.DataProvider DataProvider = DotNetNuke.Data.DataProvider.Instance();

        public int SavePersonaBarMenu(string identifier, string moduleName, string folderName, string controller, string resourceKey, string path,
            string link, string cssClass, string iconFile, int parentId, int order, bool allowHost, bool enabled, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarMenu", 
                identifier, moduleName, folderName, controller, resourceKey, path,
                Null.GetNull(link, DBNull.Value), Null.GetNull(cssClass, DBNull.Value),
                Null.GetNull(parentId, DBNull.Value), order, allowHost, enabled, currentUserId, Null.GetNull(iconFile, DBNull.Value));
        }

        public IDataReader GetPersonaBarMenu()
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarMenu");
        }

        public void DeletePersonaBarMenuByIdentifier(string identifier)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuByIdentifier", identifier);
        }

        public int SavePersonaBarExtension(string identifier, int menuId, string folderName, string controller, string container,
            string path, int order, bool enabled, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarExtension", identifier, menuId, folderName,
                controller, container, path, order, enabled, currentUserId);
        }

        public void DeletePersonaBarExtension(string identifier)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarExtension", identifier);
        }

        public IDataReader GetPersonaBarExtensions()
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarExtensions");
        }

        public int SavePersonaBarMenuDefaultPermissions(int menuId, string roleNames)
        {
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarMenuDefaultPermissions", menuId, roleNames);
        }

        public string GetPersonaBarMenuDefaultPermissions(int menuId)
        {
            return DataProvider.ExecuteScalar<string>("PersonaBar_GetPersonaBarMenuDefaultPermissions", menuId);
        }

        public int SavePersonaBarMenuPermission(int portalId, int menuId, int permissionId, int roleId,
            int userId, bool allowAccees, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarMenuPermission", DataProvider.GetNull(portalId), menuId, permissionId,
                GetRoleNull(roleId), DataProvider.GetNull(userId), allowAccees, currentUserId);
        }

        public IDataReader GetPersonbaBarMenuPermissionsByPortal(int portalId)
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarMenuPermissionsByPortal", portalId);
        }

        public void DeletePersonbaBarMenuPermissionsByMenuId(int portalId, int menuId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuPermissionsByMenuId", portalId, menuId);
        }

        public void DeletePersonbaBarMenuPermissionsById(int menuPermissionId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarMenuPermissionById", menuPermissionId);
        }

        public int SavePersonaBarPermission(int menuId, string permissionKey, string permissionName, int currentUserId)
        {
            return DataProvider.ExecuteScalar<int>("PersonaBar_SavePersonaBarPermission", 
                Null.GetNull(menuId, DBNull.Value), permissionKey, permissionName, currentUserId);
        }

        public void DeletePersonaBarPermission(int permissionId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_DeletePersonaBarPermission", permissionId);
        }

        public IDataReader GetPersonaBarPermissions()
        {
            return DataProvider.ExecuteReader("PersonaBar_GetPersonaBarPermissions");
        }

        public void UpdateMenuController(string identifier, string controller, int userId)
        {
            DataProvider.ExecuteNonQuery("PersonaBar_UpdateMenuController", identifier, controller, userId);
        }

        private object GetRoleNull(int roleId)
        {
            if (roleId.ToString() == "-4")
                return DBNull.Value;
            return (object)roleId;
        }
    }
}