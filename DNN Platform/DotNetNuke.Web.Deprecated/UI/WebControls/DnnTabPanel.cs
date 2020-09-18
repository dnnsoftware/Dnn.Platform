// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Telerik.Web.UI;

    [ParseChildrenAttribute(true)]
    public class DnnTabPanel : WebControl
    {
        private DnnTabCollection _Tabs;
        private RadMultiPage _TelerikPages;
        private RadTabStrip _TelerikTabs;

        public DnnTabCollection Tabs
        {
            get
            {
                if (this._Tabs == null)
                {
                    this._Tabs = new DnnTabCollection(this);
                }

                return this._Tabs;
            }
        }

        private RadTabStrip TelerikTabs
        {
            get
            {
                if (this._TelerikTabs == null)
                {
                    this._TelerikTabs = new RadTabStrip();
                }

                return this._TelerikTabs;
            }
        }

        private RadMultiPage TelerikPages
        {
            get
            {
                if (this._TelerikPages == null)
                {
                    this._TelerikPages = new RadMultiPage();
                }

                return this._TelerikPages;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            this.EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            this.TelerikTabs.ID = this.ID + "_Tabs";
            this.TelerikTabs.Skin = "Office2007";
            this.TelerikTabs.EnableEmbeddedSkins = true;

            this.TelerikPages.ID = this.ID + "_Pages";

            this.TelerikTabs.MultiPageID = this.TelerikPages.ID;

            this.Controls.Add(this.TelerikTabs);
            this.Controls.Add(this.TelerikPages);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!this.Page.IsPostBack)
            {
                this.TelerikTabs.SelectedIndex = 0;
                this.TelerikPages.SelectedIndex = 0;

                int idIndex = 0;

                foreach (DnnTab t in this.Tabs)
                {
                    RadTab tab = new RadTab();
                    tab.TabTemplate = t.Header;
                    RadPageView pageView = new RadPageView();
                    pageView.Controls.Add(t);

                    tab.PageViewID = "PV_" + idIndex;
                    pageView.ID = "PV_" + idIndex;

                    this.TelerikTabs.Tabs.Add(tab);
                    this.TelerikPages.PageViews.Add(pageView);

                    idIndex = idIndex + 1;
                }
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }
    }
}
