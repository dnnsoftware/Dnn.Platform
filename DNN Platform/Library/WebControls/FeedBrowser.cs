#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Syndication;

#endregion

namespace DotNetNuke.UI.WebControls
{
    [DefaultProperty("RssProxyUrl"), ToolboxData("<{0}:FeedBrowser runat=server></{0}:FeedBrowser>")]
    public class FeedBrowser : WebControlBase
    {
		#region "Private Members"

        private readonly StringBuilder output = new StringBuilder("");
        private bool _allowHtmlDescription = true;
        private string _defaultTemplate = "";
        private string _opmlFile = "";
        private string _opmlText = "";
        private string _opmlUrl = "";
        private string _rssProxyUrl = "";
		
		#endregion

		#region "Public Properties"

        public string DefaultTemplate
        {
            get
            {
                return _defaultTemplate;
            }
            set
            {
                _defaultTemplate = value;
            }
        }

        public string RssProxyUrl
        {
            get
            {
                return _rssProxyUrl;
            }
            set
            {
                _rssProxyUrl = value;
            }
        }

        public bool AllowHtmlDescription
        {
            get
            {
                return _allowHtmlDescription;
            }
            set
            {
                _allowHtmlDescription = value;
            }
        }

        public string OpmlUrl
        {
            get
            {
                return _opmlUrl;
            }
            set
            {
                _opmlUrl = value;
            }
        }

        public string OpmlFile
        {
            get
            {
                return _opmlFile;
            }
            set
            {
                _opmlFile = value;
            }
        }

        public string OpmlText
        {
            get
            {
                return _opmlText;
            }
            set
            {
                _opmlText = value;
            }
        }

        public override string HtmlOutput
        {
            get
            {
                return (output.ToString());
            }
        }
		
		#endregion
		
		#region "Protected Methods"

        protected override void OnLoad(EventArgs e)
        {
            var opmlFeed = new Opml();
            string elementIdPrefix = ClientID;
            string instanceVarName = elementIdPrefix + "_feedBrowser";
            if (((String.IsNullOrEmpty(OpmlUrl)) && (String.IsNullOrEmpty(OpmlFile)) && (String.IsNullOrEmpty(OpmlText))))
            {
                opmlFeed = GetDefaultOpmlFeed();
            }
            else
            {
                if ((!String.IsNullOrEmpty(OpmlText)))
                {
                    var opmlDoc = new XmlDocument();
                    opmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><opml version=\"2.0\"><head /><body>" + OpmlText + "</body></opml>");
                    opmlFeed = Opml.LoadFromXml(opmlDoc);
                }
                else if ((!String.IsNullOrEmpty(OpmlUrl)))
                {
                    opmlFeed = Opml.LoadFromUrl(new Uri(OpmlUrl));
                }
                else
                {
                    opmlFeed = Opml.LoadFromFile(OpmlFile);
                }
            }
            var script = new StringBuilder();

            string tabInstanceVarName = instanceVarName + "_tabs";
            script.Append("var " + tabInstanceVarName + " = new DotNetNuke.UI.WebControls.TabStrip.Strip(\"" + tabInstanceVarName + "\");");
            script.Append("var " + instanceVarName + " = new DotNetNuke.UI.WebControls.FeedBrowser.Browser(\"" + instanceVarName + "\"," + tabInstanceVarName + ");");
            script.Append(tabInstanceVarName + ".setResourcesFolderUrl(\"" + ResourcesFolderUrl + "\");");
            script.Append(instanceVarName + ".setResourcesFolderUrl(\"" + ResourcesFolderUrl + "\");");

            script.Append(instanceVarName + ".setElementIdPrefix(\"" + elementIdPrefix + "\");");
            if ((!String.IsNullOrEmpty(Theme)))
            {
                script.Append(instanceVarName + ".setTheme(\"" + Theme + "\");");
            }
            if ((!String.IsNullOrEmpty(RssProxyUrl)))
            {
                script.Append(instanceVarName + ".setRssProxyUrl(\"" + RssProxyUrl + "\");");
            }
            if ((!String.IsNullOrEmpty(DefaultTemplate)))
            {
                script.Append(instanceVarName + ".setDefaultTemplate(\"" + DefaultTemplate + "\");");
            }
            script.Append(instanceVarName + ".setAllowHtmlDescription(");
            if ((AllowHtmlDescription))
            {
                script.Append("true");
            }
            else
            {
                script.Append("false");
            }
            script.Append(");");

            string renderScript = GetRenderingScript(instanceVarName, opmlFeed.Outlines);
            bool includeFallbackScript = false;
            
