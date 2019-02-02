using System.IO;

namespace ClientDependency.Core.CompositeFiles
{
    public interface IVirtualFile
    {
        string Path { get; }
        Stream Open();
    }
}