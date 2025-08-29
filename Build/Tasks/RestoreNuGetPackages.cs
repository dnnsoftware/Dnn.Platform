// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.Tools.MSBuild;
    using Cake.Frosting;

    /// <summary>A cake task to restore the NuGet packages for the solution.</summary>
    public sealed class RestoreNuGetPackages : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            context.MSBuild(
                context.DnnSolutionPath,
                new MSBuildSettings { Target = "restore", Properties = { { "RestorePackagesConfig", ["true"] }, }, });
        }
    }
}
