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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Common;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    public partial class Styles : SkinObjectBase
    {
        private bool _useSkinPath = true;

        public string Condition { get; set; }

        public bool IsFirst { get; set; }

        public string Name { get; set; }

        public string StyleSheet { get; set; }

        public bool UseSkinPath
        {
            get
            {
                return _useSkinPath;
            }
            set
            {
                _useSkinPath = value;
            }
        }

        public string Media { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            AddStyleSheet();
        }

        protected void AddStyleSheet()
        {
            //Find the placeholder control
            Control objCSS = Page.FindControl("CSS");
            if (objCSS != null)
            {
                //First see if we have already added the <LINK> control
                Control objCtrl = Page.Header.FindControl(ID);
                if (objCtrl == null)
                {
                    string skinpath = string.Empty;
                    if (UseSkinPath)
                    {
                        skinpath = ((Skin) Parent).SkinPath;
                    }
                    var objLink = new HtmlLink();
                    objLink.ID = Globals.CreateValidID(Name);
                    objLink.Attributes["rel"] = "stylesheet";
                    objLink.Attributes["type"] = "text/css";
                    objLink.Href = skinpath + StyleSheet;
                    if (Media != "")
                    {
                        objLink.Attributes["media"] = Media; //NWS: add support for "media" attribute
                    }
                    if (IsFirst)
                    {
						//Find the first HtmlLink
                        int iLink;
                        for (iLink = 0; iLink <= objCSS.Controls.Count - 1; iLink++)
                        {
                            if (objCSS.Controls[iLink] is HtmlLink)
                            {
                                break;
                            }
                        }
                        AddLink(objCSS, iLink, objLink);
                    }
                    else
                    {
                        AddLink(objCSS, -1, objLink);
                    }
                }
            }
        }

        protected void AddLink(Control cssRoot, int InsertAt, HtmlLink link)
        {
            if (string.IsNullOrEmpty(Condition))
            {
                if (InsertAt == -1)
                {
                    cssRoot.Controls.Add(link);
                }
                else
                {
                    cssRoot.Controls.AddAt(InsertAt, link);
                }
            }
            else
            {
                var openif = new Literal();
                openif.Text = string.Format("<!--[if {0}]>", Condition);
                var closeif = new Literal();
                closeif.Text = "<![endif]-->";
                if (InsertAt == -1)
                {
                    cssRoot.Controls.Add(openif);
                    cssRoot.Controls.Add(link);
                    cssRoot.Controls.Add(closeif);
                }
                else
                {
					//Since we want to add at a specific location, we do this in reverse order
                    //this allows us to use the same insertion point
                    cssRoot.Controls.AddAt(InsertAt, closeif);
                    cssRoot.Controls.AddAt(InsertAt, link);
                    cssRoot.Controls.AddAt(InsertAt, openif);
                }
            }
        }
    }
}