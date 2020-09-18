// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : TabPermissionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// TabPermissionCollection provides the a custom collection for TabPermissionInfo
    /// objects.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    [XmlRoot("tabpermissions")]
    public class TabPermissionCollection : CollectionBase
    {
        public TabPermissionCollection()
        {
        }

        public TabPermissionCollection(ArrayList tabPermissions)
        {
            this.AddRange(tabPermissions);
        }

        public TabPermissionCollection(TabPermissionCollection tabPermissions)
        {
            this.AddRange(tabPermissions);
        }

        public TabPermissionCollection(ArrayList tabPermissions, int TabId)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                if (permission.TabID == TabId)
                {
                    this.Add(permission);
                }
            }
        }

        public TabPermissionInfo this[int index]
        {
            get
            {
                return (TabPermissionInfo)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public int Add(TabPermissionInfo value)
        {
            return this.List.Add(value);
        }

        public int Add(TabPermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(IEnumerable<TabPermissionInfo> tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(TabPermissionCollection tabPermissions)
        {
            foreach (TabPermissionInfo permission in tabPermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(TabPermissionCollection objTabPermissionCollection)
        {
            if (objTabPermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareTabPermissions());
            objTabPermissionCollection.InnerList.Sort(new CompareTabPermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objTabPermissionCollection[i].TabPermissionID != this[i].TabPermissionID
                        || objTabPermissionCollection[i].PermissionID != this[i].PermissionID
                        || objTabPermissionCollection[i].RoleID != this[i].RoleID
                        || objTabPermissionCollection[i].UserID != this[i].UserID
                        || objTabPermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Contains(TabPermissionInfo value)
        {
            return this.List.Contains(value);
        }

        public int IndexOf(TabPermissionInfo value)
        {
            return this.List.IndexOf(value);
        }

        public void Insert(int index, TabPermissionInfo value)
        {
            this.List.Insert(index, value);
        }

        public void Remove(TabPermissionInfo value)
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

        public IEnumerable<TabPermissionInfo> Where(Func<TabPermissionInfo, bool> predicate)
        {
            return this.Cast<TabPermissionInfo>().Where(predicate);
        }
    }
}
