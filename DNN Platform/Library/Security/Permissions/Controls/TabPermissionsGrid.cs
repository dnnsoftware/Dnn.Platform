﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Security.Permissions.Controls
{
    public class TabPermissionsGrid : PermissionsGrid
    {
        #region Private Members

        private List<PermissionInfoBase> _PermissionsList;
        private int _TabID = -1;
        private TabPermissionCollection _TabPermissions;

        #endregion

        #region Protected Methods

        protected override bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        protected override bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "VIEW");
        }

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this._PermissionsList == null && this._TabPermissions != null)
                {
                    this._PermissionsList = this._TabPermissions.ToList();
                }
                return this._PermissionsList;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Permissions Collection
        /// </summary>
        /// -----------------------------------------------------------------------------
        public TabPermissionCollection Permissions
        {
            get
            {
                //First Update Permissions in case they have been changed
                this.UpdatePermissions();

                //Return the TabPermissions
                return this._TabPermissions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Id of the Tab
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabID
        {
            get
            {
                return this._TabID;
            }
            set
            {
                this._TabID = value;
                if (!this.Page.IsPostBack)
                {
                    this.GetTabPermissions();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the TabPermissions from the Data Store
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void GetTabPermissions()
        {
            this._TabPermissions = new TabPermissionCollection(TabPermissionController.GetTabPermissions(this.TabID, this.PortalId));
            this._PermissionsList = null;
        }

        public override void DataBind()
        {
            this.GetTabPermissions();
            base.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Parse the Permission Keys used to persist the Permissions in the ViewState
        /// </summary>
        /// <param name="Settings">A string array of settings</param>
        /// -----------------------------------------------------------------------------
        private TabPermissionInfo ParseKeys(string[] Settings)
        {
            var objTabPermission = new TabPermissionInfo();

            //Call base class to load base properties
            base.ParsePermissionKeys(objTabPermission, Settings);
            if (String.IsNullOrEmpty(Settings[2]))
            {
                objTabPermission.TabPermissionID = -1;
            }
            else
            {
                objTabPermission.TabPermissionID = Convert.ToInt32(Settings[2]);
            }
            objTabPermission.TabID = this.TabID;

            return objTabPermission;
        }

        private void rolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var roleID = Int32.Parse(((DataRowView)item.DataItem)[0].ToString());
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

        private bool IsImplicitRole(int portalId, int roleId)
        {
            return TabPermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += this.rolePermissionsGrid_ItemDataBound;
        }
        
        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new TabPermissionInfo(permission);
            objPermission.TabID = this.TabID;
            objPermission.RoleID = roleId;
            objPermission.RoleName = roleName;
            objPermission.AllowAccess = allowAccess;
            objPermission.UserID = userId;
            objPermission.DisplayName = displayName;
            this._TabPermissions.Add(objPermission, true);

            //Clear Permission List
            this._PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Permission
        /// </summary>
        /// <param name="permissions">The permissions collection</param>
        /// <param name="user">The user to add</param>
        /// -----------------------------------------------------------------------------
        protected override void AddPermission(ArrayList permissions, UserInfo user)
        {
            //Search TabPermission Collection for the user 
            bool isMatch = false;
            foreach (TabPermissionInfo objTabPermission in this._TabPermissions)
            {
                if (objTabPermission.UserID == user.UserID)
                {
                    isMatch = true;
                    break;
                }
            }

            //user not found so add new
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
        /// Updates a Permission
        /// </summary>
        /// <param name="permissions">The permissions collection</param>
        /// <param name="role">The role to add</param>
        /// -----------------------------------------------------------------------------
        protected override void AddPermission(ArrayList permissions, RoleInfo role)
        {
            //Search TabPermission Collection for the user 
            if (this._TabPermissions.Cast<TabPermissionInfo>().Any(objTabPermission => objTabPermission.RoleID == role.RoleID))
            {
                return;
            }

            //role not found so add new            
            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Enabled status of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="role">The role</param>
        /// <param name="column">The column of the Grid</param>
        /// -----------------------------------------------------------------------------
        protected override bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return !this.IsImplicitRole(role.PortalID, role.RoleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Value of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="role">The role</param>
        /// <param name="column">The column of the Grid</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>A Boolean (True or False)</returns>
        /// -----------------------------------------------------------------------------
        protected override string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string permission;

            if (role.RoleID == this.AdministratorRoleId)
            {
                permission = PermissionTypeGrant;
            }
            else
            {
                //Call base class method to handle standard permissions
                permission = base.GetPermission(objPerm, role, column, PermissionTypeNull);
            }
            return permission;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the permissions from the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override ArrayList GetPermissions()
        {
            return PermissionController.GetPermissionsByTab();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Load the ViewState
        /// </summary>
        /// <param name="savedState">The saved state</param>
        /// -----------------------------------------------------------------------------
        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                //Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

                //Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                //Load TabId
                if (myState[1] != null)
                {
                    this.TabID = Convert.ToInt32(myState[1]);
                }

                //Load TabPermissions
                if (myState[2] != null)
                {
                    this._TabPermissions = new TabPermissionCollection();
                    string state = Convert.ToString(myState[2]);
                    if (!String.IsNullOrEmpty(state))
                    {
                        //First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] Settings = key.Split('|');
                            this._TabPermissions.Add(this.ParseKeys(Settings));
                        }
                    }
                }
            }
        }

        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this._TabPermissions.Remove(permissionID, roleID, userID);
            //Clear Permission List
            this._PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Saves the ViewState
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override object SaveViewState()
        {
            var allStates = new object[3];

            //Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            //Save the Tab Id
            allStates[1] = this.TabID;

            //Persist the TabPermisisons
            var sb = new StringBuilder();
            if (this._TabPermissions != null)
            {
                bool addDelimiter = false;
                foreach (TabPermissionInfo objTabPermission in this._TabPermissions)
                {
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }
                    sb.Append(this.BuildKey(objTabPermission.AllowAccess,
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// returns whether or not the derived grid supports Deny permissions
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return true;
        }

        #endregion

        #region Public Methods

        public override void GenerateDataGrid()
        {
        }

        #endregion
    }
}
