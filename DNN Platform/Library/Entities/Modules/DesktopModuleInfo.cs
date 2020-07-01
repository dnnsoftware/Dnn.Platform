// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Entities.Modules
    /// Class    : DesktopModuleInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DesktopModuleInfo provides the Entity Layer for Desktop Modules.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class DesktopModuleInfo : ContentItem, IXmlSerializable
    {
        private Dictionary<string, ModuleDefinitionInfo> _moduleDefinitions;
        private PageInfo _pageInfo;

        public DesktopModuleInfo()
        {
            this.IsPremium = Null.NullBoolean;
            this.IsAdmin = Null.NullBoolean;
            this.CodeSubDirectory = Null.NullString;
            this.PackageID = Null.NullInteger;
            this.DesktopModuleID = Null.NullInteger;
            this.SupportedFeatures = Null.NullInteger;
            this.Shareable = ModuleSharing.Unknown;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Module Definitions for this Desktop Module.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public Dictionary<string, ModuleDefinitionInfo> ModuleDefinitions
        {
            get
            {
                if (this._moduleDefinitions == null)
                {
                    if (this.DesktopModuleID > Null.NullInteger)
                    {
                        this._moduleDefinitions = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(this.DesktopModuleID);
                    }
                    else
                    {
                        this._moduleDefinitions = new Dictionary<string, ModuleDefinitionInfo>();
                    }
                }

                return this._moduleDefinitions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ID of the Desktop Module.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int DesktopModuleID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the ID of the Package for this Desktop Module.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int PackageID { get; set; }

        /// <summary>
        /// Gets or sets whether this has an associated Admin page.
        /// </summary>
        public string AdminPage { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the BusinessControllerClass of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string BusinessControllerClass { get; set; }

        public string Category
        {
            get
            {
                Term term = (from Term t in this.Terms select t).FirstOrDefault();
                return (term != null) ? term.Name : string.Empty;
            }

            set
            {
                this.Terms.Clear();
                ITermController termController = Util.GetTermController();
                var term = (from Term t in termController.GetTermsByVocabulary("Module_Categories")
                            where t.Name == value
                            select t)
                            .FirstOrDefault();
                if (term != null)
                {
                    this.Terms.Add(term);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the AppCode Folder Name of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string CodeSubDirectory { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets a Regular Expression that matches the versions of the core
        /// that this module is compatible with.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string CompatibleVersions { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets a list of Dependencies for the module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string Dependencies { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the  Description of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string Description { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Folder Name of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string FolderName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Friendly Name of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets whether this has an associated hostpage.
        /// </summary>
        public string HostPage { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Module is an Admin Module.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public bool IsAdmin { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Module is Portable.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public bool IsPortable
        {
            get
            {
                return this.GetFeature(DesktopModuleSupportedFeature.IsPortable);
            }

            set
            {
                this.UpdateFeature(DesktopModuleSupportedFeature.IsPortable, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Module is a Premium Module.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public bool IsPremium { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Module is Searchable.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public bool IsSearchable
        {
            get
            {
                return this.GetFeature(DesktopModuleSupportedFeature.IsSearchable);
            }

            set
            {
                this.UpdateFeature(DesktopModuleSupportedFeature.IsSearchable, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Module is Upgradable.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        public bool IsUpgradeable
        {
            get
            {
                return this.GetFeature(DesktopModuleSupportedFeature.IsUpgradeable);
            }

            set
            {
                this.UpdateFeature(DesktopModuleSupportedFeature.IsUpgradeable, value);
            }
        }

        /// <summary>
        /// Gets or sets is the module allowed to be shared across sites?.
        /// </summary>
        public ModuleSharing Shareable
        {
            get;
            set;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the  Name of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string ModuleName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets a list of Permissions for the module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string Permissions { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Supported Features of the Module.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        public int SupportedFeatures { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Version of the Desktop Module.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        public string Version { get; set; }

        public PageInfo Page
        {
            get
            {
                if (this._pageInfo == null && this.PackageID > Null.NullInteger)
                {
                    var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.PackageID);
                    if (package != null && !string.IsNullOrEmpty(package.Manifest))
                    {
                        var xmlDocument = new XmlDocument { XmlResolver = null };
                        xmlDocument.LoadXml(package.Manifest);
                        var pageNode = xmlDocument.SelectSingleNode("//package//components//component[@type=\"Module\"]//page");
                        if (pageNode != null)
                        {
                            this._pageInfo = CBO.DeserializeObject<PageInfo>(new StringReader(pageNode.OuterXml));
                        }
                    }
                }

                return this._pageInfo;
            }

            set
            {
                this._pageInfo = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a DesktopModuleInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public override void Fill(IDataReader dr)
        {
            this.DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
            this.PackageID = Null.SetNullInteger(dr["PackageID"]);
            this.ModuleName = Null.SetNullString(dr["ModuleName"]);
            this.FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.FolderName = Null.SetNullString(dr["FolderName"]);
            this.Version = Null.SetNullString(dr["Version"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.IsPremium = Null.SetNullBoolean(dr["IsPremium"]);
            this.IsAdmin = Null.SetNullBoolean(dr["IsAdmin"]);
            this.BusinessControllerClass = Null.SetNullString(dr["BusinessControllerClass"]);
            this.SupportedFeatures = Null.SetNullInteger(dr["SupportedFeatures"]);
            this.CompatibleVersions = Null.SetNullString(dr["CompatibleVersions"]);
            this.Dependencies = Null.SetNullString(dr["Dependencies"]);
            this.Permissions = Null.SetNullString(dr["Permissions"]);
            this.Shareable = (ModuleSharing)Null.SetNullInteger(dr["Shareable"]);
            this.AdminPage = Null.SetNullString(dr["AdminPage"]);
            this.HostPage = Null.SetNullString(dr["HostPage"]);

            // Call the base classes fill method to populate base class proeprties
            this.FillInternal(dr);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an XmlSchema for the DesktopModule.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a DesktopModuleInfo from an XmlReader.
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

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "moduleDefinitions" && !reader.IsEmptyElement)
                {
                    this.ReadModuleDefinitions(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "supportedFeatures" && !reader.IsEmptyElement)
                {
                    this.ReadSupportedFeatures(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "shareable" && !reader.IsEmptyElement)
                {
                    this.ReadModuleSharing(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "page" && !reader.IsEmptyElement)
                {
                    this.ReadPageInfo(reader);
                }
                else
                {
                    switch (reader.Name)
                    {
                        case "desktopModule":
                            break;
                        case "moduleName":
                            this.ModuleName = reader.ReadElementContentAsString();
                            break;
                        case "foldername":
                            this.FolderName = reader.ReadElementContentAsString();
                            break;
                        case "businessControllerClass":
                            this.BusinessControllerClass = reader.ReadElementContentAsString();
                            break;
                        case "codeSubDirectory":
                            this.CodeSubDirectory = reader.ReadElementContentAsString();
                            break;
                        case "page":
                            this.ReadPageInfo(reader);

                            if (this.Page.HasAdminPage())
                            {
                                this.AdminPage = this.Page.Name;
                            }

                            if (this.Page.HasHostPage())
                            {
                                this.HostPage = this.Page.Name;
                            }

                            break;
                        case "isAdmin":
                            bool isAdmin;
                            bool.TryParse(reader.ReadElementContentAsString(), out isAdmin);
                            this.IsAdmin = isAdmin;
                            break;
                        case "isPremium":
                            bool isPremium;
                            bool.TryParse(reader.ReadElementContentAsString(), out isPremium);
                            this.IsPremium = isPremium;
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
        /// Writes a DesktopModuleInfo to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("desktopModule");

            // write out properties
            writer.WriteElementString("moduleName", this.ModuleName);
            writer.WriteElementString("foldername", this.FolderName);
            writer.WriteElementString("businessControllerClass", this.BusinessControllerClass);
            if (!string.IsNullOrEmpty(this.CodeSubDirectory))
            {
                writer.WriteElementString("codeSubDirectory", this.CodeSubDirectory);
            }

            // Write out Supported Features
            writer.WriteStartElement("supportedFeatures");
            if (this.IsPortable)
            {
                writer.WriteStartElement("supportedFeature");
                writer.WriteAttributeString("type", "Portable");
                writer.WriteEndElement();
            }

            if (this.IsSearchable)
            {
                writer.WriteStartElement("supportedFeature");
                writer.WriteAttributeString("type", "Searchable");
                writer.WriteEndElement();
            }

            if (this.IsUpgradeable)
            {
                writer.WriteStartElement("supportedFeature");
                writer.WriteAttributeString("type", "Upgradeable");
                writer.WriteEndElement();
            }

            // Write end of Supported Features
            writer.WriteEndElement();

            // Write admin/host page info.
            if (this.Page != null)
            {
                this.Page.WriteXml(writer);
            }

            // Module sharing
            if (this.Shareable != ModuleSharing.Unknown)
            {
                writer.WriteStartElement("shareable");
                switch (this.Shareable)
                {
                    case ModuleSharing.Supported:
                        writer.WriteString("Supported");
                        break;
                    case ModuleSharing.Unsupported:
                        writer.WriteString("Unsupported");
                        break;
                }

                writer.WriteEndElement();
            }

            // Write start of Module Definitions
            writer.WriteStartElement("moduleDefinitions");

            // Iterate through definitions
            foreach (ModuleDefinitionInfo definition in this.ModuleDefinitions.Values)
            {
                definition.WriteXml(writer);
            }

            // Write end of Module Definitions
            writer.WriteEndElement();

            // Write end of main element
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Clears a Feature from the Features.
        /// </summary>
        /// <param name="feature">The feature to Clear.</param>
        /// -----------------------------------------------------------------------------
        private void ClearFeature(DesktopModuleSupportedFeature feature)
        {
            // And with the 1's complement of Feature to Clear the Feature flag
            this.SupportedFeatures = this.SupportedFeatures & ~((int)feature);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Feature from the Features.
        /// </summary>
        /// <param name="feature">The feature to Get.</param>
        /// -----------------------------------------------------------------------------
        private bool GetFeature(DesktopModuleSupportedFeature feature)
        {
            return this.SupportedFeatures > Null.NullInteger && (this.SupportedFeatures & (int)feature) == (int)feature;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets a Feature in the Features.
        /// </summary>
        /// <param name="feature">The feature to Set.</param>
        /// -----------------------------------------------------------------------------
        private void SetFeature(DesktopModuleSupportedFeature feature)
        {
            this.SupportedFeatures |= (int)feature;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Feature in the Features.
        /// </summary>
        /// <param name="feature">The feature to Set.</param>
        /// <param name="isSet">A Boolean indicating whether to set or clear the feature.</param>
        /// -----------------------------------------------------------------------------
        private void UpdateFeature(DesktopModuleSupportedFeature feature, bool isSet)
        {
            if (isSet)
            {
                this.SetFeature(feature);
            }
            else
            {
                this.ClearFeature(feature);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a Supported Features from an XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        /// -----------------------------------------------------------------------------
        private void ReadSupportedFeatures(XmlReader reader)
        {
            this.SupportedFeatures = 0;
            reader.ReadStartElement("supportedFeatures");
            do
            {
                if (reader.HasAttributes)
                {
                    reader.MoveToFirstAttribute();
                    switch (reader.ReadContentAsString())
                    {
                        case "Portable":
                            this.IsPortable = true;
                            break;
                        case "Searchable":
                            this.IsSearchable = true;
                            break;
                        case "Upgradeable":
                            this.IsUpgradeable = true;
                            break;
                    }
                }
            }
            while (reader.ReadToNextSibling("supportedFeature"));
        }

        private void ReadModuleSharing(XmlReader reader)
        {
            var sharing = reader.ReadElementString("shareable");

            if (string.IsNullOrEmpty(sharing))
            {
                this.Shareable = ModuleSharing.Unknown;
            }
            else
            {
                switch (sharing.ToLowerInvariant())
                {
                    case "supported":
                        this.Shareable = ModuleSharing.Supported;
                        break;
                    case "unsupported":
                        this.Shareable = ModuleSharing.Unsupported;
                        break;
                    default:
                    case "unknown":
                        this.Shareable = ModuleSharing.Unknown;
                        break;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a Module Definitions from an XmlReader.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        /// -----------------------------------------------------------------------------
        private void ReadModuleDefinitions(XmlReader reader)
        {
            reader.ReadStartElement("moduleDefinitions");
            do
            {
                reader.ReadStartElement("moduleDefinition");

                // Create new ModuleDefinition object
                var moduleDefinition = new ModuleDefinitionInfo();

                // Load it from the Xml
                moduleDefinition.ReadXml(reader);

                // Add to the collection
                this.ModuleDefinitions.Add(moduleDefinition.FriendlyName, moduleDefinition);
            }
            while (reader.ReadToNextSibling("moduleDefinition"));
        }

        private void ReadPageInfo(XmlReader reader)
        {
            this.Page = new PageInfo();

            // Load it from the Xml
            this.Page.ReadXml(reader.ReadSubtree());

            if (this.Page.HasAdminPage())
            {
                this.AdminPage = this.Page.Name;
            }

            if (this.Page.HasHostPage())
            {
                this.HostPage = this.Page.Name;
            }
        }

        [Serializable]
        public class PageInfo : IXmlSerializable
        {
            [XmlAttribute("type")]
            public string Type { get; set; }

            [XmlAttribute("common")]
            public bool IsCommon { get; set; }

            [XmlElement("name")]
            public string Name { get; set; }

            [XmlElement("icon")]
            public string Icon { get; set; }

            [XmlElement("largeIcon")]
            public string LargeIcon { get; set; }

            [XmlElement("description")]
            public string Description { get; set; }

            public bool HasAdminPage()
            {
                return this.Type.IndexOf("admin", StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger;
            }

            public bool HasHostPage()
            {
                return this.Type.IndexOf("host", StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger;
            }

            public XmlSchema GetSchema()
            {
                return null;
            }

            public void ReadXml(XmlReader reader)
            {
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "page")
                        {
                            break;
                        }

                        reader.Read();
                        continue;
                    }

                    if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        reader.Read();
                        continue;
                    }

                    switch (reader.Name)
                    {
                        case "page":
                            this.Type = reader.GetAttribute("type");
                            var commonValue = reader.GetAttribute("common");
                            if (!string.IsNullOrEmpty(commonValue))
                            {
                                this.IsCommon = commonValue.Equals("true", StringComparison.InvariantCultureIgnoreCase);
                            }

                            reader.Read();
                            break;
                        case "name":
                            this.Name = reader.ReadElementContentAsString();
                            break;
                        case "icon":
                            this.Icon = reader.ReadElementContentAsString();
                            break;
                        case "largeIcon":
                            this.LargeIcon = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }
            }

            public void WriteXml(XmlWriter writer)
            {
                // Write start of main elemenst
                writer.WriteStartElement("page");
                writer.WriteAttributeString("type", this.Type);
                writer.WriteAttributeString("common", this.IsCommon.ToString().ToLowerInvariant());

                // write out properties
                writer.WriteElementString("name", this.Name);
                writer.WriteElementString("icon", this.Icon);
                writer.WriteElementString("largeIcon", this.LargeIcon);
                writer.WriteElementString("description", this.Description);

                // Write end of main element
                writer.WriteEndElement();
            }
        }
    }
}
