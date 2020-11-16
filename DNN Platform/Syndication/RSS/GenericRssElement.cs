// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System.Collections.Generic;

    /// <summary>
    ///   Late-bound RSS element (used for late bound item and image).
    /// </summary>
    public sealed class GenericRssElement : RssElementBase
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
                return this.GetAttributeValue(attributeName);
            }

            set
            {
                this.Attributes[attributeName] = value;
            }
        }
    }
}
