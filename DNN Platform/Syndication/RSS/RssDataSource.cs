#region Usings

using System.Collections;
using System.ComponentModel;
using System.Web.UI;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   RSS data source control implementation, including the designer
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
                if (_channel == null)
                {
                    if (string.IsNullOrEmpty(_url))
                    {
                        _channel = new GenericRssChannel();
                    }
                    else
                    {
                        _channel = GenericRssChannel.LoadChannel(_url);
                    }
                }

                return _channel;
            }
        }

        public int MaxItems { get; set; }

        public string Url
        {
            get
            {
                return _url;
            }

            set
            {
                _channel = null;
                _url = value;
            }
        }

        protected override DataSourceView GetView(string viewName)
        {
            if (_itemsView == null)
            {
                _itemsView = new RssDataSourceView(this, viewName);
            }

            return _itemsView;
        }
    }

    public class RssDataSourceView : DataSourceView
    {
        private readonly RssDataSource _owner;

        internal RssDataSourceView(RssDataSource owner, string viewName) : base(owner, viewName)
        {
            _owner = owner;
        }

        public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
        {
            callback(ExecuteSelect(arguments));
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            return _owner.Channel.SelectItems(_owner.MaxItems);
        }
    }
}
