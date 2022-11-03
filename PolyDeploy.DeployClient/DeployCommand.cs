namespace PolyDeploy.DeployClient
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Spectre.Console.Cli;
    using Spectre.Console;

    public class DeployCommand : AsyncCommand<DeployInput>
    {
        private readonly IRenderer renderer;
        private readonly IPackageFileSource packageFileSource;
        private readonly IInstaller installer;
        private readonly IEncryptor encryptor;
        private readonly IDelayer delayer;
        private readonly IFileSystem fileSystem;

        public DeployCommand(IRenderer renderer, IPackageFileSource packageFileSource, IInstaller installer, IEncryptor encryptor, IDelayer delayer, IFileSystem fileSystem)
        {
            this.renderer = renderer;
            this.packageFileSource = packageFileSource;
            this.installer = installer;
            this.encryptor = encryptor;
            this.delayer = delayer;
            this.fileSystem = fileSystem;
        }

        public DeployCommand()
            : this(
                new Renderer(AnsiConsole.Console),
                new PackageFileSource(new FileSystem()),
                new Installer(new HttpClient(), new Stopwatch()),
                new Encryptor(),
                new Delayer(),
                new FileSystem())
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input)
        {
            var deployer = new Deployer(
                this.renderer,
                this.packageFileSource,
                this.installer,
                this.encryptor,
                this.delayer);
            var exitCode = await deployer.StartAsync(input);
            return (int)exitCode;
        }

        public override ValidationResult Validate(CommandContext context, DeployInput settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.PackagesDirectoryPath) && !this.fileSystem.Directory.Exists(settings.PackagesDirectoryPath))
            {
                return ValidationResult.Error("--packages-directory must be a valid path");
            }

            if (!Uri.TryCreate(settings.TargetUri, UriKind.Absolute, out _))
            {
                return ValidationResult.Error("--target-uri must be a valid URI");
            }

            if (settings.InstallationStatusTimeout < 0)
            {
                return ValidationResult.Error("--installation-status-timeout must be non-negative");
            }

            return ValidationResult.Success();
        }
    }
}
