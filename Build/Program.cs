﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build
{
    using System;
    using System.Reflection;

    using Cake.AzurePipelines.Module;
    using Cake.Frosting;

    /// <summary>Runs the build process.</summary>
    public class Program
    {
        /// <summary>The version of the Microsoft.TestPlatform NuGet package.</summary>
        internal const string MicrosoftTestPlatformVersion = "17.4.1";

        /// <summary>The version of the NUnit3TestAdapter NuGet package.</summary>
        internal const string NUnit3TestAdapterVersion = "4.3.1";

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
                .InstallTool(new Uri("dotnet:?package=GitVersion.Tool&version=5.11.1"))
                .InstallTool(new Uri("nuget:?package=Microsoft.TestPlatform&version=" + MicrosoftTestPlatformVersion))
                .InstallTool(new Uri("nuget:?package=NUnit3TestAdapter&version=" + NUnit3TestAdapterVersion))
                .InstallTool(new Uri("nuget:?package=NuGet.CommandLine&version=6.4.0"))
                .Run(args);
        }
    }
}
