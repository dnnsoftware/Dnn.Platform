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

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    [ToolboxData("<{0}:DnnDropDownList runat='server'></{0}:DnnDropDownList>")]
    public class DnnDropDownList : Panel, INamingContainer
    {
        private static readonly object EventSelectionChanged = new object();

        private readonly Lazy<DnnDropDownListOptions> _options =
            new Lazy<DnnDropDownListOptions>(() => new DnnDropDownListOptions());

        private DnnGenericHiddenField<DnnDropDownListState> _stateControl;
        private HtmlAnchor _selectedValue;

        /// <summary>
        /// Occurs when the selection from the list control changes between posts to the server.
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add
            {
                this.Events.AddHandler(EventSelectionChanged, value);
            }

            remove
            {
                this.Events.RemoveHandler(EventSelectionChanged, value);
            }
        }

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
                    int valueAsInt;
                    var parsed = int.TryParse(this.SelectedItem.Value, out valueAsInt);
                    return parsed ? valueAsInt : Null.NullInteger;
                }

                return Null.NullInteger;
            }
        }

        public ItemListServicesOptions Services
        {
            get
            {
                return this.Options.Services;
            }
        }

        /// <summary>
        /// Gets register a list of JavaScript methods that are executed when the selection from the list control changes on the client.
        /// </summary>
        public List<string> OnClientSelectionChanged
        {
            get
            {
                return this.Options.OnClientSelectionChanged;
            }
        }

        /// <summary>
        /// Gets or sets the selected item in the control, or selects the item in the control.
        /// </summary>
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
        /// Gets or sets selectedItem's value when SelectedItem is not explicitly specified (i.e. equals null);
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

        /// <summary>
        /// Gets or sets item to be displayed as first item.
        /// </summary>
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

        /// <summary>
        /// Sets dropDownList Caption when no Item is selected.
        /// </summary>
        public string SelectItemDefaultText
        {
            set
            {
                this.Options.SelectItemDefaultText = value;
            }
        }

        public string Skin
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a postback to the server automatically occurs when the user changes the list selection.
        /// </summary>
        /// <returns>
        /// true if a postback to the server automatically occurs whenever the user changes the selection of the list; otherwise, false. The default is false.
        /// </returns>
        public bool AutoPostBack
        {
            get
            {
                return this.ViewState.GetValue("AutoPostBack", false);
            }

            set
            {
                this.ViewState.SetValue("AutoPostBack", value, false);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether validation is performed when a control is clicked.
        /// </summary>
        public virtual bool CausesValidation
        {
            get
            {
                return this.ViewState.GetValue("CausesValidation", false);
            }

            set
            {
                this.ViewState.SetValue("CausesValidation", value, false);
            }
        }

        /// <summary>
        /// Gets or sets the group of controls for which the control causes validation when it posts back to the server.
        /// </summary>
        public virtual string ValidationGroup
        {
            get
            {
                return this.ViewState.GetValue("ValidationGroup", string.Empty);
            }

            set
            {
                this.ViewState.SetValue("ValidationGroup", value, string.Empty);
            }
        }

        /// <summary>
        /// Gets or sets when the tree view in drop down has multiple level nodes, and the initial selected item is a child node.
        /// we need expand its parent nodes to make it selected.
        /// </summary>
        public string ExpandPath
        {
            get
            {
                return ClientAPI.GetClientVariable(this.Page, this.ClientID + "_expandPath");
            }

            set
            {
                ClientAPI.RegisterClientVariable(this.Page, this.ClientID + "_expandPath", value, true);
            }
        }

        internal DnnDropDownListOptions Options
        {
            get
            {
                return this._options.Value;
            }
        }

        protected DnnGenericHiddenField<DnnDropDownListState> StateControl
        {
            get
            {
                this.EnsureChildControls();
                return this._stateControl;
            }
        }

        private HtmlAnchor SelectedValue
        {
            get
            {
                this.EnsureChildControls();
                return this._selectedValue;
            }
        }

        private bool UseUndefinedItem
        {
            get
            {
                return this.ViewState.GetValue("UseUndefinedItem", false);
            }

            set
            {
                this.ViewState.SetValue("UseUndefinedItem", value, false);
            }
        }

        internal static void RegisterClientScript(Page page, string skin)
        {
            ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/components/DropDownList/dnn.DropDownList.css", FileOrder.Css.ResourceCss);
            if (!string.IsNullOrEmpty(skin))
            {
                ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/components/DropDownList/dnn.DropDownList." + skin + ".css", FileOrder.Css.ResourceCss);
            }

            ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/scripts/jquery/dnn.jScrollBar.css", FileOrder.Css.ResourceCss);

            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.extensions.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.DataStructures.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/jquery/jquery.mousewheel.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/jquery/dnn.jScrollBar.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/TreeView/dnn.TreeView.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js");
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Components/DropDownList/dnn.DropDownList.js");
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            var selectedItemPanel = new Panel { CssClass = "selected-item" };

            this._selectedValue = new HtmlAnchor { HRef = "javascript:void(0);", Title = LocalizeString("DropDownList.SelectedItemExpandTooltip") };
            this._selectedValue.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "selected-value");
            this._selectedValue.ViewStateMode = ViewStateMode.Disabled;
            selectedItemPanel.Controls.Add(this._selectedValue);
            this.Controls.Add(selectedItemPanel);

            this._stateControl = new DnnGenericHiddenField<DnnDropDownListState> { ID = "state" };
            this._stateControl.ValueChanged += (sender, args) => this.OnSelectionChanged(EventArgs.Empty);
            this.Controls.Add(this._stateControl);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.StateControl.Value = string.Empty; // for state persistence (stateControl)
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }

        protected override void OnPreRender(EventArgs e)
        {
            RegisterClientScript(this.Page, this.Skin);

            this.AddCssClass("dnnDropDownList");

            base.OnPreRender(e);

            this.RegisterStartupScript();
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var eventHandler = (EventHandler)this.Events[EventSelectionChanged];
            if (eventHandler == null)
            {
                return;
            }

            eventHandler(this, e);
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

            var options = new PostBackOptions(this, string.Empty);
            if (this.CausesValidation)
            {
                options.PerformValidation = true;
                options.ValidationGroup = this.ValidationGroup;
            }

            if (this.Page.Form != null)
            {
                options.AutoPostBack = true;
                options.TrackFocus = true;
            }

            return script.Append(this.Page.ClientScript.GetPostBackEventReference(options), "; ");
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
                SelectedItem = this.StateControl.TypedValue != null ? this.StateControl.TypedValue.SelectedItem : null,
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

            var script = string.Format("dnn.createDropDownList('#{0}', {1}, {2});{3}", this.ClientID, optionsAsJsonString, methodsAsJsonString, Environment.NewLine);

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
