using System;
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : WorkflowStatePermissionCollection
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   DesktopModulePermissionCollection provides the a custom collection for WorkflowStatePermissionInfo
    ///   objects
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class WorkflowStatePermissionCollection : CollectionBase
    {
        #region Constructors

        public WorkflowStatePermissionCollection()
        {
        }

        public WorkflowStatePermissionCollection(ArrayList WorkflowStatePermissions)
        {
            AddRange(WorkflowStatePermissions);
        }

        public WorkflowStatePermissionCollection(WorkflowStatePermissionCollection WorkflowStatePermissions)
        {
            AddRange(WorkflowStatePermissions);
        }

        public WorkflowStatePermissionCollection(ArrayList WorkflowStatePermissions, int WorkflowStatePermissionID)
        {
            foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissions)
            {
                if (permission.WorkflowStatePermissionID == WorkflowStatePermissionID)
                {
                    Add(permission);
                }
            }
        }

        #endregion

        #region Public Properties

        public WorkflowStatePermissionInfo this[int index]
        {
            get
            {
                return (WorkflowStatePermissionInfo) (List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        #endregion

        #region Public Methods

        public int Add(WorkflowStatePermissionInfo value)
        {
            return List.Add(value);
        }

        public int Add(WorkflowStatePermissionInfo value, bool checkForDuplicates)
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

        public void AddRange(ArrayList WorkflowStatePermissions)
        {
            foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissions)
            {
                Add(permission);
            }
        }

        public void AddRange(WorkflowStatePermissionCollection WorkflowStatePermissions)
        {
            foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissions)
            {
                Add(permission);
            }
        }

        public bool CompareTo(WorkflowStatePermissionCollection objWorkflowStatePermissionCollection)
        {
            if (objWorkflowStatePermissionCollection.Count != Count)
            {
                return false;
            }
            InnerList.Sort(new CompareWorkflowStatePermissions());
            objWorkflowStatePermissionCollection.InnerList.Sort(new CompareWorkflowStatePermissions());

            for (int i = 0; i < Count; i++)
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
            return List.Contains(value);
        }

        public int IndexOf(WorkflowStatePermissionInfo value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, WorkflowStatePermissionInfo value)
        {
            List.Insert(index, value);
        }

        public void Remove(WorkflowStatePermissionInfo value)
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

        #endregion
    }
}
