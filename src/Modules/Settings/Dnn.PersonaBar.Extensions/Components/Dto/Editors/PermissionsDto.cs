#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.DTO;
using Dnn.PersonaBar.Library.Helper;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    public class PermissionsDto : Permissions
    {
        public PermissionsDto(bool needDefinitions) : base(needDefinitions)
        {
        }

        protected override void LoadPermissionDefinitions()
        {
            foreach (PermissionInfo permission in PermissionController.GetPermissionsByPortalDesktopModule())
            {
                PermissionDefinitions.Add(new Permission()
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