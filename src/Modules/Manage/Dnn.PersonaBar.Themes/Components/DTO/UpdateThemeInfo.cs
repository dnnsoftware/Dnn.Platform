using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    [Serializable]
    [DataContract]
    public class UpdateThemeInfo
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "token")]
        public string Token { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}