#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Security.Permissions
{
    public class FolderPermissionController
    {
        private static readonly PermissionProvider provider = PermissionProvider.Instance();

        private static void ClearPermissionCache(int PortalID)
        {
            DataCache.ClearFolderPermissionsCache(PortalID);
            DataCache.ClearCache(string.Format("Folders|{0}|", PortalID));
            DataCache.ClearFolderCache(PortalID);
        }

        public static bool CanAddFolder(FolderInfo folder)
        {
            return provider.CanAddFolder(folder) || CanAdminFolder(folder);
        }

        public static bool CanAdminFolder(FolderInfo folder)
        {
            return provider.CanAdminFolder(folder);
        }

        public static bool CanCopyFolder(FolderInfo folder)
        {
            return provider.CanCopyFolder(folder) || CanAdminFolder(folder);
        }

        public static bool CanDeleteFolder(FolderInfo folder)
        {
            return provider.CanDeleteFolder(folder) || CanAdminFolder(folder);
        }

        public static bool CanManageFolder(FolderInfo folder)
        {
            return provider.CanManageFolder(folder) || CanAdminFolder(folder);
        }

        public static bool CanViewFolder(FolderInfo folder)
        {
            return provider.CanViewFolder(folder) || CanAdminFolder(folder);
        }

        public static void DeleteFolderPermissionsByUser(UserInfo objUser)
        {
            provider.DeleteFolderPermissionsByUser(objUser);
            ClearPermissionCache(objUser.PortalID);
        }

        public static FolderPermissionCollection GetFolderPermissionsCollectionByFolder(int PortalID, string Folder)
        {
            return provider.GetFolderPermissionsCollectionByFolder(PortalID, Folder);
        }

        public static bool HasFolderPermission(int portalId, string folderPath, string permissionKey)
        {
            return HasFolderPermission(GetFolderPermissionsCollectionByFolder(portalId, folderPath), permissionKey);
        }

        public static bool HasFolderPermission(FolderPermissionCollection objFolderPermissions, string PermissionKey)
        {
            bool hasPermission = provider.HasFolderPermission(objFolderPermissions, "WRITE");
            if (!hasPermission)
            {
                if (PermissionKey.Contains(","))
                {
                    foreach (string permission in PermissionKey.Split(','))
                    {
                        if (provider.HasFolderPermission(objFolderPermissions, permission))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }
                else
                {
                    hasPermission = provider.HasFolderPermission(objFolderPermissions, PermissionKey);
                }
            }
            return hasPermission;
        }

        /// <summary>
        /// Copies the permissions to subfolders.
        /// </summary>
        /// <param name="folder">The parent folder.</param>
        /// <param name="newPermissions">The new permissions.</param>
        public static void CopyPermissionsToSubfolders(IFolderInfo folder, FolderPermissionCollection newPermissions)
        {
            bool clearCache = CopyPermissionsToSubfoldersRecursive(folder, newPermissions);
            if (clearCache)
            {
                DataCache.ClearFolderCache(folder.PortalID);
            }
        }

        private static bool CopyPermissionsToSubfoldersRecursive(IFolderInfo folder, FolderPermissionCollection newPermissions)
        {
            bool clearCache = Null.NullBoolean;
            IEnumerable<IFolderInfo> childFolders = FolderManager.Instance.GetFolders(folder);
            foreach (FolderInfo f in childFolders)
            {
                if (CanAdminFolder(f))
                {
                    f.FolderPermissions.Clear();
                    f.FolderPermissions.AddRange(newPermissions);
                    SaveFolderPermissions(f);
                    clearCache = true;
                }
                clearCache = CopyPermissionsToSubfoldersRecursive(f, newPermissions) || clearCache;
            }
            return clearCache;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// SaveFolderPermissions updates a Folder's permissions
        /// </summary>
        /// <param name="folder">The Folder to update</param>
        /// <history>
        /// 	[cnurse]	04/15/2009   Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SaveFolderPermissions(FolderInfo folder)
        {
            SaveFolderPermissions((IFolderInfo)folder);
        }

        /// <summary>
        /// SaveFolderPermissions updates a Folder's permissions
        /// </summary>
        /// <param name="folder">The Folder to update</param>
        public static void SaveFolderPermissions(IFolderInfo folder)
        {
            provider.SaveFolderPermissions(folder);
            ClearPermissionCache(folder.PortalID);
        }

        #region "Obsolete Methods"

        [Obsolete("Deprecated in DNN 5.1.")]
        public int AddFolderPermission(FolderPermissionInfo objFolderPermission)
        {
            ClearPermissionCache(objFolderPermission.PortalID);
            return DataProvider.Instance().AddFolderPermission(objFolderPermission.FolderID,
                                                               objFolderPermission.PermissionID,
                                                               objFolderPermission.RoleID,
                                                               objFolderPermission.AllowAccess,
                                                               objFolderPermission.UserID,
                                                               UserController.GetCurrentUserInfo().UserID);
        }

        [Obsolete("Deprecated in DNN 5.1.")]
        public void DeleteFolderPermission(int FolderPermissionID)
        {
            DataProvider.Instance().DeleteFolderPermission(FolderPermissionID);
        }

        [Obsolete("Deprecated in DNN 5.1.")]
        public void DeleteFolderPermissionsByFolder(int PortalID, string FolderPath)
        {
            DataProvider.Instance().DeleteFolderPermissionsByFolderPath(PortalID, FolderPath);
            ClearPermissionCache(PortalID);
        }

        [Obsolete("Deprecated in DNN 5.0. Use DeleteFolderPermissionsByUser(UserInfo) ")]
        public void DeleteFolderPermissionsByUserID(UserInfo objUser)
        {
            DataProvider.Instance().DeleteFolderPermissionsByUserID(objUser.PortalID, objUser.UserID);
            ClearPermissionCache(objUser.PortalID);
        }

        [Obsolete("Deprecated in DNN 5.1.")]
        public FolderPermissionInfo GetFolderPermission(int FolderPermissionID)
        {
            return CBO.FillObject<FolderPermissionInfo>(DataProvider.Instance().GetFolderPermission(FolderPermissionID), true);
        }

        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolderPath(PortalId, Folder)")]
        public ArrayList GetFolderPermissionsByFolder(int PortalID, string Folder)
        {
            return CBO.FillCollection(DataProvider.Instance().GetFolderPermissionsByFolderPath(PortalID, Folder, -1), typeof(FolderPermissionInfo));
        }

        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolderPath(PortalId, Folder)")]
        public FolderPermissionCollection GetFolderPermissionsByFolder(ArrayList arrFolderPermissions, string FolderPath)
        {
            return new FolderPermissionCollection(arrFolderPermissions, FolderPath);
        }

        [Obsolete("Deprecated in DNN 5.1. GetModulePermissions(ModulePermissionCollection, String) ")]
        public string GetFolderPermissionsByFolderPath(ArrayList arrFolderPermissions, string FolderPath, string PermissionKey)
        {
            //Create a Folder Permission Collection from the ArrayList
            var folderPermissions = new FolderPermissionCollection(arrFolderPermissions, FolderPath);

            //Return the permission string for permissions with specified FolderPath
            return folderPermissions.ToString(PermissionKey);
        }

        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolder(PortalId, Folder)")]
        public FolderPermissionCollection GetFolderPermissionsCollectionByFolderPath(int PortalID, string Folder)
        {
            return GetFolderPermissionsCollectionByFolder(PortalID, Folder);
        }

        [Obsolete("Deprecated in DNN 5.0. Please use GetFolderPermissionsCollectionByFolder(PortalId, Folder)")]
        public FolderPermissionCollection GetFolderPermissionsCollectionByFolderPath(ArrayList arrFolderPermissions, string FolderPath)
        {
            var objFolderPermissionCollection = new FolderPermissionCollection(arrFolderPermissions, FolderPath);
            return objFolderPermissionCollection;
        }

        [Obsolete("Deprecated in DNN 5.1.")]
        public void UpdateFolderPermission(FolderPermissionInfo objFolderPermission)
        {
            DataProvider.Instance().UpdateFolderPermission(objFolderPermission.FolderPermissionID,
                                                           objFolderPermission.FolderID,
                                                           objFolderPermission.PermissionID,
                                                           objFolderPermission.RoleID,
                                                           objFolderPermission.AllowAccess,
                                                           objFolderPermission.UserID,
                                                           UserController.GetCurrentUserInfo().UserID);
            ClearPermissionCache(objFolderPermission.PortalID);
        }

        #endregion
    }
}
