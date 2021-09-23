namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;
    using System.IO.Abstractions;

    public class PackageFileSource : IPackageFileSource
    {
        private readonly IFileSystem fileSystem;

        public PackageFileSource(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IReadOnlyCollection<string> GetPackageFiles()
        {
            return fileSystem.Directory.GetFiles(".", "*.zip");
        }
    }
}