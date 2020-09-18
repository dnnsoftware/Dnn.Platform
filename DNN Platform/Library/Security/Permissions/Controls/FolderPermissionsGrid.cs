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
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    public class FolderPermissionsGrid : PermissionsGrid
    {
        protected FolderPermissionCollection FolderPermissions;
        private string _folderPath = string.Empty;
        private List<PermissionInfoBase> _permissionsList;
        private bool _refreshGrid;
        private IList<PermissionInfo> _systemFolderPermissions;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Permission Collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public FolderPermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the FolderPermissions
                return this.FolderPermissions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and Sets the path of the Folder.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string FolderPath
        {
            get
            {
                return this._folderPath;
            }

            set
            {
                this._folderPath = value;
                this._refreshGrid = true;
                this.GetFolderPermissions();
            }
        }

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this._permissionsList == null && this.FolderPermissions != null)
                {
                    this._permissionsList = this.FolderPermissions.ToList();
                }

                return this._permissionsList;
            }
        }

        protected override bool RefreshGrid
        {
            get
            {
                return this._refreshGrid;
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the TabPermissions from the Data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void GetFolderPermissions()
        {
            this.FolderPermissions = new FolderPermissionCollection(FolderPermissionController.GetFolderPermissionsCollectionByFolder(this.PortalId, this.FolderPath));
            this._permissionsList = null;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += this.rolePermissionsGrid_ItemDataBound;
        }

        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new FolderPermissionInfo(permission)
            {
                FolderPath = this.FolderPath,
                RoleID = roleId,
                RoleName = roleName,
                AllowAccess = allowAccess,
                UserID = userId,
                DisplayName = displayName,
            };
            this.FolderPermissions.Add(objPermission, true);

            // Clear Permission List
            this._permissionsList = null;
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
            bool isMatch = false;
            foreach (FolderPermissionInfo objFolderPermission in this.FolderPermissions)
            {
                if (objFolderPermission.UserID == user.UserID)
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
                    if (objPermission.PermissionKey == "READ")
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
            if (this.FolderPermissions.Cast<FolderPermissionInfo>().Any(p => p.RoleID == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "READ")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
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
            return !this.IsImplicitRole(role.PortalID, role.RoleID);
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
            if (role.RoleID == this.AdministratorRoleId && this.IsPermissionAlwaysGrantedToAdmin(objPerm))
            {
                permission = PermissionTypeGrant;
            }
            else
            {
                // Call base class method to handle standard permissions
                permission = base.GetPermission(objPerm, role, column, defaultState);
            }

            return permission;
        }

        protected override bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "WRITE") && PermissionProvider.Instance().SupportsFullControl();
        }

        protected override bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return permissionInfo.PermissionKey == "READ";
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the permissions from the Database.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected override ArrayList GetPermissions()
        {
            ArrayList perms = PermissionController.GetPermissionsByFolder();
            this._systemFolderPermissions = perms.Cast<PermissionInfo>().ToList();
            return perms;
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

                // Load FolderPath
                if (myState[1] != null)
                {
                    this._folderPath = Convert.ToString(myState[1]);
                }

                // Load FolderPermissions
                if (myState[2] != null)
                {
                    this.FolderPermissions = new FolderPermissionCollection();
                    string state = Convert.ToString(myState[2]);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.FolderPermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
        }

        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this.FolderPermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this._permissionsList = null;
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

            // Save the Tab Id
            allStates[1] = this.FolderPath;

            // Persist the TabPermisisons
            var sb = new StringBuilder();
            if (this.FolderPermissions != null)
            {
                bool addDelimiter = false;
                foreach (FolderPermissionInfo objFolderPermission in this.FolderPermissions)
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
                        objFolderPermission.AllowAccess,
                        objFolderPermission.PermissionID,
                        objFolderPermission.FolderPermissionID,
                        objFolderPermission.RoleID,
                        objFolderPermission.RoleName,
                        objFolderPermission.UserID,
                        objFolderPermission.DisplayName));
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
            return this.IsSystemFolderPermission(permissionInfo);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Parse the Permission Keys used to persist the Permissions in the ViewState.
        /// </summary>
        /// <param name="settings">A string array of settings.</param>
        /// -----------------------------------------------------------------------------
        private FolderPermissionInfo ParseKeys(string[] settings)
        {
            var objFolderPermission = new FolderPermissionInfo();

            // Call base class to load base properties
            this.ParsePermissionKeys(objFolderPermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objFolderPermission.FolderPermissionID = -1;
            }
            else
            {
                objFolderPermission.FolderPermissionID = Convert.ToInt32(settings[2]);
            }

            objFolderPermission.FolderPath = this.FolderPath;
            return objFolderPermission;
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

        private bool IsPermissionAlwaysGrantedToAdmin(PermissionInfo permissionInfo)
        {
            return this.IsSystemFolderPermission(permissionInfo);
        }

        private bool IsSystemFolderPermission(PermissionInfo permissionInfo)
        {
            return this._systemFolderPermissions.Any(pi => pi.PermissionID == permissionInfo.PermissionID);
        }

        private bool IsImplicitRole(int portalId, int roleId)
        {
            return FolderPermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }
    }
}
