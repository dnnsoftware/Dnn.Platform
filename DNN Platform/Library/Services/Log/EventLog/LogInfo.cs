#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.IO;
using System.Text;
using System.Xml;
using DotNetNuke.Services.Exceptions;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    [Serializable]
    public class LogInfo
    {
		#region Constructors

        public LogInfo()
        {
            LogGUID = Guid.NewGuid().ToString();
            BypassBuffering = false;
            LogProperties = new LogProperties();
            LogPortalID = -1;
            LogPortalName = "";
            LogUserID = -1;
            LogEventID = -1;
            LogUserName = "";
			Exception = new ExceptionInfo();
        }

        public LogInfo(string content) : this()
        {
            Deserialize(content);
        }
		
		#endregion

		#region "Properties"

        public string LogGUID { get; set; }

        public string LogFileID { get; set; }

        public string LogTypeKey { get; set; }

        public int LogUserID { get; set; }

        public int LogEventID { get; set; }

        public string LogUserName { get; set; }

        public int LogPortalID { get; set; }

        public string LogPortalName { get; set; }

        public DateTime LogCreateDate { get; set; }

        public long LogCreateDateNum { get; set; }

        public LogProperties LogProperties { get; set; }

        public bool BypassBuffering { get; set; }

        public string LogServerName { get; set; }

        public string LogConfigID { get; set; }

		public ExceptionInfo Exception { get; set; }

        #endregion

		#region Public Methods

        public void AddProperty(string PropertyName, string PropertyValue)
        {
            try
            {
                if (PropertyValue == null)
                {
                    PropertyValue = string.Empty;
                }
                if (PropertyName.Length > 50)
                {
                    PropertyName = PropertyName.Substring(0, 50);
                }
                if (PropertyValue.Length > 500)
                {
                    PropertyValue = "(TRUNCATED TO 500 CHARS): " + PropertyValue.Substring(0, 500);
                }
                var objLogDetailInfo = new LogDetailInfo();
                objLogDetailInfo.PropertyName = PropertyName;
                objLogDetailInfo.PropertyValue = PropertyValue;
                LogProperties.Add(objLogDetailInfo);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }

        public void Deserialize(string content)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(content)))
            {
                if (reader.Read())
                {
                    ReadXml(reader);
                }
                reader.Close();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    switch (reader.Name)
                    {
                        case "LogGUID":
                            LogGUID = reader.ReadContentAsString();
                            break;
                        case "LogFileID":
                            LogFileID = reader.ReadContentAsString();
                            break;
                        case "LogTypeKey":
                            LogTypeKey = reader.ReadContentAsString();
                            break;
                        case "LogUserID":
                            LogUserID = reader.ReadContentAsInt();
                            break;
                        case "LogEventID":
                            LogEventID = reader.ReadContentAsInt();
                            break;
                        case "LogUserName":
                            LogUserName = reader.ReadContentAsString();
                            break;
                        case "LogPortalID":
                            LogPortalID = reader.ReadContentAsInt();
                            break;
                        case "LogPortalName":
                            LogPortalName = reader.ReadContentAsString();
                            break;
                        case "LogCreateDate":
                            LogCreateDate = DateTime.Parse(reader.ReadContentAsString());
                            break;
                        case "LogCreateDateNum":
                            LogCreateDateNum = reader.ReadContentAsLong();
                            break;
                        case "BypassBuffering":
                            BypassBuffering = bool.Parse(reader.ReadContentAsString());
                            break;
                        case "LogServerName":
                            LogServerName = reader.ReadContentAsString();
                            break;
						case "LogConfigID":
							LogConfigID = reader.ReadContentAsString();
							break;
					}
                }
            }
			
            //Check for LogProperties child node
            reader.Read();
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "LogProperties")
            {
                reader.ReadStartElement("LogProperties");
                if (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !String.IsNullOrEmpty(reader.LocalName))
                {
                    LogProperties.ReadXml(reader);
                }
			}
			//Check for Exception child node
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Exception")
	        {
				Exception.ReadXml(reader);
	        }
        }

        public static bool IsSystemType(string PropName)
        {
            switch (PropName)
            {
                case "LogGUID":
                case "LogFileID":
                case "LogTypeKey":
                case "LogCreateDate":
                case "LogCreateDateNum":
                case "LogPortalID":
                case "LogPortalName":
                case "LogUserID":
                case "LogUserName":
                case "BypassBuffering":
                case "LogServerName":
                    return true;
            }
            return false;
        }

        public string Serialize()
        {
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.OmitXmlDeclaration = true;
            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                WriteXml(writer);
                writer.Close();
                return sb.ToString();
            }
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append("<p><strong>LogGUID:</strong>" + LogGUID + "</p>");
            str.Append("<p><strong>LogType:</strong>" + LogTypeKey + "</p>");
            str.Append("<p><strong>UserID:</strong>" + LogUserID + "</p>");
            str.Append("<p><strong>EventID:</strong>" + LogEventID + "</p>");
            str.Append("<p><strong>Username:</strong>" + LogUserName + "</p>");
            str.Append("<p><strong>PortalID:</strong>" + LogPortalID + "</p>");
            str.Append("<p><strong>PortalName:</strong>" + LogPortalName + "</p>");
            str.Append("<p><strong>CreateDate:</strong>" + LogCreateDate + "</p>");
            str.Append("<p><strong>ServerName:</strong>" + LogServerName + "</p>");
            str.Append(LogProperties.ToString());
	        if (!string.IsNullOrEmpty(Exception.ExceptionHash))
	        {
				str.Append(Exception.ToString());
			}
            return str.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("log");
            writer.WriteAttributeString("LogGUID", LogGUID);
            writer.WriteAttributeString("LogFileID", LogFileID);
            writer.WriteAttributeString("LogTypeKey", LogTypeKey);
            writer.WriteAttributeString("LogUserID", LogUserID.ToString());
            writer.WriteAttributeString("LogEventID", LogEventID.ToString());
            writer.WriteAttributeString("LogUserName", LogUserName);
            writer.WriteAttributeString("LogPortalID", LogPortalID.ToString());
            writer.WriteAttributeString("LogPortalName", LogPortalName);
            writer.WriteAttributeString("LogCreateDate", LogCreateDate.ToString());
            writer.WriteAttributeString("LogCreateDateNum", LogCreateDateNum.ToString());
            writer.WriteAttributeString("BypassBuffering", BypassBuffering.ToString());
            writer.WriteAttributeString("LogServerName", LogServerName);
			writer.WriteAttributeString("LogConfigID", LogConfigID);
            LogProperties.WriteXml(writer);
	        if (!string.IsNullOrEmpty(Exception.ExceptionHash))
	        {
		        Exception.WriteXml(writer);
	        }
            writer.WriteEndElement();
        }
		
		#endregion
    }
}