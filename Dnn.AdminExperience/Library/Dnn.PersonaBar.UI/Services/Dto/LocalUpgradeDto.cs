// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services.DTO;

using System;
using System.Runtime.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Services.Installer;

/// <summary>Represents a local DNN upgrade package which is available to be applied.</summary>
[DataContract]
[Serializable]
public class LocalUpgradeDto
{
    /// <summary>Gets or sets a value indicating whether the package is valid.</summary>
    [DataMember]
    public bool IsValid { get; set; }

    /// <summary>Gets or sets a value indicating whether the package is not for a newer version of DNN.</summary>
    [DataMember]
    public bool IsOutdated { get; set; }

    /// <summary>Gets or sets the name of the upgrade package.</summary>
    [DataMember]
    public string PackageName { get; set; }

    /// <summary>Gets or sets the version of the upgrade package.</summary>
    public Version Version { get; set; }

    /// <summary>Gets the string representation of <see cref="Version"/>.</summary>
    [DataMember(Name = "Version")]
    public string VersionString => this.Version is null ? null : $"v. {Globals.FormatVersion(this.Version, true)}";

    /// <summary>Converts a <see cref="LocalUpgradeInfo"/> instance to a <see cref="LocalUpgradeDto"/>.</summary>
    /// <param name="info">The <see cref="LocalUpgradeInfo"/> instance.</param>
    /// <returns>A new <see cref="LocalUpgradeDto"/> instance.</returns>
    public static LocalUpgradeDto FromLocalUpgradeInfo(LocalUpgradeInfo info)
    {
        return new LocalUpgradeDto
        {
            Version = info.Version,
            IsOutdated = info.IsOutdated,
            IsValid = info.IsValid,
            PackageName = info.PackageName,
        };
    }
}
