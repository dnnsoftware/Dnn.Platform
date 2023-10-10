// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Skins;

using System;

/// <summary>The scope of a skin.</summary>
/// <remarks>This enum is used for <see cref="ISkinService.GetSkinsInFolder"/>.</remarks>
public enum SkinFolder
{
    /// <summary>All scopes are specified.</summary>
    All = 0,

    /// <summary>The skin can be used for all portals.</summary>
    /// <remarks>These skins are by default in the folder 'Portals\_default\'.</remarks>
    Host = 1,

    /// <summary>The skin can only be used for the given portal.</summary>
    /// <remarks>These skins are by default in the folder 'Portals\[PortalId]\' and 'Portals\[PortalId]-System\'.</remarks>
    Portal = 2,
}
