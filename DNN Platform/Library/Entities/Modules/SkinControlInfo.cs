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

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class    : SkinControlInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SkinControlInfo provides the Entity Layer for Skin Controls (SkinObjects).
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class SkinControlInfo : ControlInfo, IXmlSerializable, IHydratable
    {
        public SkinControlInfo()
        {
            this.PackageID = Null.NullInteger;
            this.SkinControlID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the SkinControl ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int SkinControlID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ID of the Package for this Desktop Module.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

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
                return this.SkinControlID;
            }

            set
            {
                this.SkinControlID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a SkinControlInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.SkinControlID = Null.SetNullInteger(dr["SkinControlID"]);
            this.PackageID = Null.SetNullInteger(dr["PackageID"]);
            this.FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the SkinControlInfo.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a SkinControlInfo from an XmlReader.
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
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a SkinControlInfo to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("moduleControl");

            // write out properties
            this.WriteXmlInternal(writer);

            // Write end of main element
            writer.WriteEndElement();
        }
    }
}
