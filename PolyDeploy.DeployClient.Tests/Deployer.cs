namespace PolyDeploy.DeployClient.Tests
{
    using System.Threading.Tasks;

    public class Deployer : IDeployer
    {

        private readonly IRenderer renderer;
        private readonly IPackageFileSource packageFileSource;

        public Deployer(IRenderer renderer, IPackageFileSource packageFileSource)
        {
            this.renderer = renderer;
            this.packageFileSource = packageFileSource;
        }

        public async Task StartAsync()
        {
            var packageFiles = await this.packageFileSource.GetPackageFiles();
            await this.renderer.RenderListOfFiles(packageFiles);
        }
    }
}