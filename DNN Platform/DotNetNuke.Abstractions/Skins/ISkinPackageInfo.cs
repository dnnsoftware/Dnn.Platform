// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Skins;

using System.Collections.Generic;

using DotNetNuke.Abstractions.Collections;

/// <summary>The skin package info.</summary>
public interface ISkinPackageInfo
{
    /// <summary>Gets or sets the ID of the package.</summary>
    int PackageId { get; set; }

    /// <summary>Gets or sets the ID of the skin package.</summary>
    int SkinPackageId { get; set; }

    /// <summary>Gets or sets the ID of the portal.</summary>
    /// <remarks>If the portal ID is <c>-1</c>, then the skin package is a global skin package.</remarks>
    int PortalId { get; set; }

    /// <summary>Gets or sets the name of the skin.</summary>
    string SkinName { get; set; }

    /// <summary>Gets the skins in the skin package.</summary>
    IObjectList<ISkinInfo> Skins { get; }

    /// <summary>Gets or sets the type of the skin.</summary>
    SkinPackageType SkinType { get; set; }
}
