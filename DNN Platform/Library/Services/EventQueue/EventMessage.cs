// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue;

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;

using DotNetNuke.Common.Utilities;

public enum MessagePriority
{
    High = 0,
    Medium = 1,
    Low = 2,
}

[Serializable]
public class EventMessage
{
    private NameValueCollection attributes;
    private string authorizedRoles = Null.NullString;
    private string body = Null.NullString;
    private int eventMessageID = Null.NullInteger;
    private string exceptionMessage = Null.NullString;
    private DateTime expirationDate;
    private MessagePriority priority = MessagePriority.Low;
    private string processorCommand = Null.NullString;
    private string processorType = Null.NullString;
    private string sender = Null.NullString;
    private DateTime sentDate;
    private string subscribers = Null.NullString;

    /// <summary>Initializes a new instance of the <see cref="EventMessage"/> class.</summary>
    public EventMessage()
    {
        this.attributes = new NameValueCollection();
    }

    public int EventMessageID
    {
        get
        {
            return this.eventMessageID;
        }

        set
        {
            this.eventMessageID = value;
        }
    }

    public string ProcessorType
    {
        get
        {
            if (this.processorType == null)
            {
                return string.Empty;
            }
            else
            {
                return this.processorType;
            }
        }

        set
        {
            this.processorType = value;
        }
    }

    public string ProcessorCommand
    {
        get
        {
            if (this.processorCommand == null)
            {
                return string.Empty;
            }
            else
            {
                return this.processorCommand;
            }
        }

        set
        {
            this.processorCommand = value;
        }
    }

    public string Body
    {
        get
        {
            if (this.body == null)
            {
                return string.Empty;
            }
            else
            {
                return this.body;
            }
        }

        set
        {
            this.body = value;
        }
    }

    public string Sender
    {
        get
        {
            if (this.sender == null)
            {
                return string.Empty;
            }
            else
            {
                return this.sender;
            }
        }

        set
        {
            this.sender = value;
        }
    }

    public string Subscribers
    {
        get
        {
            if (this.subscribers == null)
            {
                return string.Empty;
            }
            else
            {
                return this.subscribers;
            }
        }

        set
        {
            this.subscribers = value;
        }
    }

    public string AuthorizedRoles
    {
        get
        {
            if (this.authorizedRoles == null)
            {
                return string.Empty;
            }
            else
            {
                return this.authorizedRoles;
            }
        }

        set
        {
            this.authorizedRoles = value;
        }
    }

    public MessagePriority Priority
    {
        get
        {
            return this.priority;
        }

        set
        {
            this.priority = value;
        }
    }

    public string ExceptionMessage
    {
        get
        {
            if (this.exceptionMessage == null)
            {
                return string.Empty;
            }
            else
            {
                return this.exceptionMessage;
            }
        }

        set
        {
            this.exceptionMessage = value;
        }
    }

    public DateTime SentDate
    {
        get
        {
            return this.sentDate.ToLocalTime();
        }

        set
        {
            this.sentDate = value.ToUniversalTime();
        }
    }

    public DateTime ExpirationDate
    {
        get
        {
            return this.expirationDate.ToLocalTime();
        }

        set
        {
            this.expirationDate = value.ToUniversalTime();
        }
    }

    public NameValueCollection Attributes
    {
        get
        {
            return this.attributes;
        }

        set
        {
            this.attributes = value;
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
                this.attributes.Add(attName, attValue);
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
