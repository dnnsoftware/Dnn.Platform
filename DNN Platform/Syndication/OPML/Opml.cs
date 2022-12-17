﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Xml;

    using DotNetNuke.Instrumentation;

    /// <summary>
    ///   Class for managing an OPML feed.
    /// </summary>
    public class Opml
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private DateTime dateCreated = DateTime.MinValue;
        private DateTime dateModified = DateTime.MinValue;
        private string docs = string.Empty;
        private string expansionState = string.Empty;
        private OpmlOutlines outlines;
        private string ownerEmail = string.Empty;
        private string ownerId = string.Empty;
        private string ownerName = string.Empty;
        private string title = string.Empty;
        private DateTime utcExpiry = DateTime.Now.AddMinutes(180);
        private string vertScrollState = string.Empty;
        private string windowBottom = string.Empty;
        private string windowLeft = string.Empty;
        private string windowRight = string.Empty;
        private string windowTop = string.Empty;
        private XmlDocument opmlDoc;

        public Opml()
        {
            this.outlines = new OpmlOutlines();
        }

        public DateTime UtcExpiry
        {
            get
            {
                return this.utcExpiry;
            }

            set
            {
                this.utcExpiry = value;
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

        public DateTime DateCreated
        {
            get
            {
                return this.dateCreated;
            }

            set
            {
                this.dateCreated = value;
            }
        }

        public DateTime DateModified
        {
            get
            {
                return this.dateModified;
            }

            set
            {
                this.dateModified = value;
            }
        }

        public string OwnerName
        {
            get
            {
                return this.ownerName;
            }

            set
            {
                this.ownerName = value;
            }
        }

        public string OwnerEmail
        {
            get
            {
                return this.ownerEmail;
            }

            set
            {
                this.ownerEmail = value;
            }
        }

        public string OwnerId
        {
            get
            {
                return this.ownerId;
            }

            set
            {
                this.ownerId = value;
            }
        }

        public string Docs
        {
            get
            {
                return this.docs;
            }

            set
            {
                this.docs = value;
            }
        }

        public string ExpansionState
        {
            get
            {
                return this.expansionState;
            }

            set
            {
                this.expansionState = value;
            }
        }

        public string VertScrollState
        {
            get
            {
                return this.vertScrollState;
            }

            set
            {
                this.vertScrollState = value;
            }
        }

        public string WindowTop
        {
            get
            {
                return this.windowTop;
            }

            set
            {
                this.windowTop = value;
            }
        }

        public string WindowLeft
        {
            get
            {
                return this.windowLeft;
            }

            set
            {
                this.windowLeft = value;
            }
        }

        public string WindowBottom
        {
            get
            {
                return this.windowBottom;
            }

            set
            {
                this.windowBottom = value;
            }
        }

        public string WindowRight
        {
            get
            {
                return this.windowRight;
            }

            set
            {
                this.windowRight = value;
            }
        }

        public OpmlOutlines Outlines
        {
            get
            {
                return this.outlines;
            }

            set
            {
                this.outlines = value;
            }
        }

        public static Opml LoadFromUrl(Uri uri)
        {
            return OpmlDownloadManager.GetOpmlFeed(uri);
        }

        public static Opml LoadFromFile(string path)
        {
            try
            {
                var opmlDoc = new XmlDocument { XmlResolver = null };
                opmlDoc.Load(path);

                return LoadFromXml(opmlDoc);
            }
            catch
            {
                return new Opml();
            }
        }

        public static Opml LoadFromXml(XmlDocument doc)
        {
            var @out = new Opml();
            try
            {
                // Parse head
                XmlNode head = doc.GetElementsByTagName("head")[0];
                XmlNode title = head.SelectSingleNode("./title");
                XmlNode dateCreated = head.SelectSingleNode("./dateCreated");
                XmlNode dateModified = head.SelectSingleNode("./dateModified");
                XmlNode ownerName = head.SelectSingleNode("./ownerName");
                XmlNode ownerEmail = head.SelectSingleNode("./ownerEmail");
                XmlNode ownerId = head.SelectSingleNode("./ownerId");
                XmlNode docs = head.SelectSingleNode("./docs");
                XmlNode expansionState = head.SelectSingleNode("./expansionState");
                XmlNode vertScrollState = head.SelectSingleNode("./vertScrollState");
                XmlNode windowTop = head.SelectSingleNode("./windowTop");
                XmlNode windowLeft = head.SelectSingleNode("./windowLeft");
                XmlNode windowBottom = head.SelectSingleNode("./windowBottom");
                XmlNode windowRight = head.SelectSingleNode("./windowRight");

                if (title != null)
                {
                    @out.Title = title.InnerText;
                }

                if (dateCreated != null)
                {
                    @out.DateCreated = DateTime.Parse(dateCreated.InnerText);
                }

                if (dateModified != null)
                {
                    @out.DateModified = DateTime.Parse(dateModified.InnerText);
                }

                if (ownerName != null)
                {
                    @out.OwnerName = ownerName.InnerText;
                }

                if (ownerEmail != null)
                {
                    @out.OwnerEmail = ownerEmail.InnerText;
                }

                if (ownerId != null)
                {
                    @out.OwnerId = ownerId.InnerText;
                }

                if (docs != null)
                {
                    @out.Docs = docs.InnerText;
                }

                if (expansionState != null)
                {
                    @out.ExpansionState = expansionState.InnerText;
                }

                if (vertScrollState != null)
                {
                    @out.VertScrollState = vertScrollState.InnerText;
                }

                if (windowTop != null)
                {
                    @out.WindowTop = windowTop.InnerText;
                }

                if (windowLeft != null)
                {
                    @out.WindowLeft = windowLeft.InnerText;
                }

                if (windowBottom != null)
                {
                    @out.WindowBottom = windowBottom.InnerText;
                }

                if (windowLeft != null)
                {
                    @out.WindowLeft = windowLeft.InnerText;
                }

                // Parse body
                XmlNode body = doc.GetElementsByTagName("body")[0];
                XmlNodeList outlineList = body.SelectNodes("./outline");
                foreach (XmlElement outline in outlineList)
                {
                    @out.Outlines.Add(ParseXml(outline));
                }

                return @out;
            }
            catch
            {
                return new Opml();
            }
        }

        public void AddOutline(OpmlOutline item)
        {
            this.outlines.Add(item);
        }

        public void AddOutline(string text, string type, Uri xmlUrl, string category)
        {
            this.AddOutline(text, type, xmlUrl, category, null);
        }

        public void AddOutline(string text, string type, Uri xmlUrl, string category, OpmlOutlines outlines)
        {
            var item = new OpmlOutline();
            item.Text = text;
            item.Type = type;
            item.XmlUrl = xmlUrl;
            item.Category = category;
            item.Outlines = outlines;
            this.outlines.Add(item);
        }

        public string GetXml()
        {
            return this.opmlDoc.OuterXml;
        }

        public void Save(string fileName)
        {
            this.opmlDoc = new XmlDocument { XmlResolver = null };
            XmlElement opml = this.opmlDoc.CreateElement("opml");
            opml.SetAttribute("version", "2.0");
            this.opmlDoc.AppendChild(opml);

            // create head
            XmlElement head = this.opmlDoc.CreateElement("head");
            opml.AppendChild(head);

            // set Title
            XmlElement title = this.opmlDoc.CreateElement("title");
            title.InnerText = this.Title;
            head.AppendChild(title);

            // set date created
            XmlElement dateCreated = this.opmlDoc.CreateElement("dateCreated");
            dateCreated.InnerText = this.DateCreated != DateTime.MinValue ? this.DateCreated.ToString("r", null) : DateTime.Now.ToString("r", null);
            head.AppendChild(dateCreated);

            // set date modified
            XmlElement dateModified = this.opmlDoc.CreateElement("dateModified");
            dateCreated.InnerText = this.DateModified != DateTime.MinValue ? this.DateModified.ToString("r", null) : DateTime.Now.ToString("r", null);
            head.AppendChild(dateModified);

            // set owner email
            XmlElement ownerEmail = this.opmlDoc.CreateElement("ownerEmail");
            ownerEmail.InnerText = this.OwnerEmail;
            head.AppendChild(ownerEmail);

            // set owner name
            XmlElement ownerName = this.opmlDoc.CreateElement("ownerName");
            ownerName.InnerText = this.OwnerName;
            head.AppendChild(ownerName);

            // set owner id
            XmlElement ownerId = this.opmlDoc.CreateElement("ownerId");
            ownerId.InnerText = this.OwnerId;
            head.AppendChild(ownerId);

            // set docs
            XmlElement docs = this.opmlDoc.CreateElement("docs");
            docs.InnerText = this.Docs;
            head.AppendChild(docs);

            // set expansionState
            XmlElement expansionState = this.opmlDoc.CreateElement("expansionState");
            expansionState.InnerText = this.ExpansionState;
            head.AppendChild(expansionState);

            // set vertScrollState
            XmlElement vertScrollState = this.opmlDoc.CreateElement("vertScrollState");
            vertScrollState.InnerText = this.VertScrollState;
            head.AppendChild(vertScrollState);

            // set windowTop
            XmlElement windowTop = this.opmlDoc.CreateElement("windowTop");
            windowTop.InnerText = this.WindowTop;
            head.AppendChild(windowTop);

            // set windowLeft
            XmlElement windowLeft = this.opmlDoc.CreateElement("windowLeft");
            windowLeft.InnerText = this.WindowLeft;
            head.AppendChild(windowLeft);

            // set windowBottom
            XmlElement windowBottom = this.opmlDoc.CreateElement("windowBottom");
            windowBottom.InnerText = this.WindowBottom;
            head.AppendChild(windowBottom);

            // set windowRight
            XmlElement windowRight = this.opmlDoc.CreateElement("windowRight");
            windowRight.InnerText = this.WindowRight;
            head.AppendChild(windowRight);

            // create body
            XmlElement opmlBody = this.opmlDoc.CreateElement("body");
            opml.AppendChild(opmlBody);

            foreach (OpmlOutline outline in this.outlines)
            {
                opmlBody.AppendChild(outline.ToXml);
            }

            this.opmlDoc.Save(fileName);
        }

        internal static OpmlOutline ParseXml(XmlElement node)
        {
            var newOutline = new OpmlOutline();

            newOutline.Text = ParseElement(node, "text");
            newOutline.Type = ParseElement(node, "type");
            newOutline.IsComment = ParseElement(node, "isComment") == "true" ? true : false;
            newOutline.IsBreakpoint = ParseElement(node, "isBreakpoint") == "true" ? true : false;
            try
            {
                newOutline.Created = DateTime.Parse(ParseElement(node, "created"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            newOutline.Category = ParseElement(node, "category");
            try
            {
                newOutline.XmlUrl = new Uri(ParseElement(node, "xmlUrl"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            try
            {
                newOutline.HtmlUrl = new Uri(ParseElement(node, "htmlUrl"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            newOutline.Language = ParseElement(node, "language");
            newOutline.Title = ParseElement(node, "title");

            try
            {
                newOutline.Url = new Uri(ParseElement(node, "url"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            newOutline.Description = ParseElement(node, "description");

            if (node.HasChildNodes)
            {
                foreach (XmlElement childNode in node.SelectNodes("./outline"))
                {
                    newOutline.Outlines.Add(ParseXml(childNode));
                }
            }

            return newOutline;
        }

        private static string ParseElement(XmlElement node, string attribute)
        {
            string attrValue = node.GetAttribute(attribute);
            return !string.IsNullOrEmpty(attrValue) ? attrValue : string.Empty;
        }
    }
}
