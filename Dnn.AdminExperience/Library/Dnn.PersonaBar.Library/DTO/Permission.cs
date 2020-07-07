// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.DTO
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Permission
    {
        [DataMember(Name = "permissionId")]
        public int PermissionId { get; set; }

        [DataMember(Name = "permissionName")]
        public string PermissionName { get; set; }

        [DataMember(Name = "permissionKey")]
        public string PermissionKey { get; set; }

        [DataMember(Name = "permissionCode")]
        public string PermissionCode { get; set; }

        [DataMember(Name = "fullControl")]
        public bool FullControl { get; set; }

        [DataMember(Name = "view")]
        public bool View { get; set; }

        [DataMember(Name = "allowAccess")]
        public bool AllowAccess { get; set; }
    }
}
