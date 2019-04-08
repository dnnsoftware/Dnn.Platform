using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class CopyPermissionsRequest
    {
        [DataMember(Name = "pageId")]
        public int PageId { get; set; }
    }
}