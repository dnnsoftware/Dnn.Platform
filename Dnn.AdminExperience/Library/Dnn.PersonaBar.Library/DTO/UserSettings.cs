#region Copyright
// 
// DotNetNuke® - https://www.dnnsoftware.com
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DotNetNuke.Common.Utilities;

namespace Dnn.PersonaBar.Library.DTO
{
    /// <summary>
    /// Persona Bar Settings For User.
    /// </summary>
    [DataContract]
    public class UserSettings : Dictionary<string, object>, IXmlSerializable
    {
        [IgnoreDataMember]
        public bool ExpandPersonaBar
        {
            get
            {
                return Convert.ToBoolean(this["expandPersonaBar"]);
            }
            set
            {
                this["expandPersonaBar"] = value;
            } 
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            switch (reader.Name)
            {
                case "data":
                    ReadSettings(reader);
                    break;
                default:
                    ReadLegacySettings(reader);
                    break;

            }
        }

        public void WriteXml(XmlWriter writer)
        {            
            writer.WriteStartElement("data");
            writer.WriteCData(Json.Serialize(this));
            writer.WriteEndElement();
        }

        private void ReadSettings(XmlReader reader)
        {
            reader.ReadStartElement("data");

            var settingsData = reader.ReadContentAsString();
            var settings = Json.Deserialize<IDictionary<string, object>>(settingsData);
            foreach (var key in settings.Keys)
            {
                this[key] = settings[key];
            }

            reader.ReadEndElement();
        }

        [Obsolete("The method add for backward compatible, should remove this in future release.")]
        private void ReadLegacySettings(XmlReader reader)
        {
            while(!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var settingName = reader.Name;
                reader.ReadStartElement();

                var settingValue = string.Empty;
                if (reader.NodeType == XmlNodeType.Text)
                {
                    settingValue = reader.ReadContentAsString();
                }

                if (!string.IsNullOrEmpty(settingValue))
                {
                    switch (settingName)
                    {
                        case "ExpandPersonaBar":
                            this["expandPersonaBar"] = Convert.ToBoolean(settingValue);
                            break;
                        case "ActiveIdentifier":
                            this["activeIdentifier"] = settingValue;
                            break;
                        case "ActivePath":
                            this["activePath"] = settingValue;
                            break;
                        case "ExpandTasksPane":
                            this["expandTasksPane"] = Convert.ToBoolean(settingValue);
                            break;
                        case "ComparativeTerm":
                            this["comparativeTerm"] = settingValue;
                            break;
                        case "EndDate":
                            this["endDate"] = Convert.ToDateTime(settingValue);
                            break;
                        case "Legends":
                            this["legends"] = settingValue.Split(',');
                            break;
                        case "Period":
                            this["period"] = settingValue;
                            break;
                        case "StartDate":
                            this["startDate"] = Convert.ToDateTime(settingValue);
                            break;
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                }
            }
        }
    }
}