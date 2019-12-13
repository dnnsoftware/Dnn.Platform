// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Entities.Modules
{
	/// -----------------------------------------------------------------------------
	/// Project	 : DotNetNuke
	/// Namespace: DotNetNuke.Entities.Modules
	/// Class	 : ModuleControlInfo
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// ModuleControlInfo provides the Entity Layer for Module Controls
	/// </summary>
	/// -----------------------------------------------------------------------------
    [Serializable]
    public class ModuleControlInfo : ControlInfo, IXmlSerializable, IHydratable
    {
        public ModuleControlInfo()
        {
            ModuleControlID = Null.NullInteger;
            ModuleDefID = Null.NullInteger;
            ControlType = SecurityAccessLevel.Anonymous;
            SupportsPopUps = false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Control Title
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string ControlTitle { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Control Type
        /// </summary>
        /// <returns>A SecurityAccessLevel</returns>
        /// -----------------------------------------------------------------------------
        public SecurityAccessLevel ControlType { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Help URL
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string HelpURL { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Icon  Source
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string IconFile { get; set; }
		
		/// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Module Control ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int ModuleControlID { get; set; }
		
		/// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Module Definition ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether to support popup.
        /// </summary>
        /// <returns>A Boolean value</returns>
        /// -----------------------------------------------------------------------------
        public bool SupportsPopUps { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the View Order
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int ViewOrder { get; set; }

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModuleControlInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            ModuleControlID = Null.SetNullInteger(dr["ModuleControlID"]);
            FillInternal(dr);
            ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            ControlTitle = Null.SetNullString(dr["ControlTitle"]);
            IconFile = Null.SetNullString(dr["IconFile"]);
            HelpURL = Null.SetNullString(dr["HelpUrl"]);
            ControlType = (SecurityAccessLevel) Enum.Parse(typeof (SecurityAccessLevel), Null.SetNullString(dr["ControlType"]));
            ViewOrder = Null.SetNullInteger(dr["ViewOrder"]);
            SupportsPopUps = Null.SetNullBoolean(dr["SupportsPopUps"]);
			//Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);
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
                return ModuleControlID;
            }
            set
            {
                ModuleControlID = value;
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the ModuleControlInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a ModuleControlInfo from an XmlReader
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
                switch (reader.Name)
                {
                    case "controlTitle":
                        ControlTitle = reader.ReadElementContentAsString();
                        break;
                    case "controlType":
                        ControlType = (SecurityAccessLevel) Enum.Parse(typeof (SecurityAccessLevel), reader.ReadElementContentAsString());
                        break;
                    case "iconFile":
                        IconFile = reader.ReadElementContentAsString();
                        break;
                    case "helpUrl":
                        HelpURL = reader.ReadElementContentAsString();
                        break;
                    case "supportsPopUps":
                        SupportsPopUps = Boolean.Parse(reader.ReadElementContentAsString());
                        break;
                    case "viewOrder":
                        string elementvalue = reader.ReadElementContentAsString();
                        if (!string.IsNullOrEmpty(elementvalue))
                        {
                            ViewOrder = int.Parse(elementvalue);
                        }
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a ModuleControlInfo to an XmlWriter
        /// </summary>
        /// <param name="writer">The XmlWriter to use</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("moduleControl");

            //write out properties
            WriteXmlInternal(writer);
            writer.WriteElementString("controlTitle", ControlTitle);
            writer.WriteElementString("controlType", ControlType.ToString());
            writer.WriteElementString("iconFile", IconFile);
            writer.WriteElementString("helpUrl", HelpURL);
            writer.WriteElementString("supportsPopUps", SupportsPopUps.ToString());
            if (ViewOrder > Null.NullInteger)
            {
                writer.WriteElementString("viewOrder", ViewOrder.ToString());
            }
            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion
    }
}