			//Is there any OPML structure to render?
			if ((String.IsNullOrEmpty(renderScript)))
            {
                includeFallbackScript = true;
                script.Append(instanceVarName + ".setTabs(defaultFeedBrowser());");
            }
            else
            {
				//Javascript function that renders the OPML structure
                script.Append("function " + instanceVarName + "customFeedBrowser() ");
                script.Append("{");
                script.Append("     var " + instanceVarName + "tabs = [];");
                script.Append("     with (DotNetNuke.UI.WebControls.FeedBrowser) ");
                script.Append("     {");
                script.Append(renderScript);
                script.Append("     }");
                script.Append("     return(" + instanceVarName + "tabs);");
                script.Append("} ");
                script.Append(instanceVarName + ".setTabs(" + instanceVarName + "customFeedBrowser());");
            }
            		
            //NK 11/25/08
            //This code has a jQuery dependency so it can't be loaded using standard client script registration
            //It must come later in the page which is why it is inline; also ClientScript buggy when used in webcontrols
			if (!Page.ClientScript.IsClientScriptBlockRegistered("FBHostUrl"))
            {
                Page.ClientScript.RegisterClientScriptBlock(GetType(),
                                                            "FBHostUrl",
                                                            "<script type=\"text/javascript\">var $dnn = new Object(); $dnn.hostUrl = '" + Globals.ResolveUrl("~/") + "';</script>");
                output.Append("<script type=\"text/javascript\" src=\"" + Globals.ResolveUrl("~/Resources/Shared/scripts/init.js") + "\"></script>");
                output.Append("<script type=\"text/javascript\" src=\"" + Globals.ResolveUrl("~/Resources/Shared/scripts/DotNetNukeAjaxShared.js") + "\"></script>");
                output.Append("<script type=\"text/javascript\" src=\"" + Globals.ResolveUrl("~/Resources/TabStrip/scripts/tabstrip.js") + "\"></script>");
                output.Append("<script type=\"text/javascript\" src=\"" + Globals.ResolveUrl("~/Resources/FeedBrowser/scripts/feedbrowser.js") + "\"></script>");
                output.Append("<script type=\"text/javascript\" src=\"" + Globals.ResolveUrl("~/Resources/FeedBrowser/scripts/templates.js") + "\"></script>");
                if ((includeFallbackScript))
                {
                    output.Append("<script type=\"text/javascript\" src=\"" + Globals.ResolveUrl("~/Resources/FeedBrowser/scripts/fallback.js") + "\"></script>");
                }
                if ((!String.IsNullOrEmpty(StyleSheetUrl)))
                {
                    script.Append(tabInstanceVarName + ".setStyleSheetUrl(\"" + StyleSheetUrl + "\");");
                    script.Append(instanceVarName + ".setStyleSheetUrl(\"" + StyleSheetUrl + "\");");
                }
            }
            script.Append(instanceVarName + ".render();");
            output.Append("<script type=\"text/javascript\">" + script + "</script>");

            base.OnLoad(e);
        }

		#endregion

		#region "Private Methods"

        private Opml GetDefaultOpmlFeed()
        {
            var opmlFeed = new Opml();
            string filePath = Config.GetPathToFile(Config.ConfigFileType.SolutionsExplorer);
            if (File.Exists(filePath))
            {
                opmlFeed = Opml.LoadFromFile(filePath);
            }
            return opmlFeed;
        }

        private string GetRenderingScript(string instanceVarName, OpmlOutlines _outlines)
        {
            string script = "";

            //First fetch any linked OPML files
            //Only one level of link fetching is supported so
            //no recursion
            var expandedOutlines = new OpmlOutlines();
            foreach (OpmlOutline item in _outlines)
            {
                if ((item.Type == "link"))
                {
                    Opml linkedFeed = Opml.LoadFromUrl(item.XmlUrl);
                    if ((item.Category.StartsWith("Tab")))
                    {
                        expandedOutlines.Add(item);
                        foreach (OpmlOutline linkedOutline in linkedFeed.Outlines)
                        {
                            item.Outlines.Add(linkedOutline);
                        }
                    }
                    else
                    {
                        foreach (OpmlOutline linkedOutline in linkedFeed.Outlines)
                        {
                            expandedOutlines.Add(linkedOutline);
                        }
                    }
                }
                else
                {
                    expandedOutlines.Add(item);
                }
            }
            script = GetTabsScript(instanceVarName, expandedOutlines);

            return script;
        }

