// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;
using System.Text;
using System.Xml;

using DotNetNuke.Abstractions.Logging;

/// <inheritdoc />
[Serializable]
public class LogDetailInfo : ILogDetailInfo
{
    /// <summary>Initializes a new instance of the <see cref="LogDetailInfo"/> class.</summary>
    public LogDetailInfo()
        : this(string.Empty, string.Empty)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="LogDetailInfo"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public LogDetailInfo(string name, string value)
    {
        this.PropertyName = name;
        this.PropertyValue = value;
    }

    /// <inheritdoc />
    public string PropertyName { get; set; }

    /// <inheritdoc />
    public string PropertyValue { get; set; }

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

    /// <inheritdoc/>
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
