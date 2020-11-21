using System;
using System.Collections.Generic;
using System.IO;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Frosting;
using Cake.NuGet;

public class Program : IFrostingStartup
{
    public static int Main(string[] args)
    {
        // Create the host.
        var host = new CakeHostBuilder()
            .WithArguments(args)
            .UseStartup<Program>()
            .Build();

        // Run the host.
        return host.Run();
    }

    public void Configure(ICakeServices services)
    {
        services.UseContext<Context>();
        services.UseLifetime<Lifetime>();
        services.UseWorkingDirectory("..");

        // from https://github.com/cake-build/cake/discussions/2931
        var module = new NuGetModule(new CakeConfiguration(new Dictionary<string, string>()));
        module.Register(services);
        
        services.UseTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.0.1"));
        services.UseTool(new Uri("nuget:?package=Microsoft.TestPlatform&version=16.8.0"));
        services.UseTool(new Uri("nuget:?package=NUnitTestAdapter&version=2.3.0"));
    }
}
