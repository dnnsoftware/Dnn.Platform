namespace PolyDeploy.DeployClient
{
    using System.Threading.Tasks;

    public class Installer : IInstaller
    {
        public Task<object> GetSessionAsync(DeployInput options, string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public Task InstallPackagesAsync(DeployInput options, string sessionId)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> StartSessionAsync(DeployInput options)
        {
            throw new System.NotImplementedException();
        }

        public Task UploadPackageAsync(DeployInput options, string sessionId, string packageName)
        {
            throw new System.NotImplementedException();
        }
    }
    public interface IInstaller
    {
        Task<string> StartSessionAsync(DeployInput options);

        Task<object> GetSessionAsync(DeployInput options, string sessionId);

        Task UploadPackageAsync(DeployInput options, string sessionId, string packageName);

        Task InstallPackagesAsync(DeployInput options, string sessionId);
    }
}