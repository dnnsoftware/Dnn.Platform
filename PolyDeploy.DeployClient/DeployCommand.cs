namespace PolyDeploy.DeployClient
{
    using System.IO.Abstractions;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Spectre.Cli;
    using Spectre.Console;

    public class DeployCommand : AsyncCommand<DeployInput>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input)
        {
            var deployer = new Deployer(
                new Renderer(AnsiConsole.Console),
                new PackageFileSource(new FileSystem()),
                new Installer(new HttpClient()),
                new Encryptor(),
                new Delayer());
            await deployer.StartAsync(input);
            return 0;
        }
    }
}
