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
using System.Text;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    [Serializable]
    public class LogDetailInfo
    {
        private string _PropertyName;
        private string _PropertyValue;

        public LogDetailInfo() : this("", "")
        {
        }

        public LogDetailInfo(string name, string value)
        {
            _PropertyName = name;
            _PropertyValue = value;
        }

        public string PropertyName
        {
            get
            {
                return _PropertyName;
            }
            set
            {
                _PropertyName = value;
            }
        }

        public string PropertyValue
        {
            get
            {
                return _PropertyValue;
            }
            set
            {
                _PropertyValue = value;
            }
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("PropertyName");
            PropertyName = reader.ReadString();
            reader.ReadEndElement();
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement("PropertyValue");
                PropertyValue = reader.ReadString();
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("<p><strong>");
            sb.Append(PropertyName);
            sb.Append("</strong>: ");
            sb.Append(PropertyValue);
            sb.Append("</p>");
            return sb.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("LogProperty");
            writer.WriteElementString("PropertyName", PropertyName);
            writer.WriteElementString("PropertyValue", PropertyValue);
            writer.WriteEndElement();
        }
    }
}