// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    public class ModulePermissionsGrid : PermissionsGrid
    {
        private bool _InheritViewPermissionsFromTab;
        private int _ModuleID = -1;
        private ModulePermissionCollection _ModulePermissions;
        private List<PermissionInfoBase> _PermissionsList;
        private int _ViewColumnIndex;

        public ModulePermissionsGrid()
        {
            this.TabId = -1;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModulePermission Collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModulePermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the ModulePermissions
                return this._ModulePermissions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and Sets whether the Module inherits the Page's(Tab's) permissions.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool InheritViewPermissionsFromTab
        {
            get
            {
                return this._InheritViewPermissionsFromTab;
            }

            set
            {
                this._InheritViewPermissionsFromTab = value;
                this._PermissionsList = null;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Id of the Module.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int ModuleID
        {
            get
            {
                return this._ModuleID;
            }

            set
            {
                this._ModuleID = value;
                if (!this.Page.IsPostBack)
                {
                    this.GetModulePermissions();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the Id of the Tab associated with this module.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabId { get; set; }

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this._PermissionsList == null && this._ModulePermissions != null)
                {
                    this._PermissionsList = this._ModulePermissions.ToList();
                }

                return this._PermissionsList;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Overrides the Base method to Generate the Data Grid.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void GenerateDataGrid()
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += this.rolePermissionsGrid_ItemDataBound;
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
            bool isMatch = this._ModulePermissions.Cast<ModulePermissionInfo>()
                            .Any(objModulePermission => objModulePermission.UserID == user.UserID);

            // user not found so add new
            if (!isMatch)
            {
                foreach (PermissionInfo objPermission in permissions)
                {
                    if (objPermission.PermissionKey == "VIEW")
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
        /// <param name="role">The role to add.</param>
        /// -----------------------------------------------------------------------------
        protected override void AddPermission(ArrayList permissions, RoleInfo role)
        {
            // Search TabPermission Collection for the user
            if (
                this._ModulePermissions.Cast<ModulePermissionInfo>().Any(p => p.RoleID == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new ModulePermissionInfo(permission)
            {
                ModuleID = this.ModuleID,
                RoleID = roleId,
                RoleName = roleName,
                AllowAccess = allowAccess,
                UserID = userId,
                DisplayName = displayName,
            };
            this._ModulePermissions.Add(objPermission, true);

            // Clear Permission List
            this._PermissionsList = null;
        }

        protected override void UpdatePermission(PermissionInfo permission, int roleId, string roleName, string stateKey)
        {
            if (this.InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }

            base.UpdatePermission(permission, roleId, roleName, stateKey);
        }

        protected override void UpdatePermission(PermissionInfo permission, string displayName, int userId, string stateKey)
        {
            if (this.InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }

            base.UpdatePermission(permission, displayName, userId, stateKey);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Enabled status of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this._ViewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = !this.IsImplicitRole(role.PortalID, role.RoleID);
            }

            return enabled;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Enabled status of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override bool GetEnabled(PermissionInfo objPerm, UserInfo user, int column)
        {
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this._ViewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = true;
            }

            return enabled;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Value of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>A Boolean (True or False).</returns>
        /// -----------------------------------------------------------------------------
        protected override string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string permission;
            if (this.InheritViewPermissionsFromTab && column == this._ViewColumnIndex)
            {
                permission = PermissionTypeNull;
            }
            else
            {
                permission = role.RoleID == this.AdministratorRoleId
                                ? PermissionTypeGrant
                                : base.GetPermission(objPerm, role, column, defaultState);
            }

            return permission;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Value of the permission.
        /// </summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>A Boolean (True or False).</returns>
        /// -----------------------------------------------------------------------------
        protected override string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
        {
            string permission;
            if (this.InheritViewPermissionsFromTab && column == this._ViewColumnIndex)
            {
                permission = PermissionTypeNull;
            }
            else
            {
                // Call base class method to handle standard permissions
                permission = base.GetPermission(objPerm, user, column, defaultState);
            }

            return permission;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Permissions from the Data Store.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override ArrayList GetPermissions()
        {
            var moduleInfo = ModuleController.Instance.GetModule(this.ModuleID, this.TabId, false);

            var permissionController = new PermissionController();
            var permissions = permissionController.GetPermissionsByModule(this.ModuleID, this.TabId);

            var permissionList = new ArrayList();
            for (int i = 0; i <= permissions.Count - 1; i++)
            {
                var permission = (PermissionInfo)permissions[i];
                if (permission.PermissionKey == "VIEW")
                {
                    this._ViewColumnIndex = i + 1;
                    permissionList.Add(permission);
                }
                else
                {
                    if (!(moduleInfo.IsShared && moduleInfo.IsShareableViewOnly))
                    {
                        permissionList.Add(permission);
                    }
                }
            }

            return permissionList;
        }

        protected override bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        protected override bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return permissionInfo.PermissionKey == "VIEW";
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

                // Load ModuleID
                if (myState[1] != null)
                {
                    this.ModuleID = Convert.ToInt32(myState[1]);
                }

                // Load TabId
                if (myState[2] != null)
                {
                    this.TabId = Convert.ToInt32(myState[2]);
                }

                // Load InheritViewPermissionsFromTab
                if (myState[3] != null)
                {
                    this.InheritViewPermissionsFromTab = Convert.ToBoolean(myState[3]);
                }

                // Load ModulePermissions
                if (myState[4] != null)
                {
                    this._ModulePermissions = new ModulePermissionCollection();
                    string state = Convert.ToString(myState[4]);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] Settings = key.Split('|');
                            this._ModulePermissions.Add(this.ParseKeys(Settings));
                        }
                    }
                }
            }
        }

        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this._ModulePermissions.Remove(permissionID, roleID, userID);

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
            var allStates = new object[5];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the ModuleID
            allStates[1] = this.ModuleID;

            // Save the TabID
            allStates[2] = this.TabId;

            // Save the InheritViewPermissionsFromTab
            allStates[3] = this.InheritViewPermissionsFromTab;

            // Persist the ModulePermissions
            var sb = new StringBuilder();
            if (this._ModulePermissions != null)
            {
                bool addDelimiter = false;
                foreach (ModulePermissionInfo modulePermission in this._ModulePermissions)
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
                        modulePermission.AllowAccess,
                        modulePermission.PermissionID,
                        modulePermission.ModulePermissionID,
                        modulePermission.RoleID,
                        modulePermission.RoleName,
                        modulePermission.UserID,
                        modulePermission.DisplayName));
                }
            }

            allStates[4] = sb.ToString();
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
        /// Check if a role is implicit for Module Permissions.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool IsImplicitRole(int portalId, int roleId)
        {
            return ModulePermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModulePermissions from the Data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void GetModulePermissions()
        {
            this._ModulePermissions = new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(this.ModuleID, this.TabId));
            this._PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Parse the Permission Keys used to persist the Permissions in the ViewState.
        /// </summary>
        /// <param name="Settings">A string array of settings.</param>
        /// -----------------------------------------------------------------------------
        private ModulePermissionInfo ParseKeys(string[] Settings)
        {
            var objModulePermission = new ModulePermissionInfo();

            // Call base class to load base properties
            this.ParsePermissionKeys(objModulePermission, Settings);
            if (string.IsNullOrEmpty(Settings[2]))
            {
                objModulePermission.ModulePermissionID = -1;
            }
            else
            {
                objModulePermission.ModulePermissionID = Convert.ToInt32(Settings[2]);
            }

            objModulePermission.ModuleID = this.ModuleID;
            return objModulePermission;
        }

        private void rolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var roleID = int.Parse(((DataRowView)item.DataItem)[0].ToString());
                if (this.IsImplicitRole(PortalSettings.Current.PortalId, roleID))
                {
                    var actionImage = item.Controls.Cast<Control>().Last().Controls[0] as ImageButton;
                    if (actionImage != null)
                    {
                        actionImage.Visible = false;
                    }
                }
            }
        }
    }
}
