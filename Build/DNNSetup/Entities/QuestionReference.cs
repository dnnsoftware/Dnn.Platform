using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    public class QuestionReference
    {
        [DataMember(Name = "postId")]
        public int PostId { get; set; }

        [DataMember(Name = "parentId")]
        public int ParentId { get; set; }

        [DataMember(Name = "approved")]
        public bool Approved { get; set; }

        [DataMember(Name = "contentTitle")]
        public string ContentTitle { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }
    }
}
