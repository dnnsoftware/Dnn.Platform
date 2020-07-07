// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Common.Utilities;

    public enum MessagePriority
    {
        High,
        Medium,
        Low,
    }

    [Serializable]
    public class EventMessage
    {
        private NameValueCollection _attributes;
        private string _authorizedRoles = Null.NullString;
        private string _body = Null.NullString;
        private int _eventMessageID = Null.NullInteger;
        private string _exceptionMessage = Null.NullString;
        private DateTime _expirationDate;
        private MessagePriority _priority = MessagePriority.Low;
        private string _processorCommand = Null.NullString;
        private string _processorType = Null.NullString;
        private string _sender = Null.NullString;
        private DateTime _sentDate;
        private string _subscribers = Null.NullString;

        public EventMessage()
        {
            this._attributes = new NameValueCollection();
        }

        public int EventMessageID
        {
            get
            {
                return this._eventMessageID;
            }

            set
            {
                this._eventMessageID = value;
            }
        }

        public string ProcessorType
        {
            get
            {
                if (this._processorType == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._processorType;
                }
            }

            set
            {
                this._processorType = value;
            }
        }

        public string ProcessorCommand
        {
            get
            {
                if (this._processorCommand == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._processorCommand;
                }
            }

            set
            {
                this._processorCommand = value;
            }
        }

        public string Body
        {
            get
            {
                if (this._body == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._body;
                }
            }

            set
            {
                this._body = value;
            }
        }

        public string Sender
        {
            get
            {
                if (this._sender == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._sender;
                }
            }

            set
            {
                this._sender = value;
            }
        }

        public string Subscribers
        {
            get
            {
                if (this._subscribers == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._subscribers;
                }
            }

            set
            {
                this._subscribers = value;
            }
        }

        public string AuthorizedRoles
        {
            get
            {
                if (this._authorizedRoles == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._authorizedRoles;
                }
            }

            set
            {
                this._authorizedRoles = value;
            }
        }

        public MessagePriority Priority
        {
            get
            {
                return this._priority;
            }

            set
            {
                this._priority = value;
            }
        }

        public string ExceptionMessage
        {
            get
            {
                if (this._exceptionMessage == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this._exceptionMessage;
                }
            }

            set
            {
                this._exceptionMessage = value;
            }
        }

        public DateTime SentDate
        {
            get
            {
                return this._sentDate.ToLocalTime();
            }

            set
            {
                this._sentDate = value.ToUniversalTime();
            }
        }

        public DateTime ExpirationDate
        {
            get
            {
                return this._expirationDate.ToLocalTime();
            }

            set
            {
                this._expirationDate = value.ToUniversalTime();
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return this._attributes;
            }

            set
            {
                this._attributes = value;
            }
        }

        public void DeserializeAttributes(string configXml)
        {
            string attName = Null.NullString;
            string attValue = Null.NullString;
            var settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlReader reader = XmlReader.Create(new StringReader(configXml));
            reader.ReadStartElement("Attributes");
            if (!reader.IsEmptyElement)
            {
                // Loop throug the Attributes
                do
                {
                    reader.ReadStartElement("Attribute");

                    // Load it from the Xml
                    reader.ReadStartElement("Name");
                    attName = reader.ReadString();
                    reader.ReadEndElement();
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement("Value");
                        attValue = reader.ReadString();
                        reader.ReadEndElement();
                    }
                    else
                    {
                        reader.Read();
                    }

                    // Add attribute to the collection
                    this._attributes.Add(attName, attValue);
                }
                while (reader.ReadToNextSibling("Attribute"));
            }
        }

        public string SerializeAttributes()
        {
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.OmitXmlDeclaration = true;

            var sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("Attributes");

                foreach (string key in this.Attributes.Keys)
                {
                    writer.WriteStartElement("Attribute");

                    // Write the Name element
                    writer.WriteElementString("Name", key);

                    // Write the Value element
                    if (this.Attributes[key].IndexOfAny("<'>\"&".ToCharArray()) > -1)
                    {
                        writer.WriteStartElement("Value");
                        writer.WriteCData(this.Attributes[key]);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        // Write value
                        writer.WriteElementString("Value", this.Attributes[key]);
                    }

                    // Close the Attribute node
                    writer.WriteEndElement();
                }

                // Close the Attributes node
                writer.WriteEndElement();

                // Close Writer
                writer.Close();

                return sb.ToString();
            }
        }
    }
}
