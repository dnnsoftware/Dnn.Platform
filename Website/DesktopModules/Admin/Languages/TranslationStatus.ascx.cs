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
using System.Collections.Generic;

using DotNetNuke.Admin.Modules;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Modules.Admin.Tabs;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;

using Telerik.Web.UI;


#endregion

namespace DotNetNuke.Modules.Admin.Languages
{
    public partial class TranslationStatus : ModuleUserControlBase
    {
        #region "Public Properties"

        protected Locale Language
        {
            get
            {
                return LocaleController.Instance.GetLocale(Request.QueryString["locale"]);
            }
        }

        protected List<TabInfo> Tabs
        {
            get
            {
                var tabList = new List<TabInfo>();
                foreach (TabInfo t in
                    TabController.GetPortalTabs(TabController.GetTabsBySortOrder(ModuleContext.PortalId, Language.Code, false), Null.NullInteger, false, "", false, false, false, false, false))
                {
                    TabInfo newTab = t.Clone();
                    if (newTab.ParentId == Null.NullInteger)
                    {
                        newTab.ParentId = 0;
                    }
                    tabList.Add(newTab);
                }
                return tabList;
            }
        }

        #endregion

        #region "Private Methods"

        private void LocalizeSelectedItems(bool localize, RadTreeNodeCollection nodes)
        {
            foreach (RadTreeNode node in nodes)
            {
                var moduleLocalization = (ModuleLocalization) node.FindControl("moduleLocalization");
                if (moduleLocalization != null)
                {
                    moduleLocalization.LocalizeSelectedItems(localize);

                    //Recursively call for child nodes
                    LocalizeSelectedItems(localize, node.Nodes);
                }
            }
        }

        private void MarkTranslatedSelectedItems(bool translated, RadTreeNodeCollection nodes)
        {
            foreach (RadTreeNode node in nodes)
            {
                var moduleLocalization = (ModuleLocalization) node.FindControl("moduleLocalization");
                var tabLocalization = (TabLocalization) node.FindControl("tabLocalization");
                if (moduleLocalization != null)
                {
                    moduleLocalization.MarkTranslatedSelectedItems(translated);
                }
                if (tabLocalization != null)
                {
                    tabLocalization.MarkTranslatedSelectedItems(translated);
                }

                //Recursively call for child nodes
                MarkTranslatedSelectedItems(translated, node.Nodes);
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            delocalizeModuleButton.Click += delocalizeModuleButton_Click;
            localizeModuleButton.Click += localizeModuleButton_Click;
            markModuleTranslatedButton.Click += markModuleTranslatedButton_Click;
            markModuleUnTranslatedButton.Click += markModuleUnTranslatedButton_Click;
            pagesTreeView.NodeDataBound += pagesTreeView_NodeDataBound;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            pagesTreeView.DataSource = Tabs;
            if (!Page.IsPostBack)
            {
                pagesTreeView.DataBind();
            }
        }

        protected void delocalizeModuleButton_Click(object sender, EventArgs e)
        {
            LocalizeSelectedItems(false, pagesTreeView.Nodes);
        }

        protected void localizeModuleButton_Click(object sender, EventArgs e)
        {
            LocalizeSelectedItems(true, pagesTreeView.Nodes);
        }

        protected void markModuleTranslatedButton_Click(object sender, EventArgs e)
        {
            MarkTranslatedSelectedItems(true, pagesTreeView.Nodes);
        }

        protected void markModuleUnTranslatedButton_Click(object sender, EventArgs e)
        {
            MarkTranslatedSelectedItems(false, pagesTreeView.Nodes);
        }

        protected void pagesTreeView_NodeDataBound(object sender, RadTreeNodeEventArgs e)
        {
            var moduleLocalization = (ModuleLocalization) e.Node.FindControl("moduleLocalization");
            var tabLocalization = (TabLocalization) e.Node.FindControl("tabLocalization");
            var boundTab = e.Node.DataItem as TabInfo;
            if (boundTab != null)
            {
                moduleLocalization.TabId = boundTab.TabID;
                tabLocalization.ToLocalizeTabId = boundTab.TabID;

                if (!Page.IsPostBack)
                {
                    moduleLocalization.DataBind();
                    tabLocalization.DataBind();
                }
            }
        }
    }
}