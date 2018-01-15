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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    [ParseChildren(true)]
    public class DnnFormEditor : WebControl, INamingContainer
    {
        private object _dataSource;
        private int _itemCount;

        public DnnFormEditor()
        {
            Items = new List<DnnFormItemBase>();
            Sections = new List<DnnFormSection>();
            Tabs = new List<DnnFormTab>();

            FormMode = DnnFormMode.Long;
            ViewStateMode = ViewStateMode.Disabled;
        }

        protected string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(this);
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        public object DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    if (Page.IsPostBack)
                    {
                        DataBindItems(false);
                    }
                }
            }
        }

        public DnnFormMode FormMode { get; set; }

        public bool EncryptIds { get; set; }

        public bool IsValid
        {
            get
            {
                bool isValid = true;
                foreach (var item in GetAllItems())
                {
                    item.CheckIsValid();
                    if(!item.IsValid)
                    {
                        isValid = false;
                        break;
                    }
                }
                return isValid;
            }
        }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormSection> Sections { get; private set; }

        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormTab> Tabs { get; private set; }

        private List<DnnFormItemBase> GetAllItems()
        {
            var items = new List<DnnFormItemBase>();

            //iterate over pages
            foreach (DnnFormTab page in Tabs)
            {
                foreach (DnnFormSection section in page.Sections)
                {
                    items.AddRange(section.Items);
                }
                items.AddRange(page.Items);
            }

            //iterate over section
            foreach (DnnFormSection section in Sections)
            {
                items.AddRange(section.Items);
            }

            //Add base items
            items.AddRange(Items);

            return items;
        }

        #region Private Methods

        [Obsolete("Obsolted in Platform 7.4.1, please add encryptIds")]
        internal static void SetUpItems(IEnumerable<DnnFormItemBase> items, WebControl parentControl, string localResourceFile)
        {
            SetUpItems(items, parentControl, localResourceFile, false);
        }

        internal static void SetUpItems(IEnumerable<DnnFormItemBase> items, WebControl parentControl, string localResourceFile, bool encryptIds)
        {
            foreach (DnnFormItemBase item in items)
            {
                if (encryptIds)
                {
                    item.ID = (Host.GUID.Substring(0, 7) + item.ID + DateTime.Now.Day).GenerateHash();
                }

                parentControl.Controls.Add(item);
            }
        }

        private void SetUpSections(List<DnnFormSection> sections, WebControl parentControl)
        {
            if (sections.Count > 0)
            {
                foreach (DnnFormSection section in sections)
                {
                    var panel = new DnnFormPanel {CssClass = "dnnFormSectionHead"};
                    parentControl.Controls.Add(panel);

                    var resourceKey = section.ResourceKey;
                    if (String.IsNullOrEmpty(resourceKey))
                    {
                        resourceKey = section.ID;
                    }
                    panel.Text = Localization.GetString(resourceKey, LocalResourceFile);
                    panel.Expanded = section.Expanded;

                    SetUpItems(section.Items, panel, LocalResourceFile, EncryptIds);
                }
            }
        }

        private void SetUpTabs()
        {
            if (Tabs.Count > 0)
            {
                var tabStrip = new DnnFormTabStrip {CssClass = "dnnAdminTabNav dnnClear"};
                Controls.Add(tabStrip);
                tabStrip.Items.Clear();

                foreach (DnnFormTab formTab in Tabs)
                {
                    var resourceKey = formTab.ResourceKey;
                    if (String.IsNullOrEmpty(resourceKey))
                    {
                        resourceKey = formTab.ID;
                    }

                    var tab = new Panel {CssClass = formTab.ID + " dnnClear", ID = "tab_" + formTab.ID};
                    Controls.Add(tab);

                    if (formTab.IncludeExpandAll)
                    {
                        var expandAll = new Panel {CssClass = "dnnFormExpandContent"};
                        string expandAllText = Localization.GetString("ExpandAll", Localization.SharedResourceFile);
                        expandAll.Controls.Add(new LiteralControl("<a href=\"\">" + expandAllText + "</a>"));
                        tab.Controls.Add(expandAll);

                        formTab.ExpandAllScript = "\t\t\t$('#" + tab.ClientID + " .dnnFormExpandContent a').dnnExpandAll({\r\n";
                        formTab.ExpandAllScript += "\t\t\t\texpandText: '" + Localization.GetString("ExpandAll", Localization.SharedResourceFile) + "',\r\n";
                        formTab.ExpandAllScript += "\t\t\t\tcollapseText: '" + Localization.GetString("CollapseAll", Localization.SharedResourceFile) + "',\r\n";
                        formTab.ExpandAllScript += "\t\t\t\ttargetArea: '#" + tab.ClientID + "' });\r\n";
                    }

                    tabStrip.Items.Add(new ListItem(Localization.GetString(resourceKey, LocalResourceFile), "#" + tab.ClientID));

                    if (formTab.Sections.Count > 0)
                    {
                        SetUpSections(formTab.Sections, tab);
                    }
                    else
                    {
                        tab.CssClass += " dnnFormNoSections";
                    }

                    SetUpItems(formTab.Items, tab, LocalResourceFile, EncryptIds);
                }
            }
        }

        #endregion

        #region Control Hierarchy and Data Binding

        protected override void CreateChildControls()
        {
            // CreateChildControls re-creates the children (the items)
            // using the saved view state.
            // First clear any existing child controls.
            Controls.Clear();

            // Create the items only if there is view state
            // corresponding to the children.
            if (_itemCount > 0)
            {
                CreateControlHierarchy(false);
            }
        }

        private void DataBindItems(bool useDataSource)
        {
            var items = GetAllItems();

            foreach (DnnFormItemBase item in items)
            {
                if (String.IsNullOrEmpty(item.LocalResourceFile))
                {
                    item.LocalResourceFile = LocalResourceFile;
                }
                if (item.FormMode == DnnFormMode.Inherit)
                {
                    item.FormMode = FormMode;
                }

                if (DataSource != null)
                {
                    item.DataSource = DataSource;
                    item.DataBindItem(useDataSource);
                }
            }
            _itemCount = GetAllItems().Count;
        }

        protected virtual void CreateControlHierarchy(bool useDataSource)
        {
        	CssClass = string.IsNullOrEmpty(CssClass) ? "dnnForm" : CssClass.Contains("dnnForm") ? CssClass : string.Format("dnnForm {0}", CssClass);

        	SetUpTabs();

            SetUpSections(Sections, this);

            SetUpItems(Items, this, LocalResourceFile, EncryptIds);

            DataBindItems(useDataSource);
        }

        public override void DataBind()
        {
            base.OnDataBinding(EventArgs.Empty);
            Controls.Clear();
            ClearChildViewState();
            TrackViewState();
            CreateControlHierarchy(true);
            ChildControlsCreated = true;
        }

        #endregion

        #region Protected Methods

        protected override void LoadControlState(object state)
        {
            if (state != null)
            {
                _itemCount = (int) state;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if(Tabs.Count > 0)
            {
                const string scriptName = "FormEditorjQuery";
                ClientScriptManager cs = Page.ClientScript;


                if (!cs.IsClientScriptBlockRegistered(GetType(), scriptName))
                {
                    //Render Script
                    var scriptBuilder = new StringBuilder();
                    scriptBuilder.Append("<script language=\"javascript\" type=\"text/javascript\">\r\n");
                    scriptBuilder.Append("\t(function ($, Sys) {\r\n");
                    scriptBuilder.Append("\t\tfunction setupFormEditor() {\r\n");
                    scriptBuilder.Append("\t\t\t$('#" + ClientID + "').dnnTabs().dnnPanels();\r\n");
                    foreach (DnnFormTab formTab in Tabs)
                    {
                        if (formTab.IncludeExpandAll)
                        {
                            scriptBuilder.Append(formTab.ExpandAllScript);
                        }
                    }
                    scriptBuilder.Append("\t\t}\r\n");
                    scriptBuilder.Append("\t\t$(document).ready(function () {\r\n");
                    scriptBuilder.Append("\t\t\tsetupFormEditor();\r\n");
                    scriptBuilder.Append("\t\t\tif (typeof Sys != 'undefined') {\r\n");
                    scriptBuilder.Append("\t\t\t\tSys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {\r\n");
                    scriptBuilder.Append("\t\t\t\t\tsetupFormEditor();\r\n");
                    scriptBuilder.Append("\t\t\t\t});\r\n");
                    scriptBuilder.Append("\t\t\t}\r\n");
                    scriptBuilder.Append("\t\t});\r\n");
                    scriptBuilder.Append("\t} (jQuery, window.Sys));\r\n");

                    scriptBuilder.Append("</script>\r\n");
                    cs.RegisterClientScriptBlock(GetType(), scriptName, scriptBuilder.ToString());
                }
            }
        }

        protected override object SaveControlState()
        {
            return _itemCount > 0 ? (object) _itemCount : null;
        }

        #endregion
    }
}