        private string GetTabsScript(string instanceVarName, OpmlOutlines _outlines)
        {
            var js = new StringBuilder("");
            int tabCounter = -1;
            foreach (OpmlOutline item in _outlines)
            {
                if ((item.Category.StartsWith("Tab")))
                {
                    tabCounter = tabCounter + 1;
                    string tabVarName = instanceVarName + "tab" + tabCounter;

                    //Create a call to the "addTab" method
                    //addTab accepts one parameter -- a TabInfo object
                    //Here the TabInfo object is dynamically created
                	//with the parameters  Label, Url and Template
                    js.Append("var " + tabVarName + " = new TabInfo('" + item.Text + "',");

                    if ((item.Type == "none"))
                    {
                        js.Append("''");
                    }
                    else
                    {
                        js.Append("'" + item.XmlUrl.AbsoluteUri + "'");
                    }
					
                    //Template detection
                    //The category field indicates if the outline node is a tab, section or category
                    //If the field value includes a / character, then portion of the value to the right of /
                    //contains the name of the template that should be used for that tab/section/category
                    //and its children
                    if ((item.Category.IndexOf("/") > 0))
                    {
                        js.Append(",'" + item.Category.Substring(item.Category.IndexOf("/") + 1) + "'");
                    }
                    js.Append(");");
                    if ((item.Outlines != null))
                    {
                        js.Append(GetSectionsScript(item.Outlines, tabVarName));
                    }
                    js.Append(instanceVarName + "tabs[" + instanceVarName + "tabs.length] = " + tabVarName + ";");
                }
            }
            return js.ToString();
        }

        private string GetSectionsScript(OpmlOutlines _outlines, string tabVarName)
        {
            var js = new StringBuilder("");
            int sectionCounter = -1;
            foreach (OpmlOutline item in _outlines)
            {
                if ((item.Category.StartsWith("Section")))
                {
                    sectionCounter = sectionCounter + 1;
                    string sectionVarName = tabVarName + "_" + sectionCounter;
                    string sectionUrl = "";
                    if ((item.XmlUrl != null))
                    {
                        sectionUrl = ", '" + item.XmlUrl.AbsoluteUri + "'";
                    }
                    //Create a call to the addSection method
                    //addSection accepts one parameter -- a SectionInfo object
                    //Here the SectionInfo object is dyncamically created
                    //with the parameters Label, Url and Template
                    //A section Url is the Url called for obtaining search results
                    //If the Url contains a [KEYWORD] token, the user's search keyword
                    //is substituted for the token. If no token exists then &keyword={keyword value}
                    //is appended to the Url
                    js.Append("var " + sectionVarName + " = " + tabVarName + ".addSection(new SectionInfo('" + item.Text + "'" + sectionUrl);

                    //Template detection
                    //The category field indicates if the outline node is a tab, section or category
                    //If the field value includes a / character, then portion of the value to the right of /
                    //contains the name of the template that should be used for that section/category
                    //and its children
                    if ((item.Category.IndexOf("/") > 0))
                    {
                        js.Append(",'" + item.Category.Substring(item.Category.IndexOf("/") + 1) + "'");
                    }
                    js.Append("));");
                    if ((item.Outlines != null))
                    {
                        int counter = -1;
                        js.Append(GetCategoriesScript(item.Outlines, sectionVarName, -1, ref counter));
                    }
                }
            }
            return js.ToString();
        }

        private string GetCategoriesScript(OpmlOutlines _outlines, string sectionVarName, int depth, ref int counter)
        {
            var js = new StringBuilder("");
            depth = depth + 1;
            foreach (OpmlOutline item in _outlines)
            {
                if ((item.Category.StartsWith("Category")))
                {
                    counter = counter + 1;
                    js.Append(sectionVarName + ".addCategory(new CategoryInfo('" + item.Text + "','" + item.XmlUrl.AbsoluteUri + "'," + depth);

                    //Template detection
                    //The category field indicates if the outline node is a tab, section or category
                    //If the field value includes a / character, then portion of the value to the right of /
                    //contains the name of the template that should be used for that category
                    if ((item.Category.IndexOf("/") > 0))
                    {
                        js.Append(",'" + item.Category.Substring(item.Category.IndexOf("/") + 1) + "'");
                    }
                    js.Append("));");

                    //If the Category field includes "Default" in its list of values,
                    //the item is marked as the default category
                    if ((item.Category.IndexOf("Default") > -1))
                    {
                        js.Append(sectionVarName + ".setDefaultCategory(" + counter + ");");
                    }
                    if ((item.Outlines != null))
                    {
                        js.Append(GetCategoriesScript(item.Outlines, sectionVarName, depth, ref counter));
                    }
                }
            }
            return js.ToString();
        }
		
		#endregion
    }
}