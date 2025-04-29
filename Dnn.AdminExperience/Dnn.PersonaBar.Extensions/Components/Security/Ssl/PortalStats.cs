// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Security.Ssl;

/// <summary>Statistics about a portal.</summary>
public class PortalStats
{
    /// <summary>Gets or sets ID of the portal.</summary>
    public int PortalID { get; set; }

    /// <summary>Gets or sets the total nr of (non-deleted) tabs of the portal.</summary>
    public int NumberOfTabs { get; set; }

    /// <summary>Gets or sets the nr of published secure (i.e. https) tabs of the portal.</summary>
    public int NumberOfSecureTabs { get; set; }

    /// <summary>Gets or sets the nr of published non-secure (i.e. http) tabs of the portal.</summary>
    public int NumberOfNonSecureTabs { get; set; }
}
