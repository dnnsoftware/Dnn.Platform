// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : WorkflowStatePermissionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   DesktopModulePermissionCollection provides the a custom collection for WorkflowStatePermissionInfo
    ///   objects.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class WorkflowStatePermissionCollection : CollectionBase
    {
        public WorkflowStatePermissionCollection()
        {
        }

        public WorkflowStatePermissionCollection(ArrayList WorkflowStatePermissions)
        {
            this.AddRange(WorkflowStatePermissions);
        }

        public WorkflowStatePermissionCollection(WorkflowStatePermissionCollection WorkflowStatePermissions)
        {
            this.AddRange(WorkflowStatePermissions);
        }

        public WorkflowStatePermissionCollection(ArrayList WorkflowStatePermissions, int WorkflowStatePermissionID)
        {
            foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissions)
            {
                if (permission.WorkflowStatePermissionID == WorkflowStatePermissionID)
                {
                    this.Add(permission);
                }
            }
        }

        public WorkflowStatePermissionInfo this[int index]
        {
            get
            {
                return (WorkflowStatePermissionInfo)this.List[index];
            }

            set
            {
                this.List[index] = value;
            }
        }

        public int Add(WorkflowStatePermissionInfo value)
        {
            return this.List.Add(value);
        }

        public int Add(WorkflowStatePermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList WorkflowStatePermissions)
        {
            foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissions)
            {
                this.Add(permission);
            }
        }

        public void AddRange(WorkflowStatePermissionCollection WorkflowStatePermissions)
        {
            foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissions)
            {
                this.Add(permission);
            }
        }

        public bool CompareTo(WorkflowStatePermissionCollection objWorkflowStatePermissionCollection)
        {
            if (objWorkflowStatePermissionCollection.Count != this.Count)
            {
                return false;
            }

            this.InnerList.Sort(new CompareWorkflowStatePermissions());
            objWorkflowStatePermissionCollection.InnerList.Sort(new CompareWorkflowStatePermissions());

            for (int i = 0; i < this.Count; i++)
            {
                if (objWorkflowStatePermissionCollection[i].WorkflowStatePermissionID != this[i].WorkflowStatePermissionID || objWorkflowStatePermissionCollection[i].AllowAccess != this[i].AllowAccess)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Contains(WorkflowStatePermissionInfo value)
        {
            return this.List.Contains(value);
        }

        public int IndexOf(WorkflowStatePermissionInfo value)
        {
            return this.List.IndexOf(value);
        }

        public void Insert(int index, WorkflowStatePermissionInfo value)
        {
            this.List.Insert(index, value);
        }

        public void Remove(WorkflowStatePermissionInfo value)
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
    }
}
