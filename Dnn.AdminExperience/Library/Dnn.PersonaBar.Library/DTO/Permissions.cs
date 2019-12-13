// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dnn.PersonaBar.Library.Helper;

namespace Dnn.PersonaBar.Library.DTO
{
    [DataContract]
    public abstract class Permissions
    {
        [DataMember(Name = "permissionDefinitions")]
        public IList<Permission> PermissionDefinitions { get; set; }

        [DataMember(Name = "rolePermissions")]
        public IList<RolePermission> RolePermissions { get; set; }

        [DataMember(Name = "userPermissions")]
        public IList<UserPermission> UserPermissions { get; set; }
        protected abstract void LoadPermissionDefinitions();

        protected Permissions() : this(false)
        {            
        }

        protected Permissions(bool needDefinitions)
        {
            RolePermissions = new List<RolePermission>();
            UserPermissions = new List<UserPermission>();

            if (needDefinitions)
            {
                PermissionDefinitions = new List<Permission>();
                this.LoadPermissionDefinitions();
                this.EnsureDefaultRoles();
            }
        }
    }
}
