// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
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

    [ParseChildren(true)]
    public class DnnFormEditor : WebControl, INamingContainer
    {
        private object _dataSource;
        private int _itemCount;

        public DnnFormEditor()
        {
            this.Items = new List<DnnFormItemBase>();
            this.Sections = new List<DnnFormSection>();
            this.Tabs = new List<DnnFormTab>();

            this.FormMode = DnnFormMode.Long;
            this.ViewStateMode = ViewStateMode.Disabled;
        }

        public bool IsValid
        {
            get
            {
                bool isValid = true;
                foreach (var item in this.GetAllItems())
                {
                    item.CheckIsValid();
                    if (!item.IsValid)
                    {
                        isValid = false;
                        break;
                    }
                }

                return isValid;
            }
        }

        public object DataSource
        {
            get
            {
                return this._dataSource;
            }

            set
            {
                if (this._dataSource != value)
                {
                    this._dataSource = value;
                    if (this.Page.IsPostBack)
                    {
                        this.DataBindItems(false);
                    }
                }
            }
        }

        public DnnFormMode FormMode { get; set; }

        public bool EncryptIds { get; set; }

        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormSection> Sections { get; private set; }

        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormTab> Tabs { get; private set; }

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

        public override void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
            this.Controls.Clear();
            this.ClearChildViewState();
            this.TrackViewState();
            this.CreateControlHierarchy(true);
            this.ChildControlsCreated = true;
        }

        [Obsolete("Obsoleted in Platform 7.4.1, please add encryptIds. Scheduled removal in v10.0.0.")]
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

        protected override void CreateChildControls()
        {
            // CreateChildControls re-creates the children (the items)
            // using the saved view state.
            // First clear any existing child controls.
            this.Controls.Clear();

            // Create the items only if there is view state
            // corresponding to the children.
            if (this._itemCount > 0)
            {
                this.CreateControlHierarchy(false);
            }
        }

        protected virtual void CreateControlHierarchy(bool useDataSource)
        {
            this.CssClass = string.IsNullOrEmpty(this.CssClass) ? "dnnForm" : this.CssClass.Contains("dnnForm") ? this.CssClass : string.Format("dnnForm {0}", this.CssClass);

            this.SetUpTabs();

            this.SetUpSections(this.Sections, this);

            SetUpItems(this.Items, this, this.LocalResourceFile, this.EncryptIds);

            this.DataBindItems(useDataSource);
        }

        protected override void LoadControlState(object state)
        {
            if (state != null)
            {
                this._itemCount = (int)state;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            this.Page.RegisterRequiresControlState(this);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Tabs.Count > 0)
            {
                const string scriptName = "FormEditorjQuery";
                ClientScriptManager cs = this.Page.ClientScript;

                if (!cs.IsClientScriptBlockRegistered(this.GetType(), scriptName))
                {
                    // Render Script
                    var scriptBuilder = new StringBuilder();
                    scriptBuilder.Append("<script language=\"javascript\" type=\"text/javascript\">\r\n");
                    scriptBuilder.Append("\t(function ($, Sys) {\r\n");
                    scriptBuilder.Append("\t\tfunction setupFormEditor() {\r\n");
                    scriptBuilder.Append("\t\t\t$('#" + this.ClientID + "').dnnTabs().dnnPanels();\r\n");
                    foreach (DnnFormTab formTab in this.Tabs)
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
                    cs.RegisterClientScriptBlock(this.GetType(), scriptName, scriptBuilder.ToString());
                }
            }
        }

        protected override object SaveControlState()
        {
            return this._itemCount > 0 ? (object)this._itemCount : null;
        }

        private List<DnnFormItemBase> GetAllItems()
        {
            var items = new List<DnnFormItemBase>();

            // iterate over pages
            foreach (DnnFormTab page in this.Tabs)
            {
                foreach (DnnFormSection section in page.Sections)
                {
                    items.AddRange(section.Items);
                }

                items.AddRange(page.Items);
            }

            // iterate over section
            foreach (DnnFormSection section in this.Sections)
            {
                items.AddRange(section.Items);
            }

            // Add base items
            items.AddRange(this.Items);

            return items;
        }

        private void SetUpSections(List<DnnFormSection> sections, WebControl parentControl)
        {
            if (sections.Count > 0)
            {
                foreach (DnnFormSection section in sections)
                {
                    var panel = new DnnFormPanel { CssClass = "dnnFormSectionHead" };
                    parentControl.Controls.Add(panel);

                    var resourceKey = section.ResourceKey;
                    if (string.IsNullOrEmpty(resourceKey))
                    {
                        resourceKey = section.ID;
                    }

                    panel.Text = Localization.GetString(resourceKey, this.LocalResourceFile);
                    panel.Expanded = section.Expanded;

                    SetUpItems(section.Items, panel, this.LocalResourceFile, this.EncryptIds);
                }
            }
        }

        private void SetUpTabs()
        {
            if (this.Tabs.Count > 0)
            {
                var tabStrip = new DnnFormTabStrip { CssClass = "dnnAdminTabNav dnnClear" };
                this.Controls.Add(tabStrip);
                tabStrip.Items.Clear();

                foreach (DnnFormTab formTab in this.Tabs)
                {
                    var resourceKey = formTab.ResourceKey;
                    if (string.IsNullOrEmpty(resourceKey))
                    {
                        resourceKey = formTab.ID;
                    }

                    var tab = new Panel { CssClass = formTab.ID + " dnnClear", ID = "tab_" + formTab.ID };
                    this.Controls.Add(tab);

                    if (formTab.IncludeExpandAll)
                    {
                        var expandAll = new Panel { CssClass = "dnnFormExpandContent" };
                        string expandAllText = Localization.GetString("ExpandAll", Localization.SharedResourceFile);
                        expandAll.Controls.Add(new LiteralControl("<a href=\"\">" + expandAllText + "</a>"));
                        tab.Controls.Add(expandAll);

                        formTab.ExpandAllScript = "\t\t\t$('#" + tab.ClientID + " .dnnFormExpandContent a').dnnExpandAll({\r\n";
                        formTab.ExpandAllScript += "\t\t\t\texpandText: '" + Localization.GetString("ExpandAll", Localization.SharedResourceFile) + "',\r\n";
                        formTab.ExpandAllScript += "\t\t\t\tcollapseText: '" + Localization.GetString("CollapseAll", Localization.SharedResourceFile) + "',\r\n";
                        formTab.ExpandAllScript += "\t\t\t\ttargetArea: '#" + tab.ClientID + "' });\r\n";
                    }

                    tabStrip.Items.Add(new ListItem(Localization.GetString(resourceKey, this.LocalResourceFile), "#" + tab.ClientID));

                    if (formTab.Sections.Count > 0)
                    {
                        this.SetUpSections(formTab.Sections, tab);
                    }
                    else
                    {
                        tab.CssClass += " dnnFormNoSections";
                    }

                    SetUpItems(formTab.Items, tab, this.LocalResourceFile, this.EncryptIds);
                }
            }
        }

        private void DataBindItems(bool useDataSource)
        {
            var items = this.GetAllItems();

            foreach (DnnFormItemBase item in items)
            {
                if (string.IsNullOrEmpty(item.LocalResourceFile))
                {
                    item.LocalResourceFile = this.LocalResourceFile;
                }

                if (item.FormMode == DnnFormMode.Inherit)
                {
                    item.FormMode = this.FormMode;
                }

                if (this.DataSource != null)
                {
                    item.DataSource = this.DataSource;
                    item.DataBindItem(useDataSource);
                }
            }

            this._itemCount = this.GetAllItems().Count;
        }
    }
}
