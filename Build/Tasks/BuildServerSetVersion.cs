// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Frosting;

/// <summary>A cake task to update the build number in CI.</summary>
[IsDependentOn(typeof(SetVersion))]
public sealed class BuildServerSetVersion : FrostingTask<Context>
{
    /// <inheritdoc/>
    public override void Run(Context context)
    {
        Console.WriteLine($"##vso[build.updatebuildnumber]{context.Version.FullSemVer}.{context.BuildId}");
    }
}
