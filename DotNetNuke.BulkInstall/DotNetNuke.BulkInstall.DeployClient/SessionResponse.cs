// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

/// <summary>A response about a package file.</summary>
public record SessionResponse
{
    /// <summary>Gets the file name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the packages in the file.</summary>
    public List<PackageResponse?>? Packages { get; init; }

    /// <summary>Gets the failures installing the file.</summary>
    public List<string?>? Failures { get; init; }

    /// <summary>Gets a value indicating whether the package file has been attempted to be installed.</summary>
    public bool Attempted { get; init; }

    /// <summary>Gets a value indicating whether the package file installation was successful.</summary>
    public bool Success { get; init; }

    /// <summary>Gets a value indicating whether the package file can be installed.</summary>
    public bool CanInstall { get; init; }
}
