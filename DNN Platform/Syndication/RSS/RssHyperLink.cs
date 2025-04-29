// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>RssHyperLink control - works with <see cref="RssHttpHandlerBase{TRssChannelType,TRssItemType,TRssImageType}"/>.</summary>
public class RssHyperLink : HyperLink
{
    /// <summary>Initializes a new instance of the <see cref="RssHyperLink"/> class.</summary>
    public RssHyperLink()
    {
        this.Text = "RSS";
    }

    /// <summary>Gets or sets the channel name.</summary>
    public string ChannelName { get; set; }

    /// <summary>Gets or sets a value indicating whether the current user's name is passed to RssHttpHandler.</summary>
    public bool IncludeUserName { get; set; }

    /// <inheritdoc/>
    protected override void OnPreRender(EventArgs e)
    {
        // modify the NavigateUrl to include optional user name and channel name
        string channel = this.ChannelName ?? string.Empty;
        string user = this.IncludeUserName ? this.Context.User.Identity.Name : string.Empty;
        this.NavigateUrl = RssHttpHandlerHelper.GenerateChannelLink(this.NavigateUrl, channel, user);

        // add <link> to <head> tag (if <head runat=server> is present)
        if (this.Page.Header != null)
        {
            string title = string.IsNullOrEmpty(channel) ? this.Text : channel;

            this.Page.Header.Controls.Add(new LiteralControl(
                $"\r\n<link rel=\"alternate\" type=\"application/rss+xml\" title=\"{title}\" href=\"{this.NavigateUrl}\" />"));
        }

        base.OnPreRender(e);
    }
}
