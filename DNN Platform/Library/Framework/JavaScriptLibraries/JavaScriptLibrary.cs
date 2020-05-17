// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    [Serializable]
    public class JavaScriptLibrary : IXmlSerializable
    {
        /// <summary>
        /// unique identifier id for a javscript library package
        /// </summary>
        public int JavaScriptLibraryID { get; set; }
        /// <summary>
        /// package id associated with the javscript library package
        /// </summary>
        public int PackageID { get; set; }
        /// <summary>
        /// name of the javscript library package (used when requesting library)
        /// </summary>
        public string LibraryName { get; set; }
        /// <summary>
        /// version of the the javscript library package from the database
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// main object (where relevant) of the javscript library package
        /// used to generate the local file fallback code in the case where the CDN file is not available
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// filename of the script in the filesystem
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// position in the page the script reference is injected
        /// </summary>
        public ScriptLocation PreferredScriptLocation { get; set; }
        /// <summary>
        /// location of the content delivery network (CDN) where the script is loaded from when CDN has been enabled in host
        /// </summary>
        public string CDNPath { get; set; }

        #region IXmlSerializable Implementation

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Reads a JavaScriptLibrary from an XmlReader
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
                else
                {
                    switch (reader.Name)
                    {
                        case "javaScriptLibrary":
                            break;
                        case "libraryName":
                            LibraryName = reader.ReadElementContentAsString();
                            break;
                        case "objectName":
                            ObjectName = reader.ReadElementContentAsString();
                            break;
                        case "fileName":
                            FileName = reader.ReadElementContentAsString();
                            break;
                        case "preferredScriptLocation":
                            var location = reader.ReadElementContentAsString();
                            switch (location)
                            {
                                case "BodyTop":
                                    PreferredScriptLocation = ScriptLocation.BodyTop;
                                    break;
                                case "BodyBottom":
                                    PreferredScriptLocation = ScriptLocation.BodyBottom;
                                    break;
                                default:
                                    PreferredScriptLocation = ScriptLocation.PageHead;
                                    break;
                            }
                            break;
                        case "CDNPath":
                            CDNPath = reader.ReadElementContentAsString();
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
        /// Writes a JavaScriptLibrary to an XmlWriter
        /// </summary>
        /// <param name="writer">The XmlWriter to use</param>
        /// -----------------------------------------------------------------------------
        public void WriteXml(XmlWriter writer)
        {
            //Write start of main elemenst
            writer.WriteStartElement("javaScriptLibrary");

            //write out properties
            writer.WriteElementString("libraryName", LibraryName);
            writer.WriteElementString("fileName", FileName);
            writer.WriteElementString("objectName", ObjectName);
            writer.WriteElementString("preferredScriptLocation", PreferredScriptLocation.ToString());
            writer.WriteElementString("CDNPath", CDNPath);

            //Write end of main element
            writer.WriteEndElement();
        }

        #endregion
    }
}
