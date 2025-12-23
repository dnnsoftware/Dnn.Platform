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

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Urls;

    using NewBrowserType = DotNetNuke.Abstractions.Urls.BrowserTypes;

    /// <inheritdoc />
    [Serializable]
    public class PortalAliasInfo : BaseEntityInfo, IHydratable, IXmlSerializable, IPortalAliasInfo
    {
        /// <summary>Initializes a new instance of the <see cref="PortalAliasInfo"/> class.</summary>
        public PortalAliasInfo()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalAliasInfo"/> class.</summary>
        /// <param name="alias">The alias to clone.</param>
        public PortalAliasInfo(PortalAliasInfo alias)
            : this((IPortalAliasInfo)alias)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalAliasInfo"/> class.</summary>
        /// <param name="alias">The alias to clone.</param>
        public PortalAliasInfo(IPortalAliasInfo alias)
        {
            this.ThisAsInterface.HttpAlias = alias.HttpAlias;
            this.ThisAsInterface.PortalAliasId = alias.PortalAliasId;
            this.ThisAsInterface.PortalId = alias.PortalId;
            this.IsPrimary = alias.IsPrimary;
            this.Redirect = alias.Redirect;
            this.ThisAsInterface.BrowserType = alias.BrowserType;
            this.CultureCode = alias.CultureCode;
            this.Skin = alias.Skin;
        }

        /// <inheritdoc />
        string IPortalAliasInfo.HttpAlias { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.HttpAlias instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string HTTPAlias
        {
            get => this.ThisAsInterface.HttpAlias;
            set => this.ThisAsInterface.HttpAlias = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        int IPortalAliasInfo.PortalAliasId { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.PortalAliasId instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int PortalAliasID
        {
            get => this.ThisAsInterface.PortalAliasId;
            set => this.ThisAsInterface.PortalAliasId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        int IPortalAliasInfo.PortalId { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.PortalId instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int PortalID
        {
            get => this.ThisAsInterface.PortalId;
            set => this.ThisAsInterface.PortalId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        public bool IsPrimary { get; set; }

        /// <inheritdoc />
        public bool Redirect { get; set; }

        /// <summary>Gets or sets the Browser Type.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.BrowserType instead. Scheduled removal in v11.0.0.")]
        public BrowserTypes BrowserType
        {
            get => (BrowserTypes)this.ThisAsInterface.BrowserType;
            set => this.ThisAsInterface.BrowserType = (NewBrowserType)value;
        }

        /// <inheritdoc />
        NewBrowserType IPortalAliasInfo.BrowserType { get; set; }

        /// <inheritdoc />
        public string CultureCode { get; set; }

        /// <inheritdoc />
        public string Skin { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get { return this.ThisAsInterface.PortalAliasId; }
            set { this.ThisAsInterface.PortalAliasId = value; }
        }

        private IPortalAliasInfo ThisAsInterface => this;

        /// <inheritdoc/>
        public void Fill(IDataReader dr)
        {
            this.FillInternal(dr);

            this.ThisAsInterface.PortalAliasId = Null.SetNullInteger(dr["PortalAliasID"]);
            this.ThisAsInterface.PortalId = Null.SetNullInteger(dr["PortalID"]);
            this.ThisAsInterface.HttpAlias = Null.SetNullString(dr["HTTPAlias"]);
            this.IsPrimary = Null.SetNullBoolean(dr["IsPrimary"]);
            var browserType = Null.SetNullString(dr["BrowserType"]);
            this.BrowserType = string.IsNullOrEmpty(browserType) || browserType.Equals("normal", StringComparison.OrdinalIgnoreCase)
                              ? BrowserTypes.Normal
                              : BrowserTypes.Mobile;
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);
            this.Skin = Null.SetNullString(dr["Skin"]);
        }

        /// <inheritdoc/>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <inheritdoc/>
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
                        this.ThisAsInterface.PortalId = reader.ReadElementContentAsInt();
                        break;
                    case "portalAliasID":
                        this.ThisAsInterface.PortalAliasId = reader.ReadElementContentAsInt();
                        break;
                    case "HTTPAlias":
                        this.ThisAsInterface.HttpAlias = reader.ReadElementContentAsString();
                        break;
                    case "skin":
                        this.Skin = reader.ReadElementContentAsString();
                        break;
                    case "cultureCode":
                        this.CultureCode = reader.ReadElementContentAsString();
                        break;
                    case "browserType":
                        string type = reader.ReadElementContentAsString();
                        this.BrowserType = type.Equals("mobile", StringComparison.OrdinalIgnoreCase) ? BrowserTypes.Mobile : BrowserTypes.Normal;
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

        /// <inheritdoc/>
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("portalAlias");

            // write out properties
            writer.WriteElementString("portalID", this.ThisAsInterface.PortalId.ToString());
            writer.WriteElementString("portalAliasID", this.ThisAsInterface.PortalAliasId.ToString());
            writer.WriteElementString("HTTPAlias", this.ThisAsInterface.HttpAlias);
            writer.WriteElementString("skin", this.Skin);
            writer.WriteElementString("cultureCode", this.CultureCode);
            writer.WriteElementString("browserType", this.BrowserType.ToString().ToLowerInvariant());
            writer.WriteElementString("primary", this.IsPrimary.ToString().ToLowerInvariant());

            // Write end of main element
            writer.WriteEndElement();
        }
    }
}
