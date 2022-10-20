using System;
using System.Threading.Tasks;
using PolyDeploy.DeployClient;
using Spectre.Console.Cli;

try
{
    var app = new CommandApp<DeployCommand>();
    return app.Run(args);
}
catch
{
    await Task.Delay(TimeSpan.FromSeconds(1));
    return 1;
}
