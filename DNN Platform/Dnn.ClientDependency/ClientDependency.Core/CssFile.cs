namespace ClientDependency.Core
{
    /// <summary>
    /// Represents a CSS file
    /// </summary>
    public class CssFile : BasicFile
    {
        public CssFile(string filePath)
            : base(ClientDependencyType.Css)
        {
            FilePath = filePath;
        }
    }
}