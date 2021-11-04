namespace PolyDeploy.DeployClient
{
    using System.Threading.Tasks;

    public class Deployer : IDeployer
    {
        private readonly IRenderer renderer;
        private readonly IPackageFileSource packageFileSource;
        private readonly IInstaller installer;
        private readonly IEncryptor encryptor;

        public Deployer(IRenderer renderer, IPackageFileSource packageFileSource, IInstaller installer, IEncryptor encryptor)
        {
            this.renderer = renderer;
            this.packageFileSource = packageFileSource;
            this.installer = installer;
            this.encryptor = encryptor;
        }

        public async Task StartAsync(DeployInput options)
        {
            var packageFiles = this.packageFileSource.GetPackageFiles();
            this.renderer.RenderListOfFiles(packageFiles);

            var sessionId = await this.installer.StartSessionAsync(options);

            foreach (var packageFile in packageFiles)
            {
                using var packageFileStream = this.packageFileSource.GetFileStream(packageFile);
                using var encryptedPackageStream = await this.encryptor.GetEncryptedStream(options, packageFileStream);
                await this.installer.UploadPackageAsync(options, sessionId, encryptedPackageStream, packageFile);
            }
        }
    }
}