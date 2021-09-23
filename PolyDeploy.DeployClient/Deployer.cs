namespace PolyDeploy.DeployClient
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
            var packageFiles = this.packageFileSource.GetPackageFiles();
            this.renderer.RenderListOfFiles(packageFiles);
        }
    }
}