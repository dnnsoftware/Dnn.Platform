// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class RelationshipDto
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "targetUserId")]
        public int TargetUserId { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
