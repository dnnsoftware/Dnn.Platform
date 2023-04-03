// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>A response about a package.</summary>
public class PackageResponse
{
    /// <summary>Gets the package's name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the package's dependencies.</summary>
    public List<DependencyResponse?>? Dependencies { get; init; }

    /// <summary>Gets the package's version.</summary>
    public string? VersionStr { get; init; }

    /// <summary>Gets a value indicating whether the package can be installed.</summary>
    public bool CanInstall { get; init; }
}
