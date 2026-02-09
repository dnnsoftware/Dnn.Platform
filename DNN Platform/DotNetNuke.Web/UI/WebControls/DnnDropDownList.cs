// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>A dropdown list control.</summary>
    [ToolboxData("<{0}:DnnDropDownList runat='server'></{0}:DnnDropDownList>")]
    public class DnnDropDownList : Panel, INamingContainer
    {
        private static readonly object EventSelectionChanged = new object();

        private readonly Lazy<DnnDropDownListOptions> options = new Lazy<DnnDropDownListOptions>(() => new DnnDropDownListOptions());
        private readonly IClientResourceController clientResourceController;
        private readonly IServicesFramework servicesFramework;

        private DnnGenericHiddenField<DnnDropDownListState> stateControl;
        private HtmlAnchor selectedValue;

        /// <summary>Initializes a new instance of the <see cref="DnnDropDownList"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnDropDownList()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnDropDownList"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnDropDownList(IClientResourceController clientResourceController, IServicesFramework servicesFramework)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        }

        /// <summary>Occurs when the selection from the list control changes between posts to the server.</summary>
        public event EventHandler SelectionChanged
        {
            add => this.Events.AddHandler(EventSelectionChanged, value);
            remove => this.Events.RemoveHandler(EventSelectionChanged, value);
        }

        /// <inheritdoc />
        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        /// <summary>
        /// Gets when this method returns, contains the 32-bit signed integer value equivalent to the number contained in
        /// SelectedItem.Value, if the conversion succeeded, or Null.NullInteger if the conversion failed.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedItemValueAsInt
        {
            get
            {
                if (this.SelectedItem != null && !string.IsNullOrEmpty(this.SelectedItem.Value))
                {
                    var parsed = int.TryParse(this.SelectedItem.Value, out var valueAsInt);
                    return parsed ? valueAsInt : Null.NullInteger;
                }

                return Null.NullInteger;
            }
        }

        /// <summary>Gets the services options.</summary>
        public ItemListServicesOptions Services => this.Options.Services;

        /// <summary>Gets register a list of JavaScript methods that are executed when the selection from the list control changes on the client.</summary>
        public List<string> OnClientSelectionChanged => this.Options.OnClientSelectionChanged;

        /// <summary>Gets or sets the selected item in the control, or selects the item in the control.</summary>
        public ListItem SelectedItem
        {
            get
            {
                if (this.StateControl.TypedValue != null && this.StateControl.TypedValue.SelectedItem != null)
                {
                    return new ListItem { Text = this.StateControl.TypedValue.SelectedItem.Value, Value = this.StateControl.TypedValue.SelectedItem.Key };
                }

                return null;
            }

            set
            {
                this.StateControl.TypedValueOrDefault.SelectedItem = (value == null) ? null : new SerializableKeyValuePair<string, string>(value.Value, value.Text);
            }
        }

        /// <summary>
        /// Gets or sets selectedItem's value when SelectedItem is not explicitly specified (i.e. equals <see langword="null"/>);
        /// Always displayed as first option in the list.
        /// </summary>
        public ListItem UndefinedItem
        {
            get
            {
                return this.FirstItem;
            }

            set
            {
                this.FirstItem = value;
                this.UseUndefinedItem = true;
            }
        }

        /// <summary>Gets or sets item to be displayed as first item.</summary>
        public ListItem FirstItem
        {
            get
            {
                return (this.Options.ItemList.FirstItem == null) ? null : new ListItem(this.Options.ItemList.FirstItem.Value, this.Options.ItemList.FirstItem.Key);
            }

            set
            {
                this.Options.ItemList.FirstItem = (value == null) ? null : new SerializableKeyValuePair<string, string>(value.Value, value.Text);
                this.UseUndefinedItem = false;
            }
        }

        /// <summary>Sets dropDownList Caption when no Item is selected.</summary>
        public string SelectItemDefaultText
        {
            set => this.Options.SelectItemDefaultText = value;
        }

        /// <summary>Gets or sets the skin.</summary>
        public string Skin { get; set; }

        /// <summary>Gets or sets a value indicating whether a postback to the server automatically occurs when the user changes the list selection.</summary>
        /// <returns>
        /// <see langword="true"/> if a postback to the server automatically occurs whenever the user changes the selection of the list; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </returns>
        public bool AutoPostBack
        {
            get => this.ViewState.GetValue("AutoPostBack", false);
            set => this.ViewState.SetValue("AutoPostBack", value, false);
        }

        /// <summary>Gets or sets a value indicating whether validation is performed when a control is clicked.</summary>
        public virtual bool CausesValidation
        {
            get => this.ViewState.GetValue("CausesValidation", false);
            set => this.ViewState.SetValue("CausesValidation", value, false);
        }

        /// <summary>Gets or sets the group of controls for which the control causes validation when it posts back to the server.</summary>
        public virtual string ValidationGroup
        {
            get => this.ViewState.GetValue("ValidationGroup", string.Empty);
            set => this.ViewState.SetValue("ValidationGroup", value, string.Empty);
        }

        /// <summary>
        /// Gets or sets when the tree view in drop down has multiple level nodes, and the initial selected item is a child node.
        /// we need expand its parent nodes to make it selected.
        /// </summary>
        public string ExpandPath
        {
            get => ClientAPI.GetClientVariable(this.Page, this.ClientID + "_expandPath");
            set => ClientAPI.RegisterClientVariable(this.Page, this.ClientID + "_expandPath", value, true);
        }

        /// <summary>Gets the options.</summary>
        internal DnnDropDownListOptions Options => this.options.Value;

        /// <summary>Gets the state control.</summary>
        protected DnnGenericHiddenField<DnnDropDownListState> StateControl
        {
            get
            {
                this.EnsureChildControls();
                return this.stateControl;
            }
        }

        private HtmlAnchor SelectedValue
        {
            get
            {
                this.EnsureChildControls();
                return this.selectedValue;
            }
        }

        private bool UseUndefinedItem
        {
            get => this.ViewState.GetValue("UseUndefinedItem", false);
            set => this.ViewState.SetValue("UseUndefinedItem", value, false);
        }

        /// <summary>Registers scripts.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="skin">The skin.</param>
        internal static void RegisterClientScript(IClientResourceController clientResourceController, string skin)
        {
            clientResourceController.RegisterStylesheet("~/Resources/Shared/components/DropDownList/dnn.DropDownList.css", FileOrder.Css.ResourceCss);
            if (!string.IsNullOrEmpty(skin))
            {
                clientResourceController.RegisterStylesheet("~/Resources/Shared/components/DropDownList/dnn.DropDownList." + skin + ".css", FileOrder.Css.ResourceCss);
            }

            clientResourceController.RegisterStylesheet("~/Resources/Shared/scripts/jquery/dnn.jScrollBar.css", FileOrder.Css.ResourceCss);

            clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.extensions.js");
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.DataStructures.js");
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/jquery/jquery.mousewheel.js");
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/jquery/dnn.jScrollBar.js");
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/TreeView/dnn.TreeView.js");
            clientResourceController.RegisterScript("~/Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js");
            clientResourceController.RegisterScript("~/Resources/Shared/Components/DropDownList/dnn.DropDownList.js");
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            var selectedItemPanel = new Panel { CssClass = "selected-item", };

            this.selectedValue = new HtmlAnchor { HRef = "javascript:void(0);", Title = LocalizeString("DropDownList.SelectedItemExpandTooltip"), };
            this.selectedValue.Attributes.Add(nameof(HtmlTextWriterAttribute.Class), "selected-value");
            this.selectedValue.ViewStateMode = ViewStateMode.Disabled;
            selectedItemPanel.Controls.Add(this.selectedValue);
            this.Controls.Add(selectedItemPanel);

            this.stateControl = new DnnGenericHiddenField<DnnDropDownListState> { ID = "state", };
            this.stateControl.ValueChanged += (sender, args) => this.OnSelectionChanged(EventArgs.Empty);
            this.Controls.Add(this.stateControl);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.StateControl.Value = string.Empty; // for state persistence (stateControl)
            this.servicesFramework.RequestAjaxAntiForgerySupport();
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            RegisterClientScript(this.clientResourceController, this.Skin);

            this.AddCssClass("dnnDropDownList");

            base.OnPreRender(e);

            this.RegisterStartupScript();
        }

        /// <summary>A method called when the <see cref="SelectionChanged"/> event triggers.</summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var eventHandler = (EventHandler)this.Events[EventSelectionChanged];
            eventHandler?.Invoke(this, e);
        }

        private static string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.SharedResourceFile);
        }

        private string GetPostBackScript()
        {
            var script = string.Empty;
            if (this.HasAttributes)
            {
                script = this.Attributes["onchange"];
                if (script != null)
                {
                    this.Attributes.Remove("onchange");
                }
            }

            var postBackOptions = new PostBackOptions(this, string.Empty);
            if (this.CausesValidation)
            {
                postBackOptions.PerformValidation = true;
                postBackOptions.ValidationGroup = this.ValidationGroup;
            }

            if (this.Page.Form != null)
            {
                postBackOptions.AutoPostBack = true;
                postBackOptions.TrackFocus = true;
            }

            return script.Append(this.Page.ClientScript.GetPostBackEventReference(postBackOptions), "; ");
        }

        private void RegisterStartupScript()
        {
            this.Options.InternalStateFieldId = this.StateControl.ClientID;

            if (this.SelectedItem == null && this.UseUndefinedItem)
            {
                this.SelectedItem = this.UndefinedItem;
            }

            this.Options.InitialState = new DnnDropDownListState
            {
                SelectedItem = this.StateControl.TypedValue?.SelectedItem,
            };

            this.SelectedValue.InnerText = (this.SelectedItem != null) ? this.SelectedItem.Text : this.Options.SelectItemDefaultText;

            this.Options.Disabled = !this.Enabled;

            var optionsAsJsonString = Json.Serialize(this.Options);

            var methods = new JavaScriptObjectDictionary();
            if (this.AutoPostBack)
            {
                methods.AddMethodBody("onSelectionChangedBackScript", this.GetPostBackScript());
            }

            var methodsAsJsonString = methods.ToJsonString();

            var script = $"dnn.createDropDownList('#{this.ClientID}', {optionsAsJsonString}, {methodsAsJsonString});{Environment.NewLine}";

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "DnnDropDownList", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "DnnDropDownList", script, true);
            }
        }
    }
}
