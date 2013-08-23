using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    [DataContract]
    public class IdeaReference
    {
        [DataMember(Name = "ideaId")]
        public int IdeaId { get; set; }

        [DataMember(Name = "groupId")]
        public int GroupId { get; set; }

        [DataMember(Name = "portalId")]
        public int PortalID { get; set; }

        [DataMember(Name = "contentTitle")]
        public string ContentTitle { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "authorized")]
        public bool Authorized { get; set; }

        [DataMember(Name = "initialVotes")]
        public int InitialVotes { get; set; }

        [DataMember(Name = "selectedTypeId")]
        public int SelectedTypeId { get; set; }

        [DataMember(Name = "selectedCategoryId")]
        public int SelectedCategoryId { get; set; }
    }
}
