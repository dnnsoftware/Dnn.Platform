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

    public class DeployCommand : AsyncCommand<DeployCommand.DeployInput>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, DeployInput input)
        {
            await new Deployer(new Renderer(AnsiConsole.Console), new PackageFileSource(new FileSystem())).StartAsync();
            return 0;
        }

        public class DeployInput : CommandSettings
        {

        }
    }
}
