#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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