// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System.ComponentModel;
using System.Web.UI;

/// <summary>RSS data source control implementation, including the designer.</summary>
[DefaultProperty("Url")]
public class RssDataSource : DataSourceControl
{
    private GenericRssChannel channel;
    private RssDataSourceView itemsView;
    private string url;

    /// <summary>Gets the channel.</summary>
    public GenericRssChannel Channel
    {
        get
        {
            if (this.channel == null)
            {
                if (string.IsNullOrEmpty(this.url))
                {
                    this.channel = new GenericRssChannel();
                }
                else
                {
                    this.channel = GenericRssChannel.LoadChannel(this.url);
                }
            }

            return this.channel;
        }
    }

    /// <summary>Gets or sets the maximum number of items.</summary>
    public int MaxItems { get; set; }

    /// <summary>Gets or sets the URL.</summary>
    public string Url
    {
        get
        {
            return this.url;
        }

        set
        {
            this.channel = null;
            this.url = value;
        }
    }

    /// <inheritdoc/>
    protected override DataSourceView GetView(string viewName)
    {
        if (this.itemsView == null)
        {
            this.itemsView = new RssDataSourceView(this, viewName);
        }

        return this.itemsView;
    }
}
