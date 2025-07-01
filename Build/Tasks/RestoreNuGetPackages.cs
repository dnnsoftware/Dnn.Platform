// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.Tools.NuGet;
    using Cake.Common.Tools.NuGet.Restore;
    using Cake.Common.Tools.VSWhere;
    using Cake.Common.Tools.VSWhere.Latest;
    using Cake.Core;
    using Cake.Frosting;

    /// <summary>A cake task to restore the NuGet packages for the solution.</summary>
    public sealed class RestoreNuGetPackages : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            // need to manually look up MSBuild path until https://github.com/nuget/home/issues/14349 is resolved
            var msbuildPath = context.VSWhereLatest(new VSWhereLatestSettings
            {
                Products = "*",
                Requires = "Microsoft.Component.MSBuild",
                ReturnProperty = null,
                ArgumentCustomization = args => args.Append("-find MSBuild/**/bin/MSBuild.exe"),
            });
            context.NuGetRestore(context.DnnSolutionPath, new NuGetRestoreSettings { MSBuildPath = msbuildPath.GetParent(), });
        }
    }
}
