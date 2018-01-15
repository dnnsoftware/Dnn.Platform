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

using System.Collections.Generic;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Base class for RSS channel (for strongly-typed and late-bound channel types)
    /// </summary>
    /// <typeparam name = "RssItemType"></typeparam>
    /// <typeparam name = "RssImageType"></typeparam>
    public abstract class RssChannelBase<RssItemType, RssImageType> : RssElementBase where RssItemType : RssElementBase, new() where RssImageType : RssElementBase, new()
    {
        private readonly List<RssItemType> _items = new List<RssItemType>();
        private RssImageType _image;
        private string _url;

        public List<RssItemType> Items
        {
            get
            {
                return _items;
            }
        }

        internal string Url
        {
            get
            {
                return _url;
            }
        }

        protected void LoadFromUrl(string url)
        {
            // download the feed
            RssChannelDom dom = RssDownloadManager.GetChannel(url);

            // create the channel
            LoadFromDom(dom);

            // remember the url
            _url = url;
        }

        protected void LoadFromXml(XmlDocument doc)
        {
            // parse XML
            RssChannelDom dom = RssXmlHelper.ParseChannelXml(doc);

            // create the channel
            LoadFromDom(dom);
        }

        internal void LoadFromDom(RssChannelDom dom)
        {
            // channel attributes
            SetAttributes(dom.Channel);

            // image attributes
            if (dom.Image != null)
            {
                var image = new RssImageType();
                image.SetAttributes(dom.Image);
                _image = image;
            }

            // items
            foreach (Dictionary<string, string> i in dom.Items)
            {
                var item = new RssItemType();
                item.SetAttributes(i);
                _items.Add(item);
            }
        }

        public XmlDocument SaveAsXml()
        {
            return SaveAsXml(RssXmlHelper.CreateEmptyRssXml());
        }

        public XmlDocument SaveAsXml(XmlDocument EmptyRssXml)
        {
            XmlDocument doc = EmptyRssXml;
            XmlNode channelNode = RssXmlHelper.SaveRssElementAsXml(doc.DocumentElement, this, "channel");

            if (_image != null)
            {
                RssXmlHelper.SaveRssElementAsXml(channelNode, _image, "image");
            }

            foreach (RssItemType item in _items)
            {
                RssXmlHelper.SaveRssElementAsXml(channelNode, item, "item");
            }

            return doc;
        }

        protected RssImageType GetImage()
        {
            if (_image == null)
            {
                _image = new RssImageType();
            }

            return _image;
        }
    }
}