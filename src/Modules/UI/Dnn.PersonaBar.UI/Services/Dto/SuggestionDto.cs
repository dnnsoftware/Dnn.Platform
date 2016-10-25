using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Security.Roles;

namespace Dnn.PersonaBar.UI.Services.DTO
{
    [DataContract]
    public class SuggestionDto
    {  
        [DataMember(Name = "value")]
        public int Value { get; set; }

        [DataMember(Name = "label")]
        public string Label { get; set; }
    }
}