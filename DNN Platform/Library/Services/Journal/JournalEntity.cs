using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetNuke.Services.Tokens;
namespace DotNetNuke.Services.Journal {
    public class JournalEntity :IPropertyAccess  {
        public int Id {get;set;}
        public string Name {get;set;}
        public string Vanity {get; set;}
        public string Avatar { get; set; }
        public JournalEntity() {
        }
        public JournalEntity(string entityXML) {
            if (!string.IsNullOrEmpty(entityXML)) {
                System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                xDoc.LoadXml(entityXML);
                if ((xDoc != null)) {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNode xNode = null;
                    xNode = xRoot.SelectSingleNode("//entity");
                    if ((xNode != null)) {
                        Id = int.Parse(xNode["id"].InnerText);
                        Name = xNode["name"].InnerText.ToString();
                        if ((xNode["vanity"] != null)) {
                            Vanity= xNode["vanity"].InnerText.ToString();
                        }

                    }
                }
            }
        }

        public CacheLevel Cacheability {
            get { return CacheLevel.fullyCacheable; }
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
                case "id":
                    return PropertyAccess.FormatString(Id.ToString(), format);
                case "name":
                    return PropertyAccess.FormatString(Name.ToString(), format);
                case "vanity":
                    return PropertyAccess.FormatString(Vanity.ToString(), format);
                case "avatar":
                    return PropertyAccess.FormatString(Avatar.ToString(), format);
                

            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
