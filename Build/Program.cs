using System;

using Cake.Frosting;

public class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<Context>()
            .UseLifetime<Lifetime>()
            .UseWorkingDirectory("..")
            .UseTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.0.1"))
            .UseTool(new Uri("nuget:?package=Microsoft.TestPlatform&version=16.8.0"))
            .UseTool(new Uri("nuget:?package=NUnitTestAdapter&version=2.3.0"))
            .UseTool(new Uri("nuget:?package=NuGet.CommandLine&version=5.8.0"))
            .Run(args);
    }
}
