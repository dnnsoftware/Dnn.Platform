// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Library.DTO
{
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
