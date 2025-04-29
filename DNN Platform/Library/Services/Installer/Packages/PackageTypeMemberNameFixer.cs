// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages;

using System;

/// <summary>
/// This class allows PackageType to have a member named PackageType
/// to remain compatible with the original VB implementation.
/// </summary>
[Serializable]
public class PackageTypeMemberNameFixer
{
    /// <summary>Initializes a new instance of the <see cref="PackageTypeMemberNameFixer"/> class.</summary>
    public PackageTypeMemberNameFixer()
    {
        this.PackageType = string.Empty;
    }

    public string PackageType { get; set; }
}
