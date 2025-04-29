// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

using System;
using System.Linq;

using Cake.Frosting;

/// <summary>A cake task to build the platform.</summary>
[IsDependentOn(typeof(SetVersion))]
[IsDependentOn(typeof(UpdateDnnManifests))]
[IsDependentOn(typeof(ResetDatabase))]
[IsDependentOn(typeof(PreparePackaging))]
[IsDependentOn(typeof(OtherPackages))]
public sealed class BuildToTempFolder : FrostingTask<Context>
{
}
