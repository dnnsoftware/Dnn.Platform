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
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : DesktopModulePermissionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DesktopModulePermissionCollection provides the a custom collection for DesktopModulePermissionInfo
    /// objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class DesktopModulePermissionCollection : CollectionBase
    {
        public DesktopModulePermissionCollection()
        {
        }

        public DesktopModulePermissionCollection(ArrayList DesktopModulePermissions)
        {
            AddRange(DesktopModulePermissions);
        }

        public DesktopModulePermissionCollection(DesktopModulePermissionCollection DesktopModulePermissions)
        {
            AddRange(DesktopModulePermissions);
        }

        public DesktopModulePermissionCollection(ArrayList DesktopModulePermissions, int DesktopModulePermissionID)
        {
            foreach (DesktopModulePermissionInfo permission in DesktopModulePermissions)
            {
                if (permission.DesktopModulePermissionID == DesktopModulePermissionID)
                {
                    Add(permission);
                }
            }
        }

        public DesktopModulePermissionInfo this[int index]
        {
            get
            {
                return (DesktopModulePermissionInfo) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(DesktopModulePermissionInfo value)
        {
            return List.Add(value);
        }

        public int Add(DesktopModulePermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList DesktopModulePermissions)
        {
            foreach (DesktopModulePermissionInfo permission in DesktopModulePermissions)
            {
                Add(permission);
            }
        }

        public void AddRange(DesktopModulePermissionCollection DesktopModulePermissions)
        {
            foreach (DesktopModulePermissionInfo permission in DesktopModulePermissions)
            {
                Add(permission);
            }
        }

        public bool CompareTo(DesktopModulePermissionCollection objDesktopModulePermissionCollection)
        {
            if (objDesktopModulePermissionCollection.Count != Count)
            {
                return false;
            }
            InnerList.Sort(new CompareDesktopModulePermissions());
            objDesktopModulePermissionCollection.InnerList.Sort(new CompareDesktopModulePermissions());
            for (int i = 0; i <= Count - 1; i++)
            {
                if (objDesktopModulePermissionCollection[i].DesktopModulePermissionID != this[i].DesktopModulePermissionID || objDesktopModulePermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Contains(DesktopModulePermissionInfo value)
        {
            return List.Contains(value);
        }

        public int IndexOf(DesktopModulePermissionInfo value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, DesktopModulePermissionInfo value)
        {
            List.Insert(index, value);
        }

        public void Remove(DesktopModulePermissionInfo value)
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
