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
    public class ProfileVisibilityDto
    {
        public ProfileVisibilityDto()
        {

        }

        public ProfileVisibilityDto(ProfileVisibility visibility)
        {
            this.VisibilityMode = visibility.VisibilityMode;
            this.RoleVisibilities = visibility.RoleVisibilities.Select(r => r.RoleID).ToList();
            this.RelationshipVisibilities = visibility.RelationshipVisibilities.Select(r => r.RelationshipId).ToList();
        }

        [DataMember(Name = "visibilityMode")]
        public UserVisibilityMode VisibilityMode { get; set; }

        [DataMember(Name = "roleVisibilities")]
        public IList<int> RoleVisibilities { get; set; } = new List<int>();

        [DataMember(Name = "relationshipVisibilities")]
        public IList<int> RelationshipVisibilities { get; set; } = new List<int>();
    }
}
