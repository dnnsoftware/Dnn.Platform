namespace PolyDeploy.DeployClient
{
    using System.Collections.Generic;

    public interface IPackageFileSource
    {
        IReadOnlyCollection<string> GetPackageFiles();
    }
}