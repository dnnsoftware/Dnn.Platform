// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web control for editing a form.</summary>
    [ParseChildren(true)]
    public partial class DnnFormEditor : WebControl, INamingContainer
    {
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private object dataSource;
        private int itemCount;

        /// <summary>Initializes a new instance of the <see cref="DnnFormEditor"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnFormEditor()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormEditor"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnFormEditor(IHostSettings hostSettings, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

            this.Items = [];
            this.Sections = [];
            this.Tabs = [];

            this.FormMode = DnnFormMode.Long;
            this.ViewStateMode = ViewStateMode.Disabled;
        }

        /// <summary>Gets a value indicating whether the editor is valid.</summary>
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

        /// <summary>Gets or sets the data source.</summary>
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }

            set
            {
                if (this.dataSource != value)
                {
                    this.dataSource = value;
                    if (this.Page.IsPostBack)
                    {
                        this.DataBindItems(false);
                    }
                }
            }
        }

        /// <summary>Gets or sets the form mode.</summary>
        public DnnFormMode FormMode { get; set; }

        /// <summary>Gets or sets a value indicating whether to encrypt IDs.</summary>
        public bool EncryptIds { get; set; }

        /// <summary>Gets the items.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormItemBase> Items { get; private set; }

        /// <summary>Gets the sections.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormSection> Sections { get; private set; }

        /// <summary>Gets the tabs.</summary>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<DnnFormTab> Tabs { get; private set; }

        /// <summary>Gets the local resource file path.</summary>
        protected string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(this);
            }
        }

        /// <inheritdoc />
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <inheritdoc />
        public override void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
            this.Controls.Clear();
            this.ClearChildViewState();
            this.TrackViewState();
            this.CreateControlHierarchy(true);
            this.ChildControlsCreated = true;
        }

        /// <summary>Sets the items up.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="items">The items.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="encryptIds">Whether to encrypt the IDs.</param>
        internal static void SetUpItems(IHostSettings hostSettings, IEnumerable<DnnFormItemBase> items, WebControl parentControl, bool encryptIds)
        {
            foreach (DnnFormItemBase item in items)
            {
                if (encryptIds)
                {
                    item.ID = (hostSettings.Guid.Substring(0, 7) + item.ID + DateTime.Now.Day).GenerateHash();
                }

                parentControl.Controls.Add(item);
            }
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            // CreateChildControls re-creates the children (the items)
            // using the saved view state.
            // First clear any existing child controls.
            this.Controls.Clear();

            // Create the items only if there is view state
            // corresponding to the children.
            if (this.itemCount > 0)
            {
                this.CreateControlHierarchy(false);
            }
        }

        /// <summary>Creates the control hierarchy.</summary>
        /// <param name="useDataSource">Whether to use the data source.</param>
        protected virtual void CreateControlHierarchy(bool useDataSource)
        {
            this.CssClass = string.IsNullOrEmpty(this.CssClass)
                ? "dnnForm"
                : this.CssClass.Contains("dnnForm")
                    ? this.CssClass
                    : $"dnnForm {this.CssClass}";

            this.SetUpTabs();

            this.SetUpSections(this.Sections, this);

            SetUpItems(this.hostSettings, this.Items, this, this.EncryptIds);

            this.DataBindItems(useDataSource);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override void LoadControlState(object state)
        {
            if (state != null)
            {
                this.itemCount = (int)state;
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            this.Page.RegisterRequiresControlState(this);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.DnnPlugins);
            base.OnInit(e);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override object SaveControlState()
        {
            return this.itemCount > 0 ? (object)this.itemCount : null;
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

                    SetUpItems(this.hostSettings, this.Items, panel, this.EncryptIds);
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

                    SetUpItems(this.hostSettings, formTab.Items, tab, this.EncryptIds);
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

            this.itemCount = this.GetAllItems().Count;
        }
    }
}
