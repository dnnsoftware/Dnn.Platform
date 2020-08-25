// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;

    [Serializable]
    public class ExceptionInfo
    {
        public ExceptionInfo()
        {
        }

        public ExceptionInfo(Exception e)
        {
            this.Message = e.Message;
            this.StackTrace = e.StackTrace;
            this.Source = e.Source;
            if (e.InnerException != null)
            {
                this.InnerMessage = e.InnerException.Message;
                this.InnerStackTrace = e.InnerException.StackTrace;
            }

            this.ExceptionHash = e.Hash();
        }

        public string AssemblyVersion { get; set; }

        public int PortalId { get; set; }

        public int UserId { get; set; }

        public int TabId { get; set; }

        public string RawUrl { get; set; }

        public string Referrer { get; set; }

        public string UserAgent { get; set; }

        public string ExceptionHash { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public string InnerMessage { get; set; }

        public string InnerStackTrace { get; set; }

        public string Source { get; set; }

        public string FileName { get; set; }

        public int FileLineNumber { get; set; }

        public int FileColumnNumber { get; set; }

        public string Method { get; set; }

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
            do
            {
                switch (reader.Name)
                {
                    case "AssemblyVersion":
                        this.AssemblyVersion = reader.ReadContentAsString();
                        break;
                    case "PortalId":
                        this.PortalId = reader.ReadContentAsInt();
                        break;
                    case "UserId":
                        this.UserId = reader.ReadContentAsInt();
                        break;
                    case "TabId":
                        this.TabId = reader.ReadContentAsInt();
                        break;
                    case "RawUrl":
                        this.RawUrl = reader.ReadContentAsString();
                        break;
                    case "Referrer":
                        this.Referrer = reader.ReadContentAsString();
                        break;
                    case "UserAgent":
                        this.UserAgent = reader.ReadContentAsString();
                        break;
                    case "ExceptionHash":
                        this.ExceptionHash = reader.ReadContentAsString();
                        break;
                    case "Message":
                        this.Message = reader.ReadContentAsString();
                        break;
                    case "StackTrace":
                        this.StackTrace = reader.ReadContentAsString();
                        break;
                    case "InnerMessage":
                        this.InnerMessage = reader.ReadContentAsString();
                        break;
                    case "InnerStackTrace":
                        this.InnerStackTrace = reader.ReadContentAsString();
                        break;
                    case "Source":
                        this.Source = reader.ReadContentAsString();
                        break;
                    case "FileName":
                        this.FileName = reader.ReadContentAsString();
                        break;
                    case "FileLineNumber":
                        this.FileLineNumber = reader.ReadContentAsInt();
                        break;
                    case "FileColumnNumber":
                        this.FileColumnNumber = reader.ReadContentAsInt();
                        break;
                    case "Method":
                        this.Method = reader.ReadContentAsString();
                        break;
                }

                reader.ReadEndElement();
                reader.Read();
            }
            while (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !string.IsNullOrEmpty(reader.LocalName));
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
            str.Append("<p><strong>AssemblyVersion:</strong>" + WebUtility.HtmlEncode(this.AssemblyVersion) + "</p>");
            str.Append("<p><strong>PortalId:</strong>" + this.PortalId + "</p>");
            str.Append("<p><strong>UserId:</strong>" + this.UserId + "</p>");
            str.Append("<p><strong>TabId:</strong>" + this.TabId + "</p>");
            str.Append("<p><strong>RawUrl:</strong>" + WebUtility.HtmlEncode(this.RawUrl) + "</p>");
            str.Append("<p><strong>Referrer:</strong>" + WebUtility.HtmlEncode(this.Referrer) + "</p>");
            str.Append("<p><strong>UserAgent:</strong>" + WebUtility.HtmlEncode(this.UserAgent) + "</p>");
            str.Append("<p><strong>ExceptionHash:</strong>" + WebUtility.HtmlEncode(this.ExceptionHash) + "</p>");
            str.Append("<p><strong>Message:</strong>" + WebUtility.HtmlEncode(this.Message) + "</p>");
            str.Append("<p><strong>StackTrace:</strong><pre>" + WebUtility.HtmlEncode(this.StackTrace)?.Replace(") at ", ")<br/>at ") + "</pre></p>");
            str.Append("<p><strong>InnerMessage:</strong>" + WebUtility.HtmlEncode(this.InnerMessage) + "</p>");
            str.Append("<p><strong>InnerStackTrace:</strong><pre>" + WebUtility.HtmlEncode(this.InnerStackTrace)?.Replace(") at ", ")<br/>at ") + "</pre></p>");
            str.Append("<p><strong>Source:</strong>" + WebUtility.HtmlEncode(this.Source) + "</p>");
            str.Append("<p><strong>FileName:</strong>" + WebUtility.HtmlEncode(this.FileName) + "</p>");
            str.Append("<p><strong>FileLineNumber:</strong>" + this.FileLineNumber + "</p>");
            str.Append("<p><strong>FileColumnNumber:</strong>" + this.FileColumnNumber + "</p>");
            str.Append("<p><strong>Method:</strong>" + WebUtility.HtmlEncode(this.Method) + "</p>");
            return str.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Exception");
            writer.WriteElementString("AssemblyVersion", this.AssemblyVersion);
            writer.WriteElementString("PortalId", this.PortalId.ToString());
            writer.WriteElementString("UserId", this.UserId.ToString());
            writer.WriteElementString("TabId", this.TabId.ToString());
            writer.WriteElementString("RawUrl", this.RawUrl);
            writer.WriteElementString("Referrer", this.Referrer);
            writer.WriteElementString("UserAgent", this.UserAgent);
            writer.WriteElementString("ExceptionHash", this.ExceptionHash);
            writer.WriteElementString("Message", this.Message);
            writer.WriteElementString("StackTrace", this.StackTrace);
            writer.WriteElementString("InnerMessage", this.InnerMessage);
            writer.WriteElementString("InnerStackTrace", this.InnerStackTrace);
            writer.WriteElementString("Source", this.Source);
            writer.WriteElementString("FileName", this.FileName);
            writer.WriteElementString("FileLineNumber", this.FileLineNumber.ToString());
            writer.WriteElementString("FileColumnNumber", this.FileColumnNumber.ToString());
            writer.WriteElementString("Method", this.Method);
            writer.WriteEndElement();
        }
    }
}
