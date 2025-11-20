// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Security.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Security.Models;

    public abstract class PermissionsGridController : Controller
    {
        protected const string PermissionTypeGrant = "True";
        protected const string PermissionTypeDeny = "False";
        protected const string PermissionTypeNull = "Null";

        protected readonly IClientResourceController clientResourceController;

        protected PermissionsGridController(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController;
        }

        private List<PermissionInfo> permissions;
        private IList<RoleInfo> roles;

        protected List<UserModel> UserPermissions { get; set; }

        protected List<RoleModel> RolePermissions { get; set; }

        protected virtual List<PermissionInfoBase> PermissionsList
        {
            get { return null; }
        }

        protected List<PermissionInfo> Permissions
        {
            get { return this.permissions; }
        }

        protected virtual bool RefreshGrid
        {
            get { return false; }
        }

        protected int PortalId
        {
            get
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                return Globals.IsHostTab(portalSettings.ActiveTab.TabID)
                    ? Null.NullInteger
                    : portalSettings.PortalId;
            }
        }

        protected int UnAuthUsersRoleId => int.Parse(Globals.glbRoleUnauthUser);

        protected int AllUsersRoleId => int.Parse(Globals.glbRoleAllUsers);

        protected int AdministratorRoleId => PortalController.Instance.GetCurrentPortalSettings().AdministratorRoleId;

        protected int RegisteredUsersRoleId => PortalController.Instance.GetCurrentPortalSettings().RegisteredRoleId;

        public void BindData()
        {
            this.permissions = this.GetPermissions();
            this.BindRolesGrid();
            this.BindUsersGrid();
        }

        [HttpPost]
        public virtual ActionResult UpdatePermissions(PermissionsUpdateModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.Json(new { success = false, message = "Invalid model state" });
            }

            try
            {
                foreach (var permission in model.Permissions)
                {
                    if (permission.IsRolePermission)
                    {
                        this.UpdateRolePermission(permission);
                    }
                    else
                    {
                        this.UpdateUserPermission(permission);
                    }
                }

                return this.Json(new { success = true });
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public virtual ActionResult AddRole(int roleId)
        {
            try
            {
                var role = this.GetRole(roleId);
                if (role == null)
                {
                    return this.Json(new { success = false, message = "Role not found" });
                }

                this.AddPermission(this.permissions, role);

                return this.Json(new { success = true });
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public virtual ActionResult AddUser(int userId)
        {
            try
            {
                var user = UserController.GetUserById(this.PortalId, userId);
                if (user == null)
                {
                    return this.Json(new { success = false, message = "User not found" });
                }

                this.AddPermission(this.permissions, user);

                return this.Json(new { success = true });
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, message = ex.Message });
            }
        }

        protected abstract List<PermissionInfo> GetPermissions();

        protected abstract void AddPermission(List<PermissionInfo> permissions, RoleInfo role);

        protected abstract void AddPermission(List<PermissionInfo> permissions, UserInfo user);

        protected abstract void UpdateRolePermission(PermissionUpdateModel permission);

        protected abstract void UpdateUserPermission(PermissionUpdateModel permission);

        protected virtual IEnumerable<RoleGroupInfo> GetRoleGroups()
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var groups = RoleController.GetRoleGroups(portalSettings.PortalId).Cast<RoleGroupInfo>();

            // Add default items
            var allGroups = new List<RoleGroupInfo>
            {
                new RoleGroupInfo { RoleGroupID = -2, RoleGroupName = Localization.GetString("AllRoles") },
                new RoleGroupInfo { RoleGroupID = -1, RoleGroupName = Localization.GetString("GlobalRoles") },
            };

            allGroups.AddRange(groups);
            return allGroups;
        }

        protected virtual List<RoleInfo> GetRolesComboBox(int roleGroupId = -1)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var roles = new List<RoleInfo>();

            // Get roles based on group filter
            if (roleGroupId > -2)
            {
                roles.AddRange(RoleController.Instance.GetRoles(
                    portalSettings.PortalId,
                    r => r.RoleGroupID == roleGroupId &&
                    r.SecurityMode != SecurityMode.SocialGroup &&
                    r.Status == RoleStatus.Approved));
            }
            else
            {
                roles.AddRange(RoleController.Instance.GetRoles(
                    portalSettings.PortalId,
                    r => r.SecurityMode != SecurityMode.SocialGroup &&
                    r.Status == RoleStatus.Approved));
            }

            // Add system roles if global roles selected
            if (roleGroupId < 0)
            {
                roles.Add(new RoleInfo
                {
                    RoleID = this.UnAuthUsersRoleId,
                    RoleName = Globals.glbRoleUnauthUserName,
                });

                roles.Add(new RoleInfo
                {
                    RoleID = this.AllUsersRoleId,
                    RoleName = Globals.glbRoleAllUsersName,
                });
            }

            // Ensure administrator role is always included
            this.EnsureRole(roles, portalSettings.AdministratorRoleId);

            // Ensure registered users role is included
            this.EnsureRole(roles, portalSettings.RegisteredRoleId);

            return roles;
        }

        protected virtual List<UserInfo> GetUsers()
        {
            var users = new List<UserInfo>();

            if (this.PermissionsList == null)
            {
                return users;
            }

            foreach (var permission in this.PermissionsList)
            {
                if (!Null.IsNull(permission.UserID) && users.Cast<UserInfo>().All(u => u.UserID != permission.UserID))
                {
                    var user = new UserInfo
                    {
                        UserID = permission.UserID,
                        Username = permission.Username,
                        DisplayName = permission.DisplayName,
                    };
                    users.Add(user);
                }
            }

            return users;
        }

        protected virtual RoleInfo GetRole(int roleId)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            if (roleId == this.AllUsersRoleId)
            {
                return new RoleInfo
                {
                    RoleID = this.AllUsersRoleId,
                    RoleName = Globals.glbRoleAllUsersName,
                    PortalID = portalSettings.PortalId,
                };
            }

            if (roleId == this.UnAuthUsersRoleId)
            {
                return new RoleInfo
                {
                    RoleID = this.UnAuthUsersRoleId,
                    RoleName = Globals.glbRoleUnauthUserName,
                    PortalID = portalSettings.PortalId,
                };
            }

            return RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
        }

        protected virtual void EnsureRole(List<RoleInfo> roles, int roleId)
        {
            if (roles.Cast<RoleInfo>().All(r => r.RoleID != roleId))
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleId);
                if (role != null)
                {
                    roles.Add(role);
                }
            }
        }

        protected virtual string BuildPermissionKey(
            bool allowAccess,
            int permissionId,
            int objectPermissionId,
            int roleId,
            string roleName,
            int userId = -1,
            string displayName = null)
        {
            var key = allowAccess ? PermissionTypeGrant : PermissionTypeDeny;

            key += $"|{permissionId}|{(objectPermissionId > -1 ? objectPermissionId.ToString() : string.Empty)}";
            key += $"|{roleName}|{roleId}|{userId}|{displayName}";

            return key;
        }

        protected virtual bool GetEnabled(PermissionInfo permission, RoleInfo role, int column)
        {
            // Base implementation - override in derived classes for specific logic
            return true;
        }

        protected virtual bool GetEnabled(PermissionInfo permission, UserInfo user, int column)
        {
            // Base implementation - override in derived classes for specific logic
            return true;
        }

        protected virtual bool IsFullControl(PermissionInfo permissionInfo)
        {
            return false;
        }

        protected virtual bool IsViewPermisison(PermissionInfo permissionInfo)
        {
            return false;
        }

        protected virtual string GetPermissionState(PermissionInfo permission, RoleInfo role, string defaultState = null)
        {
            if (this.PermissionsList == null)
            {
                return defaultState ?? PermissionTypeNull;
            }

            foreach (var existingPermission in this.PermissionsList)
            {
                if (
                    existingPermission.PermissionID == permission.PermissionID &&
                    existingPermission.RoleID == role.RoleID)
                {
                    return existingPermission.AllowAccess ? PermissionTypeGrant : PermissionTypeDeny;
                }
            }

            return defaultState ?? PermissionTypeNull;
        }

        protected virtual string GetPermissionState(PermissionInfo permission, UserInfo user, string defaultState = null)
        {
            if (this.PermissionsList == null)
            {
                return defaultState ?? PermissionTypeNull;
            }

            foreach (var existingPermission in this.PermissionsList)
            {
                if (
                    existingPermission.PermissionID == permission.PermissionID &&
                    existingPermission.UserID == user.UserID)
                {
                    return existingPermission.AllowAccess ? PermissionTypeGrant : PermissionTypeDeny;
                }
            }

            return defaultState ?? PermissionTypeNull;
        }

        protected virtual bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            // to maintain backward compatibility the base implementation must always call the simple parameterless version of this method
            return false;
        }

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is granted, otherwise <see langword="false"/>.</returns>
        protected virtual bool GetPermission(PermissionInfo objPerm, UserInfo user, int column)
        {
            return Convert.ToBoolean(this.GetPermission(objPerm, user, column, PermissionTypeDeny));
        }

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>The permission state (one of <see cref="PermissionTypeGrant"/>, <see cref="PermissionTypeDeny"/> or <paramref name="defaultState"/>).</returns>
        protected virtual string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
        {
            var stateKey = defaultState;
            if (this.PermissionsList != null)
            {
                foreach (var permission in this.PermissionsList)
                {
                    if (permission.PermissionID == objPerm.PermissionID && permission.UserID == user.UserID)
                    {
                        if (permission.AllowAccess)
                        {
                            stateKey = PermissionTypeGrant;
                        }
                        else
                        {
                            stateKey = PermissionTypeDeny;
                        }

                        break;
                    }
                }
            }

            return stateKey;
        }

        protected virtual bool GetPermission(PermissionInfo objPerm, RoleInfo role, int column)
        {
            return Convert.ToBoolean(this.GetPermission(objPerm, role, column, PermissionTypeDeny));
        }

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>The permission state (one of <see cref="PermissionTypeGrant"/>, <see cref="PermissionTypeDeny"/> or <paramref name="defaultState"/>).</returns>
        protected virtual string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string stateKey = defaultState;
            if (this.PermissionsList != null)
            {
                foreach (PermissionInfoBase permission in this.PermissionsList)
                {
                    if (permission.PermissionID == objPerm.PermissionID && permission.RoleID == role.RoleID)
                    {
                        if (permission.AllowAccess)
                        {
                            stateKey = PermissionTypeGrant;
                        }
                        else
                        {
                            stateKey = PermissionTypeDeny;
                        }

                        break;
                    }
                }
            }

            return stateKey;
        }

        private void BindRolesGrid()
        {
            this.RolePermissions = new List<RoleModel>();

            /*
            this.dtRolePermissions.Columns.Clear();
            this.dtRolePermissions.Rows.Clear();

            // Add Roles Column
            this.dtRolePermissions.Columns.Add(new DataColumn("RoleId"));

            // Add Roles Column
            this.dtRolePermissions.Columns.Add(new DataColumn("RoleName"));

            for (int i = 0; i <= this.permissions.Count - 1; i++)
            {
                var permissionInfo = (PermissionInfo)this.permissions[i];

                // Add Enabled Column
                this.dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName + "_Enabled"));

                // Add Permission Column
                this.dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName));
            }
            */

            this.GetRoles();

            // this.UpdateRolePermissions();
            for (int i = 0; i <= this.roles.Count - 1; i++)
            {
                var role = this.roles[i];
                var roleModel = new RoleModel();
                roleModel.RoleId = role.RoleID;
                roleModel.RoleName = Localization.LocalizeRole(role.RoleName);
                roleModel.Permissions = new Dictionary<string, PermissionModel>();
                int j;
                for (j = 0; j <= this.permissions.Count - 1; j++)
                {
                    var permModel = new PermissionModel();
                    PermissionInfo objPerm;
                    objPerm = (PermissionInfo)this.permissions[j];
                    roleModel.Permissions.Add(objPerm.PermissionName, permModel);
                    permModel.Enabled = this.GetEnabled(objPerm, role, j + 1);
                    permModel.Locked = !permModel.Enabled;
                    if (this.SupportsDenyPermissions(objPerm))
                    {
                        permModel.State = this.GetPermission(objPerm, role, j + 1, PermissionTypeNull);
                    }
                    else
                    {
                        if (this.GetPermission(objPerm, role, j + 1))
                        {
                            permModel.State = PermissionTypeGrant;
                        }
                        else
                        {
                            permModel.State = PermissionTypeNull;
                        }
                    }
                }

                this.RolePermissions.Add(roleModel);
            }
        }

        private void BindUsersGrid()
        {
            /*
            this.dtUserPermissions.Columns.Clear();
            this.dtUserPermissions.Rows.Clear();

            // Add Roles Column
            var col = new DataColumn("UserId");
            this.dtUserPermissions.Columns.Add(col);

            // Add Roles Column
            col = new DataColumn("DisplayName");
            this.dtUserPermissions.Columns.Add(col);
            int i;
            for (i = 0; i <= this.permissions.Count - 1; i++)
            {
                PermissionInfo objPerm;
                objPerm = (PermissionInfo)this.permissions[i];

                // Add Enabled Column
                col = new DataColumn(objPerm.PermissionName + "_Enabled");
                this.dtUserPermissions.Columns.Add(col);

                // Add Permission Column
                col = new DataColumn(objPerm.PermissionName);
                this.dtUserPermissions.Columns.Add(col);
            }

            if (this.userPermissionsGrid != null)
            {
                this.users = this.GetUsers();

                if (this.users.Count != 0)
                {
                    this.userPermissionsGrid.Visible = true;
                    DataRow row;
                    for (i = 0; i <= this.users.Count - 1; i++)
                    {
                        var user = (UserInfo)this.users[i];
                        row = this.dtUserPermissions.NewRow();
                        row["UserId"] = user.UserID;
                        row["DisplayName"] = user.DisplayName;
                        int j;
                        for (j = 0; j <= this.permissions.Count - 1; j++)
                        {
                            PermissionInfo objPerm;
                            objPerm = (PermissionInfo)this.permissions[j];
                            row[objPerm.PermissionName + "_Enabled"] = this.GetEnabled(objPerm, user, j + 1);
                            if (this.SupportsDenyPermissions(objPerm))
                            {
                                row[objPerm.PermissionName] = this.GetPermission(objPerm, user, j + 1, PermissionTypeNull);
                            }
                            else
                            {
                                if (this.GetPermission(objPerm, user, j + 1))
                                {
                                    row[objPerm.PermissionName] = PermissionTypeGrant;
                                }
                                else
                                {
                                    row[objPerm.PermissionName] = PermissionTypeNull;
                                }
                            }
                        }

                        this.dtUserPermissions.Rows.Add(row);
                    }

                    this.userPermissionsGrid.DataSource = this.dtUserPermissions;
                    this.userPermissionsGrid.DataBind();
                }
                else
                {
                    this.dtUserPermissions.Rows.Clear();
                    this.userPermissionsGrid.DataSource = this.dtUserPermissions;
                    this.userPermissionsGrid.DataBind();
                    this.userPermissionsGrid.Visible = false;
                }
            }
            */
        }

        private void GetRoles()
        {
            var checkedRoles = this.GetCheckedRoles();
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            this.roles = RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved && checkedRoles.Contains(r.RoleID));

            if (checkedRoles.Contains(this.UnAuthUsersRoleId))
            {
                this.roles.Add(new RoleInfo { RoleID = this.UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });
            }

            if (checkedRoles.Contains(this.AllUsersRoleId))
            {
                this.roles.Add(new RoleInfo { RoleID = this.AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });
            }

            // Administrators Role always has implicit permissions, then it should be always in
            this.EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId));

            // Show also default roles
            this.EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.RegisteredRoleId));
            this.EnsureRole(new RoleInfo { RoleID = this.AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });

            this.roles.Reverse();

            // this.roles.Sort(new RoleComparer());
        }

        private IEnumerable<int> GetCheckedRoles()
        {
            if (this.PermissionsList == null)
            {
                return new List<int>();
            }

            return this.PermissionsList.Select(r => r.RoleID).Distinct();
        }

        private void EnsureRole(RoleInfo role)
        {
            if (this.roles.Cast<RoleInfo>().All(r => r.RoleID != role.RoleID))
            {
                this.roles.Add(role);
            }
        }
    }
}
