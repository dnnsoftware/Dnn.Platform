namespace Dnn.PersonaBar.Pages.Components.Prompt.Models
{
    public class PageModelBase
    {
        public int TabId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int ParentId { get; set; }
        public string Skin { get; set; }
        public string Path { get; set; }
        public bool IncludeInMenu { get; set; }
        public bool IsDeleted { get; set; }

        #region Command Links
        public string __TabId => $"get-page {TabId}";

        public string __ParentId => $"list-pages --parentid {ParentId}";

        public string __IncludeInMenu => $"list-pages --visible{(IncludeInMenu ? "" : " false")}";

        public string __IsDeleted => $"list-pages --deleted{(IsDeleted ? "" : " false")}";

        #endregion

        #region constructors
        public PageModelBase()
        {
        }
        public PageModelBase(DotNetNuke.Entities.Tabs.TabInfo tab)
        {
            Name = tab.TabName;
            ParentId = tab.ParentId;
            Path = tab.TabPath;
            TabId = tab.TabID;
            Skin = tab.SkinSrc;
            Title = tab.Title;
            IncludeInMenu = tab.IsVisible;
            IsDeleted = tab.IsDeleted;
        }
        #endregion
    }
}