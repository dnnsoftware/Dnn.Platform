// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to create the Install package.</summary>
    [IsDependentOn(typeof(PreparePackaging))]
    [IsDependentOn(typeof(OtherPackages))]
    public sealed class CreateInstall : FrostingTask<Context>
    {
        private static readonly string[] IncludeAll = ["**/*",];

        /// <inheritdoc/>
        public override void Run(Context context)
        {
            context.CreateDirectory(context.ArtifactsFolder);
            var files = context.GetFilesByPatterns(context.WebsiteFolder, IncludeAll, context.PackagingPatterns.InstallExclude);
            files.Add(context.GetFilesByPatterns(context.WebsiteFolder, context.PackagingPatterns.InstallInclude));
            context.Information("Zipping {0} files for Install zip", files.Count);

            var packageZip = $"{context.ArtifactsFolder}DNN_Platform_{context.GetBuildNumber()}_Install.zip";
            context.Zip(context.WebsiteFolder, packageZip, files);
        }
    }
}
