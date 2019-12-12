// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using Dnn.PersonaBar.Library.DTO;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
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
                PermissionDefinitions.Add(new Permission
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
