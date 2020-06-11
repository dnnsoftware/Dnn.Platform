﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    public class LogProperties : ArrayList
    {
		#region "Public Properties"
		
        public string Summary
        {
            get
            {
                string summary = HtmlUtils.Clean(this.ToString(), true);
                if (summary.Length > 75)
                {
                    summary = summary.Substring(0, 75);
                }
                return summary;
            }
        }
		
		#endregion

		#region "Public Methods"

        public void Deserialize(string content)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(content)))
            {
                reader.ReadStartElement("LogProperties");
                if (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !String.IsNullOrEmpty(reader.LocalName))
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
                reader.ReadStartElement("LogProperty");

                //Create new LogDetailInfo object
                var logDetail = new LogDetailInfo();

                //Load it from the Xml
                logDetail.ReadXml(reader);

                //Add to the collection
                this.Add(logDetail);

            } while (reader.ReadToNextSibling("LogProperty"));
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
            var sb = new StringBuilder();
            foreach (LogDetailInfo logDetail in this)
            {
                sb.Append(logDetail.ToString());
            }
            return sb.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("LogProperties");
            foreach (LogDetailInfo logDetail in this)
            {
                logDetail.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
		
		#endregion
    }
}
