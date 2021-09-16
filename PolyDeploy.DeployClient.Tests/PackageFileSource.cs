namespace PolyDeploy.DeployClient.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class PackageFileSource : IPackageFileSource
    {
        public Task<IReadOnlyCollection<string>> GetPackageFiles()
        {
            throw new System.NotImplementedException();
        }
    }
}