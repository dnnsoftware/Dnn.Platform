// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;

    // -----------------------------------------------------------------------------
    [Serializable]
    public class MenuPermissionCollection : CollectionBase
    {
        public MenuPermissionCollection()
        {
        }

        public MenuPermissionCollection(IList<MenuPermissionInfo> menuPermissions)
        {
            this.AddRange(menuPermissions);
        }

        public MenuPermissionCollection(MenuPermissionCollection permissions)
        {
            this.AddRange(permissions);
        }

        public MenuPermissionCollection(ArrayList menuPermissions, int menuId)
        {
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                if (permission.MenuId == menuId)
                {
                    this.Add(permission);
                }
            }
        }

        public MenuPermissionInfo this[int index]
        {
            get
            {
                return (MenuPermissionInfo)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public int Add(MenuPermissionInfo value)
        {
            return this.List.Add(value);
        }

        public int Add(MenuPermissionInfo value, bool checkForDuplicates)
        {
            int id = Null.NullInteger;
            if (!checkForDuplicates)
            {
                id = this.Add(value);
            }
            else
            {
                bool isMatch = false;
                foreach (MenuPermissionInfo permission in this.List)
                {
                    if (permission.PortalId == value.PortalId
                            && permission.MenuId == value.MenuId
                            && permission.PermissionID == value.PermissionID
                            && permission.UserID == value.UserID
                            && permission.RoleID == value.RoleID)
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

        public void AddRange(ArrayList menuPermissions)
        {
            this.AddRange(menuPermissions.Cast<MenuPermissionInfo>().ToList());
        }

        public void AddRange(IList<MenuPermissionInfo> menuPermissions)
        {
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(MenuPermissionCollection menuPermissions)
        {
            foreach (MenuPermissionInfo permission in menuPermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(MenuPermissionCollection menuPermissionCollection)
        {
            if (menuPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareMenuPermissions());
            menuPermissionCollection.InnerList.Sort(new CompareMenuPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (menuPermissionCollection[i].MenuPermissionId != this[i].MenuPermissionId
                    || menuPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Contains(MenuPermissionInfo value)
        {
            return this.List.Contains(value);
        }

        public int IndexOf(MenuPermissionInfo value)
        {
            return this.List.IndexOf(value);
        }

        public void Insert(int index, MenuPermissionInfo value)
        {
            this.List.Insert(index, value);
        }

        public void Remove(MenuPermissionInfo value)
        {
            this.List.Remove(value);
        }

        public void Remove(int permissionId, int roleId, int userId)
        {
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionId && permission.UserID == userId && permission.RoleID == roleId)
                {
                    this.List.Remove(permission);
                    break;
                }
            }
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

        public IEnumerable<MenuPermissionInfo> Where(Func<MenuPermissionInfo, bool> predicate)
        {
            return this.Cast<MenuPermissionInfo>().Where(predicate);
        }
    }
}
