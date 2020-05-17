﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Class for managing an OPML feed outline
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
            Outlines = new OpmlOutlines();
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public Uri HtmlUrl { get; set; }

        public Uri XmlUrl { get; set; }

        public Uri Url { get; set; }

        public DateTime Created
        {
            get
            {
                return _created;
            }
            set
            {
                _created = value;
            }
        }

        public bool IsComment { get; set; }

        public bool IsBreakpoint { get; set; }

        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
            }
        }

        public string Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        public string Version
        {
            get
            {
                return "2.0";
            }
        }


        public OpmlOutlines Outlines { get; set; }

        public XmlElement ToXml
        {
            get
            {
                var opmlDoc = new XmlDocument { XmlResolver = null };
                XmlElement outlineNode = opmlDoc.CreateElement("outline");

                if (!String.IsNullOrEmpty(Title))
                {
                    outlineNode.SetAttribute("title", Title);
                }

                if (!String.IsNullOrEmpty(Description))
                {
                    outlineNode.SetAttribute("description", Description);
                }

                if (!String.IsNullOrEmpty(Text))
                {
                    outlineNode.SetAttribute("text", Text);
                }

                if (!String.IsNullOrEmpty(Type))
                {
                    outlineNode.SetAttribute("type", Type);
                }

                if (!String.IsNullOrEmpty(Language))
                {
                    outlineNode.SetAttribute("language", Language);
                }

                if (!String.IsNullOrEmpty(Category))
                {
                    outlineNode.SetAttribute("category", Category);
                }

                if (Created > DateTime.MinValue)
                {
                    outlineNode.SetAttribute("created", Created.ToString("r", null));
                }

                if (HtmlUrl != null)
                {
                    outlineNode.SetAttribute("htmlUrl", HtmlUrl.ToString());
                }

                if (XmlUrl != null)
                {
                    outlineNode.SetAttribute("xmlUrl", XmlUrl.ToString());
                }

                if (Url != null)
                {
                    outlineNode.SetAttribute("url", Url.ToString());
                }

                outlineNode.SetAttribute("isComment", (IsComment ? "true" : "false"));
                outlineNode.SetAttribute("isBreakpoint", (IsBreakpoint ? "true" : "false"));

                foreach (OpmlOutline childOutline in Outlines)
                {
                    outlineNode.AppendChild(childOutline.ToXml);
                }

                return outlineNode;
            }
        }
    }

    public class OpmlOutlines : List<OpmlOutline>
    {
    }
}
