// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    ///   Class to consume (or create) a channel in a late-bound way.
    /// </summary>
    public sealed class GenericRssChannel : RssChannelBase<GenericRssElement, GenericRssElement>
    {
        public new Dictionary<string, string> Attributes
        {
            get
            {
                return base.Attributes;
            }
        }

        public GenericRssElement Image
        {
            get
            {
                return this.GetImage();
            }
        }

        public string this[string attributeName]
        {
            get
            {
                return this.GetAttributeValue(attributeName);
            }

            set
            {
                this.Attributes[attributeName] = value;
            }
        }

        public static GenericRssChannel LoadChannel(string url)
        {
            var channel = new GenericRssChannel();
            channel.LoadFromUrl(url);
            return channel;
        }

        public static GenericRssChannel LoadChannel(XmlDocument doc)
        {
            var channel = new GenericRssChannel();
            channel.LoadFromXml(doc);
            return channel;
        }

        // Select method for programmatic databinding
        public IEnumerable SelectItems()
        {
            return this.SelectItems(-1);
        }

        public IEnumerable SelectItems(int maxItems)
        {
            var data = new ArrayList();

            foreach (GenericRssElement element in this.Items)
            {
                if (maxItems > 0 && data.Count >= maxItems)
                {
                    break;
                }

                data.Add(new RssElementCustomTypeDescriptor(element.Attributes));
            }

            return data;
        }
    }
}
