// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages;

using System;

using DotNetNuke.Security;

/// <summary>The PackageType class represents a single Installer Package Type.</summary>
[Serializable]
public class PackageType : PackageTypeMemberNameFixer
{
    public string Description { get; set; }

    public string EditorControlSrc { get; set; }

    public SecurityAccessLevel SecurityAccessLevel { get; set; }

    public bool SupportsSideBySideInstallation { get; set; }
}
