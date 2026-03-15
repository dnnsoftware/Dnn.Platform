// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Frosting;
    using Cake.Json;
    using Dnn.CakeUtils;

    /// <summary>A cake task to prepare for packaging (by building the platform and copying files).</summary>
    [IsDependentOn(typeof(CopyWebsite))]
    [IsDependentOn(typeof(Build))]
    [IsDependentOn(typeof(CopyWebConfig))]
    [IsDependentOn(typeof(CopyWebsiteBinFolder))]
    public sealed class PreparePackaging : FrostingTask<Context>
    {
        private static readonly string[] SampleModuleArtifactsPattern = ["SampleModules/*.zip",];

        /// <inheritdoc />
        public override void Run(Context context)
        {
            context.PackagingPatterns = context.DeserializeJsonFromFile<PackagingPatterns>("./Build/Tasks/packaging.json");

            // Various fixes
            context.CopyFile(
                "./DNN Platform/Components/DataAccessBlock/bin/Microsoft.ApplicationBlocks.Data.dll",
                context.WebsiteFolder + "bin/Microsoft.ApplicationBlocks.Data.dll");
            context.CopyFiles(
                "./DNN Platform/Components/Lucene.Net.Contrib/bin/Lucene.Net.Contrib.Analyzers.*",
                context.WebsiteFolder + "bin/");
            context.CopyFile(
                "./DNN Platform/Library/bin/PetaPoco.dll",
                context.WebsiteFolder + "bin/PetaPoco.dll");

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
