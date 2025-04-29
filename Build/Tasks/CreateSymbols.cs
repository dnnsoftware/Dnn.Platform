// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.IO;
using Cake.Frosting;

using Dnn.CakeUtils;

/// <summary>A cake task to create the Symbols package.</summary>
[IsDependentOn(typeof(PreparePackaging))]
[IsDependentOn(typeof(OtherPackages))]
public sealed class CreateSymbols : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        context.CreateDirectory(context.ArtifactsFolder);
        var packageZip = $"{context.ArtifactsFolder}DNN_Platform_{context.GetBuildNumber()}_Symbols.zip";
        context.Zip("./Build/Symbols/", packageZip, context.GetFiles("./Build/Symbols/*"));

        // Fix for WebUtility symbols missing from bin folder
        context.CopyFiles(
            context.GetFiles("./DNN Platform/DotNetNuke.WebUtility/bin/DotNetNuke.WebUtility.*"),
            context.WebsiteFolder + "bin/");
        var files = context.GetFilesByPatterns(
            context.WebsiteFolder,
            context.PackagingPatterns.SymbolsInclude,
            context.PackagingPatterns.SymbolsExclude);
        var resFile = context.ZipToBytes(context.WebsiteFolder.TrimEnd('/'), files);
        context.AddBinaryFileToZip(packageZip, resFile, "Resources.zip", true);
    }
}
