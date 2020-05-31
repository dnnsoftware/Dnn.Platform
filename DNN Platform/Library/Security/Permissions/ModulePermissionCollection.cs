// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : ModulePermissionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModulePermissionCollection provides the a custom collection for ModulePermissionInfo
    /// objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ModulePermissionCollection : CollectionBase
    {
        public ModulePermissionCollection()
        {
        }

        public ModulePermissionCollection(ArrayList modulePermissions)
        {
            AddRange(modulePermissions);
        }

        public ModulePermissionCollection(ModulePermissionCollection modulePermissions)
        {
            AddRange(modulePermissions);
        }

        public ModulePermissionCollection(ArrayList modulePermissions, int ModuleID)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                if (permission.ModuleID == ModuleID)
                {
                    Add(permission);
                }
            }
        }

        public ModulePermissionCollection(ModuleInfo objModule)
        {
            foreach (ModulePermissionInfo permission in objModule.ModulePermissions)
            {
                if (permission.ModuleID == objModule.ModuleID)
                {
                    Add(permission);
                }
            }
        }

        public ModulePermissionInfo this[int index]
        {
            get
            {
                return (ModulePermissionInfo) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(ModulePermissionInfo value)
        {
            return List.Add(value);
        }

        public int Add(ModulePermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                Add(permission);
            }
        }

        public void AddRange(ModulePermissionCollection modulePermissions)
        {
            foreach (ModulePermissionInfo permission in modulePermissions)
            {
                Add(permission);
            }
        }

        public bool CompareTo(ModulePermissionCollection objModulePermissionCollection)
        {
            if (objModulePermissionCollection.Count != Count)
            {
                return false;
            }
            InnerList.Sort(new CompareModulePermissions());
            objModulePermissionCollection.InnerList.Sort(new CompareModulePermissions());
            for (int i = 0; i <= Count - 1; i++)
            {
                if (objModulePermissionCollection[i].ModulePermissionID != this[i].ModulePermissionID || objModulePermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Contains(ModulePermissionInfo value)
        {
            return List.Contains(value);
        }

        public int IndexOf(ModulePermissionInfo value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, ModulePermissionInfo value)
        {
            List.Insert(index, value);
        }

        public void Remove(ModulePermissionInfo value)
        {
            List.Remove(value);
        }

        public void Remove(int permissionID, int roleID, int userID)
        {
            var idx = 0;
            foreach (PermissionInfoBase permission in List)
            {
                if (permission.PermissionID == permissionID && permission.UserID == userID && permission.RoleID == roleID)
                {
                    List.RemoveAt(idx);
                    break;
                }
                idx++;
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

        public IEnumerable<ModulePermissionInfo> Where(Func<ModulePermissionInfo, bool> predicate)
        {
            return this.Cast<ModulePermissionInfo>().Where(predicate);
        }
    }
}
