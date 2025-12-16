// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    public class FolderPermissionsGrid : PermissionsGrid
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected FolderPermissionCollection FolderPermissions;

        private static readonly string[] PermissionKeySeparator = ["##",];

        private string folderPath = string.Empty;
        private List<PermissionInfoBase> permissionsList;
        private bool refreshGrid;
        private IList<PermissionInfo> systemFolderPermissions;

        /// <summary>Gets the Permission Collection.</summary>
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

        /// <summary>Gets or sets the path of the Folder.</summary>
        public string FolderPath
        {
            get
            {
                return this.folderPath;
            }

            set
            {
                this.folderPath = value;
                this.refreshGrid = true;
                this.GetFolderPermissions();
            }
        }

        /// <inheritdoc/>
        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this.permissionsList == null && this.FolderPermissions != null)
                {
                    this.permissionsList = this.FolderPermissions.ToList();
                }

                return this.permissionsList;
            }
        }

        /// <inheritdoc/>
        protected override bool RefreshGrid
        {
            get
            {
                return this.refreshGrid;
            }
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
        }

        /// <summary>Gets the TabPermissions from the Data Store.</summary>
        protected virtual void GetFolderPermissions()
        {
            this.FolderPermissions = new FolderPermissionCollection(FolderPermissionController.GetFolderPermissionsCollectionByFolder(this.PortalId, this.FolderPath));
            this.permissionsList = null;
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
            this.permissionsList = null;
        }

        /// <inheritdoc />
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
                        this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, user.UserID, user.DisplayName, true);
                    }
                }
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return !IsImplicitRole(role.PortalID, role.RoleID);
        }

        /// <inheritdoc />
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

        /// <inheritdoc/>
        protected override bool IsFullControl(PermissionInfo permissionInfo)
        {
            return (permissionInfo.PermissionKey == "WRITE") && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <inheritdoc/>
        protected override bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return permissionInfo.PermissionKey == "READ";
        }

        /// <inheritdoc />
        protected override ArrayList GetPermissions()
        {
            ArrayList perms = PermissionController.GetPermissionsByFolder();
            this.systemFolderPermissions = perms.Cast<PermissionInfo>().ToList();
            return perms;
        }

        /// <inheritdoc />
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
                    this.folderPath = Convert.ToString(myState[1], CultureInfo.InvariantCulture);
                }

                // Load FolderPermissions
                if (myState[2] != null)
                {
                    this.FolderPermissions = new FolderPermissionCollection();
                    string state = Convert.ToString(myState[2], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.FolderPermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this.FolderPermissions.Remove(permissionID, roleID, userID);

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

        /// <inheritdoc />
        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return this.IsSystemFolderPermission(permissionInfo);
        }

        private static bool IsImplicitRole(int portalId, int roleId)
        {
            return FolderPermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        private static void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;

            if (item.ItemType is ListItemType.Item or ListItemType.AlternatingItem or ListItemType.SelectedItem)
            {
                var roleId = int.Parse(((DataRowView)item.DataItem)[0].ToString(), CultureInfo.InvariantCulture);
                if (IsImplicitRole(PortalSettings.Current.PortalId, roleId))
                {
                    if (item.Controls.Cast<Control>().Last().Controls[0] is ImageButton actionImage)
                    {
                        actionImage.Visible = false;
                    }
                }
            }
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
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
                objFolderPermission.FolderPermissionID = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objFolderPermission.FolderPath = this.FolderPath;
            return objFolderPermission;
        }

        private bool IsPermissionAlwaysGrantedToAdmin(PermissionInfo permissionInfo)
        {
            return this.IsSystemFolderPermission(permissionInfo);
        }

        private bool IsSystemFolderPermission(IPermissionDefinitionInfo permissionInfo)
        {
            return this.systemFolderPermissions.Any((IPermissionDefinitionInfo pi) => pi.PermissionId == permissionInfo.PermissionId);
        }
    }
}
