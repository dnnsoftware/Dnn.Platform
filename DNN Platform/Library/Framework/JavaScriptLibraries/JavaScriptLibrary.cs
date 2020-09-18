// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public class JavaScriptLibrary : IXmlSerializable
    {
        /// <summary>
        /// Gets or sets unique identifier id for a javscript library package.
        /// </summary>
        public int JavaScriptLibraryID { get; set; }

        /// <summary>
        /// Gets or sets package id associated with the javscript library package.
        /// </summary>
        public int PackageID { get; set; }

        /// <summary>
        /// Gets or sets name of the javscript library package (used when requesting library).
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// Gets or sets version of the the javscript library package from the database.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets main object (where relevant) of the javscript library package
        /// used to generate the local file fallback code in the case where the CDN file is not available.
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Gets or sets filename of the script in the filesystem.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets position in the page the script reference is injected.
        /// </summary>
        public ScriptLocation PreferredScriptLocation { get; set; }

        /// <summary>
        /// Gets or sets location of the content delivery network (CDN) where the script is loaded from when CDN has been enabled in host.
        /// </summary>
        public string CDNPath { get; set; }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a JavaScriptLibrary from an XmlReader.
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
                else
                {
                    switch (reader.Name)
                    {
                        case "javaScriptLibrary":
                            break;
                        case "libraryName":
                            this.LibraryName = reader.ReadElementContentAsString();
                            break;
                        case "objectName":
                            this.ObjectName = reader.ReadElementContentAsString();
                            break;
                        case "fileName":
                            this.FileName = reader.ReadElementContentAsString();
                            break;
                        case "preferredScriptLocation":
                            var location = reader.ReadElementContentAsString();
                            switch (location)
                            {
                                case "BodyTop":
                                    this.PreferredScriptLocation = ScriptLocation.BodyTop;
                                    break;
                                case "BodyBottom":
                                    this.PreferredScriptLocation = ScriptLocation.BodyBottom;
                                    break;
                                default:
                                    this.PreferredScriptLocation = ScriptLocation.PageHead;
                                    break;
                            }

                            break;
                        case "CDNPath":
                            this.CDNPath = reader.ReadElementContentAsString();
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
        /// Writes a JavaScriptLibrary to an XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            // Write start of main elemenst
            writer.WriteStartElement("javaScriptLibrary");

            // write out properties
            writer.WriteElementString("libraryName", this.LibraryName);
            writer.WriteElementString("fileName", this.FileName);
            writer.WriteElementString("objectName", this.ObjectName);
            writer.WriteElementString("preferredScriptLocation", this.PreferredScriptLocation.ToString());
            writer.WriteElementString("CDNPath", this.CDNPath);

            // Write end of main element
            writer.WriteEndElement();
        }
    }
}
