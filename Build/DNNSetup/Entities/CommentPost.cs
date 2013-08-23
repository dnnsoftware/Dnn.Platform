using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    [DataContract]
    public class CommentResponse
    {
        /// <summary>
        /// The Content Item the comment is being posted to.
        /// </summary>
        [DataMember(Name = "Comment")]
        public CommentPost Comment { get; set; }

    }

    [DataContract]
    public class CommentPost
    {
        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        /// <summary>
        /// The Content Item the comment is being posted to.
        /// </summary>
        [DataMember(Name = "CommentId")]
        public int CommentId { get; set; }

        /// <summary>
        /// The encoded HTML contents of the comment.
        /// </summary>
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        // unused, defaults to 5
        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }

        // unused, don't know what it is for!
        [DataMember(Name = "context")]
        public int[] Context { get; set; }

        [DataMember(Name = "journalTypeId")]
        public int JournalTypeId { get; set; }

        [DataMember(Name = "objectKey")]
        public string ObjectKey { get; set; }

    }

}
