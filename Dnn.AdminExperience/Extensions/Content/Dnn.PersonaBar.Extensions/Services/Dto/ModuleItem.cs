using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class ModuleItem
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "friendlyName")]
        public string FriendlyName { get; set; }

        [DataMember(Name = "editContentUrl")]
        public string EditContentUrl { get; set; }

        [DataMember(Name="editSettingUrl")]
        public string EditSettingUrl { get; set; }

        [DataMember(Name = "includedInCopy")]
        public bool? IncludedInCopy { get; set; }

        [DataMember(Name = "copyType")]
        public ModuleCopyType? CopyType { get; set; }

        [DataMember(Name = "isPortable")]
        public bool? IsPortable { get; set; }

        [DataMember(Name = "allTabs")]
        public bool AllTabs { get; set; }
    }
}