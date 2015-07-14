namespace ClientDependency.Core.Controls
{
    /// <summary>
    /// A control used to specify a Css folder dependency
    /// </summary>
    public class CssFolderInclude : DependencyFolderInclude
    {
        private const string SearchPattern = "*.css";

        public CssFolderInclude() : base()
        {
            FileSearchPattern = SearchPattern;
        }

        public CssFolderInclude(string folderVirtualPath) : base(folderVirtualPath)
        {
            FileSearchPattern = SearchPattern;         
        }

        protected override ClientDependencyType DependencyType
        {
            get { return ClientDependencyType.Css; }
        }        
        
    }
}