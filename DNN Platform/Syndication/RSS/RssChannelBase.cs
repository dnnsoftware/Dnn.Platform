// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System.Collections.Generic;
using System.Xml;

/// <summary>Base class for RSS channel (for strongly-typed and late-bound channel types).</summary>
/// <typeparam name="TRssItemType">The item type.</typeparam>
/// <typeparam name="TRssImageType">The image type.</typeparam>
public abstract class RssChannelBase<TRssItemType, TRssImageType> : RssElementBase
    where TRssItemType : RssElementBase, new()
    where TRssImageType : RssElementBase, new()
{
    private TRssImageType image;

    /// <summary>Gets the channel items.</summary>
    public List<TRssItemType> Items { get; } = new List<TRssItemType>();

    /// <summary>Gets the channel URL.</summary>
    internal string Url { get; private set; }

    /// <summary>Gets the channel as an XML document.</summary>
    /// <returns>A new <see cref="XmlDocument"/> instance.</returns>
    public XmlDocument SaveAsXml()
    {
        return this.SaveAsXml(RssXmlHelper.CreateEmptyRssXml());
    }

    /// <summary>Gets the channel as an XML document.</summary>
    /// <param name="emptyRssXml">An empty <see cref="XmlDocument"/> to save the channel contents into.</param>
    /// <returns>The <paramref name="emptyRssXml"/> passed in with the channel contents added.</returns>
    public XmlDocument SaveAsXml(XmlDocument emptyRssXml)
    {
        XmlDocument doc = emptyRssXml;
        XmlNode channelNode = RssXmlHelper.SaveRssElementAsXml(doc.DocumentElement, this, "channel");

        if (this.image != null)
        {
            RssXmlHelper.SaveRssElementAsXml(channelNode, this.image, "image");
        }

        foreach (TRssItemType item in this.Items)
        {
            RssXmlHelper.SaveRssElementAsXml(channelNode, item, "item");
        }

        return doc;
    }

    /// <summary>Loads the contents from the <paramref name="dom"/> into this channel.</summary>
    /// <param name="dom">The DOM to load from.</param>
    internal void LoadFromDom(RssChannelDom dom)
    {
        // channel attributes
        this.SetAttributes(dom.Channel);

        // image attributes
        if (dom.Image != null)
        {
            var image = new TRssImageType();
            image.SetAttributes(dom.Image);
            this.image = image;
        }

        // items
        foreach (Dictionary<string, string> i in dom.Items)
        {
            var item = new TRssItemType();
            item.SetAttributes(i);
            this.Items.Add(item);
        }
    }

    /// <summary>Loads a channel from a <paramref name="url"/> into this channel.</summary>
    /// <param name="url">The URL to load the channel from.</param>
    protected void LoadFromUrl(string url)
    {
        // download the feed
        RssChannelDom dom = RssDownloadManager.GetChannel(url);

        // create the channel
        this.LoadFromDom(dom);

        // remember the url
        this.Url = url;
    }

    /// <summary>Loads a channel from a <paramref name="doc"/> into this channel.</summary>
    /// <param name="doc">The XML to load the channel from.</param>
    protected void LoadFromXml(XmlDocument doc)
    {
        // parse XML
        RssChannelDom dom = RssXmlHelper.ParseChannelXml(doc);

        // create the channel
        this.LoadFromDom(dom);
    }

    /// <summary>Gets the channel image.</summary>
    /// <returns>An <typeparamref name="TRssImageType"/> instance.</returns>
    protected TRssImageType GetImage()
    {
        if (this.image == null)
        {
            this.image = new TRssImageType();
        }

        return this.image;
    }
}
