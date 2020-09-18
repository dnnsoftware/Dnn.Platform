// Licensed to the .NET Foundation under one or more agreements.
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
        private DateTime _dateCreated = DateTime.MinValue;
        private DateTime _dateModified = DateTime.MinValue;
        private string _docs = string.Empty;
        private string _expansionState = string.Empty;
        private OpmlOutlines _outlines;
        private string _ownerEmail = string.Empty;
        private string _ownerId = string.Empty;
        private string _ownerName = string.Empty;
        private string _title = string.Empty;
        private DateTime _utcExpiry = DateTime.Now.AddMinutes(180);
        private string _vertScrollState = string.Empty;
        private string _windowBottom = string.Empty;
        private string _windowLeft = string.Empty;
        private string _windowRight = string.Empty;
        private string _windowTop = string.Empty;
        private XmlDocument opmlDoc;

        public Opml()
        {
            this._outlines = new OpmlOutlines();
        }

        public DateTime UtcExpiry
        {
            get
            {
                return this._utcExpiry;
            }

            set
            {
                this._utcExpiry = value;
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

        public DateTime DateCreated
        {
            get
            {
                return this._dateCreated;
            }

            set
            {
                this._dateCreated = value;
            }
        }

        public DateTime DateModified
        {
            get
            {
                return this._dateModified;
            }

            set
            {
                this._dateModified = value;
            }
        }

        public string OwnerName
        {
            get
            {
                return this._ownerName;
            }

            set
            {
                this._ownerName = value;
            }
        }

        public string OwnerEmail
        {
            get
            {
                return this._ownerEmail;
            }

            set
            {
                this._ownerEmail = value;
            }
        }

        public string OwnerId
        {
            get
            {
                return this._ownerId;
            }

            set
            {
                this._ownerId = value;
            }
        }

        public string Docs
        {
            get
            {
                return this._docs;
            }

            set
            {
                this._docs = value;
            }
        }

        public string ExpansionState
        {
            get
            {
                return this._expansionState;
            }

            set
            {
                this._expansionState = value;
            }
        }

        public string VertScrollState
        {
            get
            {
                return this._vertScrollState;
            }

            set
            {
                this._vertScrollState = value;
            }
        }

        public string WindowTop
        {
            get
            {
                return this._windowTop;
            }

            set
            {
                this._windowTop = value;
            }
        }

        public string WindowLeft
        {
            get
            {
                return this._windowLeft;
            }

            set
            {
                this._windowLeft = value;
            }
        }

        public string WindowBottom
        {
            get
            {
                return this._windowBottom;
            }

            set
            {
                this._windowBottom = value;
            }
        }

        public string WindowRight
        {
            get
            {
                return this._windowRight;
            }

            set
            {
                this._windowRight = value;
            }
        }

        public OpmlOutlines Outlines
        {
            get
            {
                return this._outlines;
            }

            set
            {
                this._outlines = value;
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
            var _out = new Opml();
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
                    _out.Title = title.InnerText;
                }

                if (dateCreated != null)
                {
                    _out.DateCreated = DateTime.Parse(dateCreated.InnerText);
                }

                if (dateModified != null)
                {
                    _out.DateModified = DateTime.Parse(dateModified.InnerText);
                }

                if (ownerName != null)
                {
                    _out.OwnerName = ownerName.InnerText;
                }

                if (ownerEmail != null)
                {
                    _out.OwnerEmail = ownerEmail.InnerText;
                }

                if (ownerId != null)
                {
                    _out.OwnerId = ownerId.InnerText;
                }

                if (docs != null)
                {
                    _out.Docs = docs.InnerText;
                }

                if (expansionState != null)
                {
                    _out.ExpansionState = expansionState.InnerText;
                }

                if (vertScrollState != null)
                {
                    _out.VertScrollState = vertScrollState.InnerText;
                }

                if (windowTop != null)
                {
                    _out.WindowTop = windowTop.InnerText;
                }

                if (windowLeft != null)
                {
                    _out.WindowLeft = windowLeft.InnerText;
                }

                if (windowBottom != null)
                {
                    _out.WindowBottom = windowBottom.InnerText;
                }

                if (windowLeft != null)
                {
                    _out.WindowLeft = windowLeft.InnerText;
                }

                // Parse body
                XmlNode body = doc.GetElementsByTagName("body")[0];
                XmlNodeList outlineList = body.SelectNodes("./outline");
                foreach (XmlElement outline in outlineList)
                {
                    _out.Outlines.Add(ParseXml(outline));
                }

                return _out;
            }
            catch
            {
                return new Opml();
            }
        }

        public void AddOutline(OpmlOutline item)
        {
            this._outlines.Add(item);
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
            this._outlines.Add(item);
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

            foreach (OpmlOutline outline in this._outlines)
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
