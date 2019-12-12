using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class PageItem
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "childCount")]
        public int ChildrenCount { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "publishStatus")]
        public string PublishStatus { get; set; }

        [DataMember(Name = "parentId")]
        public int ParentId { get; set; }

        [DataMember(Name = "level")]
        public int Level { get; set; }

        [DataMember(Name = "tabpath")]
        public string TabPath { get; set; }

        [DataMember(Name = "isspecial")]
        public bool IsSpecial { get; set; }

        [DataMember(Name = "pageType")]
        public string PageType { get; set; }

        [DataMember(Name = "canViewPage")]
        public bool CanViewPage { get; set; }

        [DataMember(Name = "canManagePage")]
        public bool CanManagePage { get; set; }

        [DataMember(Name = "canAddPage")]
        public bool CanAddPage { get; set; }

        [DataMember(Name = "canAdminPage")]
        public bool CanAdminPage { get; set; }

        [DataMember(Name = "canCopyPage")]
        public bool CanCopyPage { get; set; }

        [DataMember(Name = "canDeletePage")]
        public bool CanDeletePage { get; set; }

        [DataMember(Name = "canAddContentToPage")]
        public bool CanAddContentToPage { get; set; }

        [DataMember(Name = "canNavigateToPage")]
        public bool CanNavigateToPage { get; set; }

        [DataMember(Name = "lastModifiedOnDate")]
        public string LastModifiedOnDate { get; set; }

        [DataMember(Name = "friendlyLastModifiedOnDate")]
        public string FriendlyLastModifiedOnDate { get; set; }

        [DataMember(Name = "publishDate")]
        public string PublishDate { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "tabOrder")]
        public int TabOrder { get; set; }
    }
}