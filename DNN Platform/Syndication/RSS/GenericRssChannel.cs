// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System.Collections;
using System.Collections.Generic;
using System.Xml;

/// <summary>Class to consume (or create) a channel in a late-bound way.</summary>
public sealed class GenericRssChannel : RssChannelBase<GenericRssElement, GenericRssElement>
{
    /// <inheritdoc cref="RssElementBase.Attributes" />
    public new Dictionary<string, string> Attributes => base.Attributes;

    /// <summary>Gets the channel image.</summary>
    public GenericRssElement Image => this.GetImage();

    /// <summary>Gets or sets the channel's attributes.</summary>
    /// <param name="attributeName">The attribute name.</param>
    public string this[string attributeName]
    {
        get => this.GetAttributeValue(attributeName);
        set => this.Attributes[attributeName] = value;
    }

    /// <summary>Loads an RSS channel from a <paramref name="url"/>.</summary>
    /// <param name="url">The channel's URL.</param>
    /// <returns>A new <see cref="GenericRssChannel"/> instance.</returns>
    public static GenericRssChannel LoadChannel(string url)
    {
        var channel = new GenericRssChannel();
        channel.LoadFromUrl(url);
        return channel;
    }

    /// <summary>Loads an RSS channel from a <paramref name="doc"/>.</summary>
    /// <param name="doc">The XML document.</param>
    /// <returns>A new <see cref="GenericRssChannel"/> instance.</returns>
    public static GenericRssChannel LoadChannel(XmlDocument doc)
    {
        var channel = new GenericRssChannel();
        channel.LoadFromXml(doc);
        return channel;
    }

    /// <summary>Get a sequence of the channel's items.</summary>
    /// <returns>A sequence of <see cref="RssElementCustomTypeDescriptor"/> instances.</returns>
    /// <remarks>This method is for programmatic data-binding.</remarks>
    public IEnumerable SelectItems()
    {
        return this.SelectItems(-1);
    }

    /// <summary>Get a sequence of the channel's items.</summary>
    /// <param name="maxItems">The maximum number of items to include, or include all items if the value is not positive.</param>
    /// <returns>A sequence of <see cref="RssElementCustomTypeDescriptor"/> instances.</returns>
    public IEnumerable SelectItems(int maxItems)
    {
        var data = new ArrayList();

        foreach (GenericRssElement element in this.Items)
        {
            if (maxItems > 0 && data.Count >= maxItems)
            {
                break;
            }

            data.Add(new RssElementCustomTypeDescriptor(element.Attributes));
        }

        return data;
    }
}
