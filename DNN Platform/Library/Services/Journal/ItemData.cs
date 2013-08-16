using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Services.Tokens;
namespace DotNetNuke.Services.Journal {
    
        public class ItemData : IPropertyAccess {
            public string Url { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public CacheLevel Cacheability {
                get {
                    return CacheLevel.fullyCacheable;
                }
            }

            public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound) {
                string OutputFormat = string.Empty;
                if (format == string.Empty) {
                    OutputFormat = "g";
                } else {
                    OutputFormat = format;
                }
                propertyName = propertyName.ToLowerInvariant();
                switch (propertyName) {
                    case "url":
                        return PropertyAccess.FormatString(Url, format);
                    case "title":
                        return PropertyAccess.FormatString(Title, format);
                    case "description":
                        return PropertyAccess.FormatString(Description, format);
                    case "imageurl":
                        return PropertyAccess.FormatString(ImageUrl, format);


                }

                propertyNotFound = true;
                return string.Empty;

            }
        }



   
}

