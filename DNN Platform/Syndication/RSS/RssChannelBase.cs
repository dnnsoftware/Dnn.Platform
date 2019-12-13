﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
