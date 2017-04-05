using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dnn.PersonaBar.Library.Dto
{
    [DataContract]
    public class TimeZoneDto
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        [DataMember(Name = "baseUtcOffset")]
        public string BaseUtcOffset { get; set; }

        [DataMember(Name = "currentUtcOffset")]
        public string CurrentUtcOffset { get; set; }
    }
}
