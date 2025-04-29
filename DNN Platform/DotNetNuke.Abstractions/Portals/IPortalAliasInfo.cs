// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals;

using DotNetNuke.Abstractions.Urls;

/// <summary>Portal Alias Info interface.</summary>
public interface IPortalAliasInfo
{
    /// <summary>Gets or sets the http alias.</summary>
    string HttpAlias { get; set; }

    /// <summary>Gets or sets the Portal Alias ID.</summary>
    int PortalAliasId { get; set; }

    /// <summary>Gets or sets the Portal ID.</summary>
    int PortalId { get; set; }

    /// <summary>Gets or sets a value indicating whether this alias is the primary alias for the site/portal.</summary>
    bool IsPrimary { get; set; }

    /// <summary>Gets or sets a value indicating whether the alias is a redirect alias.</summary>
    bool Redirect { get; set; }

    /// <summary>Gets or sets the Browser Type.</summary>
    BrowserTypes BrowserType { get; set; }

    /// <summary>Gets or sets the culture code for this alias, if there is one.</summary>
    string CultureCode { get; set; }

    /// <summary>Gets or sets the skin/theme for this alias, if there is one.</summary>
    string Skin { get; set; }
}
