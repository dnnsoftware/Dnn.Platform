using System;

using Cake.AzurePipelines.Module;
using Cake.Frosting;

public class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<Context>()
            .UseLifetime<Lifetime>()
            .UseWorkingDirectory("..")
            .UseModule<AzurePipelinesModule>()
            .SetToolPath("../tools")
            .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.0.1"))
            .InstallTool(new Uri("nuget:?package=Microsoft.TestPlatform&version=16.8.0"))
            .InstallTool(new Uri("nuget:?package=NUnitTestAdapter&version=2.3.0"))
            .InstallTool(new Uri("nuget:?package=NuGet.CommandLine&version=5.8.0"))
            .Run(args);
    }
}
