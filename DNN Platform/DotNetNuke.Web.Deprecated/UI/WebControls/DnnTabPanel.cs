#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Web.UI.WebControls;

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildrenAttribute(true)]
    public class DnnTabPanel : WebControl
    {
        private DnnTabCollection _Tabs;
        private RadMultiPage _TelerikPages;
        private RadTabStrip _TelerikTabs;

        private RadTabStrip TelerikTabs
        {
            get
            {
                if (_TelerikTabs == null)
                {
                    _TelerikTabs = new RadTabStrip();
                }

                return _TelerikTabs;
            }
        }

        private RadMultiPage TelerikPages
        {
            get
            {
                if (_TelerikPages == null)
                {
                    _TelerikPages = new RadMultiPage();
                }

                return _TelerikPages;
            }
        }

        public DnnTabCollection Tabs
        {
            get
            {
                if (_Tabs == null)
                {
                    _Tabs = new DnnTabCollection(this);
                }

                return _Tabs;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            TelerikTabs.ID = ID + "_Tabs";
            TelerikTabs.Skin = "Office2007";
            TelerikTabs.EnableEmbeddedSkins = true;

            TelerikPages.ID = ID + "_Pages";

            TelerikTabs.MultiPageID = TelerikPages.ID;

            Controls.Add(TelerikTabs);
            Controls.Add(TelerikPages);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if ((!Page.IsPostBack))
            {
                TelerikTabs.SelectedIndex = 0;
                TelerikPages.SelectedIndex = 0;

                int idIndex = 0;

                foreach (DnnTab t in Tabs)
                {
                    RadTab tab = new RadTab();
                    tab.TabTemplate = t.Header;
                    RadPageView pageView = new RadPageView();
                    pageView.Controls.Add(t);

                    tab.PageViewID = "PV_" + idIndex;
                    pageView.ID = "PV_" + idIndex;

                    TelerikTabs.Tabs.Add(tab);
                    TelerikPages.PageViews.Add(pageView);

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