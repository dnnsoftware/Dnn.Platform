// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.IO;
using Cake.Frosting;

/// <summary>A cake task to copy the <c>release.config</c> to the <c>web.config</c>.</summary>
public sealed class CopyWebConfig : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        context.CopyFile(context.WebsiteFolder + "release.config", context.WebsiteFolder + "web.config");
    }
}
