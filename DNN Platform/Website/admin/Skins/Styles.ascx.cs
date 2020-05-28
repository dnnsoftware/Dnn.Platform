// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
