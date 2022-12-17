// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   The base class for all RSS elements (item, image, channel)
    ///   has collection of attributes.
    /// </summary>
    public abstract class RssElementBase
    {
        private Dictionary<string, string> attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected internal Dictionary<string, string> Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        public virtual void SetDefaults()
        {
        }

        internal void SetAttributes(Dictionary<string, string> attributes)
        {
            this.attributes = attributes;
        }

        protected string GetAttributeValue(string attributeName)
        {
            string attributeValue;

            if (!this.attributes.TryGetValue(attributeName, out attributeValue))
            {
                attributeValue = string.Empty;
            }

            return attributeValue;
        }

        protected void SetAttributeValue(string attributeName, string attributeValue)
        {
            this.attributes[attributeName] = attributeValue;
        }
    }
}
