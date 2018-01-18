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
using System.Xml;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Syndication
{
    /// <summary>
    ///   Class for managing an OPML feed
    /// </summary>
    public class Opml
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Opml));
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
            _outlines = new OpmlOutlines();
        }

        public DateTime UtcExpiry
        {
            get
            {
                return _utcExpiry;
            }
            set
            {
                _utcExpiry = value;
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

        public DateTime DateCreated
        {
            get
            {
                return _dateCreated;
            }
            set
            {
                _dateCreated = value;
            }
        }

        public DateTime DateModified
        {
            get
            {
                return _dateModified;
            }
            set
            {
                _dateModified = value;
            }
        }

        public string OwnerName
        {
            get
            {
                return _ownerName;
            }
            set
            {
                _ownerName = value;
            }
        }

        public string OwnerEmail
        {
            get
            {
                return _ownerEmail;
            }
            set
            {
                _ownerEmail = value;
            }
        }

        public string OwnerId
        {
            get
            {
                return _ownerId;
            }
            set
            {
                _ownerId = value;
            }
        }

        public string Docs
        {
            get
            {
                return _docs;
            }
            set
            {
                _docs = value;
            }
        }

        public string ExpansionState
        {
            get
            {
                return _expansionState;
            }
            set
            {
                _expansionState = value;
            }
        }

        public string VertScrollState
        {
            get
            {
                return _vertScrollState;
            }
            set
            {
                _vertScrollState = value;
            }
        }

        public string WindowTop
        {
            get
            {
                return _windowTop;
            }
            set
            {
                _windowTop = value;
            }
        }

        public string WindowLeft
        {
            get
            {
                return _windowLeft;
            }
            set
            {
                _windowLeft = value;
            }
        }

        public string WindowBottom
        {
            get
            {
                return _windowBottom;
            }
            set
            {
                _windowBottom = value;
            }
        }

        public string WindowRight
        {
            get
            {
                return _windowRight;
            }
            set
            {
                _windowRight = value;
            }
        }

        public OpmlOutlines Outlines
        {
            get
            {
                return _outlines;
            }
            set
            {
                _outlines = value;
            }
        }

        public void AddOutline(OpmlOutline item)
        {
            _outlines.Add(item);
        }

        public void AddOutline(string text, string type, Uri xmlUrl, string category)
        {
            AddOutline(text, type, xmlUrl, category, null);
        }

        public void AddOutline(string text, string type, Uri xmlUrl, string category, OpmlOutlines outlines)
        {
            var item = new OpmlOutline();
            item.Text = text;
            item.Type = type;
            item.XmlUrl = xmlUrl;
            item.Category = category;
            item.Outlines = outlines;
            _outlines.Add(item);
        }

        public string GetXml()
        {
            return opmlDoc.OuterXml;
        }

        public void Save(string fileName)
        {
            opmlDoc = new XmlDocument { XmlResolver = null };
            XmlElement opml = opmlDoc.CreateElement("opml");
            opml.SetAttribute("version", "2.0");
            opmlDoc.AppendChild(opml);

            // create head    
            XmlElement head = opmlDoc.CreateElement("head");
            opml.AppendChild(head);

            // set Title
            XmlElement title = opmlDoc.CreateElement("title");
            title.InnerText = Title;
            head.AppendChild(title);

            // set date created
            XmlElement dateCreated = opmlDoc.CreateElement("dateCreated");
            dateCreated.InnerText = DateCreated != DateTime.MinValue ? DateCreated.ToString("r", null) : DateTime.Now.ToString("r", null);
            head.AppendChild(dateCreated);

            // set date modified
            XmlElement dateModified = opmlDoc.CreateElement("dateModified");
            dateCreated.InnerText = DateModified != DateTime.MinValue ? DateModified.ToString("r", null) : DateTime.Now.ToString("r", null);
            head.AppendChild(dateModified);

            // set owner email
            XmlElement ownerEmail = opmlDoc.CreateElement("ownerEmail");
            ownerEmail.InnerText = OwnerEmail;
            head.AppendChild(ownerEmail);

            // set owner name
            XmlElement ownerName = opmlDoc.CreateElement("ownerName");
            ownerName.InnerText = OwnerName;
            head.AppendChild(ownerName);

            // set owner id
            XmlElement ownerId = opmlDoc.CreateElement("ownerId");
            ownerId.InnerText = OwnerId;
            head.AppendChild(ownerId);

            // set docs
            XmlElement docs = opmlDoc.CreateElement("docs");
            docs.InnerText = Docs;
            head.AppendChild(docs);

            // set expansionState
            XmlElement expansionState = opmlDoc.CreateElement("expansionState");
            expansionState.InnerText = ExpansionState;
            head.AppendChild(expansionState);

            // set vertScrollState
            XmlElement vertScrollState = opmlDoc.CreateElement("vertScrollState");
            vertScrollState.InnerText = VertScrollState;
            head.AppendChild(vertScrollState);

            // set windowTop
            XmlElement windowTop = opmlDoc.CreateElement("windowTop");
            windowTop.InnerText = WindowTop;
            head.AppendChild(windowTop);

            // set windowLeft
            XmlElement windowLeft = opmlDoc.CreateElement("windowLeft");
            windowLeft.InnerText = WindowLeft;
            head.AppendChild(windowLeft);

            // set windowBottom
            XmlElement windowBottom = opmlDoc.CreateElement("windowBottom");
            windowBottom.InnerText = WindowBottom;
            head.AppendChild(windowBottom);

            // set windowRight
            XmlElement windowRight = opmlDoc.CreateElement("windowRight");
            windowRight.InnerText = WindowRight;
            head.AppendChild(windowRight);

            // create body
            XmlElement opmlBody = opmlDoc.CreateElement("body");
            opml.AppendChild(opmlBody);

            foreach (OpmlOutline outline in _outlines)
            {
                opmlBody.AppendChild(outline.ToXml);
            }

            opmlDoc.Save(fileName);
        }

        public static Opml LoadFromUrl(Uri uri)
        {
            return (OpmlDownloadManager.GetOpmlFeed(uri));
        }

        public static Opml LoadFromFile(string path)
        {
            try
            {
                var opmlDoc = new XmlDocument { XmlResolver = null };
                opmlDoc.Load(path);

                return (LoadFromXml(opmlDoc));
            }
            catch
            {
                return (new Opml());
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
                return (new Opml());
            }
        }

        internal static OpmlOutline ParseXml(XmlElement node)
        {
            var newOutline = new OpmlOutline();

            newOutline.Text = ParseElement(node, "text");
            newOutline.Type = ParseElement(node, "type");
            newOutline.IsComment = (ParseElement(node, "isComment") == "true" ? true : false);
            newOutline.IsBreakpoint = (ParseElement(node, "isBreakpoint") == "true" ? true : false);
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
            return (!String.IsNullOrEmpty(attrValue) ? attrValue : string.Empty);
        }
    }
}