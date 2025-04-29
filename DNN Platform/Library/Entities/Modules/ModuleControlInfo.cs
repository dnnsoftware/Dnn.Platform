// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules;

using System;
using System.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security;

/// <summary>ModuleControlInfo provides the Entity Layer for Module Controls.</summary>
[Serializable]
public class ModuleControlInfo : ControlInfo, IXmlSerializable, IHydratable
{
    /// <summary>Initializes a new instance of the <see cref="ModuleControlInfo"/> class.</summary>
    public ModuleControlInfo()
    {
        this.ModuleControlID = Null.NullInteger;
        this.ModuleDefID = Null.NullInteger;
        this.ControlType = SecurityAccessLevel.Anonymous;
        this.SupportsPopUps = false;
    }

    /// <summary>Gets or sets the Control Title.</summary>
    public string ControlTitle { get; set; }

    /// <summary>Gets or sets the Control Type.</summary>
    public SecurityAccessLevel ControlType { get; set; }

    /// <summary>Gets or sets the Help URL.</summary>
    public string HelpURL { get; set; }

    /// <summary>Gets or sets the Icon Source.</summary>
    public string IconFile { get; set; }

    /// <summary>Gets or sets the Module Control ID.</summary>
    public int ModuleControlID { get; set; }

    /// <summary>Gets or sets the Module Definition ID.</summary>
    public int ModuleDefID { get; set; }

    /// <summary>Gets or sets a value indicating whether to support popup.</summary>
    public bool SupportsPopUps { get; set; }

    /// <summary>Gets or sets the View Order.</summary>
    public int ViewOrder { get; set; }

    /// <inheritdoc />
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

    /// <summary>Fills a ModuleControlInfo from a Data Reader.</summary>
    /// <param name="dr">The Data Reader to use.</param>
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

        // Call the base classes fill method to populate base class properties
        this.FillInternal(dr);
    }

    /// <inheritdoc />
    public XmlSchema GetSchema()
    {
        return null;
    }

    /// <summary>Reads a ModuleControlInfo from an XmlReader.</summary>
    /// <param name="reader">The XmlReader to use.</param>
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

    /// <summary>Writes a ModuleControlInfo to an XmlWriter.</summary>
    /// <param name="writer">The XmlWriter to use.</param>
    public void WriteXml(XmlWriter writer)
    {
        // Write start of main elements
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
