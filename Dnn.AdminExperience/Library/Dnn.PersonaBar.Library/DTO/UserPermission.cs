// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Library.DTO
{
    [DataContract]
    public class UserPermission
    {
        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "permissions")]
        public IList<Permission> Permissions { get; set; }

        public UserPermission()
        {
            Permissions = new List<Permission>();
        }
    }
}
