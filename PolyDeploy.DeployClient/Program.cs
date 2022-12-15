using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PolyDeploy.DeployClient;
using Spectre.Console;
using Spectre.Console.Cli;

try
{
    var services = new ServiceCollection();
    services.AddTransient(_ => AnsiConsole.Console);
    services.AddHttpClient();
    services.AddTransient<IFileSystem, FileSystem>();
    services.AddTransient<IRenderer, Renderer>();
    services.AddTransient<IPackageFileSource, PackageFileSource>();
    services.AddTransient<IInstaller, Installer>();
    services.AddTransient<IStopwatch, Stopwatch>();
    services.AddTransient<IEncryptor, Encryptor>();
    services.AddTransient<IDelayer, Delayer>();
    services.AddTransient<IDeployer, Deployer>();
    
    var registrar = new ServiceCollectionRegistrar(services);
    var app = new CommandApp<DeployCommand>(registrar);
    return app.Run(args);
}
catch
{
    await Task.Delay(TimeSpan.FromSeconds(1));
    return 1;
}
