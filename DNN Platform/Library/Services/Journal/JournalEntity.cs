// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System.Globalization;
    using System.Xml;

    using DotNetNuke.Services.Tokens;

    public class JournalEntity : IPropertyAccess
    {
        /// <summary>Initializes a new instance of the <see cref="JournalEntity"/> class.</summary>
        public JournalEntity()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="JournalEntity"/> class.</summary>
        /// <param name="entityXML">XML serialized entity.</param>
        public JournalEntity(string entityXML)
        {
            if (!string.IsNullOrEmpty(entityXML))
            {
                var xDoc = new XmlDocument { XmlResolver = null, };
                xDoc.LoadXml(entityXML);
                if (xDoc != null)
                {
                    XmlNode xRoot = xDoc.DocumentElement;
                    XmlNode xNode = null;
                    xNode = xRoot.SelectSingleNode("//entity");
                    if (xNode != null)
                    {
                        this.Id = int.Parse(xNode["id"].InnerText, CultureInfo.InvariantCulture);
                        this.Name = xNode["name"].InnerText.ToString();
                        if (xNode["vanity"] != null)
                        {
                            this.Vanity = xNode["vanity"].InnerText.ToString();
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Vanity { get; set; }

        public string Avatar { get; set; }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            string outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }
            else
            {
                outputFormat = format;
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "id":
                    return PropertyAccess.FormatString(this.Id.ToString(formatProvider), format);
                case "name":
                    return PropertyAccess.FormatString(this.Name, format);
                case "vanity":
                    return PropertyAccess.FormatString(this.Vanity, format);
                case "avatar":
                    return PropertyAccess.FormatString(this.Avatar, format);
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
