// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;

    [Serializable]
    public class FolderPermissionCollection : GenericCollectionBase<FolderPermissionInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        public FolderPermissionCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        /// <param name="folderPermissions">An <see cref="ArrayList"/> of <see cref="FolderPermissionInfo"/> instances.</param>
        public FolderPermissionCollection(ArrayList folderPermissions)
        {
            this.AddRange(folderPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        /// <param name="folderPermissions">A collection of <see cref="FolderPermissionInfo"/> instances.</param>
        public FolderPermissionCollection(FolderPermissionCollection folderPermissions)
        {
            this.AddRange(folderPermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionCollection"/> class.</summary>
        /// <param name="folderPermissions">An <see cref="ArrayList"/> of <see cref="FolderPermissionInfo"/> instances.</param>
        /// <param name="folderPath">The path of the folder by which to filter <paramref name="folderPermissions"/>.</param>
        public FolderPermissionCollection(ArrayList folderPermissions, string folderPath)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                if (permission.FolderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase))
                {
                    this.Add(permission);
                }
            }
        }

        public int Add(FolderPermissionInfo value, bool checkForDuplicates)
        {
            int id = Null.NullInteger;
            if (!checkForDuplicates)
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
                foreach (IPermissionInfo permission in this.List)
                {
                    if (permission.PermissionId == value.PermissionID && permission.UserId == value.UserID && permission.RoleId == value.RoleID)
                    {
                        isMatch = true;
                        break;
                    }
                }

                if (!isMatch)
                {
                    id = this.Add(value);
                }
            }

            return id;
        }

        public void AddRange(IEnumerable<FolderPermissionInfo> folderPermissions)
        {
            foreach (var permission in folderPermissions)
            {
                this.List.Add(permission);
            }
        }

        public void AddRange(ArrayList folderPermissions)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                this.List.Add(permission);
            }
        }

        public void AddRange(FolderPermissionCollection folderPermissions)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                this.List.Add(permission);
            }
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
            foreach (IPermissionInfo permission in this.List)
            {
                if (permission.PermissionId == permissionID && permission.UserId == userID && permission.RoleId == roleID)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
        }

        public bool Contains(string key, int folderId, int roleId, int userId)
        {
            bool result = Null.NullBoolean;
            foreach (IFolderPermissionInfo permission in this.List)
            {
                if (permission.PermissionKey == key && permission.FolderId == folderId && permission.RoleId == roleId && permission.UserId == userId)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public bool CompareTo(FolderPermissionCollection objFolderPermissionCollection)
        {
            if (objFolderPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareFolderPermissions());
            objFolderPermissionCollection.InnerList.Sort(new CompareFolderPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                IFolderPermissionInfo otherPermission = objFolderPermissionCollection[i];
                IFolderPermissionInfo thisPermission = this[i];
                if (otherPermission.FolderPermissionId != thisPermission.FolderPermissionId || otherPermission.AllowAccess != thisPermission.AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public List<PermissionInfoBase> ToList()
        {
            return [..this.List.Cast<PermissionInfoBase>()];
        }

        public string ToString(string key)
        {
            return PermissionController.BuildPermissions(this.List, key);
        }
    }
}
