// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;

    using Dnn.PersonaBar.Library.DTO;
    using Dnn.PersonaBar.Library.Helper;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;

    public class PagePermissions : Permissions
    {
        public PagePermissions(bool needDefinitions) : base(needDefinitions)
        {
            foreach (var role in PermissionProvider.Instance().ImplicitRolesForPages(PortalSettings.Current.PortalId))
            {
                this.EnsureRole(role, true, true);
            }
        }

        protected override void LoadPermissionDefinitions()
        {
            foreach (PermissionInfo permission in PermissionController.GetPermissionsByTab())
            {
                this.PermissionDefinitions.Add(new Permission
                {
                    PermissionId = permission.PermissionID,
                    PermissionName = permission.PermissionName,
                    FullControl = PermissionHelper.IsFullControl(permission),
                    View = PermissionHelper.IsViewPermisison(permission)
                });
            }
        }
    }
}
