// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System;
using System.Web;
using System.Xml;

/// <summary>A function which handles the <see cref="RssHttpHandlerBase{TRssChannelType,TRssItemType,TRssImageType}.Init"/> event.</summary>
/// <param name="source">The event source, i.e. the <see cref="RssHttpHandlerBase{TRssChannelType,TRssItemType,TRssImageType}"/> instance.</param>
/// <param name="e">Empty event args.</param>
public delegate void InitEventHandler(object source, EventArgs e);

/// <summary>A function which handles the <see cref="RssHttpHandlerBase{TRssChannelType,TRssItemType,TRssImageType}.PreRender"/> event.</summary>
/// <param name="source">The event source, i.e. the <see cref="RssHttpHandlerBase{TRssChannelType,TRssItemType,TRssImageType}"/> instance.</param>
/// <param name="e">Empty event args.</param>
public delegate void PreRenderEventHandler(object source, EventArgs e);

/// <summary>Base class for RssHttpHandler - Generic handler and strongly typed ones are derived from it.</summary>
/// <typeparam name="TRssChannelType">The channel type.</typeparam>
/// <typeparam name="TRssItemType">The item type.</typeparam>
/// <typeparam name="TRssImageType">The image type.</typeparam>
public abstract class RssHttpHandlerBase<TRssChannelType, TRssItemType, TRssImageType> : IHttpHandler
    where TRssChannelType : RssChannelBase<TRssItemType, TRssImageType>, new()
    where TRssItemType : RssElementBase, new()
    where TRssImageType : RssElementBase, new()
{
    /// <summary>An event which is fired when the handler is initialized.</summary>
    public event InitEventHandler Init;

    /// <summary>An event which is fired just before the handler's contents are rendered.</summary>
    public event PreRenderEventHandler PreRender;

    /// <inheritdoc/>
    bool IHttpHandler.IsReusable => false;

    /// <summary>Gets the channel.</summary>
    protected TRssChannelType Channel { get; private set; }

    /// <summary>Gets the HTTP context.</summary>
    protected HttpContext Context { get; private set; }

    /// <inheritdoc/>
    void IHttpHandler.ProcessRequest(HttpContext context)
    {
        this.InternalInit(context);

        // let inherited handlers setup any special handling
        this.OnInit(EventArgs.Empty);

        // parse the channel name and the user name from the query string
        RssHttpHandlerHelper.ParseChannelQueryString(context.Request, out var channelName, out var userName);

        // populate items (call the derived class)
        this.PopulateChannel(channelName, userName);

        this.OnPreRender(EventArgs.Empty);

        this.Render(new XmlTextWriter(this.Context.Response.OutputStream, null));
    }

    /// <summary>Triggers the <see cref="Init"/> event.</summary>
    /// <param name="ea">The event args.</param>
    protected virtual void OnInit(EventArgs ea)
    {
        this.Init?.Invoke(this, ea);
    }

    /// <summary>Triggers the <see cref="PreRender"/> event.</summary>
    /// <param name="ea">The event args.</param>
    protected virtual void OnPreRender(EventArgs ea)
    {
        this.PreRender?.Invoke(this, ea);
    }

    /// <summary>When overridden in a derived class, populates the channel name and user name.</summary>
    /// <param name="channelName">The channel name.</param>
    /// <param name="userName">The user name.</param>
    protected virtual void PopulateChannel(string channelName, string userName)
    {
    }

    /// <summary>Renders the channel as XML to the <paramref name="writer"/>.</summary>
    /// <param name="writer">The write to which the channel should be rendered.</param>
    protected virtual void Render(XmlTextWriter writer)
    {
        XmlDocument doc = this.Channel.SaveAsXml();
        doc.Save(writer);
    }

    private void InternalInit(HttpContext context)
    {
        this.Context = context;

        // create the channel
        this.Channel = new TRssChannelType();
        this.Channel.SetDefaults();

        this.Context.Response.ContentType = "text/xml";
    }
}
