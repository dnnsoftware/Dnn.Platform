// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System.Collections.Generic;

/// <summary>Late-bound RSS element (used for late bound item and image).</summary>
public sealed class GenericRssElement : RssElementBase
{
    /// <inheritdoc cref="RssElementBase.Attributes" />
    public new Dictionary<string, string> Attributes => base.Attributes;

    /// <summary>Gets or sets the element's attributes.</summary>
    /// <param name="attributeName">The attribute name.</param>
    public string this[string attributeName]
    {
        get => this.GetAttributeValue(attributeName);
        set => this.Attributes[attributeName] = value;
    }
}
