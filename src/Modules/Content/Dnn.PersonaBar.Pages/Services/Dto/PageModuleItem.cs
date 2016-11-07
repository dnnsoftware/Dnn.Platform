using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class PageModuleItem
    {
        [DataMember(Name = "pageId")]
        public int PageId { get; set; }

        [DataMember(Name = "moduleId")]
        public int ModuleId { get; set; }
    }
}