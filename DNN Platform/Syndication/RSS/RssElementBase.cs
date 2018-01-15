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