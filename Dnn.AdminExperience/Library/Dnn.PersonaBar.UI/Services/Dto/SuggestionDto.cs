using System.Runtime.Serialization;

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
