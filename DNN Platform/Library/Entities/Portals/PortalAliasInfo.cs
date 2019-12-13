#region Usings

using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Urls;

#endregion

namespace DotNetNuke.Entities.Portals
{
    [Serializable]
    public class PortalAliasInfo : BaseEntityInfo, IHydratable, IXmlSerializable
    {
        public PortalAliasInfo() {}

        public PortalAliasInfo(PortalAliasInfo alias)
        {
            HTTPAlias = alias.HTTPAlias;
            PortalAliasID = alias.PortalAliasID;
            PortalID = alias.PortalID;
            IsPrimary = alias.IsPrimary;
            Redirect = alias.Redirect;
            BrowserType = alias.BrowserType;
            CultureCode = alias.CultureCode;
            Skin = alias.Skin;
        }

        #region Auto-Properties

        public string HTTPAlias { get; set; }
        public int PortalAliasID { get; set; }
        public int PortalID { get; set; }
        public bool IsPrimary { get; set; }
        public bool Redirect { get; set; }

        public BrowserTypes BrowserType { get; set; }
        public string CultureCode { get; set; }
        public string Skin { get; set; }

        #endregion

        #region IHydratable Members

        public int KeyID
        {
            get { return PortalAliasID; }
            set { PortalAliasID = value; }
        }

        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);

            PortalAliasID = Null.SetNullInteger(dr["PortalAliasID"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            HTTPAlias = Null.SetNullString(dr["HTTPAlias"]);
            IsPrimary = Null.SetNullBoolean(dr["IsPrimary"]);
            var browserType = Null.SetNullString(dr["BrowserType"]);
            BrowserType = String.IsNullOrEmpty(browserType) || browserType.Equals("normal", StringComparison.OrdinalIgnoreCase)
                              ? BrowserTypes.Normal
                              : BrowserTypes.Mobile;
            CultureCode = Null.SetNullString(dr["CultureCode"]);
            Skin = Null.SetNullString(dr["Skin"]);
        }

        #endregion

        #region IXmlSerializable Members

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
                        PortalID = reader.ReadElementContentAsInt();
                        break;
                    case "portalAliasID":
                        PortalAliasID = reader.ReadElementContentAsInt();
                        break;
                    case "HTTPAlias":
                        HTTPAlias = reader.ReadElementContentAsString();
                        break;
                    case "skin":
                        Skin = reader.ReadElementContentAsString();
                        break;
                    case "cultureCode":
                        CultureCode = reader.ReadElementContentAsString();
                        break;
                    case "browserType":
                        string type = reader.ReadElementContentAsString();
                        BrowserType = type.Equals("mobile", StringComparison.InvariantCultureIgnoreCase) ? BrowserTypes.Mobile : BrowserTypes.Normal;
                        break;
                    case "primary":
                        IsPrimary = reader.ReadElementContentAsBoolean();
                        break;
                    default:
                        if(reader.NodeType == XmlNodeType.Element && !String.IsNullOrEmpty(reader.Name))
                        {
                            reader.ReadElementContentAsString();
                        }
                        break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("portalAlias");

            //write out properties
            writer.WriteElementString("portalID", PortalID.ToString());
            writer.WriteElementString("portalAliasID", PortalAliasID.ToString());
            writer.WriteElementString("HTTPAlias", HTTPAlias);
            writer.WriteElementString("skin", Skin);
            writer.WriteElementString("cultureCode", CultureCode);
            writer.WriteElementString("browserType", BrowserType.ToString().ToLowerInvariant());
            writer.WriteElementString("primary", IsPrimary.ToString().ToLowerInvariant());

            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion

    }
}
