#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    [Serializable]
    public class JavaScriptLibrary : IXmlSerializable
    {
        public int JavaScriptLibraryID { get; set; }
        public int PackageID { get; set; }
        public string LibraryName { get; set; }
        public Version Version { get; set; }
        public string ObjectName { get; set; }
        public string FileName { get; set; }
        public ScriptLocation PreferredScriptLocation { get; set; }
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
                        case "librayName":
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
                            var content = reader.ReadElementContentAsString();
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
            writer.WriteElementString("librayName", LibraryName);
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
