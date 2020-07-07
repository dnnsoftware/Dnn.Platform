// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Services.Tokens;

    public class JournalEntity : IPropertyAccess
    {
        public JournalEntity()
        {
        }

        public JournalEntity(string entityXML)
        {
            if (!string.IsNullOrEmpty(entityXML))
            {
                XmlDocument xDoc = new XmlDocument { XmlResolver = null };
                xDoc.LoadXml(entityXML);
                if (xDoc != null)
                {
                    XmlNode xRoot = xDoc.DocumentElement;
                    XmlNode xNode = null;
                    xNode = xRoot.SelectSingleNode("//entity");
                    if (xNode != null)
                    {
                        this.Id = int.Parse(xNode["id"].InnerText);
                        this.Name = xNode["name"].InnerText.ToString();
                        if (xNode["vanity"] != null)
                        {
                            this.Vanity = xNode["vanity"].InnerText.ToString();
                        }
                    }
                }
            }
        }

        public CacheLevel Cacheability
        {
            get { return CacheLevel.fullyCacheable; }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Vanity { get; set; }

        public string Avatar { get; set; }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            string OutputFormat = string.Empty;
            if (format == string.Empty)
            {
                OutputFormat = "g";
            }
            else
            {
                OutputFormat = format;
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "id":
                    return PropertyAccess.FormatString(this.Id.ToString(), format);
                case "name":
                    return PropertyAccess.FormatString(this.Name.ToString(), format);
                case "vanity":
                    return PropertyAccess.FormatString(this.Vanity.ToString(), format);
                case "avatar":
                    return PropertyAccess.FormatString(this.Avatar.ToString(), format);
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
