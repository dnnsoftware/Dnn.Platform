// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Security.Permissions
{
    [Serializable]
    public class FolderPermissionCollection : CollectionBase
    {
        public FolderPermissionCollection()
        {
        }

        public FolderPermissionCollection(ArrayList folderPermissions)
        {
            AddRange(folderPermissions);
        }

        public FolderPermissionCollection(FolderPermissionCollection folderPermissions)
        {
            AddRange(folderPermissions);
        }

        public FolderPermissionCollection(ArrayList folderPermissions, string FolderPath)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                if (permission.FolderPath.Equals(FolderPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    Add(permission);
                }
            }
        }

        public FolderPermissionInfo this[int index]
        {
            get
            {
                return (FolderPermissionInfo) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(FolderPermissionInfo value)
        {
            return List.Add(value);
        }

        public int Add(FolderPermissionInfo value, bool checkForDuplicates)
        {
            int id = Null.NullInteger;
            if (!checkForDuplicates)
            {
                id = Add(value);
            }
            else
            {
                bool isMatch = false;
                foreach (PermissionInfoBase permission in List)
                {
                    if (permission.PermissionID == value.PermissionID && permission.UserID == value.UserID && permission.RoleID == value.RoleID)
                    {
                        isMatch = true;
                        break;
                    }
                }
                if (!isMatch)
                {
                    id = Add(value);
                }
            }

            return id;
        }

        public void AddRange(ArrayList folderPermissions)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                List.Add(permission);
            }
        }

        public void AddRange(FolderPermissionCollection folderPermissions)
        {
            foreach (FolderPermissionInfo permission in folderPermissions)
            {
                List.Add(permission);
            }
        }

        public int IndexOf(FolderPermissionInfo value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, FolderPermissionInfo value)
        {
            List.Insert(index, value);
        }

        public void Remove(FolderPermissionInfo value)
        {
            List.Remove(value);
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
            foreach (PermissionInfoBase permission in List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    List.Remove(permission);
                    break;
                }
            }
        }

        public bool Contains(FolderPermissionInfo value)
        {
            return List.Contains(value);
        }

        public bool Contains(string key, int folderId, int roleId, int userId)
        {
            bool result = Null.NullBoolean;
            foreach (FolderPermissionInfo permission in List)
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
            if (objFolderPermissionCollection.Count != Count)
            {
                return false;
            }
            InnerList.Sort(new CompareFolderPermissions());
            objFolderPermissionCollection.InnerList.Sort(new CompareFolderPermissions());
            for (int i = 0; i <= Count - 1; i++)
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
            foreach (PermissionInfoBase permission in List)
            {
                list.Add(permission);
            }
            return list;
        }

        public string ToString(string key)
        {
            return PermissionController.BuildPermissions(List, key);
        }
    }
}
