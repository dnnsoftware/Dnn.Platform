#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security.Permissions;

#endregion

namespace DotNetNuke.Entities.Modules.Definitions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules.Definitions
    /// Class	 : ModuleDefinitionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModuleDefinitionInfo provides the Entity Layer for Module Definitions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ModuleDefinitionInfo : IXmlSerializable, IHydratable
    {
        private Dictionary<string, ModuleControlInfo> _ModuleControls;
        private string _definitionName;

        public ModuleDefinitionInfo()
        {
            Permissions = new Dictionary<string, PermissionInfo>();
            DesktopModuleID = Null.NullInteger;
            ModuleDefID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Module Definition ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Default Cache Time
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int DefaultCacheTime { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the associated Desktop Module ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int DesktopModuleID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Friendly Name
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets the DefinitionName
        /// </summary>
        public string DefinitionName
        {
            get
            {
                if(String.IsNullOrEmpty(_definitionName))
                {
                    return FriendlyName;
                }

                return _definitionName;
            }

            set { _definitionName = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Dictionary of ModuleControls that are part of this definition
        /// </summary>
        /// <returns>A Dictionary(Of String, ModuleControlInfo)</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, ModuleControlInfo> ModuleControls
        {
            get
            {
                if (_ModuleControls == null)
                {
                    LoadControls();
                }
                return _ModuleControls;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Dictionary of Permissions that are part of this definition
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, PermissionInfo> Permissions { get; private set; }

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModuleDefinitionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
            DefaultCacheTime = Null.SetNullInteger(dr["DefaultCacheTime"]);
            FriendlyName = Null.SetNullString(dr["FriendlyName"]);
			if (dr.GetSchemaTable().Select("ColumnName = 'DefinitionName'").Length > 0)
			{
				DefinitionName = Null.SetNullString(dr["DefinitionName"]);
			}
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
                return ModuleDefID;
            }
            set
            {
                ModuleDefID = value;
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the ModuleDefinitionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a ModuleDefinitionInfo from an XmlReader
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
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "moduleControls")
                {
                    ReadModuleControls(reader);
                }
                else
                {
                    switch (reader.Name)
                    {
                        case "moduleDefinition":
                            break;
                        case "friendlyName":
                            FriendlyName = reader.ReadElementContentAsString();
                            break;
                        case "defaultCacheTime":
                            string elementvalue = reader.ReadElementContentAsString();
                            if (!string.IsNullOrEmpty(elementvalue))
                            {
                                DefaultCacheTime = int.Parse(elementvalue);
                            }
                            break;
                        case "permissions": //Ignore permissons node
                            reader.Skip();
                            break;
                        case "definitionName":
                            DefinitionName = reader.ReadElementContentAsString();
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
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Writes a ModuleDefinitionInfo to an XmlWriter
        /// </summary>
        /// <param name="writer">The XmlWriter to use</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("moduleDefinition");

            //write out properties
            writer.WriteElementString("friendlyName", FriendlyName);
            writer.WriteElementString("definitionName", DefinitionName);
            writer.WriteElementString("defaultCacheTime", DefaultCacheTime.ToString());

            //Write start of Module Controls
            writer.WriteStartElement("moduleControls");
            //Iterate through controls
            foreach (ModuleControlInfo control in ModuleControls.Values)
            {
                control.WriteXml(writer);
            }
            //Write end of Module Controls
            writer.WriteEndElement();

            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion

        public void LoadControls()
        {
            _ModuleControls = ModuleDefID > Null.NullInteger ? ModuleControlController.GetModuleControlsByModuleDefinitionID(ModuleDefID) : new Dictionary<string, ModuleControlInfo>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads the ModuleControls from an XmlReader
        /// </summary>
        /// <param name="reader">The XmlReader to use</param>
        /// -----------------------------------------------------------------------------
        private void ReadModuleControls(XmlReader reader)
        {
            reader.ReadStartElement("moduleControls");
            do
            {
                reader.ReadStartElement("moduleControl");
                var moduleControl = new ModuleControlInfo();
                moduleControl.ReadXml(reader);
                ModuleControls.Add(moduleControl.ControlKey, moduleControl);
            } while (reader.ReadToNextSibling("moduleControl"));
        }
    }
}
