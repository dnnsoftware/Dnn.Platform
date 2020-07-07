// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

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
