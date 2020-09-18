// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;

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
                return this._useSkinPath;
            }

            set
            {
                this._useSkinPath = value;
            }
        }

        public string Media { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.AddStyleSheet();
        }

        protected void AddStyleSheet()
        {
            // Find the placeholder control
            Control objCSS = this.Page.FindControl("CSS");
            if (objCSS != null)
            {
                // First see if we have already added the <LINK> control
                Control objCtrl = this.Page.Header.FindControl(this.ID);
                if (objCtrl == null)
                {
                    string skinpath = string.Empty;
                    if (this.UseSkinPath)
                    {
                        skinpath = ((Skin)this.Parent).SkinPath;
                    }

                    var objLink = new HtmlLink();
                    objLink.ID = Globals.CreateValidID(this.Name);
                    objLink.Attributes["rel"] = "stylesheet";
                    objLink.Attributes["type"] = "text/css";
                    objLink.Href = skinpath + this.StyleSheet;
                    if (this.Media != string.Empty)
                    {
                        objLink.Attributes["media"] = this.Media; // NWS: add support for "media" attribute
                    }

                    if (this.IsFirst)
                    {
                        // Find the first HtmlLink
                        int iLink;
                        for (iLink = 0; iLink <= objCSS.Controls.Count - 1; iLink++)
                        {
                            if (objCSS.Controls[iLink] is HtmlLink)
                            {
                                break;
                            }
                        }

                        this.AddLink(objCSS, iLink, objLink);
                    }
                    else
                    {
                        this.AddLink(objCSS, -1, objLink);
                    }
                }
            }
        }

        protected void AddLink(Control cssRoot, int InsertAt, HtmlLink link)
        {
            if (string.IsNullOrEmpty(this.Condition))
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
                openif.Text = string.Format("<!--[if {0}]>", this.Condition);
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
                    // Since we want to add at a specific location, we do this in reverse order
                    // this allows us to use the same insertion point
                    cssRoot.Controls.AddAt(InsertAt, closeif);
                    cssRoot.Controls.AddAt(InsertAt, link);
                    cssRoot.Controls.AddAt(InsertAt, openif);
                }
            }
        }
    }
}
