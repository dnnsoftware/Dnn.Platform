// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.IO;
using Cake.Frosting;

/// <summary>A cake task to copy files into the website's <c>bin</c> directory.</summary>
public sealed class CopyWebsiteBinFolder : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        context.CopyFiles(
            context.GetFiles("./DNN Platform/Website/bin/**/*"),
            context.WebsiteFolder + "bin/",
            true);
    }
}
