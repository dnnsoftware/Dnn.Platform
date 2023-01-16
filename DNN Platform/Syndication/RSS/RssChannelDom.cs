// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;

    /// <summary>  Internal representation of parsed RSS channel.</summary>
    internal class RssChannelDom
    {
        private readonly Dictionary<string, string> channel;
        private readonly Dictionary<string, string> image;
        private readonly List<Dictionary<string, string>> items;
        private DateTime utcExpiry;

        internal RssChannelDom(Dictionary<string, string> channel, Dictionary<string, string> image, List<Dictionary<string, string>> items)
        {
            this.channel = channel;
            this.image = image;
            this.items = items;
            this.utcExpiry = DateTime.MaxValue;
        }

        internal Dictionary<string, string> Channel
        {
            get
            {
                return this.channel;
            }
        }

        internal Dictionary<string, string> Image
        {
            get
            {
                return this.image;
            }
        }

        internal List<Dictionary<string, string>> Items
        {
            get
            {
                return this.items;
            }
        }

        internal DateTime UtcExpiry
        {
            get
            {
                return this.utcExpiry;
            }
        }

        internal void SetExpiry(DateTime utcExpiry)
        {
            this.utcExpiry = utcExpiry;
        }
    }
}
