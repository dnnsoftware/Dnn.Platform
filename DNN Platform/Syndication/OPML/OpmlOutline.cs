// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Xml;

    /// <summary>  Class for managing an OPML feed outline.</summary>
    public class OpmlOutline
    {
        private string category = string.Empty;
        private DateTime created = DateTime.MinValue;
        private string description = string.Empty;
        private string language = string.Empty;
        private string text = string.Empty;
        private string title = string.Empty;
        private string type = "rss";

        public OpmlOutline()
        {
            this.Outlines = new OpmlOutlines();
        }

        public string Version
        {
            get
            {
                return "2.0";
            }
        }

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

        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value;
            }
        }

        public Uri HtmlUrl { get; set; }

        public Uri XmlUrl { get; set; }

        public Uri Url { get; set; }

        public DateTime Created
        {
            get
            {
                return this.created;
            }

            set
            {
                this.created = value;
            }
        }

        public bool IsComment { get; set; }

        public bool IsBreakpoint { get; set; }

        public string Category
        {
            get
            {
                return this.category;
            }

            set
            {
                this.category = value;
            }
        }

        public string Language
        {
            get
            {
                return this.language;
            }

            set
            {
                this.language = value;
            }
        }

        public OpmlOutlines Outlines { get; set; }
    }
}
