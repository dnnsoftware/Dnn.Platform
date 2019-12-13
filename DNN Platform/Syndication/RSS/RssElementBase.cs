// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   The base class for all RSS elements (item, image, channel)
    ///   has collection of attributes
    /// </summary>
    public abstract class RssElementBase
    {
        private Dictionary<string, string> _attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected internal Dictionary<string, string> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        public virtual void SetDefaults()
        {
        }

        internal void SetAttributes(Dictionary<string, string> attributes)
        {
            _attributes = attributes;
        }

        protected string GetAttributeValue(string attributeName)
        {
            string attributeValue;

            if (!_attributes.TryGetValue(attributeName, out attributeValue))
            {
                attributeValue = string.Empty;
            }

            return attributeValue;
        }

        protected void SetAttributeValue(string attributeName, string attributeValue)
        {
            _attributes[attributeName] = attributeValue;
        }
    }
}
