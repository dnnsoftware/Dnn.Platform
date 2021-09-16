namespace PolyDeploy.DeployClient.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPackageFileSource
    {
        Task<IReadOnlyCollection<string>> GetPackageFiles();
    }
}