// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System;
using System.Collections.Generic;

/// <summary>The base class for all RSS elements (item, image, channel) has collection of attributes.</summary>
public abstract class RssElementBase
{
    /// <summary>Gets the attributes.</summary>
    protected internal Dictionary<string, string> Attributes { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>When overridden in a derived class, initializes the attributes to the defaults.</summary>
    public virtual void SetDefaults()
    {
    }

    /// <summary>Sets the element attributes.</summary>
    /// <param name="attributes">The attributes.</param>
    internal void SetAttributes(Dictionary<string, string> attributes)
    {
        this.Attributes = attributes;
    }

    /// <summary>Gets the attribute value.</summary>
    /// <param name="attributeName">The attribute name.</param>
    /// <returns>The value or <see cref="string.Empty"/>.</returns>
    protected string GetAttributeValue(string attributeName)
    {
        string attributeValue;

        if (!this.Attributes.TryGetValue(attributeName, out attributeValue))
        {
            attributeValue = string.Empty;
        }

        return attributeValue;
    }

    /// <summary>Sets the value of an attribute.</summary>
    /// <param name="attributeName">The attribute name.</param>
    /// <param name="attributeValue">The attribute value.</param>
    protected void SetAttributeValue(string attributeName, string attributeValue)
    {
        this.Attributes[attributeName] = attributeValue;
    }
}
