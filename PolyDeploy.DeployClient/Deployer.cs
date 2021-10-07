namespace PolyDeploy.DeployClient
{
    using System.Threading.Tasks;

    public class Deployer : IDeployer
    {

        private readonly IRenderer renderer;
        private readonly IPackageFileSource packageFileSource;
        private readonly IInstaller installer;

        public Deployer(IRenderer renderer, IPackageFileSource packageFileSource, IInstaller installer)
        {
            this.renderer = renderer;
            this.packageFileSource = packageFileSource;
            this.installer = installer;
        }

        public async Task StartAsync(DeployInput options)
        {
            var packageFiles = this.packageFileSource.GetPackageFiles();
            this.renderer.RenderListOfFiles(packageFiles);

            await this.installer.StartSessionAsync(options);
        }
    }
}