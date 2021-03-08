// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Frosting;

    /// <summary>A cake task to build the platform.</summary>
    [Dependency(typeof(SetVersion))]
    [Dependency(typeof(UpdateDnnManifests))]
    [Dependency(typeof(ResetDatabase))]
    [Dependency(typeof(PreparePackaging))]
    [Dependency(typeof(OtherPackages))]
    public sealed class BuildToTempFolder : FrostingTask<Context>
    {
    }
}
