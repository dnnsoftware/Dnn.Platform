// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Data;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Urls;

    [Serializable]
    public class PortalAliasInfo : BaseEntityInfo, IHydratable, IXmlSerializable
    {
        public PortalAliasInfo()
        {
        }

        public PortalAliasInfo(PortalAliasInfo alias)
        {
            this.HTTPAlias = alias.HTTPAlias;
            this.PortalAliasID = alias.PortalAliasID;
            this.PortalID = alias.PortalID;
            this.IsPrimary = alias.IsPrimary;
            this.Redirect = alias.Redirect;
            this.BrowserType = alias.BrowserType;
            this.CultureCode = alias.CultureCode;
            this.Skin = alias.Skin;
        }

        public string HTTPAlias { get; set; }

        public int PortalAliasID { get; set; }

        public int PortalID { get; set; }

        public bool IsPrimary { get; set; }

        public bool Redirect { get; set; }

        public BrowserTypes BrowserType { get; set; }

        public string CultureCode { get; set; }

        public string Skin { get; set; }

        public int KeyID
        {
            get { return this.PortalAliasID; }
            set { this.PortalAliasID = value; }
        }

        public void Fill(IDataReader dr)
        {
            this.FillInternal(dr);

            this.PortalAliasID = Null.SetNullInteger(dr["PortalAliasID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.HTTPAlias = Null.SetNullString(dr["HTTPAlias"]);
            this.IsPrimary = Null.SetNullBoolean(dr["IsPrimary"]);
            var browserType = Null.SetNullString(dr["BrowserType"]);
            this.BrowserType = string.IsNullOrEmpty(browserType) || browserType.Equals("normal", StringComparison.OrdinalIgnoreCase)
                              ? BrowserTypes.Normal
                              : BrowserTypes.Mobile;
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);
            this.Skin = Null.SetNullString(dr["Skin"]);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

                switch (reader.Name)
                {
                    case "portalAlias":
                        break;
                    case "portalID":
                        this.PortalID = reader.ReadElementContentAsInt();
                        break;
                    case "portalAliasID":
                        this.PortalAliasID = reader.ReadElementContentAsInt();
                        break;
                    case "HTTPAlias":
                        this.HTTPAlias = reader.ReadElementContentAsString();
                        break;
                    case "skin":
                        this.Skin = reader.ReadElementContentAsString();
                        break;
                    case "cultureCode":
                        this.CultureCode = reader.ReadElementContentAsString();
                        break;
                    case "browserType":
                        string type = reader.ReadElementContentAsString();
                        this.BrowserType = type.Equals("mobile", StringComparison.InvariantCultureIgnoreCase) ? BrowserTypes.Mobile : BrowserTypes.Normal;
                        break;
                    case "primary":
                        this.IsPrimary = reader.ReadElementContentAsBoolean();
                        break;
                    default:
                        if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
                        {
                            reader.ReadElementContentAsString();
                        }

                        break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("portalAlias");

            // write out properties
            writer.WriteElementString("portalID", this.PortalID.ToString());
            writer.WriteElementString("portalAliasID", this.PortalAliasID.ToString());
            writer.WriteElementString("HTTPAlias", this.HTTPAlias);
            writer.WriteElementString("skin", this.Skin);
            writer.WriteElementString("cultureCode", this.CultureCode);
            writer.WriteElementString("browserType", this.BrowserType.ToString().ToLowerInvariant());
            writer.WriteElementString("primary", this.IsPrimary.ToString().ToLowerInvariant());

            // Write end of main element
            writer.WriteEndElement();
        }
    }
}
