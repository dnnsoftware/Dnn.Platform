// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules.Definitions
    /// Class    : ModuleDefinitionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleDefinitionInfo provides the Entity Layer for Module Definitions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ModuleDefinitionInfo : IXmlSerializable, IHydratable
    {
        private Dictionary<string, ModuleControlInfo> _ModuleControls;
        private string _definitionName;

        public ModuleDefinitionInfo()
        {
            this.Permissions = new Dictionary<string, PermissionInfo>();
            this.DesktopModuleID = Null.NullInteger;
            this.ModuleDefID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Dictionary of ModuleControls that are part of this definition.
        /// </summary>
        /// <returns>A Dictionary(Of String, ModuleControlInfo).</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, ModuleControlInfo> ModuleControls
        {
            get
            {
                if (this._ModuleControls == null)
                {
                    this.LoadControls();
                }

                return this._ModuleControls;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module Definition ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Default Cache Time.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int DefaultCacheTime { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the associated Desktop Module ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int DesktopModuleID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Friendly Name.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the DefinitionName.
        /// </summary>
        public string DefinitionName
        {
            get
            {
                if (string.IsNullOrEmpty(this._definitionName))
                {
                    return this.FriendlyName;
                }

                return this._definitionName;
            }

            set { this._definitionName = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Dictionary of Permissions that are part of this definition.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, PermissionInfo> Permissions { get; private set; }

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
                return this.ModuleDefID;
            }

            set
            {
                this.ModuleDefID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModuleDefinitionInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            this.DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
            this.DefaultCacheTime = Null.SetNullInteger(dr["DefaultCacheTime"]);
            this.FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            if (dr.GetSchemaTable().Select("ColumnName = 'DefinitionName'").Length > 0)
            {
                this.DefinitionName = Null.SetNullString(dr["DefinitionName"]);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the ModuleDefinitionInfo.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a ModuleDefinitionInfo from an XmlReader.
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

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "moduleControls")
                {
                    this.ReadModuleControls(reader);
                }
                else
                {
                    switch (reader.Name)
                    {
                        case "moduleDefinition":
                            break;
                        case "friendlyName":
                            this.FriendlyName = reader.ReadElementContentAsString();
                            break;
                        case "defaultCacheTime":
                            string elementvalue = reader.ReadElementContentAsString();
                            if (!string.IsNullOrEmpty(elementvalue))
                            {
                                this.DefaultCacheTime = int.Parse(elementvalue);
                            }

                            break;
                        case "permissions": // Ignore permissons node
                            reader.Skip();
                            break;
                        case "definitionName":
                            this.DefinitionName = reader.ReadElementContentAsString();
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
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a ModuleDefinitionInfo to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("moduleDefinition");

            // write out properties
            writer.WriteElementString("friendlyName", this.FriendlyName);
            writer.WriteElementString("definitionName", this.DefinitionName);
            writer.WriteElementString("defaultCacheTime", this.DefaultCacheTime.ToString());

            // Write start of Module Controls
            writer.WriteStartElement("moduleControls");

            // Iterate through controls
            foreach (ModuleControlInfo control in this.ModuleControls.Values)
            {
                control.WriteXml(writer);
            }

            // Write end of Module Controls
            writer.WriteEndElement();

            // Write end of main element
            writer.WriteEndElement();
        }

        public void LoadControls()
        {
            this._ModuleControls = this.ModuleDefID > Null.NullInteger ? ModuleControlController.GetModuleControlsByModuleDefinitionID(this.ModuleDefID) : new Dictionary<string, ModuleControlInfo>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads the ModuleControls from an XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        /// -----------------------------------------------------------------------------
        private void ReadModuleControls(XmlReader reader)
        {
            reader.ReadStartElement("moduleControls");
            do
            {
                reader.ReadStartElement("moduleControl");
                var moduleControl = new ModuleControlInfo();
                moduleControl.ReadXml(reader);
                this.ModuleControls.Add(moduleControl.ControlKey, moduleControl);
            }
            while (reader.ReadToNextSibling("moduleControl"));
        }
    }
}
