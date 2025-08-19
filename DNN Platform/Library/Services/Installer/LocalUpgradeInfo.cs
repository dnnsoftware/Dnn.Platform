// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer;

using System;

/// <summary>Represents a local DNN upgrade package which is available to be applied.</summary>
public record LocalUpgradeInfo
{
    /// <summary>Gets or sets a value indicating whether the package is valid.</summary>
    public bool IsValid { get; set; }

    /// <summary>Gets or sets a value indicating whether the package is not for a newer version of DNN.</summary>
    public bool IsOutdated { get; set; }

    /// <summary>Gets or sets the name of the upgrade package.</summary>
    public string PackageName { get; set; }

    /// <summary>Gets or sets the version of the upgrade package.</summary>
    public Version Version { get; set; }
}
