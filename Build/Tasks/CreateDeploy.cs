// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.IO;
    using System.Linq;

    using Cake.Common.IO;
    using Cake.Frosting;
    using Dnn.CakeUtils;

    /// <summary>A cake task to crete the Deploy package.</summary>
    [Dependency(typeof(PreparePackaging))]
    [Dependency(typeof(OtherPackages))]
    public sealed class CreateDeploy : FrostingTask<Context>
    {
        /// <inheritdoc/>
        public override void Run(Context context)
        {
            context.CreateDirectory(context.ArtifactsFolder);
            var packageZip = string.Format(
                context.ArtifactsFolder + "DNN_Platform_{0}_Deploy.zip",
                context.GetBuildNumber());

            const string deployFolder = "./DotNetNuke/";
            var deployDir = context.Directory(deployFolder);
            Directory.Move(context.WebsiteDir.Path.FullPath, deployDir.Path.FullPath);
            var files = context.GetFilesByPatterns(deployFolder, new[] { "**/*" }, context.PackagingPatterns.InstallExclude);
            files.Add(context.GetFilesByPatterns(deployFolder, context.PackagingPatterns.InstallInclude));
            context.Zip(string.Empty, packageZip, files);
            Dnn.CakeUtils.Compression.AddFilesToZip(packageZip, "./Build/Deploy", context.GetFiles("./Build/Deploy/*"), append: true);
            Directory.Move(deployDir.Path.FullPath, context.WebsiteDir.Path.FullPath);
        }
    }
}
