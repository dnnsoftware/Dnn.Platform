// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using System;

    using Dnn.PersonaBar.Library.Dto;
    using Dnn.PersonaBar.Library.Helper;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;

    using Newtonsoft.Json;

    [JsonObject]
    public class PermissionsDto : Permissions
    {
        /// <summary>Initializes a new instance of the <see cref="PermissionsDto"/> class.</summary>
        /// <param name="needDefinitions">Whether to load the permission definitions.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public PermissionsDto(bool needDefinitions)
            : this(null, needDefinitions)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PermissionsDto"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="needDefinitions">Whether to load the permission definitions.</param>
        public PermissionsDto(IPermissionDefinitionService permissionDefinitionService, bool needDefinitions)
            : base(permissionDefinitionService, needDefinitions)
        {
            foreach (var role in PermissionProvider.Instance().ImplicitRolesForPages(PortalSettings.Current.PortalId))
            {
                this.EnsureRole(role, true, true);
            }
        }

        [JsonConstructor]
        private PermissionsDto()
            : this(null, false)
        {
        }

        [JsonProperty("desktopModuleId")]
        public int DesktopModuleId { get; set; }

        /// <inheritdoc />
        protected override void LoadPermissionDefinitions()
        {
            foreach (var permission in this.PermissionDefinitionService.GetDefinitionsByPortalDesktopModule())
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
