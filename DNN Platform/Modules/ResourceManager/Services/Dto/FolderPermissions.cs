// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Services.Dto
{
    using System.Linq;
    using Dnn.Modules.ResourceManager.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;

    public class FolderPermissions : Permissions
    {
        public FolderPermissions() : base(false)
        {
        }
        public FolderPermissions(bool needDefinitions) : base(needDefinitions)
        {
            foreach (var role in PermissionProvider.Instance().ImplicitRolesForFolders(PortalSettings.Current.PortalId))
            {
                this.EnsureRole(role, true, true);
            }
        }

        public FolderPermissions(bool needDefinitions, FolderPermissionCollection permissions) : base(needDefinitions)
        {
            foreach (var role in PermissionProvider.Instance().ImplicitRolesForFolders(PortalSettings.Current.PortalId))
            {
                this.EnsureRole(role, true, true);
            }
            foreach (FolderPermissionInfo permission in permissions)
            {
                if (permission.UserID != Null.NullInteger)
                {
                    this.AddUserPermission(permission);
                }
                else
                {
                    this.AddRolePermission(permission);
                }
                this.RolePermissions =
                        this.RolePermissions.OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                this.UserPermissions = this.UserPermissions.OrderBy(p => p.DisplayName).ToList();
            }
        }

        protected override void LoadPermissionDefinitions()
        {
            foreach (PermissionInfo permission in PermissionController.GetPermissionsByFolder())
            {
                this.PermissionDefinitions.Add(new Permission
                {
                    PermissionId = permission.PermissionID,
                    PermissionName = permission.PermissionName,
                    FullControl = PermissionHelper.IsFullControl(permission),
                    View = PermissionHelper.IsViewPermission(permission)
                });
            }
        }
    }
}
