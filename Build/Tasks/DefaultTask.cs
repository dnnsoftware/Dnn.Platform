// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build.Tasks;

using Cake.Frosting;

/// <summary>A cake task to build the platform and create packages.</summary>
/// <remarks>This is the default Cake target if no target is supplied.</remarks>
[TaskName("Default")]
[IsDependentOn(typeof(CleanArtifacts))]
[IsDependentOn(typeof(UpdateDnnManifests))]
[IsDependentOn(typeof(CreateInstall))]
[IsDependentOn(typeof(CreateUpgrade))]
[IsDependentOn(typeof(CreateDeploy))]
[IsDependentOn(typeof(CreateSymbols))]
public sealed class DefaultTask : FrostingTask<Context>;
