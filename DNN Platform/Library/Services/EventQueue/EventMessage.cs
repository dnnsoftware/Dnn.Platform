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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.EventQueue
{
    public enum MessagePriority
    {
        High,
        Medium,
        Low
    }

    [Serializable]
    public class EventMessage
    {
		#region "Private Members"
		
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
		
		#endregion

		#region "Constructors"

        public EventMessage()
        {
            _attributes = new NameValueCollection();
        }
		
		#endregion

		#region "Public Properties"

        public int EventMessageID
        {
            get
            {
                return _eventMessageID;
            }
            set
            {
                _eventMessageID = value;
            }
        }

        public string ProcessorType
        {
            get
            {
                if (_processorType == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _processorType;
                }
            }
            set
            {
                _processorType = value;
            }
        }

        public string ProcessorCommand
        {
            get
            {
                if (_processorCommand == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _processorCommand;
                }
            }
            set
            {
                _processorCommand = value;
            }
        }

        public string Body
        {
            get
            {
                if (_body == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _body;
                }
            }
            set
            {
                _body = value;
            }
        }

        public string Sender
        {
            get
            {
                if (_sender == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _sender;
                }
            }
            set
            {
                _sender = value;
            }
        }

        public string Subscribers
        {
            get
            {
                if (_subscribers == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _subscribers;
                }
            }
            set
            {
                _subscribers = value;
            }
        }

        public string AuthorizedRoles
        {
            get
            {
                if (_authorizedRoles == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _authorizedRoles;
                }
            }
            set
            {
                _authorizedRoles = value;
            }
        }

        public MessagePriority Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = value;
            }
        }

        public string ExceptionMessage
        {
            get
            {
                if (_exceptionMessage == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _exceptionMessage;
                }
            }
            set
            {
                _exceptionMessage = value;
            }
        }

        public DateTime SentDate
        {
            get
            {
                return _sentDate.ToLocalTime();
            }
            set
            {
                _sentDate = value.ToUniversalTime();
            }
        }

        public DateTime ExpirationDate
        {
            get
            {
                return _expirationDate.ToLocalTime();
            }
            set
            {
                _expirationDate = value.ToUniversalTime();
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }
		
		#endregion

		#region "Public Methods"

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
				//Loop throug the Attributes
                do
                {
                    reader.ReadStartElement("Attribute");

                    //Load it from the Xml
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
					
                    //Add attribute to the collection
                    _attributes.Add(attName, attValue);
                } while (reader.ReadToNextSibling("Attribute"));
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

                foreach (string key in Attributes.Keys)
                {
                    writer.WriteStartElement("Attribute");

                    //Write the Name element
                    writer.WriteElementString("Name", key);

                    //Write the Value element
                    if (Attributes[key].IndexOfAny("<'>\"&".ToCharArray()) > -1)
                    {
                        writer.WriteStartElement("Value");
                        writer.WriteCData(Attributes[key]);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        //Write value
                        writer.WriteElementString("Value", Attributes[key]);
                    }

                    //Close the Attribute node
                    writer.WriteEndElement();
                }

                //Close the Attributes node
                writer.WriteEndElement();

                //Close Writer
                writer.Close();

                return sb.ToString();
            }
        }
		
		#endregion
    }
}