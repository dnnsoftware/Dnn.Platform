// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Services.Tokens;

    public class ItemData : IPropertyAccess
    {
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

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
                case "url":
                    return PropertyAccess.FormatString(this.Url, format);
                case "title":
                    return PropertyAccess.FormatString(this.Title, format);
                case "description":
                    return PropertyAccess.FormatString(this.Description, format);
                case "imageurl":
                    return PropertyAccess.FormatString(this.ImageUrl, format);
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
