using System;
using PolyDeploy.DeployClient;
using Spectre.Cli;

var app = new CommandApp<DeployCommand>();
return app.Run(args);
