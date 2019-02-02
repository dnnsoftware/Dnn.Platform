namespace ClientDependency.Core
{
    /// <summary>
    /// Represents a JS file
    /// </summary>
    public class JavascriptFile : BasicFile
    {
        public JavascriptFile(string filePath)
            : base(ClientDependencyType.Javascript)
        {
            FilePath = filePath;
        }
    }
}