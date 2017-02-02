namespace Dnn.PersonaBar.Prompt.Models
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
        public string __TabId
        {
            get
            {
                return string.Format("get-page {0}", TabId);
            }
        }
        public string __ParentId
        {
            get
            {
                return string.Format("list-pages --parentid {0}", ParentId);
            }
        }
        public string __IncludeInMenu
        {
            get
            {
                return string.Format("list-pages --visible{0}", (IncludeInMenu ? "" : " false"));
            }
        }
        public string __IsDeleted
        {
            get
            {
                return string.Format("list-pages --deleted{0}", (IsDeleted ? "" : " false"));
            }
        }
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