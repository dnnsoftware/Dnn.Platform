// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;

    [Serializable]
    public class FolderPermissionCollection : CollectionBase
    {
        public FolderPermissionCollection()
        {
        }

        public FolderPermissionCollection(ArrayList folderPermissions)
        {
            this.AddRange(folderPermissions);
        }

        public FolderPermissionCollection(FolderPermissionCollection folderPermissions)
        {
            this.AddRange(folderPermissions);
        }

        public FolderPermissionCollection(ArrayList folderPermissions, string FolderPath)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                if (permission.FolderPath.Equals(FolderPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Add(permission);
                }
            }
        }

        public FolderPermissionInfo this[int index]
        {
            get
            {
                return (FolderPermissionInfo)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public int Add(FolderPermissionInfo value)
        {
            return this.List.Add(value);
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
                foreach (PermissionInfoBase permission in this.List)
                {
                    if (permission.PermissionID == value.PermissionID && permission.UserID == value.UserID && permission.RoleID == value.RoleID)
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

        public int IndexOf(FolderPermissionInfo value)
        {
            return this.List.IndexOf(value);
        }

        public void Insert(int index, FolderPermissionInfo value)
        {
            this.List.Insert(index, value);
        }

        public void Remove(FolderPermissionInfo value)
        {
            this.List.Remove(value);
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
        }

        public bool Contains(FolderPermissionInfo value)
        {
            return this.List.Contains(value);
        }

        public bool Contains(string key, int folderId, int roleId, int userId)
        {
            bool result = Null.NullBoolean;
            foreach (FolderPermissionInfo permission in this.List)
            {
                if (permission.PermissionKey == key && permission.FolderID == folderId && permission.RoleID == roleId && permission.UserID == userId)
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
                if (objFolderPermissionCollection[i].FolderPermissionID != this[i].FolderPermissionID || objFolderPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public List<PermissionInfoBase> ToList()
        {
            var list = new List<PermissionInfoBase>();
            foreach (PermissionInfoBase permission in this.List)
            {
                list.Add(permission);
            }

            return list;
        }

        public string ToString(string key)
        {
            return PermissionController.BuildPermissions(this.List, key);
        }
    }
}
