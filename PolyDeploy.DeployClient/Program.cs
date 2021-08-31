using PolyDeploy.DeployClient;
using Spectre.Cli;

var app = new CommandApp<DeployCommand>();
app.Configure(config => config.PropagateExceptions());
return app.Run(args);
