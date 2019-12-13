#region Usings

using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class	 : SkinControlInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SkinControlInfo provides the Entity Layer for Skin Controls (SkinObjects)
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class SkinControlInfo : ControlInfo, IXmlSerializable, IHydratable
    {
        public SkinControlInfo()
        {
            PackageID = Null.NullInteger;
            SkinControlID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the SkinControl ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int SkinControlID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the ID of the Package for this Desktop Module
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a SkinControlInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            SkinControlID = Null.SetNullInteger(dr["SkinControlID"]);
            PackageID = Null.SetNullInteger(dr["PackageID"]);
            FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return SkinControlID;
            }
            set
            {
                SkinControlID = value;
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the SkinControlInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a SkinControlInfo from an XmlReader
        /// </summary>
        /// <param name="reader">The XmlReader to use</param>
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
                ReadXmlInternal(reader);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a SkinControlInfo to an XmlWriter
        /// </summary>
        /// <param name="writer">The XmlWriter to use</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("moduleControl");

            //write out properties
            WriteXmlInternal(writer);

            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion
    }
}
