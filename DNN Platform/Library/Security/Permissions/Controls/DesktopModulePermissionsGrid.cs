// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    public class DesktopModulePermissionsGrid : PermissionsGrid
    {
        private DesktopModulePermissionCollection _DesktopModulePermissions;
        private List<PermissionInfoBase> _PermissionsList;
        private int _PortalDesktopModuleID = -1;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Permissions Collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DesktopModulePermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the DesktopModulePermissions
                return this._DesktopModulePermissions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Id of the PortalDesktopModule.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int PortalDesktopModuleID
        {
            get
            {
                return this._PortalDesktopModuleID;
            }

            set
            {
                int oldValue = this._PortalDesktopModuleID;
                this._PortalDesktopModuleID = value;
                if (this._DesktopModulePermissions == null || oldValue != value)
                {
                    this.GetDesktopModulePermissions();
                }
            }
        }

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this._PermissionsList == null && this._DesktopModulePermissions != null)
                {
                    this._PermissionsList = this._DesktopModulePermissions.ToList();
                }

                return this._PermissionsList;
            }
        }

        public void ResetPermissions()
        {
            this.GetDesktopModulePermissions();
            this._PermissionsList = null;
        }

        public override void GenerateDataGrid()
        {
        }

        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new DesktopModulePermissionInfo(permission);
            objPermission.PortalDesktopModuleID = this.PortalDesktopModuleID;
            objPermission.RoleID = roleId;
            objPermission.RoleName = roleName;
            objPermission.AllowAccess = allowAccess;
            objPermission.UserID = userId;
            objPermission.DisplayName = displayName;
            this._DesktopModulePermissions.Add(objPermission, true);

            // Clear Permission List
            this._PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permissions">The permissions collection.</param>
        /// <param name="user">The user to add.</param>
        /// -----------------------------------------------------------------------------
        protected override void AddPermission(ArrayList permissions, UserInfo user)
        {
            // Search DesktopModulePermission Collection for the user
            bool isMatch = false;
            foreach (DesktopModulePermissionInfo objDesktopModulePermission in this._DesktopModulePermissions)
            {
                if (objDesktopModulePermission.UserID == user.UserID)
                {
                    isMatch = true;
                    break;
                }
            }

            // user not found so add new
            if (!isMatch)
            {
                foreach (PermissionInfo objPermission in permissions)
                {
                    if (objPermission.PermissionKey == "DEPLOY")
                    {
                        this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing), Null.NullString, user.UserID, user.DisplayName, true);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Permission.
        /// </summary>
        /// <param name="permissions">The permissions collection.</param>
        /// <param name="role">The roleto add.</param>
        /// -----------------------------------------------------------------------------
        protected override void AddPermission(ArrayList permissions, RoleInfo role)
        {
            // Search TabPermission Collection for the user
            if (this._DesktopModulePermissions.Cast<DesktopModulePermissionInfo>().Any(p => p.RoleID == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "DEPLOY")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the permissions from the Database.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override ArrayList GetPermissions()
        {
            return PermissionController.GetPermissionsByPortalDesktopModule();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Load the ViewState.
        /// </summary>
        /// <param name="savedState">The saved state.</param>
        /// -----------------------------------------------------------------------------
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load DesktopModuleId
                if (myState[1] != null)
                {
                    this.PortalDesktopModuleID = Convert.ToInt32(myState[1]);
                }

                // Load DesktopModulePermissions
                if (myState[2] != null)
                {
                    this._DesktopModulePermissions = new DesktopModulePermissionCollection();
                    string state = Convert.ToString(myState[2]);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] Settings = key.Split('|');
                            this._DesktopModulePermissions.Add(this.ParseKeys(Settings));
                        }
                    }
                }
            }
        }

        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this._DesktopModulePermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this._PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Saves the ViewState.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override object SaveViewState()
        {
            var allStates = new object[3];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the DesktopModule Id
            allStates[1] = this.PortalDesktopModuleID;

            // Persist the DesktopModulePermisisons
            var sb = new StringBuilder();
            if (this._DesktopModulePermissions != null)
            {
                bool addDelimiter = false;
                foreach (DesktopModulePermissionInfo objDesktopModulePermission in this._DesktopModulePermissions)
                {
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }

                    sb.Append(this.BuildKey(
                        objDesktopModulePermission.AllowAccess,
                        objDesktopModulePermission.PermissionID,
                        objDesktopModulePermission.DesktopModulePermissionID,
                        objDesktopModulePermission.RoleID,
                        objDesktopModulePermission.RoleName,
                        objDesktopModulePermission.UserID,
                        objDesktopModulePermission.DisplayName));
                }
            }

            allStates[2] = sb.ToString();
            return allStates;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// returns whether or not the derived grid supports Deny permissions.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the DesktopModulePermissions from the Data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void GetDesktopModulePermissions()
        {
            this._DesktopModulePermissions = new DesktopModulePermissionCollection(DesktopModulePermissionController.GetDesktopModulePermissions(this.PortalDesktopModuleID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Parse the Permission Keys used to persist the Permissions in the ViewState.
        /// </summary>
        /// <param name="Settings">A string array of settings.</param>
        /// -----------------------------------------------------------------------------
        private DesktopModulePermissionInfo ParseKeys(string[] Settings)
        {
            var objDesktopModulePermission = new DesktopModulePermissionInfo();

            // Call base class to load base properties
            this.ParsePermissionKeys(objDesktopModulePermission, Settings);
            if (string.IsNullOrEmpty(Settings[2]))
            {
                objDesktopModulePermission.DesktopModulePermissionID = -1;
            }
            else
            {
                objDesktopModulePermission.DesktopModulePermissionID = Convert.ToInt32(Settings[2]);
            }

            objDesktopModulePermission.PortalDesktopModuleID = this.PortalDesktopModuleID;
            return objDesktopModulePermission;
        }
    }
}
