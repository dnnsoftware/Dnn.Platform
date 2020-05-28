// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Library.DTO
{
    [DataContract]
    public class RolePermission
    {
        [DataMember(Name = "roleId")]
        public int RoleId { get; set; }

        [DataMember(Name = "roleName")]
        public string RoleName { get; set; }

        [DataMember(Name = "permissions")]
        public IList<Permission> Permissions { get; set; }

        [DataMember(Name = "locked")]
        public bool Locked { get; set; }

        [DataMember(Name = "default")]
        public bool IsDefault { get; set; }

        public RolePermission()
        {
            Permissions = new List<Permission>();
        }
    }
}
