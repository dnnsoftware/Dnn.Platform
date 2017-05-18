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
using System.ComponentModel;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Security.Permissions
{
    public partial class FolderPermissionController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.")]
        public int AddFolderPermission(FolderPermissionInfo objFolderPermission)
        {
            ClearPermissionCache(objFolderPermission.PortalID);
            return DataProvider.Instance().AddFolderPermission(objFolderPermission.FolderID,
                                                               objFolderPermission.PermissionID,
                                                               objFolderPermission.RoleID,
                                                               objFolderPermission.AllowAccess,
                                                               objFolderPermission.UserID,
                                                               UserController.Instance.GetCurrentUserInfo().UserID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.")]
        public void DeleteFolderPermission(int FolderPermissionID)
        {
            DataProvider.Instance().DeleteFolderPermission(FolderPermissionID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.")]
        public void DeleteFolderPermissionsByFolder(int PortalID, string FolderPath)
        {
            DataProvider.Instance().DeleteFolderPermissionsByFolderPath(PortalID, FolderPath);
            ClearPermissionCache(PortalID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0. Use DeleteFolderPermissionsByUser(UserInfo) ")]
        public void DeleteFolderPermissionsByUserID(UserInfo objUser)
        {
            DataProvider.Instance().DeleteFolderPermissionsByUserID(objUser.PortalID, objUser.UserID);
            ClearPermissionCache(objUser.PortalID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.")]
        public FolderPermissionInfo GetFolderPermission(int FolderPermissionID)
        {
            return CBO.FillObject<FolderPermissionInfo>(DataProvider.Instance().GetFolderPermission(FolderPermissionID));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolderPath(PortalId, Folder)")]
        public ArrayList GetFolderPermissionsByFolder(int PortalID, string Folder)
        {
            return CBO.FillCollection(DataProvider.Instance().GetFolderPermissionsByFolderPath(PortalID, Folder, -1), typeof(FolderPermissionInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolderPath(PortalId, Folder)")]
        public FolderPermissionCollection GetFolderPermissionsByFolder(ArrayList arrFolderPermissions, string FolderPath)
        {
            return new FolderPermissionCollection(arrFolderPermissions, FolderPath);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. GetModulePermissions(ModulePermissionCollection, String) ")]
        public string GetFolderPermissionsByFolderPath(ArrayList arrFolderPermissions, string FolderPath, string PermissionKey)
        {
            //Create a Folder Permission Collection from the ArrayList
            var folderPermissions = new FolderPermissionCollection(arrFolderPermissions, FolderPath);

            //Return the permission string for permissions with specified FolderPath
            return folderPermissions.ToString(PermissionKey);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolder(PortalId, Folder)")]
        public FolderPermissionCollection GetFolderPermissionsCollectionByFolderPath(int PortalID, string Folder)
        {
            return GetFolderPermissionsCollectionByFolder(PortalID, Folder);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolder(PortalId, Folder)")]
        public FolderPermissionCollection GetFolderPermissionsCollectionByFolderPath(ArrayList arrFolderPermissions, string FolderPath)
        {
            var objFolderPermissionCollection = new FolderPermissionCollection(arrFolderPermissions, FolderPath);
            return objFolderPermissionCollection;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.")]
        public void UpdateFolderPermission(FolderPermissionInfo objFolderPermission)
        {
            DataProvider.Instance().UpdateFolderPermission(objFolderPermission.FolderPermissionID,
                                                           objFolderPermission.FolderID,
                                                           objFolderPermission.PermissionID,
                                                           objFolderPermission.RoleID,
                                                           objFolderPermission.AllowAccess,
                                                           objFolderPermission.UserID,
                                                           UserController.Instance.GetCurrentUserInfo().UserID);
            ClearPermissionCache(objFolderPermission.PortalID);
        }
    }
}
