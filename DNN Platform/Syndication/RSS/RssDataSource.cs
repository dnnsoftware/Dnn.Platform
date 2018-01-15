#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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