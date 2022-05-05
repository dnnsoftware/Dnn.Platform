namespace PolyDeploy.DeployClient
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class Deployer : IDeployer
    {
        private readonly IRenderer renderer;
        private readonly IPackageFileSource packageFileSource;
        private readonly IInstaller installer;
        private readonly IEncryptor encryptor;
        private readonly IDelayer delayer;

        public Deployer(IRenderer renderer, IPackageFileSource packageFileSource, IInstaller installer, IEncryptor encryptor, IDelayer delayer)
        {
            this.renderer = renderer;
            this.packageFileSource = packageFileSource;
            this.installer = installer;
            this.encryptor = encryptor;
            this.delayer = delayer;
        }

        public async Task StartAsync(DeployInput options)
        {
            this.renderer.Welcome();

            var packageFiles = this.packageFileSource.GetPackageFiles();
            this.renderer.RenderListOfFiles(packageFiles);

            var sessionId = await this.installer.StartSessionAsync(options);

            var uploads = packageFiles.Select(file => (file, this.UploadPackage(sessionId, file, options)));
            await this.renderer.RenderFileUploadsAsync(uploads);

            _ = this.installer.InstallPackagesAsync(options, sessionId);

            var hasRenderedOverview = false;
            while (true)
            {
                var session = await this.installer.GetSessionAsync(options, sessionId);
                if (session?.Responses != null)
                {
                    if (!hasRenderedOverview)
                    {
                        this.renderer.RenderInstallationOverview(session.Responses);
                        hasRenderedOverview = true;
                    }

                    this.renderer.RenderInstallationStatus(session.Responses);
                }

                if (session?.Status == SessionStatus.Complete)
                {
                    break;
                }
                
                await this.delayer.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task UploadPackage(string sessionId, string packageFile, DeployInput options)
        {
            await using var packageFileStream = this.packageFileSource.GetFileStream(packageFile);
            await using var encryptedPackageStream = await this.encryptor.GetEncryptedStream(options, packageFileStream);

            await this.installer.UploadPackageAsync(options, sessionId, encryptedPackageStream, packageFile);
        }
    }
}