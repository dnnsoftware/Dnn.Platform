#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.Text;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

#endregion

namespace DotNetNuke.Security.Permissions.Controls
{
    public class DesktopModulePermissionsGrid : PermissionsGrid
    {
        #region "Private Members"

        private DesktopModulePermissionCollection _DesktopModulePermissions;
        private List<PermissionInfoBase> _PermissionsList;
        private int _PortalDesktopModuleID = -1;

        #endregion

        #region "Protected Properties"

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (_PermissionsList == null && _DesktopModulePermissions != null)
                {
                    _PermissionsList = _DesktopModulePermissions.ToList();
                }
                return _PermissionsList;
            }
        }

        #endregion

        #region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Permissions Collection
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DesktopModulePermissionCollection Permissions
        {
            get
            {
                //First Update Permissions in case they have been changed
                UpdatePermissions();

                //Return the DesktopModulePermissions
                return _DesktopModulePermissions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and Sets the Id of the PortalDesktopModule
        /// </summary>
        /// -----------------------------------------------------------------------------
        public int PortalDesktopModuleID
        {
            get
            {
                return _PortalDesktopModuleID;
            }
            set
            {
                int oldValue = _PortalDesktopModuleID;
                _PortalDesktopModuleID = value;
                if (_DesktopModulePermissions == null || oldValue != value)
                {
                    GetDesktopModulePermissions();
                }
            }
        }

        #endregion

        #region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the DesktopModulePermissions from the Data Store
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void GetDesktopModulePermissions()
        {
            _DesktopModulePermissions = new DesktopModulePermissionCollection(DesktopModulePermissionController.GetDesktopModulePermissions(PortalDesktopModuleID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Parse the Permission Keys used to persist the Permissions in the ViewState
        /// </summary>
        /// <param name="Settings">A string array of settings</param>
        /// -----------------------------------------------------------------------------
        private DesktopModulePermissionInfo ParseKeys(string[] Settings)
        {
            var objDesktopModulePermission = new DesktopModulePermissionInfo();

            //Call base class to load base properties
            base.ParsePermissionKeys(objDesktopModulePermission, Settings);
            if (String.IsNullOrEmpty(Settings[2]))
            {
                objDesktopModulePermission.DesktopModulePermissionID = -1;
            }
            else
            {
                objDesktopModulePermission.DesktopModulePermissionID = Convert.ToInt32(Settings[2]);
            }
            objDesktopModulePermission.PortalDesktopModuleID = PortalDesktopModuleID;
            return objDesktopModulePermission;
        }

        #endregion

        #region "Protected Methods"

        protected override void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new DesktopModulePermissionInfo(permission);
            objPermission.PortalDesktopModuleID = PortalDesktopModuleID;
            objPermission.RoleID = roleId;
            objPermission.RoleName = roleName;
            objPermission.AllowAccess = allowAccess;
            objPermission.UserID = userId;
            objPermission.DisplayName = displayName;
            _DesktopModulePermissions.Add(objPermission, true);

            //Clear Permission List
            _PermissionsList = null;
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
            //Search DesktopModulePermission Collection for the user 
            bool isMatch = false;
            foreach (DesktopModulePermissionInfo objDesktopModulePermission in _DesktopModulePermissions)
            {
                if (objDesktopModulePermission.UserID == user.UserID)
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
                    if (objPermission.PermissionKey == "DEPLOY")
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
        /// <param name="role">The roleto add</param>
        /// -----------------------------------------------------------------------------
        protected override void AddPermission(ArrayList permissions, RoleInfo role)
        {
            //Search TabPermission Collection for the user 
            if (_DesktopModulePermissions.Cast<DesktopModulePermissionInfo>().Any(p => p.RoleID == role.RoleID))
            {
                return;
            }

            //role not found so add new
            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "DEPLOY")
                {
                    AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the permissions from the Database
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override ArrayList GetPermissions()
        {
            return PermissionController.GetPermissionsByPortalDesktopModule();
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

                //Load DesktopModuleId
                if (myState[1] != null)
                {
                    PortalDesktopModuleID = Convert.ToInt32(myState[1]);
                }

                //Load DesktopModulePermissions
                if (myState[2] != null)
                {
                    _DesktopModulePermissions = new DesktopModulePermissionCollection();
                    string state = Convert.ToString(myState[2]);
                    if (!String.IsNullOrEmpty(state))
                    {
                        //First Break the String into individual Keys
                        string[] permissionKeys = state.Split(new[] { "##" }, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] Settings = key.Split('|');
                            _DesktopModulePermissions.Add(ParseKeys(Settings));
                        }
                    }
                }
            }
        }

        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            _DesktopModulePermissions.Remove(permissionID, roleID, userID);
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
            var allStates = new object[3];

            //Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            //Save the DesktopModule Id
            allStates[1] = PortalDesktopModuleID;

            //Persist the DesktopModulePermisisons
            var sb = new StringBuilder();
            if (_DesktopModulePermissions != null)
            {
                bool addDelimiter = false;
                foreach (DesktopModulePermissionInfo objDesktopModulePermission in _DesktopModulePermissions)
                {
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }
                    sb.Append(BuildKey(objDesktopModulePermission.AllowAccess,
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
        /// returns whether or not the derived grid supports Deny permissions
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return true;
        }

        #endregion

        #region "Public Methods"

        public void ResetPermissions()
        {
            GetDesktopModulePermissions();
            _PermissionsList = null;
        }

        public override void GenerateDataGrid()
        {
        }

        #endregion
    }
}
