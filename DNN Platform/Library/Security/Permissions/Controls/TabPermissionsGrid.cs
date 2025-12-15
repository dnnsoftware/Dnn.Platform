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

    public class TabPermissionsGrid : PermissionsGrid
    {
        private List<PermissionInfoBase> permissionsList;
        private int tabID = -1;
        private TabPermissionCollection tabPermissions;

        /// <summary>Gets the Permissions Collection.</summary>
        public TabPermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the TabPermissions
                return this.tabPermissions;
            }
        }

        /// <summary>Gets or sets the ID of the Tab.</summary>
        public int TabID
        {
            get
            {
                return this.tabID;
            }

            set
            {
                this.tabID = value;
                if (!this.Page.IsPostBack)
                {
                    this.GetTabPermissions();
                }
            }
        }

        /// <inheritdoc/>
        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this.permissionsList == null && this.tabPermissions != null)
                {
                    this.permissionsList = this.tabPermissions.ToList();
                }

                return this.permissionsList;
            }
        }

        /// <inheritdoc/>
        public override void DataBind()
        {
            this.GetTabPermissions();
            base.DataBind();
        }

        /// <inheritdoc/>
        public override void GenerateDataGrid()
        {
        }

        /// <inheritdoc/>
        protected override bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <inheritdoc/>
        protected override bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return permissionInfo.PermissionKey == "VIEW";
        }

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += RolePermissionsGrid_ItemDataBound;
        }

        /// <inheritdoc/>
        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new TabPermissionInfo(permission);
            objPermission.TabID = this.TabID;
            objPermission.RoleID = roleId;
            objPermission.RoleName = roleName;
            objPermission.AllowAccess = allowAccess;
            objPermission.UserID = userId;
            objPermission.DisplayName = displayName;
            this.tabPermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionsList = null;
        }

        /// <inheritdoc />
        protected override void AddPermission(ArrayList permissions, UserInfo user)
        {
            // Search TabPermission Collection for the user
            bool isMatch = false;
            foreach (TabPermissionInfo objTabPermission in this.tabPermissions)
            {
                if (objTabPermission.UserID == user.UserID)
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
                    if (objPermission.PermissionKey == "VIEW")
                    {
                        this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing), Null.NullString, user.UserID, user.DisplayName, true);
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void AddPermission(ArrayList permissions, RoleInfo role)
        {
            // Search TabPermission Collection for the user
            if (this.tabPermissions.Cast<TabPermissionInfo>().Any(objTabPermission => objTabPermission.RoleID == role.RoleID))
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

        /// <inheritdoc />
        protected override bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return !IsImplicitRole(role.PortalID, role.RoleID);
        }

        /// <inheritdoc />
        protected override string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string permission;

            if (role.RoleID == this.AdministratorRoleId)
            {
                permission = PermissionTypeGrant;
            }
            else
            {
                // Call base class method to handle standard permissions
                permission = base.GetPermission(objPerm, role, column, PermissionTypeNull);
            }

            return permission;
        }

        /// <inheritdoc />
        protected override ArrayList GetPermissions()
        {
            return PermissionController.GetPermissionsByTab();
        }

        /// <summary>Load the ViewState.</summary>
        /// <param name="savedState">The saved state.</param>
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

                // Load TabId
                if (myState[1] != null)
                {
                    this.TabID = Convert.ToInt32(myState[1]);
                }

                // Load TabPermissions
                if (myState[2] != null)
                {
                    this.tabPermissions = new TabPermissionCollection();
                    string state = Convert.ToString(myState[2]);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.tabPermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this.tabPermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this.permissionsList = null;
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
            var allStates = new object[3];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the Tab Id
            allStates[1] = this.TabID;

            // Persist the TabPermisisons
            var sb = new StringBuilder();
            if (this.tabPermissions != null)
            {
                bool addDelimiter = false;
                foreach (TabPermissionInfo objTabPermission in this.tabPermissions)
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
                        objTabPermission.AllowAccess,
                        objTabPermission.PermissionID,
                        objTabPermission.TabPermissionID,
                        objTabPermission.RoleID,
                        objTabPermission.RoleName,
                        objTabPermission.UserID,
                        objTabPermission.DisplayName));
                }
            }

            allStates[2] = sb.ToString();
            return allStates;
        }

        /// <summary>returns whether the derived grid supports Deny permissions.</summary>
        /// <param name="permissionInfo">The permission info.</param>
        /// <returns><see langword="true"/> if this grid supports deny permissions, otherwise <see langword="false"/>.</returns>
        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return true;
        }

        private static void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var roleID = int.Parse(((DataRowView)item.DataItem)[0].ToString());
                if (IsImplicitRole(PortalSettings.Current.PortalId, roleID))
                {
                    if (item.Controls.Cast<Control>().Last().Controls[0] is ImageButton actionImage)
                    {
                        actionImage.Visible = false;
                    }
                }
            }
        }

        private static bool IsImplicitRole(int portalId, int roleId)
        {
            return TabPermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        /// <summary>Gets the TabPermissions from the Data Store.</summary>
        private void GetTabPermissions()
        {
            this.tabPermissions = new TabPermissionCollection(TabPermissionController.GetTabPermissions(this.TabID, this.PortalId));
            this.permissionsList = null;
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private TabPermissionInfo ParseKeys(string[] settings)
        {
            var objTabPermission = new TabPermissionInfo();

            // Call base class to load base properties
            this.ParsePermissionKeys(objTabPermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objTabPermission.TabPermissionID = -1;
            }
            else
            {
                objTabPermission.TabPermissionID = Convert.ToInt32(settings[2]);
            }

            objTabPermission.TabID = this.TabID;

            return objTabPermission;
        }
    }
}
