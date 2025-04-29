// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;

using Dnn.CakeUtils;

/// <summary>A cake task to copy the built website to the dev site's directory.</summary>
public sealed class CopyToDevSite : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        context.CleanDirectory(context.Settings.WebsitePath);
        var files = context.GetFilesByPatterns(context.WebsiteFolder, new[] { "**/*" }, context.PackagingPatterns.InstallExclude);
        files.Add(context.GetFilesByPatterns(context.WebsiteFolder, context.PackagingPatterns.InstallInclude));
        context.Information("Copying {0} files to {1}", files.Count, context.Settings.WebsitePath);
        context.CopyFiles(files, context.Settings.WebsitePath, true);
    }
}
