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
using System.IO;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Exceptions
{
	[Serializable]
	public class ExceptionInfo
	{

		#region Constructors
		public ExceptionInfo() { }

		public ExceptionInfo(Exception e)
		{
			Message = e.Message;
			StackTrace = e.StackTrace;
			Source = e.Source;
			if (e.InnerException != null)
			{
				InnerMessage = e.InnerException.Message;
				InnerStackTrace = e.InnerException.StackTrace;
			}
			ExceptionHash = e.Hash();
		}
		#endregion

		#region Properties
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
		#endregion

		#region Public Methods
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
			do
			{
				switch (reader.Name)
				{
					case "AssemblyVersion":
						AssemblyVersion = reader.ReadContentAsString();
						break;
					case "PortalId":
						PortalId = reader.ReadContentAsInt();
						break;
					case "UserId":
						UserId = reader.ReadContentAsInt();
						break;
					case "TabId":
						TabId = reader.ReadContentAsInt();
						break;
					case "RawUrl":
						RawUrl = reader.ReadContentAsString();
						break;
					case "Referrer":
						Referrer = reader.ReadContentAsString();
						break;
					case "UserAgent":
						UserAgent = reader.ReadContentAsString();
						break;
					case "ExceptionHash":
						ExceptionHash = reader.ReadContentAsString();
						break;
					case "Message":
						Message = reader.ReadContentAsString();
						break;
					case "StackTrace":
						StackTrace = reader.ReadContentAsString();
						break;
					case "InnerMessage":
						InnerMessage = reader.ReadContentAsString();
						break;
					case "InnerStackTrace":
						InnerStackTrace = reader.ReadContentAsString();
						break;
					case "Source":
						Source = reader.ReadContentAsString();
						break;
					case "FileName":
						FileName = reader.ReadContentAsString();
						break;
					case "FileLineNumber":
						FileLineNumber = reader.ReadContentAsInt();
						break;
					case "FileColumnNumber":
						FileColumnNumber = reader.ReadContentAsInt();
						break;
					case "Method":
						Method = reader.ReadContentAsString();
						break;
				}
				reader.ReadEndElement();
				reader.Read();
			} while (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !String.IsNullOrEmpty(reader.LocalName));
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
			str.Append("<p><strong>AssemblyVersion:</strong>" + AssemblyVersion + "</p>");
			str.Append("<p><strong>PortalId:</strong>" + PortalId + "</p>");
			str.Append("<p><strong>UserId:</strong>" + UserId + "</p>");
			str.Append("<p><strong>TabId:</strong>" + TabId + "</p>");
			str.Append("<p><strong>RawUrl:</strong>" + RawUrl + "</p>");
			str.Append("<p><strong>Referrer:</strong>" + Referrer + "</p>");
			str.Append("<p><strong>UserAgent:</strong>" + UserAgent + "</p>");
			str.Append("<p><strong>ExceptionHash:</strong>" + ExceptionHash + "</p>");
			str.Append("<p><strong>Message:</strong>" + Message + "</p>");
			str.Append("<p><strong>StackTrace:</strong><pre>" + StackTrace.Replace(") at ", ")<br/>at ") + "</pre></p>");
			str.Append("<p><strong>InnerMessage:</strong>" + InnerMessage + "</p>");
			str.Append("<p><strong>InnerStackTrace:</strong><pre>" + InnerStackTrace.Replace(") at ",")<br/>at ") + "</pre></p>");
			str.Append("<p><strong>Source:</strong>" + Source + "</p>");
			str.Append("<p><strong>FileName:</strong>" + FileName + "</p>");
			str.Append("<p><strong>FileLineNumber:</strong>" + FileLineNumber + "</p>");
			str.Append("<p><strong>FileColumnNumber:</strong>" + FileColumnNumber + "</p>");
			str.Append("<p><strong>Method:</strong>" + Method + "</p>");
			return str.ToString();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Exception");
			writer.WriteElementString("AssemblyVersion", AssemblyVersion);
			writer.WriteElementString("PortalId", PortalId.ToString());
			writer.WriteElementString("UserId", UserId.ToString());
			writer.WriteElementString("TabId", TabId.ToString());
			writer.WriteElementString("RawUrl", RawUrl);
			writer.WriteElementString("Referrer", Referrer);
			writer.WriteElementString("UserAgent", UserAgent);
			writer.WriteElementString("ExceptionHash", ExceptionHash);
			writer.WriteElementString("Message", Message);
			writer.WriteElementString("StackTrace", StackTrace);
			writer.WriteElementString("InnerMessage", InnerMessage);
			writer.WriteElementString("InnerStackTrace", InnerStackTrace);
			writer.WriteElementString("Source", Source);
			writer.WriteElementString("FileName", FileName);
			writer.WriteElementString("FileLineNumber", FileLineNumber.ToString());
			writer.WriteElementString("FileColumnNumber", FileColumnNumber.ToString());
			writer.WriteElementString("Method", Method);
			writer.WriteEndElement();
		}
		#endregion

	}
}