// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Text;
    using System.Xml;

    [Serializable]
    public class LogDetailInfo
    {
        private string _PropertyName;
        private string _PropertyValue;

        public LogDetailInfo()
            : this(string.Empty, string.Empty)
        {
        }

        public LogDetailInfo(string name, string value)
        {
            this._PropertyName = name;
            this._PropertyValue = value;
        }

        public string PropertyName
        {
            get
            {
                return this._PropertyName;
            }

            set
            {
                this._PropertyName = value;
            }
        }

        public string PropertyValue
        {
            get
            {
                return this._PropertyValue;
            }

            set
            {
                this._PropertyValue = value;
            }
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("PropertyName");
            this.PropertyName = reader.ReadString();
            reader.ReadEndElement();
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement("PropertyValue");
                this.PropertyValue = reader.ReadString();
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
            sb.Append(this.PropertyName);
            sb.Append("</strong>: ");
            sb.Append(this.PropertyValue);
            sb.Append("</p>");
            return sb.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("LogProperty");
            writer.WriteElementString("PropertyName", this.PropertyName);
            writer.WriteElementString("PropertyValue", this.PropertyValue);
            writer.WriteEndElement();
        }
    }
}
