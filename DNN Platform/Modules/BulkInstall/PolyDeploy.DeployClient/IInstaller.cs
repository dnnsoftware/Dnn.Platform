namespace PolyDeploy.DeployClient
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IInstaller
    {
        Task<string> StartSessionAsync(DeployInput options);

        Task<Session> GetSessionAsync(DeployInput options, string sessionId);

        Task UploadPackageAsync(DeployInput options, string sessionId, Stream encryptedPackage, string packageName);

        Task InstallPackagesAsync(DeployInput options, string sessionId);
    }
}