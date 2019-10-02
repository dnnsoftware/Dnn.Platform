using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Sites.Components.Dto
{
    [DataContract]
    public class HttpAliasDto
    {
        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "link")]
        public string Link { get; set; }
    }
}