using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class Theme
    {
        [DataMember(Name = "skinSrc")]
        public string SkinSrc { get; set; }

        [DataMember(Name = "containerSrc")]
        public string ContainerSrc { get; set; }
    }
}