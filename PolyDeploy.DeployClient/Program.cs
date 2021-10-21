using System;
using PolyDeploy.DeployClient;
using Spectre.Cli;

try
{
    var app = new CommandApp<DeployCommand>();
    app.Configure(config => config.PropagateExceptions());
    return app.Run(args);
}
catch (Exception exc)
{
    Spectre.Console.AnsiConsole.WriteLine();
    Spectre.Console.AnsiConsole.WriteException(exc);
    return 1;
}