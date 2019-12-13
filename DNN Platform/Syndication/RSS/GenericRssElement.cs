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
