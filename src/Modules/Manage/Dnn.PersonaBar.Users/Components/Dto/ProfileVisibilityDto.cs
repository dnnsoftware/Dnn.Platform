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
    public class ProfileVisibilityDto
    {
        [DataMember(Name = "visibilityMode")]
        public UserVisibilityMode VisibilityMode { get; set; }

        [DataMember(Name = "roleVisibilities")]
        public IList<int> RoleVisibilities { get; set; } = new List<int>();

        [DataMember(Name = "relationshipVisibilities")]
        public IList<int> RelationshipVisibilities { get; set; } = new List<int>();

        public ProfileVisibilityDto()
        {
            
        }

        public ProfileVisibilityDto(ProfileVisibility visibility)
        {
            VisibilityMode = visibility.VisibilityMode;
            RoleVisibilities = visibility.RoleVisibilities.Select(r => r.RoleID).ToList();
            RelationshipVisibilities = visibility.RelationshipVisibilities.Select(r => r.RelationshipId).ToList();
        }
    }
}