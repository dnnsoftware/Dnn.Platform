// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;

    /// <summary>
    ///   RSS data source control implementation, including the designer.
    /// </summary>
    [DefaultProperty("Url")]
    public class RssDataSource : DataSourceControl
    {
        private GenericRssChannel channel;
        private RssDataSourceView itemsView;
        private string url;

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

        public int MaxItems { get; set; }

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

    public class RssDataSourceView : DataSourceView
    {
        private readonly RssDataSource owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RssDataSourceView"/> class.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="viewName"></param>
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
}
