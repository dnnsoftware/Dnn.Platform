// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>A response about a package dependency.</summary>
public class DependencyResponse
{
    /// <summary>Gets a value indicating whether this dependency is a dependency on another package.</summary>
    public bool IsPackageDependency { get; init; }

    /// <summary>Gets the name of the package.</summary>
    public string? PackageName { get; init; }

    /// <summary>Gets the version of the dependency.</summary>
    public string? DependencyVersion { get; init; }
}
