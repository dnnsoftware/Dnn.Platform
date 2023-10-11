// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto
{
    using System;
    using System.Linq;
    using System.Web;

    using Dnn.Modules.ResourceManager.Components;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides information about folder permissions.</summary>
    public class FolderPermissions : Permissions
    {
        /// <summary>Initializes a new instance of the <see cref="FolderPermissions"/> class.</summary>
        public FolderPermissions()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissions"/> class.</summary>
        /// <param name="needDefinitions">A vlaue indicating whether the permissions definitions need to be loaded.</param>
        /// <param name="permissionService">The permission service.</param>
        public FolderPermissions(bool needDefinitions, IPermissionService permissionService)
            : base(needDefinitions, permissionService)
        {
            foreach (var role in PermissionProvider.Instance().ImplicitRolesForFolders(PortalSettings.Current.PortalId))
            {
                this.EnsureRole(role, true, true);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="FolderPermissions"/> class.</summary>
        /// <param name="needDefinitions">A value indicating whether the permission definitions need to be loaded.</param>
        /// <param name="permissions">A colleciton of folder permissions.</param>
        /// <param name="permissionService">The permission service.</param>
        public FolderPermissions(bool needDefinitions, FolderPermissionCollection permissions, IPermissionService permissionService)
            : base(needDefinitions, permissionService)
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

        /// <inheritdoc/>
        protected override void LoadPermissionDefinitions(IPermissionService permissionService)
        {
            foreach (var permission in permissionService.GetDefinitionsByFolder())
            {
                this.PermissionDefinitions.Add(new Permission
                {
                    PermissionId = permission.PermissionId,
                    PermissionName = permission.PermissionName,
                    FullControl = PermissionHelper.IsFullControl(permission),
                    View = PermissionHelper.IsViewPermission(permission),
                });
            }
        }
    }
}
