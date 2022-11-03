namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;
    using System.IO;

    public interface IPackageFileSource
    {
        IReadOnlyCollection<string> GetPackageFiles(string path);

        Stream GetFileStream(string fileName);
    }
}