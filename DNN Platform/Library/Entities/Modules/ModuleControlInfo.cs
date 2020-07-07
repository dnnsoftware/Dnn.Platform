// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Data;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class    : ModuleControlInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleControlInfo provides the Entity Layer for Module Controls.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ModuleControlInfo : ControlInfo, IXmlSerializable, IHydratable
    {
        public ModuleControlInfo()
        {
            this.ModuleControlID = Null.NullInteger;
            this.ModuleDefID = Null.NullInteger;
            this.ControlType = SecurityAccessLevel.Anonymous;
            this.SupportsPopUps = false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Control Title.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ControlTitle { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Control Type.
        /// </summary>
        /// <returns>A SecurityAccessLevel.</returns>
        /// -----------------------------------------------------------------------------
        public SecurityAccessLevel ControlType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Help URL.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string HelpURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Icon  Source.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string IconFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module Control ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int ModuleControlID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module Definition ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether to support popup.
        /// </summary>
        /// <returns>A Boolean value.</returns>
        /// -----------------------------------------------------------------------------
        public bool SupportsPopUps { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the View Order.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int ViewOrder { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return this.ModuleControlID;
            }

            set
            {
                this.ModuleControlID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModuleControlInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.ModuleControlID = Null.SetNullInteger(dr["ModuleControlID"]);
            this.FillInternal(dr);
            this.ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            this.ControlTitle = Null.SetNullString(dr["ControlTitle"]);
            this.IconFile = Null.SetNullString(dr["IconFile"]);
            this.HelpURL = Null.SetNullString(dr["HelpUrl"]);
            this.ControlType = (SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), Null.SetNullString(dr["ControlType"]));
            this.ViewOrder = Null.SetNullInteger(dr["ViewOrder"]);
            this.SupportsPopUps = Null.SetNullBoolean(dr["SupportsPopUps"]);

            // Call the base classes fill method to populate base class proeprties
            this.FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the ModuleControlInfo.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a ModuleControlInfo from an XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        /// -----------------------------------------------------------------------------
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

                this.ReadXmlInternal(reader);
                switch (reader.Name)
                {
                    case "controlTitle":
                        this.ControlTitle = reader.ReadElementContentAsString();
                        break;
                    case "controlType":
                        this.ControlType = (SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), reader.ReadElementContentAsString());
                        break;
                    case "iconFile":
                        this.IconFile = reader.ReadElementContentAsString();
                        break;
                    case "helpUrl":
                        this.HelpURL = reader.ReadElementContentAsString();
                        break;
                    case "supportsPopUps":
                        this.SupportsPopUps = bool.Parse(reader.ReadElementContentAsString());
                        break;
                    case "viewOrder":
                        string elementvalue = reader.ReadElementContentAsString();
                        if (!string.IsNullOrEmpty(elementvalue))
                        {
                            this.ViewOrder = int.Parse(elementvalue);
                        }

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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a ModuleControlInfo to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("moduleControl");

            // write out properties
            this.WriteXmlInternal(writer);
            writer.WriteElementString("controlTitle", this.ControlTitle);
            writer.WriteElementString("controlType", this.ControlType.ToString());
            writer.WriteElementString("iconFile", this.IconFile);
            writer.WriteElementString("helpUrl", this.HelpURL);
            writer.WriteElementString("supportsPopUps", this.SupportsPopUps.ToString());
            if (this.ViewOrder > Null.NullInteger)
            {
                writer.WriteElementString("viewOrder", this.ViewOrder.ToString());
            }

            // Write end of main element
            writer.WriteEndElement();
        }
    }
}
