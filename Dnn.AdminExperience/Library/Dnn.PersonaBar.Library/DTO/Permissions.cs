// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Library.Helper;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    [DataContract]
    public abstract class Permissions
    {
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        protected Permissions()
            : this(null, false)
        {
        }

        protected Permissions(IPermissionDefinitionService permissionDefinitionService)
            : this(permissionDefinitionService, false)
        {
        }

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        protected Permissions(bool needDefinitions)
            : this(null, needDefinitions)
        {
        }

        protected Permissions(IPermissionDefinitionService permissionDefinitionService, bool needDefinitions)
        {
            this.PermissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
            this.RolePermissions = new List<RolePermission>();
            this.UserPermissions = new List<UserPermission>();

            if (needDefinitions)
            {
                this.PermissionDefinitions = new List<Permission>();
                this.LoadPermissionDefinitions();
                this.EnsureDefaultRoles();
            }
        }

        [DataMember(Name = "permissionDefinitions")]
        public IList<Permission> PermissionDefinitions { get; set; }

        [DataMember(Name = "rolePermissions")]
        public IList<RolePermission> RolePermissions { get; set; }

        [DataMember(Name = "userPermissions")]
        public IList<UserPermission> UserPermissions { get; set; }

        [IgnoreDataMember]
        protected IPermissionDefinitionService PermissionDefinitionService { get; }

        protected abstract void LoadPermissionDefinitions();
    }
}
