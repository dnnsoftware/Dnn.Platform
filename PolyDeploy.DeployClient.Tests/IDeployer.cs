namespace PolyDeploy.DeployClient.Tests
{
    using System.Threading.Tasks;

    public interface IDeployer
    {
        Task StartAsync();
    }
}