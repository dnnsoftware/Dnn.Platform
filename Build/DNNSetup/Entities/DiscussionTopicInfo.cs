using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    [DataContract]
    public class DiscussionTopicInfo
    {
        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        [DataMember(Name = "topicId")]
        public int TopicId { get; set; }

        [DataMember(Name = "groupId")]
        public int GroupId { get; set; }

        [DataMember(Name = "viewCount")]
        public int ViewCount { get; set; }

        [DataMember(Name = "approved")]
        public bool Approved { get; set; }

        [DataMember(Name = "approvedDate")]
        public DateTime ApprovedDate { get; set; }

        [DataMember(Name = "deleted")]
        public bool Deleted { get; set; }

        [DataMember(Name = "closed")]
        public bool Closed { get; set; }

        [DataMember(Name = "closedDate")]
        public DateTime ClosedDate { get; set; }

        [DataMember(Name = "protected")]
        public bool Protected { get; set; }

        [DataMember(Name = "protectedDate")]
        public DateTime ProtectedDate { get; set; }

        [DataMember(Name = "pinned")]
        public bool Pinned { get; set; }

        [DataMember(Name = "pinnedDate")]
        public DateTime PinnedDate { get; set; }
    }
}
