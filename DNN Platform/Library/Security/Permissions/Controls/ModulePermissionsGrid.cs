// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A permissions grid for module permissions.</summary>
    public class ModulePermissionsGrid : PermissionsGrid
    {
        private static readonly string[] PermissionKeySeparator = ["##",];
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private bool inheritViewPermissionsFromTab;
        private int moduleId = -1;
        private ModulePermissionCollection modulePermissions;
        private List<IPermissionInfo> permissionCollection;
        private int viewColumnIndex;

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ModulePermissionsGrid()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionsGrid"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public ModulePermissionsGrid(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
            this.TabId = -1;
        }

        /// <summary>Gets the ModulePermission Collection.</summary>
        public ModulePermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the ModulePermissions
                return this.modulePermissions;
            }
        }

        /// <summary>Gets or sets a value indicating whether the Module inherits the Page's(Tab's) permissions.</summary>
        public bool InheritViewPermissionsFromTab
        {
            get
            {
                return this.inheritViewPermissionsFromTab;
            }

            set
            {
                this.inheritViewPermissionsFromTab = value;
                this.permissionCollection = null;
            }
        }

        /// <summary>Gets or sets the ID of the Module.</summary>
        public int ModuleID
        {
            get
            {
                return this.moduleId;
            }

            set
            {
                this.moduleId = value;
                if (!this.Page.IsPostBack)
                {
                    this.GetModulePermissions();
                }
            }
        }

        /// <summary>Gets or sets the ID of the Tab associated with this module.</summary>
        public int TabId { get; set; }

        /// <inheritdoc />
        protected override bool SupportsPermissionsAbstractions => true;

        /// <inheritdoc />
        protected override IList<IPermissionInfo> PermissionCollection
        {
            get
            {
                if (this.permissionCollection == null && this.modulePermissions != null)
                {
                    this.permissionCollection = this.modulePermissions.Cast<IPermissionInfo>().ToList();
                }

                return this.permissionCollection;
            }
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += this.RolePermissionsGrid_ItemDataBound;
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
            if (this.modulePermissions.Any((IPermissionInfo mp) => mp.UserId == user.UserID))
            {
                return;
            }

            // user not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, user.UserID, user.DisplayName, true);
                }
            }
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, RoleInfo role)
        {
            // Search TabPermission Collection for the user
            if (this.modulePermissions.Any((IPermissionInfo p) => p.RoleId == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        /// <inheritdoc />
        protected override void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new ModulePermissionInfo(permissionDefinition)
            {
                ModuleID = this.ModuleID,
                RoleName = roleName,
                AllowAccess = allowAccess,
                DisplayName = displayName,
            };
            ((IPermissionInfo)objPermission).RoleId = roleId;
            ((IPermissionInfo)objPermission).UserId = userId;
            this.modulePermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionCollection = null;
        }

        /// <inheritdoc />
        protected override void UpdatePermission(IPermissionDefinitionInfo permission, int roleId, string roleName, string stateKey)
        {
            if (this.InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }

            base.UpdatePermission(permission, roleId, roleName, stateKey);
        }

        /// <inheritdoc />
        protected override void UpdatePermission(IPermissionDefinitionInfo permission, string displayName, int userId, string stateKey)
        {
            if (this.InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }

            base.UpdatePermission(permission, displayName, userId, stateKey);
        }

        /// <inheritdoc />
        protected override bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = !IsImplicitRole(role.PortalID, role.RoleID);
            }

            return enabled;
        }

        /// <inheritdoc />
        protected override bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column)
        {
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = true;
            }

            return enabled;
        }

        /// <inheritdoc />
        protected override string GetPermission(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column, string defaultState)
        {
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                return PermissionTypeNull;
            }

            return role.RoleID == this.AdministratorRoleId
                ? PermissionTypeGrant
                : base.GetPermission(permissionDefinition, role, column, defaultState);
        }

        /// <inheritdoc />
        protected override string GetPermission(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column, string defaultState)
        {
            string permission;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                permission = PermissionTypeNull;
            }
            else
            {
                // Call base class method to handle standard permissions
                permission = base.GetPermission(permissionDefinition, user, column, defaultState);
            }

            return permission;
        }

        /// <inheritdoc />
        protected override IList<IPermissionDefinitionInfo> GetPermissionDefinitions()
        {
            var moduleInfo = ModuleController.Instance.GetModule(this.ModuleID, this.TabId, false);

            var permissions = this.permissionDefinitionService.GetDefinitionsByModule(this.ModuleID, this.TabId).ToList();

            var permissionList = new List<IPermissionDefinitionInfo>();
            for (int i = 0; i <= permissions.Count - 1; i++)
            {
                var permission = (PermissionInfo)permissions[i];
                if (permission.PermissionKey == "VIEW")
                {
                    this.viewColumnIndex = i + 1;
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

        /// <inheritdoc />
        protected override bool IsFullControl(IPermissionDefinitionInfo permissionDefinition)
        {
            return (permissionDefinition.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <inheritdoc />
        protected override bool IsViewPermission(IPermissionDefinitionInfo permissionDefinition)
        {
            return permissionDefinition.PermissionKey == "VIEW";
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

                // Load ModuleID
                if (myState[1] != null)
                {
                    this.ModuleID = Convert.ToInt32(myState[1], CultureInfo.InvariantCulture);
                }

                // Load TabId
                if (myState[2] != null)
                {
                    this.TabId = Convert.ToInt32(myState[2], CultureInfo.InvariantCulture);
                }

                // Load InheritViewPermissionsFromTab
                if (myState[3] != null)
                {
                    this.InheritViewPermissionsFromTab = Convert.ToBoolean(myState[3], CultureInfo.InvariantCulture);
                }

                // Load ModulePermissions
                if (myState[4] != null)
                {
                    this.modulePermissions = new ModulePermissionCollection();
                    string state = Convert.ToString(myState[4], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.modulePermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
            this.modulePermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this.permissionCollection = null;
        }

        /// <inheritdoc />
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
            if (this.modulePermissions != null)
            {
                bool addDelimiter = false;
                foreach (ModulePermissionInfo modulePermission in this.modulePermissions)
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
                        ((IPermissionInfo)modulePermission).PermissionId,
                        modulePermission.ModulePermissionID,
                        ((IPermissionInfo)modulePermission).RoleId,
                        modulePermission.RoleName,
                        ((IPermissionInfo)modulePermission).UserId,
                        modulePermission.DisplayName));
                }
            }

            allStates[4] = sb.ToString();
            return allStates;
        }

        /// <inheritdoc />
        protected override bool SupportsDenyPermissions(IPermissionDefinitionInfo permissionDefinition)
        {
            return true;
        }

        /// <summary>Check if a role is implicit for Module Permissions.</summary>
        private static bool IsImplicitRole(int portalId, int roleId)
        {
            return ModulePermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        /// <summary>Gets the ModulePermissions from the Data Store.</summary>
        private void GetModulePermissions()
        {
            this.modulePermissions = new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(this.ModuleID, this.TabId));
            this.permissionCollection = null;
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private ModulePermissionInfo ParseKeys(string[] settings)
        {
            var objModulePermission = new ModulePermissionInfo();

            // Call base class to load base properties
            this.ParsePermissionKeys((IPermissionInfo)objModulePermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objModulePermission.ModulePermissionID = -1;
            }
            else
            {
                objModulePermission.ModulePermissionID = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objModulePermission.ModuleID = this.ModuleID;
            return objModulePermission;
        }

        private void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
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
    }
}
