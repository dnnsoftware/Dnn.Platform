// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build
{
    using System;

    using Cake.AzurePipelines.Module;
    using Cake.Frosting;

    /// <summary>Runs the build process.</summary>
    public class Program
    {
        /// <summary>Runs the build process.</summary>
        /// <param name="args">The arguments from the command line.</param>
        /// <returns>A status code.</returns>
        public static int Main(string[] args)
        {
            return new CakeHost()
                .UseContext<Context>()
                .UseLifetime<Lifetime>()
                .UseWorkingDirectory("..")
                .UseModule<AzurePipelinesModule>()
                .InstallTool(new Uri("nuget:?package=GitVersion.CommandLine&version=5.0.1"))
                .InstallTool(new Uri("nuget:?package=Microsoft.TestPlatform&version=16.8.0"))
                .InstallTool(new Uri("nuget:?package=NUnitTestAdapter&version=2.3.0"))
                .InstallTool(new Uri("nuget:?package=NuGet.CommandLine&version=5.8.0"))
                .Run(args);
        }
    }
}
