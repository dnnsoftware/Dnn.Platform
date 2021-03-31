// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.Tools.MSBuild;
    using Cake.Frosting;

    using DotNetNuke.Build;

    /// <summary>A cake task to compile the platform.</summary>
    [Dependency(typeof(CleanWebsite))]
    [Dependency(typeof(RestoreNuGetPackages))]
    public sealed class Build : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            var cleanSettings = new MSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .WithTarget("Clean")
                .SetMaxCpuCount(0)
                .EnableBinaryLogger("clean.binlog")
                .SetNoConsoleLogger(context.IsRunningInCI);
            context.MSBuild(context.DnnSolutionPath, cleanSettings);

            var buildSettings = new MSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .SetPlatformTarget(PlatformTarget.MSIL)
                .WithTarget("Rebuild")
                .SetMaxCpuCount(0)
                .WithProperty("SourceLinkCreate", "true")
                .EnableBinaryLogger("rebuild.binlog")
                .SetNoConsoleLogger(context.IsRunningInCI);
            context.MSBuild(context.DnnSolutionPath, buildSettings);
        }
    }
}
