#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

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

#endregion

namespace DotNetNuke.Security.Permissions.Controls
{
    public class ModulePermissionsGrid : PermissionsGrid
    {
        #region Private Members

        private bool _InheritViewPermissionsFromTab;
        private int _ModuleID = -1;
        private ModulePermissionCollection _ModulePermissions;
        private List<PermissionInfoBase> _PermissionsList;
        private int _ViewColumnIndex;

        public ModulePermissionsGrid()
        {
            TabId = -1;
        }

        #endregion

        #region Protected Properties

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (_PermissionsList == null && _ModulePermissions != null)
                {
                    _PermissionsList = _ModulePermissions.ToList();
                }
                return _PermissionsList;
            }
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets whether the Module inherits the Page's(Tab's) permissions
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool InheritViewPermissionsFromTab
        {
            get
            {
                return _InheritViewPermissionsFromTab;
            }
            set
            {
                _InheritViewPermissionsFromTab = value;
                _PermissionsList = null;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Id of the Module
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int ModuleID
        {
            get
            {
                return _ModuleID;
            }
            set
            {
                _ModuleID = value;
                if (!Page.IsPostBack)
                {
                    GetModulePermissions();
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Id of the Tab associated with this module
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int TabId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModulePermission Collection
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModulePermissionCollection Permissions
        {
            get
            {
                //First Update Permissions in case they have been changed
                UpdatePermissions();

                //Return the ModulePermissions
                return _ModulePermissions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Check if a role is implicit for Module Permissions
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool IsImplicitRole(int portalId, int roleId)
        {
            return ModulePermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the ModulePermissions from the Data Store
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void GetModulePermissions()
        {
            _ModulePermissions = new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(ModuleID, TabId));
            _PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Parse the Permission Keys used to persist the Permissions in the ViewState
        /// </summary>
        /// <param name="Settings">A string array of settings</param>
        /// -----------------------------------------------------------------------------
        private ModulePermissionInfo ParseKeys(string[] Settings)
        {
            var objModulePermission = new ModulePermissionInfo();

            //Call base class to load base properties
            base.ParsePermissionKeys(objModulePermission, Settings);
            if (String.IsNullOrEmpty(Settings[2]))
            {
                objModulePermission.ModulePermissionID = -1;
            }
            else
            {
                objModulePermission.ModulePermissionID = Convert.ToInt32(Settings[2]);
            }
            objModulePermission.ModuleID = ModuleID;
            return objModulePermission;
        }


        private void rolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var roleID = Int32.Parse(((DataRowView)item.DataItem)[0].ToString());
                if (IsImplicitRole(PortalSettings.Current.PortalId, roleID))
                {
                    var actionImage = item.Controls.Cast<Control>().Last().Controls[0] as ImageButton;
                    if (actionImage != null)
                    {
                        actionImage.Visible = false;
                    }
                }
            }
        }


        #endregion

        #region Protected Methods

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            rolePermissionsGrid.ItemDataBound += rolePermissionsGrid_ItemDataBound;
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
            bool isMatch = _ModulePermissions.Cast<ModulePermissionInfo>()
                            .Any(objModulePermission => objModulePermission.UserID == user.UserID);

            //user not found so add new
            if (!isMatch)
            {
                foreach (PermissionInfo objPermission in permissions)
                {
                    if (objPermission.PermissionKey == "VIEW")
                    {
                        AddPermission(objPermission, int.Parse(Globals.glbRoleNothing), Null.NullString, user.UserID, user.DisplayName, true);
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
            if (
                _ModulePermissions.Cast<ModulePermissionInfo>().Any(p => p.RoleID == role.RoleID))
            {
                return;
            }

            //role not found so add new            
            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }            
        }

        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new ModulePermissionInfo(permission)
                {
                    ModuleID = ModuleID,
                    RoleID = roleId,
                    RoleName = roleName,
                    AllowAccess = allowAccess,
                    UserID = userId,
                    DisplayName = displayName
                };
            _ModulePermissions.Add(objPermission, true);

            //Clear Permission List
            _PermissionsList = null;
        }

        protected override void UpdatePermission(PermissionInfo permission, int roleId, string roleName, string stateKey)
        {
            if (InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }
            base.UpdatePermission(permission, roleId, roleName, stateKey);
        }

        protected override void UpdatePermission(PermissionInfo permission, string displayName, int userId, string stateKey)
        {
            if (InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }
            base.UpdatePermission(permission, displayName, userId, stateKey);
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
            bool enabled;
            if (InheritViewPermissionsFromTab && column == _ViewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = !IsImplicitRole(role.PortalID, role.RoleID);
            }
            return enabled;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Enabled status of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="user">The user</param>
        /// <param name="column">The column of the Grid</param>
        /// -----------------------------------------------------------------------------
        protected override bool GetEnabled(PermissionInfo objPerm, UserInfo user, int column)
        {
            bool enabled;
            if (InheritViewPermissionsFromTab && column == _ViewColumnIndex)
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
            if (InheritViewPermissionsFromTab && column == _ViewColumnIndex)
            {
                permission = PermissionTypeNull;
            }
            else
            {
                permission = role.RoleID == AdministratorRoleId 
                                ? PermissionTypeGrant 
                                : base.GetPermission(objPerm, role, column, defaultState);
            }
            return permission;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Value of the permission
        /// </summary>
        /// <param name="objPerm">The permission being loaded</param>
        /// <param name="user">The role</param>
        /// <param name="column">The column of the Grid</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>A Boolean (True or False)</returns>
        /// -----------------------------------------------------------------------------
        protected override string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
        {
            string permission;
            if (InheritViewPermissionsFromTab && column == _ViewColumnIndex)
            {
                permission = PermissionTypeNull;
            }
            else
            {
                //Call base class method to handle standard permissions
                permission = base.GetPermission(objPerm, user, column, defaultState);
            }
            return permission;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Permissions from the Data Store
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override ArrayList GetPermissions()
        {
            var moduleInfo = ModuleController.Instance.GetModule(ModuleID, TabId, false);

            var permissionController = new PermissionController();
            var permissions = permissionController.GetPermissionsByModule(ModuleID, TabId);

            var permissionList = new ArrayList();
            for (int i = 0; i <= permissions.Count - 1; i++)
            {
                var permission = (PermissionInfo)permissions[i];
                if (permission.PermissionKey == "VIEW")
                {
                    _ViewColumnIndex = i + 1;
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
            return (permissionInfo.PermissionKey == "VIEW");
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

                //Load ModuleID
                if (myState[1] != null)
                {
                    ModuleID = Convert.ToInt32(myState[1]);
                }

                //Load TabId
                if (myState[2] != null)
                {
                    TabId = Convert.ToInt32(myState[2]);
                }

                //Load InheritViewPermissionsFromTab
                if (myState[3] != null)
                {
                    InheritViewPermissionsFromTab = Convert.ToBoolean(myState[3]);
                }

                //Load ModulePermissions
                if (myState[4] != null)
                {
                    _ModulePermissions = new ModulePermissionCollection();
                    string state = Convert.ToString(myState[4]);
                    if (!String.IsNullOrEmpty(state))
                    {
                        //First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] Settings = key.Split('|');
                            _ModulePermissions.Add(ParseKeys(Settings));
                        }
                    }
                }
            }
        }

        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            _ModulePermissions.Remove(permissionID, roleID, userID);
            //Clear Permission List
            _PermissionsList = null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Saves the ViewState
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override object SaveViewState()
        {
            var allStates = new object[5];

            //Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            //Save the ModuleID
            allStates[1] = ModuleID;

            //Save the TabID
            allStates[2] = TabId;

            //Save the InheritViewPermissionsFromTab
            allStates[3] = InheritViewPermissionsFromTab;

            //Persist the ModulePermissions
            var sb = new StringBuilder();
            if (_ModulePermissions != null)
            {
                bool addDelimiter = false;
                foreach (ModulePermissionInfo modulePermission in _ModulePermissions)
                {
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }
                    sb.Append(BuildKey(modulePermission.AllowAccess,
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
        /// returns whether or not the derived grid supports Deny permissions
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return true;
        }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Overrides the Base method to Generate the Data Grid
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void GenerateDataGrid()
        {
        }

        #endregion
    }
}
