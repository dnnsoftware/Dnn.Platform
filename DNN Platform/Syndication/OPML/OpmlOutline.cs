// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication;

using System;
using System.Xml;

/// <summary>Class for managing an OPML feed outline.</summary>
public class OpmlOutline
{
    /// <summary>Initializes a new instance of the <see cref="OpmlOutline"/> class.</summary>
    public OpmlOutline()
    {
        this.Outlines = new OpmlOutlines();
    }

    /// <summary>Gets the OPML outline version.</summary>
    public string Version => "2.0";

    /// <summary>Gets the OPML outline as an <see cref="XmlElement"/>.</summary>
    public XmlElement ToXml
    {
        get
        {
            var opmlDoc = new XmlDocument { XmlResolver = null };
            XmlElement outlineNode = opmlDoc.CreateElement("outline");

            if (!string.IsNullOrEmpty(this.Title))
            {
                outlineNode.SetAttribute("title", this.Title);
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                outlineNode.SetAttribute("description", this.Description);
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                outlineNode.SetAttribute("text", this.Text);
            }

            if (!string.IsNullOrEmpty(this.Type))
            {
                outlineNode.SetAttribute("type", this.Type);
            }

            if (!string.IsNullOrEmpty(this.Language))
            {
                outlineNode.SetAttribute("language", this.Language);
            }

            if (!string.IsNullOrEmpty(this.Category))
            {
                outlineNode.SetAttribute("category", this.Category);
            }

            if (this.Created > DateTime.MinValue)
            {
                outlineNode.SetAttribute("created", this.Created.ToString("r", null));
            }

            if (this.HtmlUrl != null)
            {
                outlineNode.SetAttribute("htmlUrl", this.HtmlUrl.ToString());
            }

            if (this.XmlUrl != null)
            {
                outlineNode.SetAttribute("xmlUrl", this.XmlUrl.ToString());
            }

            if (this.Url != null)
            {
                outlineNode.SetAttribute("url", this.Url.ToString());
            }

            outlineNode.SetAttribute("isComment", this.IsComment ? "true" : "false");
            outlineNode.SetAttribute("isBreakpoint", this.IsBreakpoint ? "true" : "false");

            foreach (OpmlOutline childOutline in this.Outlines)
            {
                outlineNode.AppendChild(childOutline.ToXml);
            }

            return outlineNode;
        }
    }

    /// <summary>Gets or sets the OPML outline description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the OPML outline title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the OPML outline type.</summary>
    public string Type { get; set; } = "rss";

    /// <summary>Gets or sets the OPML outline text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Gets or sets the OPML outline's HTML URL.</summary>
    public Uri HtmlUrl { get; set; }

    /// <summary>Gets or sets the OPML outline's XML URL.</summary>
    public Uri XmlUrl { get; set; }

    /// <summary>Gets or sets the OPML outline's URL.</summary>
    public Uri Url { get; set; }

    /// <summary>Gets or sets the creation date for the OPML outline.</summary>
    public DateTime Created { get; set; } = DateTime.MinValue;

    /// <summary>Gets or sets a value indicating whether this OPML outline is a comment.</summary>
    public bool IsComment { get; set; }

    /// <summary>Gets or sets a value indicating whether this OPML outline is a breakpoint.</summary>
    public bool IsBreakpoint { get; set; }

    /// <summary>Gets or sets the OPML outline category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the OPML outline language.</summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>Gets or sets the outlines contained in this OPML outline.</summary>
    public OpmlOutlines Outlines { get; set; }
}
