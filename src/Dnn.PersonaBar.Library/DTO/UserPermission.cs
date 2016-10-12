﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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