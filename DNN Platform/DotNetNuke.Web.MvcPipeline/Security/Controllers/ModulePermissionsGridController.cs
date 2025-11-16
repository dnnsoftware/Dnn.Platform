// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Security.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Security.Models;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;

    public class ModulePermissionsGridController : PermissionsGridController
    {
        private bool inheritViewPermissionsFromTab;
        private int moduleId = -1;
        private ModulePermissionCollection modulePermissions;
        private List<PermissionInfoBase> permissionsList;
        private int viewColumnIndex;

        public ModulePermissionsGridController(IClientResourceController clientResourceController) : base(clientResourceController)
        {
            this.TabId = -1;
        }

        public ModulePermissionCollection ModulePermissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                // this.UpdateModulePermissions();
                return this.modulePermissions;
            }
        }

        public bool InheritViewPermissionsFromTab
        {
            get => this.inheritViewPermissionsFromTab;
            set
            {
                this.inheritViewPermissionsFromTab = value;
                this.permissionsList = null;
            }
        }

        public int ModuleId
        {
            get => this.moduleId;
            set
            {
                this.moduleId = value;
                this.GetModulePermissions();
            }
        }

        public int TabId { get; set; }

        protected override List<PermissionInfoBase> PermissionsList
        {
            get
            {
                if (this.permissionsList == null && this.modulePermissions != null)
                {
                    this.permissionsList = this.modulePermissions.ToList();
                }

                return this.permissionsList;
            }
        }

        public ActionResult Index(int tabId, int moduleId, bool inheritViewPermissionsFromTab)
        {
            this.inheritViewPermissionsFromTab = inheritViewPermissionsFromTab;
            this.TabId = tabId;
            this.ModuleId = moduleId;

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.clientResourceController.RegisterScript("~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            this.clientResourceController.RegisterScript("~/js/dnn.permissiongrid.js");

            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css", FileOrder.Css.ResourceCss);

            var script = "var pgm = new dnn.permissionGridManager('ClientID');";
            MvcClientAPI.RegisterStartupScript("ClientID-PermissionGridManager", script);

            this.BindData();

            var model = new ModulePermissionsGridViewModel
            {
                Permissions = this.Permissions,
                Users = this.GetUsers(),
                Roles = this.GetRolesComboBox(),
                RoleGroups = this.GetRoleGroups(),
                ModuleId = this.ModuleId,
                TabId = this.TabId,
                InheritViewPermissionsFromTab = this.InheritViewPermissionsFromTab,
                RolePermissions = this.RolePermissions,
            };

            return this.View(model);
        }

        protected override void AddPermission(List<PermissionInfo> permissions, UserInfo user)
        {
            bool isMatch = this.modulePermissions.Cast<ModulePermissionInfo>()
                            .Any(objModulePermission => objModulePermission.UserID == user.UserID);

            if (!isMatch)
            {
                foreach (PermissionInfo objPermission in permissions)
                {
                    if (objPermission.PermissionKey == "VIEW")
                    {
                        this.AddModulePermission(
                            objPermission,
                            int.Parse(Globals.glbRoleNothing),
                            Null.NullString,
                            user.UserID,
                            user.DisplayName,
                            true);
                    }
                }
            }
        }

        protected override void AddPermission(List<PermissionInfo> permissions, RoleInfo role)
        {
            if (this.modulePermissions.Cast<ModulePermissionInfo>().Any(p => p.RoleID == role.RoleID))
            {
                return;
            }

            foreach (PermissionInfo objPermission in permissions)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddModulePermission(
                        objPermission,
                        role.RoleID,
                        role.RoleName,
                        Null.NullInteger,
                        Null.NullString,
                        true);
                }
            }
        }

        protected override void UpdateRolePermission(PermissionUpdateModel permission)
        {
            var permissionInfo = this.GetPermissionInfo(permission.PermissionId);
            if (this.InheritViewPermissionsFromTab && permissionInfo.PermissionKey == "VIEW")
            {
                return;
            }

            this.RemovePermission(permission.PermissionId, permission.RoleId, Null.NullInteger);

            if (permission.PermissionKey == PermissionTypeGrant)
            {
                var role = this.GetRole(permission.RoleId);
                this.AddModulePermission(
                    permissionInfo,
                    permission.RoleId,
                    role.RoleName,
                    Null.NullInteger,
                    Null.NullString,
                    true);
            }
            else if (permission.PermissionKey == PermissionTypeDeny)
            {
                var role = this.GetRole(permission.RoleId);
                this.AddModulePermission(
                    permissionInfo,
                    permission.RoleId,
                    role.RoleName,
                    Null.NullInteger,
                    Null.NullString,
                    false);
            }
        }

        protected override void UpdateUserPermission(PermissionUpdateModel permission)
        {
            var permissionInfo = this.GetPermissionInfo(permission.PermissionId);
            if (this.InheritViewPermissionsFromTab && permissionInfo.PermissionKey == "VIEW")
            {
                return;
            }

            this.RemovePermission(permission.PermissionId, Null.NullInteger, permission.UserId);

            if (permission.PermissionKey == PermissionTypeGrant || permission.PermissionKey == PermissionTypeDeny)
            {
                var user = UserController.GetUserById(this.PortalId, permission.UserId);
                this.AddModulePermission(
                    permissionInfo,
                    Null.NullInteger,
                    Null.NullString,
                    permission.UserId,
                    user.DisplayName,
                    permission.PermissionKey == PermissionTypeGrant);
            }
        }

        protected void RemovePermission(int permissionID, int roleID, int userID)
        {
            this.modulePermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this.permissionsList = null;
        }

        protected override List<PermissionInfo> GetPermissions()
        {
            var moduleInfo = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
            var permissionController = new PermissionController();
            var permissions = permissionController.GetPermissionsByModule(this.ModuleId, this.TabId).Cast<PermissionInfo>().ToList();

            var permissionList = new List<PermissionInfo>();
            for (int i = 0; i < permissions.Count; i++)
            {
                var permission = (PermissionInfo)permissions[i];
                if (permission.PermissionKey == "VIEW")
                {
                    this.viewColumnIndex = i + 1;
                    permissionList.Add(permission);
                }
                else if (!(moduleInfo.IsShared && moduleInfo.IsShareableViewOnly))
                {
                    permissionList.Add(permission);
                }
            }

            return permissionList;
        }

        protected override bool SupportsDenyPermissions(PermissionInfo permissionInfo)
        {
            return true;
        }

        /// <inheritdoc />
        protected override bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
        {
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = !this.IsImplicitRole(role.PortalID, role.RoleID);
            }

            return enabled;
        }

        /// <inheritdoc />
        protected override bool GetEnabled(PermissionInfo objPerm, UserInfo user, int column)
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
        protected override string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
        {
            string permission;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
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

        /// <inheritdoc />
        protected override string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
        {
            string permission;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
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

        private void AddModulePermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new ModulePermissionInfo(permission)
            {
                ModuleID = this.ModuleId,
                RoleID = roleId,
                RoleName = roleName,
                AllowAccess = allowAccess,
                UserID = userId,
                DisplayName = displayName,
            };
            this.modulePermissions.Add(objPermission, true);
            this.permissionsList = null;
        }

        private void GetModulePermissions()
        {
            this.modulePermissions = new ModulePermissionCollection(
                ModulePermissionController.GetModulePermissions(this.ModuleId, this.TabId));
            this.permissionsList = null;
        }

        private void UpdateModulePermissions()
        {
            // Implementation of permission updates to the database
            foreach (ModulePermissionInfo permission in this.modulePermissions)
            {
                // ModulePermissionController.SaveModulePermission(permission);
            }
        }

        private bool IsImplicitRole(int portalId, int roleId)
        {
            return ModulePermissionController.ImplicitRoles(portalId)
                .Any(r => r.RoleID == roleId);
        }

        private PermissionInfo GetPermissionInfo(int permissionId)
        {
            return this.GetPermissions().Cast<PermissionInfo>()
                .FirstOrDefault(p => p.PermissionID == permissionId);
        }
    }
}
