// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Frosting;
    using Dnn.CakeUtils;

    /// <summary>A cake task to build the platform.</summary>
    [IsDependentOn(typeof(SetVersion))]
    [IsDependentOn(typeof(UpdateDnnManifests))]
    [IsDependentOn(typeof(ResetDatabase))]
    [IsDependentOn(typeof(PreparePackaging))]
    [IsDependentOn(typeof(OtherPackages))]
    public sealed class BuildToTempFolder : FrostingTask<Context>
    {
        private static readonly string[] SampleModuleArtifactsPattern = ["SampleModules/*.zip",];

        /// <inheritdoc/>
        public override void Run(Context context)
        {
            if (context.Settings.CopySampleProjects)
            {
                context.Information("Copying Sample Projects to Temp Folder");
                var files = context.GetFilesByPatterns(context.ArtifactsFolder, SampleModuleArtifactsPattern);
                foreach (var file in files)
                {
                    var destination = context.File(System.IO.Path.Combine(context.WebsiteFolder, "Install", "Module", file.GetFilename().ToString()));
                    context.CopyFile(file, destination);
                    context.Information($"  Copied {file.GetFilename()} to {destination}");
                }
            }
        }
    }
}
