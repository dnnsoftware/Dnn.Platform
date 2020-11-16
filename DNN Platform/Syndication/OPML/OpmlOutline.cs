// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    ///   Class for managing an OPML feed outline.
    /// </summary>
    public class OpmlOutline
    {
        private string _category = string.Empty;
        private DateTime _created = DateTime.MinValue;
        private string _description = string.Empty;
        private string _language = string.Empty;
        private string _text = string.Empty;
        private string _title = string.Empty;
        private string _type = "rss";

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
                return this._description;
            }

            set
            {
                this._description = value;
            }
        }

        public string Title
        {
            get
            {
                return this._title;
            }

            set
            {
                this._title = value;
            }
        }

        public string Type
        {
            get
            {
                return this._type;
            }

            set
            {
                this._type = value;
            }
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
            }
        }

        public Uri HtmlUrl { get; set; }

        public Uri XmlUrl { get; set; }

        public Uri Url { get; set; }

        public DateTime Created
        {
            get
            {
                return this._created;
            }

            set
            {
                this._created = value;
            }
        }

        public bool IsComment { get; set; }

        public bool IsBreakpoint { get; set; }

        public string Category
        {
            get
            {
                return this._category;
            }

            set
            {
                this._category = value;
            }
        }

        public string Language
        {
            get
            {
                return this._language;
            }

            set
            {
                this._language = value;
            }
        }

        public OpmlOutlines Outlines { get; set; }
    }

    public class OpmlOutlines : List<OpmlOutline>
    {}
}
