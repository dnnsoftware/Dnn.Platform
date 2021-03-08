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
            context.MSBuild(context.DnnSolutionPath, settings => settings.WithTarget("Clean"));

            var buildSettings = new MSBuildSettings().SetConfiguration(context.BuildConfiguration)
                .SetPlatformTarget(PlatformTarget.MSIL)
                .WithTarget("Rebuild")
                .SetMaxCpuCount(4)
                .WithProperty("SourceLinkCreate", "true");
            context.MSBuild(context.DnnSolutionPath, buildSettings);
        }
    }
}
