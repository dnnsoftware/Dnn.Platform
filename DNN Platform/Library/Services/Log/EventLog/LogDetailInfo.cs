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
