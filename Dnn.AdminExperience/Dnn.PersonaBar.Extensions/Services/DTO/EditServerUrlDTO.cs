namespace Dnn.PersonaBar.Extensions.Services.Dto
{
    using System.Runtime.Serialization;

    [DataContract]
    public class EditServerUrlDTO
    {
        [DataMember(Name = "serverId")]
        public int ServerId { get; set; }

        [DataMember(Name = "newUrl")]
        public string NewUrl { get; set; }
    }
}
