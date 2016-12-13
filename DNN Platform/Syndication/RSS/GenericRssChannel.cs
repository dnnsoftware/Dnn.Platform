#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Class to consume (or create) a channel in a late-bound way
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

        public string this[string attributeName]
        {
            get
            {
                return GetAttributeValue(attributeName);
            }
            set
            {
                Attributes[attributeName] = value;
            }
        }

        public GenericRssElement Image
        {
            get
            {
                return GetImage();
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
            return SelectItems(-1);
        }

        public IEnumerable SelectItems(int maxItems)
        {
            var data = new ArrayList();

            foreach (GenericRssElement element in Items)
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