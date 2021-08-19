﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Frosting;

    /// <summary>A cake task to compile the platform and create a localdb database.</summary>
    [Dependency(typeof(CleanArtifacts))]
    [Dependency(typeof(UpdateDnnManifests))]
    [Dependency(typeof(CreateInstall))]
    [Dependency(typeof(CreateUpgrade))]
    [Dependency(typeof(CreateDeploy))]
    [Dependency(typeof(CreateSymbols))]
    [Dependency(typeof(CreateDatabase))]
    public sealed class BuildWithDatabase : FrostingTask<Context>
    {
    }
}
