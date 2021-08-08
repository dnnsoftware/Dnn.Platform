﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Tools.NuGet;
    using Cake.Common.Tools.NuGet.Pack;
    using Cake.Core;
    using Cake.Frosting;
    using DotNetNuke.Build;

    /// <summary>A cake task to create the platform's NuGet packages.</summary>
    [Dependency(typeof(PreparePackaging))]
    public sealed class CreateNugetPackages : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            // look for solutions and start building them
            var nuspecFiles = context.GetFiles("./Build/Tools/NuGet/*.nuspec");
            var noSymbolsNuspecFiles = context.GetFiles("./Build/Tools/NuGet/DotNetNuke.WebApi.nuspec");

            context.Information("Found {0} nuspec files.", nuspecFiles.Count);

            // basic nuget package configuration
            var nuGetPackSettings = new NuGetPackSettings
                                    {
                                        Version = context.GetBuildNumber(),
                                        OutputDirectory = @"./Artifacts/",
                                        IncludeReferencedProjects = true,
                                        Symbols = true,
                                        Properties = new Dictionary<string, string> { { "Configuration", "Release" } },
                                        ArgumentCustomization = args => args.Append("-SymbolPackageFormat snupkg"),
                                    };

            nuspecFiles -= noSymbolsNuspecFiles;
            foreach (var spec in nuspecFiles)
            {
                context.Information("Starting to pack: {0}", spec);
                context.NuGetPack(spec.FullPath, nuGetPackSettings);
            }

            nuGetPackSettings.Symbols = false;
            nuGetPackSettings.ArgumentCustomization = null;
            foreach (var spec in noSymbolsNuspecFiles)
            {
                context.Information("Starting to pack: {0}", spec);
                context.NuGetPack(spec.FullPath, nuGetPackSettings);
            }
        }
    }
}
