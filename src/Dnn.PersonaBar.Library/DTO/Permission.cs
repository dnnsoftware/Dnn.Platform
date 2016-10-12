#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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

        [DataMember(Name = "fullControl")]
        public bool FullControl { get; set; }

        [DataMember(Name = "view")]
        public bool View { get; set; }

        [DataMember(Name = "allowAccess")]
        public bool AllowAccess { get; set; }
    }
}