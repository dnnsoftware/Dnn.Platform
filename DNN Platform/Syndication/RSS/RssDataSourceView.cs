// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System.Collections;
using System.Web.UI;

/// <summary>An RSS <see cref="DataSourceView"/>.</summary>
public class RssDataSourceView : DataSourceView
{
    private readonly RssDataSource owner;

    /// <summary>Initializes a new instance of the <see cref="RssDataSourceView"/> class.</summary>
    /// <inheritdoc cref="DataSourceView(IDataSource, string)"/>
    internal RssDataSourceView(RssDataSource owner, string viewName)
        : base(owner, viewName)
    {
        this.owner = owner;
    }

    /// <inheritdoc/>
    public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
    {
        callback(this.ExecuteSelect(arguments));
    }

    /// <inheritdoc/>
    protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
    {
        return this.owner.Channel.SelectItems(this.owner.MaxItems);
    }
}
