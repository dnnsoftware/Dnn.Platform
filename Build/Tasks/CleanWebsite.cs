// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Common.IO;
using Cake.Frosting;

using DotNetNuke.Build;

/// <summary>A cake task to clean the website directory.</summary>
public sealed class CleanWebsite : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        context.CleanDirectory(context.WebsiteDir);
    }
}
