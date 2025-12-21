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

    /// <summary>DesktopModulePermissionCollection provides a custom collection for <see cref="DesktopModulePermissionInfo"/> objects.</summary>
    [Serializable]
    public class DesktopModulePermissionCollection : GenericCollectionBase<DesktopModulePermissionInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
        public DesktopModulePermissionCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
        /// <param name="desktopModulePermissions">An <see cref="ArrayList"/> of <see cref="DesktopModulePermissionInfo"/> instances.</param>
        public DesktopModulePermissionCollection(ArrayList desktopModulePermissions)
        {
            this.AddRange(desktopModulePermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
        /// <param name="desktopModulePermissions">A collection of <see cref="DesktopModulePermissionInfo"/> instances.</param>
        public DesktopModulePermissionCollection(DesktopModulePermissionCollection desktopModulePermissions)
        {
            this.AddRange(desktopModulePermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionCollection"/> class.</summary>
        /// <param name="desktopModulePermissions">An <see cref="ArrayList"/> of <see cref="DesktopModulePermissionInfo"/> instances.</param>
        /// <param name="desktopModulePermissionID">The ID of the desktop module by which to filter <paramref name="desktopModulePermissions"/>.</param>
        public DesktopModulePermissionCollection(ArrayList desktopModulePermissions, int desktopModulePermissionID)
        {
            foreach (DesktopModulePermissionInfo permission in desktopModulePermissions)
            {
                if (permission.DesktopModulePermissionID == desktopModulePermissionID)
                {
                    this.Add(permission);
                }
            }
        }

        public int Add(DesktopModulePermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList desktopModulePermissions)
        {
            foreach (DesktopModulePermissionInfo permission in desktopModulePermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(DesktopModulePermissionCollection desktopModulePermissions)
        {
            foreach (DesktopModulePermissionInfo permission in desktopModulePermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(DesktopModulePermissionCollection objDesktopModulePermissionCollection)
        {
            if (objDesktopModulePermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareDesktopModulePermissions());
            objDesktopModulePermissionCollection.InnerList.Sort(new CompareDesktopModulePermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objDesktopModulePermissionCollection[i].DesktopModulePermissionID != this[i].DesktopModulePermissionID || objDesktopModulePermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
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
            return [..this.List.Cast<PermissionInfoBase>()];
        }

        public string ToString(string key)
        {
            return PermissionController.BuildPermissions(this.List, key);
        }
    }
}
