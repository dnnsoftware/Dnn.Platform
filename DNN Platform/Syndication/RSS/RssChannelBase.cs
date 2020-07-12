// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    ///   Base class for RSS channel (for strongly-typed and late-bound channel types).
    /// </summary>
    /// <typeparam name = "RssItemType"></typeparam>
    /// <typeparam name = "RssImageType"></typeparam>
    public abstract class RssChannelBase<RssItemType, RssImageType> : RssElementBase
        where RssItemType : RssElementBase, new()
        where RssImageType : RssElementBase, new()
    {
        private readonly List<RssItemType> _items = new List<RssItemType>();
        private RssImageType _image;
        private string _url;

        public List<RssItemType> Items
        {
            get
            {
                return this._items;
            }
        }

        internal string Url
        {
            get
            {
                return this._url;
            }
        }

        public XmlDocument SaveAsXml()
        {
            return this.SaveAsXml(RssXmlHelper.CreateEmptyRssXml());
        }

        public XmlDocument SaveAsXml(XmlDocument EmptyRssXml)
        {
            XmlDocument doc = EmptyRssXml;
            XmlNode channelNode = RssXmlHelper.SaveRssElementAsXml(doc.DocumentElement, this, "channel");

            if (this._image != null)
            {
                RssXmlHelper.SaveRssElementAsXml(channelNode, this._image, "image");
            }

            foreach (RssItemType item in this._items)
            {
                RssXmlHelper.SaveRssElementAsXml(channelNode, item, "item");
            }

            return doc;
        }

        internal void LoadFromDom(RssChannelDom dom)
        {
            // channel attributes
            this.SetAttributes(dom.Channel);

            // image attributes
            if (dom.Image != null)
            {
                var image = new RssImageType();
                image.SetAttributes(dom.Image);
                this._image = image;
            }

            // items
            foreach (Dictionary<string, string> i in dom.Items)
            {
                var item = new RssItemType();
                item.SetAttributes(i);
                this._items.Add(item);
            }
        }

        protected void LoadFromUrl(string url)
        {
            // download the feed
            RssChannelDom dom = RssDownloadManager.GetChannel(url);

            // create the channel
            this.LoadFromDom(dom);

            // remember the url
            this._url = url;
        }

        protected void LoadFromXml(XmlDocument doc)
        {
            // parse XML
            RssChannelDom dom = RssXmlHelper.ParseChannelXml(doc);

            // create the channel
            this.LoadFromDom(dom);
        }

        protected RssImageType GetImage()
        {
            if (this._image == null)
            {
                this._image = new RssImageType();
            }

            return this._image;
        }
    }
}
