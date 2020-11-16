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
        private GenericRssChannel _channel;
        private RssDataSourceView _itemsView;
        private string _url;

        public GenericRssChannel Channel
        {
            get
            {
                if (this._channel == null)
                {
                    if (string.IsNullOrEmpty(this._url))
                    {
                        this._channel = new GenericRssChannel();
                    }
                    else
                    {
                        this._channel = GenericRssChannel.LoadChannel(this._url);
                    }
                }

                return this._channel;
            }
        }

        public int MaxItems { get; set; }

        public string Url
        {
            get
            {
                return this._url;
            }

            set
            {
                this._channel = null;
                this._url = value;
            }
        }

        protected override DataSourceView GetView(string viewName)
        {
            if (this._itemsView == null)
            {
                this._itemsView = new RssDataSourceView(this, viewName);
            }

            return this._itemsView;
        }
    }

    public class RssDataSourceView : DataSourceView
    {
        private readonly RssDataSource _owner;

        internal RssDataSourceView(RssDataSource owner, string viewName)
            : base(owner, viewName)
        {
            this._owner = owner;
        }

        public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
        {
            callback(this.ExecuteSelect(arguments));
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            return this._owner.Channel.SelectItems(this._owner.MaxItems);
        }
    }
}
