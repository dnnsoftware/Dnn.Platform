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
    using DotNetNuke.Entities.Modules;

    /// <summary>ModulePermissionCollection provides a custom collection for <see cref="ModulePermissionInfo"/> objects.</summary>
    [Serializable]
    public class ModulePermissionCollection : GenericCollectionBase<ModulePermissionInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        public ModulePermissionCollection()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">An <see cref="ArrayList"/> of <see cref="ModulePermissionInfo"/> instances.</param>
        public ModulePermissionCollection(ArrayList modulePermissions)
        {
            this.AddRange(modulePermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">A collection of <see cref="ModulePermissionInfo"/> instances.</param>
        public ModulePermissionCollection(ModulePermissionCollection modulePermissions)
        {
            this.AddRange(modulePermissions);
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="modulePermissions">An <see cref="ArrayList"/> of <see cref="ModulePermissionInfo"/> instances.</param>
        /// <param name="moduleID">The ID of the module by which to filter <paramref name="modulePermissions"/>.</param>
        public ModulePermissionCollection(ArrayList modulePermissions, int moduleID)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                if (permission.ModuleID == moduleID)
                {
                    this.Add(permission);
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionCollection"/> class.</summary>
        /// <param name="objModule">A module from which to copy <see cref="ModuleInfo.ModulePermissions"/>.</param>
        public ModulePermissionCollection(ModuleInfo objModule)
        {
            foreach (ModulePermissionInfo permission in objModule.ModulePermissions)
            {
                if (permission.ModuleID == objModule.ModuleID)
                {
                    this.Add(permission);
                }
            }
        }

        public int Add(ModulePermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(ModulePermissionCollection modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(ModulePermissionCollection objModulePermissionCollection)
        {
            if (objModulePermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareModulePermissions());
            objModulePermissionCollection.InnerList.Sort(new CompareModulePermissions());
            for (int i = 0; i <= this.Count - 1; i++)
            {
                if (objModulePermissionCollection[i].ModulePermissionID != this[i].ModulePermissionID || objModulePermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
            var idx = 0;
            foreach (PermissionInfoBase permission in this.List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    this.List.RemoveAt(idx);
                    break;
                }

                idx++;
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

        public IEnumerable<ModulePermissionInfo> Where(Func<ModulePermissionInfo, bool> predicate)
        {
            return this.Cast<ModulePermissionInfo>().Where(predicate);
        }
    }
}
