// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Skins;

/// <summary>Represents a skin.</summary>
public interface ISkinInfo
{
    /// <summary>Gets or sets the ID of the skin.</summary>
    int SkinId { get; set; }

    /// <summary>Gets or sets the ID of the skin package.</summary>
    int SkinPackageId { get; set; }

    /// <summary>Gets or sets the source of the skin.</summary>
    string SkinSrc { get; set; }
}
