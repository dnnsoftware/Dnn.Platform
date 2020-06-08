// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Internal representation of parsed RSS channel
    /// </summary>
    internal class RssChannelDom
    {
        private readonly Dictionary<string, string> _channel;
        private readonly Dictionary<string, string> _image;
        private readonly List<Dictionary<string, string>> _items;
        private DateTime _utcExpiry;

        internal RssChannelDom(Dictionary<string, string> channel, Dictionary<string, string> image, List<Dictionary<string, string>> items)
        {
            _channel = channel;
            _image = image;
            _items = items;
            _utcExpiry = DateTime.MaxValue;
        }

        internal Dictionary<string, string> Channel
        {
            get
            {
                return _channel;
            }
        }

        internal Dictionary<string, string> Image
        {
            get
            {
                return _image;
            }
        }

        internal List<Dictionary<string, string>> Items
        {
            get
            {
                return _items;
            }
        }

        internal DateTime UtcExpiry
        {
            get
            {
                return _utcExpiry;
            }
        }

        internal void SetExpiry(DateTime utcExpiry)
        {
            _utcExpiry = utcExpiry;
        }
    }
}
