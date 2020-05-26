// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Late-bound RSS element (used for late bound item and image)
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
                return GetAttributeValue(attributeName);
            }
            set
            {
                Attributes[attributeName] = value;
            }
        }
    }
}
