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
