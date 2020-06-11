﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.LogGUID = Guid.NewGuid().ToString();
            this.BypassBuffering = false;
            this.LogProperties = new LogProperties();
            this.LogPortalID = -1;
            this.LogPortalName = "";
            this.LogUserID = -1;
            this.LogEventID = -1;
            this.LogUserName = "";
			this.Exception = new ExceptionInfo();
        }

        public LogInfo(string content) : this()
        {
            this.Deserialize(content);
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
                this.LogProperties.Add(objLogDetailInfo);
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
                    this.ReadXml(reader);
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
                            this.LogGUID = reader.ReadContentAsString();
                            break;
                        case "LogFileID":
                            this.LogFileID = reader.ReadContentAsString();
                            break;
                        case "LogTypeKey":
                            this.LogTypeKey = reader.ReadContentAsString();
                            break;
                        case "LogUserID":
                            this.LogUserID = reader.ReadContentAsInt();
                            break;
                        case "LogEventID":
                            this.LogEventID = reader.ReadContentAsInt();
                            break;
                        case "LogUserName":
                            this.LogUserName = reader.ReadContentAsString();
                            break;
                        case "LogPortalID":
                            this.LogPortalID = reader.ReadContentAsInt();
                            break;
                        case "LogPortalName":
                            this.LogPortalName = reader.ReadContentAsString();
                            break;
                        case "LogCreateDate":
                            this.LogCreateDate = DateTime.Parse(reader.ReadContentAsString());
                            break;
                        case "LogCreateDateNum":
                            this.LogCreateDateNum = reader.ReadContentAsLong();
                            break;
                        case "BypassBuffering":
                            this.BypassBuffering = bool.Parse(reader.ReadContentAsString());
                            break;
                        case "LogServerName":
                            this.LogServerName = reader.ReadContentAsString();
                            break;
						case "LogConfigID":
							this.LogConfigID = reader.ReadContentAsString();
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
                    this.LogProperties.ReadXml(reader);
                }
			}
			//Check for Exception child node
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Exception")
	        {
				this.Exception.ReadXml(reader);
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
                this.WriteXml(writer);
                writer.Close();
                return sb.ToString();
            }
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append("<p><strong>LogGUID:</strong>" + this.LogGUID + "</p>");
            str.Append("<p><strong>LogType:</strong>" + this.LogTypeKey + "</p>");
            str.Append("<p><strong>UserID:</strong>" + this.LogUserID + "</p>");
            str.Append("<p><strong>EventID:</strong>" + this.LogEventID + "</p>");
            str.Append("<p><strong>Username:</strong>" + this.LogUserName + "</p>");
            str.Append("<p><strong>PortalID:</strong>" + this.LogPortalID + "</p>");
            str.Append("<p><strong>PortalName:</strong>" + this.LogPortalName + "</p>");
            str.Append("<p><strong>CreateDate:</strong>" + this.LogCreateDate + "</p>");
            str.Append("<p><strong>ServerName:</strong>" + this.LogServerName + "</p>");
            str.Append(this.LogProperties.ToString());
	        if (!string.IsNullOrEmpty(this.Exception.ExceptionHash))
	        {
				str.Append(this.Exception.ToString());
			}
            return str.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("log");
            writer.WriteAttributeString("LogGUID", this.LogGUID);
            writer.WriteAttributeString("LogFileID", this.LogFileID);
            writer.WriteAttributeString("LogTypeKey", this.LogTypeKey);
            writer.WriteAttributeString("LogUserID", this.LogUserID.ToString());
            writer.WriteAttributeString("LogEventID", this.LogEventID.ToString());
            writer.WriteAttributeString("LogUserName", this.LogUserName);
            writer.WriteAttributeString("LogPortalID", this.LogPortalID.ToString());
            writer.WriteAttributeString("LogPortalName", this.LogPortalName);
            writer.WriteAttributeString("LogCreateDate", this.LogCreateDate.ToString());
            writer.WriteAttributeString("LogCreateDateNum", this.LogCreateDateNum.ToString());
            writer.WriteAttributeString("BypassBuffering", this.BypassBuffering.ToString());
            writer.WriteAttributeString("LogServerName", this.LogServerName);
			writer.WriteAttributeString("LogConfigID", this.LogConfigID);
            this.LogProperties.WriteXml(writer);
	        if (!string.IsNullOrEmpty(this.Exception.ExceptionHash))
	        {
		        this.Exception.WriteXml(writer);
	        }
            writer.WriteEndElement();
        }
		
		#endregion
    }
}
