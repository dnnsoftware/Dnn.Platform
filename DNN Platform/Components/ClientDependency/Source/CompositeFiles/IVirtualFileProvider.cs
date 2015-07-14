namespace ClientDependency.Core.CompositeFiles
{
    public interface IVirtualFileProvider
    {
        bool FileExists(string virtualPath);
        IVirtualFile GetFile(string virtualPath);
    }
}