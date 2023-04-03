// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.IO;
    using Cake.Common.Tools.DotNet;
    using Cake.Common.Tools.DotNet.Pack;
    using Cake.Common.Tools.DotNet.Publish;
    using Cake.Frosting;

    /// <summary>A cake task to generate the BulkInstall package.</summary>
    [IsDependentOn(typeof(Build))]
    public sealed class PackageBulkInstall : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            const string BulkInstallProject = "./DotNetNuke.BulkInstall/DotNetNuke.BulkInstall.DeployClient/DotNetNuke.BulkInstall.DeployClient.csproj";
            var buildDir = context.TempDir.Path.Combine("DotNetNuke.BulkInstall");

            context.DotNetPublish(
                BulkInstallProject,
                new DotNetPublishSettings
                {
                    Configuration = context.BuildConfiguration,
                    NoBuild = true,
                    OutputDirectory = buildDir,
                });

            context.Zip(buildDir, context.ArtifactsDir.Path.CombineWithFilePath($"DotNetNuke.BulkInstall.DeployClient_{context.GetBuildNumber()}.zip"));

            context.DotNetPack(
                BulkInstallProject,
                new DotNetPackSettings
                {
                    Configuration = context.BuildConfiguration,
                    NoBuild = true,
                    OutputDirectory = context.ArtifactsDir,
                });
        }
    }
}
