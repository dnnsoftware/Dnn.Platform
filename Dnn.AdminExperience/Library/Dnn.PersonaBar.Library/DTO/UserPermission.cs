// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.DTO
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class UserPermission
    {
        public UserPermission()
        {
            this.Permissions = new List<Permission>();
        }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "permissions")]
        public IList<Permission> Permissions { get; set; }
    }
}
