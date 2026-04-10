// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.IO;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to crete the Deploy package.</summary>
    [IsDependentOn(typeof(PreparePackaging))]
    [IsDependentOn(typeof(OtherPackages))]
    public sealed class CreateDeploy : FrostingTask<Context>
    {
        private static readonly string[] IncludeAll = ["**/*",];

        /// <inheritdoc />
        public override void Run(Context context)
        {
            context.CreateDirectory(context.ArtifactsDir);
            var packageZip = context.ArtifactsDir + context.File($"DNN_Platform_{context.GetBuildNumber()}_Deploy.zip");

            var deployDir = context.Directory("./DotNetNuke/");
            context.MoveDirectory(context.WebsiteDir, deployDir);
            var files = context.GetFilesByPatterns(deployDir, IncludeAll, context.PackagingPatterns.InstallExclude);
            files.Add(context.GetFilesByPatterns(deployDir, context.PackagingPatterns.InstallInclude));
            context.Zip(string.Empty, packageZip, files);
            context.AddFilesToZip(packageZip, "./Build/Deploy", context.GetFiles("./Build/Deploy/*"), append: true);
            context.MoveDirectory(deployDir, context.WebsiteDir);
        }
    }
}
