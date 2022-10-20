namespace PolyDeploy.DeployClient
{
    using System.IO.Abstractions;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Spectre.Console.Cli;
    using Spectre.Console;

    public class DeployCommand : AsyncCommand<DeployInput>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input)
        {
            var deployer = new Deployer(
                new Renderer(AnsiConsole.Console),
                new PackageFileSource(new FileSystem()),
                new Installer(new HttpClient(), new Stopwatch()),
                new Encryptor(),
                new Delayer());
            var exitCode = await deployer.StartAsync(input);
            return (int)exitCode;
        }

        public override ValidationResult Validate(CommandContext context, DeployInput settings)
        {
            return settings.Validate();
        }
    }
}
