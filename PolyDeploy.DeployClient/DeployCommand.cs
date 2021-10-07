namespace PolyDeploy.DeployClient
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;

    using IsCI;

    using PolyDeploy.Encryption;

    using Spectre.Cli;
    using Spectre.Console;
    using Spectre.Console.Rendering;
    using ValidationResult = Spectre.Cli.ValidationResult;

    public class DeployCommand : AsyncCommand<DeployInput>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input)
        {
            var deployer = new Deployer(new Renderer(AnsiConsole.Console), new PackageFileSource(new FileSystem()), new Installer());
            await deployer.StartAsync(input);
            return 0;
        }
    }

    public class DeployInput : CommandSettings
    {
        public DeployInput(string targetUri)
        {
            this.TargetUri = targetUri;
        }

        [CommandOption("-u|--target-uri")]
        [Description("The URL of the site to which the packages will be deployed.")]
        public string TargetUri { get; init; }

        public Uri GetTargetUri() => new Uri(this.TargetUri, UriKind.Absolute);

        public override ValidationResult Validate()
        {
            return Uri.TryCreate(this.TargetUri, UriKind.Absolute, out _)
                ? ValidationResult.Error("--target-uri must be a valid URI")
                : ValidationResult.Success();
        }
    }
}
