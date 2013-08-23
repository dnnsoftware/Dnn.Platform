using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNukeSetup.Entities
{
    [DataContract]
    public class IdeaResponse
    {

        /// <summary>
        /// The content item PK value. 
        /// </summary>
        [DataMember(Name = "Idea")]
        public IdeaInfo Idea { get; set; }
    }

    [DataContract]
    public class IdeaInfo
    {

        /// <summary>
        /// The content item PK value. 
        /// </summary>
        [DataMember(Name = "contentItemId")]
        public int ContentItemId { get; set; }

        [DataMember(Name = "ideaId")]
        public int IdeaId { get; set; }

    }

}
