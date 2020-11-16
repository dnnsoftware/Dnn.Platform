// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Services.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using DotNetNuke.Security.Roles;

    [DataContract]
    public class UserRoleDto
    {
        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "roleId")]
        public int RoleId { get; set; }

        [DataMember(Name = "startTime")]
        public DateTime StartTime { get; set; }

        [DataMember(Name = "expiresTime")]
        public DateTime ExpiresTime { get; set; }

        [DataMember(Name = "allowExpired")]
        public bool AllowExpired { get; set; }

        [DataMember(Name = "allowDelete")]
        public bool AllowDelete { get; set; }
    }
}
