using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
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

namespace DotNetNuke.Web.UI.WebControls
{

    [DataContract]
    public class DnnDropDownListState
    {
        [DataMember(Name = "selectedItem")]
        public SerializableKeyValuePair<string, string> SelectedItem;
    }

    [ToolboxData("<{0}:DnnDropDownList runat='server'></{0}:DnnDropDownList>")]
    public class DnnDropDownList : Panel, INamingContainer
    {
        #region Private Fields

        private static readonly object EventSelectionChanged = new object();

        private readonly Lazy<DnnDropDownListOptions> _options =
            new Lazy<DnnDropDownListOptions>(() => new DnnDropDownListOptions());

        private DnnGenericHiddenField<DnnDropDownListState> _stateControl;
        private HtmlAnchor _selectedValue;

        #endregion

        #region Protected Properties

        internal DnnDropDownListOptions Options
        {
            get
            {
                return _options.Value;
            }
        }

        protected DnnGenericHiddenField<DnnDropDownListState> StateControl
        {
            get
            {
                EnsureChildControls();
                return _stateControl;
            }
        }

        private HtmlAnchor SelectedValue
        {
            get
            {
                EnsureChildControls();
                return _selectedValue;
            }
        }

        private bool UseUndefinedItem
        {
            get
            {
                return ViewState.GetValue("UseUndefinedItem", false);
            }
            set
            {
                ViewState.SetValue("UseUndefinedItem", value, false);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the selection from the list control changes between posts to the server.
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add
            {
                Events.AddHandler(EventSelectionChanged, value);
            }
            remove
            {
                Events.RemoveHandler(EventSelectionChanged, value);
            }
        }

        #endregion

        #region Public Properties

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return base.Controls;
            }
        }

        /// <summary>
        /// Gets the selected item in the control, or selects the item in the control.
        /// </summary>
        public ListItem SelectedItem
        {
            get
            {
                if (StateControl.TypedValue != null && StateControl.TypedValue.SelectedItem != null)
                {
                    return new ListItem { Text = StateControl.TypedValue.SelectedItem.Value, Value = StateControl.TypedValue.SelectedItem.Key };
                }
                return null;
            }
            set
            {
                StateControl.TypedValueOrDefault.SelectedItem = (value == null) ? null : new SerializableKeyValuePair<string, string>(value.Value, value.Text);
            }
        }

        /// <summary>
        /// When this method returns, contains the 32-bit signed integer value equivalent to the number contained in
        /// SelectedItem.Value, if the conversion succeeded, or Null.NullInteger if the conversion failed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedItemValueAsInt
        {
            get
            {
                if (SelectedItem != null && !string.IsNullOrEmpty(SelectedItem.Value))
                {
                    int valueAsInt;
                    var parsed = Int32.TryParse(SelectedItem.Value, out valueAsInt);
                    return parsed ? valueAsInt : Null.NullInteger;
                }
                return Null.NullInteger;
            }
        }

        /// <summary>
        /// SelectedItem's value when SelectedItem is not explicitly specified (i.e. equals null);
        /// Always displayed as first option in the list
        /// </summary>
        public ListItem UndefinedItem
        {
            get
            {
                return FirstItem;
            }
            set
            {
                FirstItem = value;
                UseUndefinedItem = true;
            }
        }

        /// <summary>
        /// Item to be displayed as first item
        /// </summary>
        public ListItem FirstItem
        {
            get
            {
                return (Options.ItemList.FirstItem == null) ? null : new ListItem(Options.ItemList.FirstItem.Value, Options.ItemList.FirstItem.Key);
            }
            set
            {
                Options.ItemList.FirstItem = (value == null) ? null : new SerializableKeyValuePair<string, string>(value.Value, value.Text);
                UseUndefinedItem = false;
            }
        }

        public ItemListServicesOptions Services
        {
            get
            {
                return Options.Services;
            }
        }

        /// <summary>
        /// DropDownList Caption when no Item is selected.
        /// </summary>
        public string SelectItemDefaultText
        {
            set
            {
                Options.SelectItemDefaultText = value;
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
                return ViewState.GetValue("AutoPostBack", false);
            }
            set
            {
                ViewState.SetValue("AutoPostBack", value, false);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether validation is performed when a control is clicked.
        /// </summary>
        public virtual bool CausesValidation
        {
            get
            {
                return ViewState.GetValue("CausesValidation", false);
            }
            set
            {
                ViewState.SetValue("CausesValidation", value, false);
            }
        }

        /// <summary>
        /// Gets or sets the group of controls for which the control causes validation when it posts back to the server.
        /// </summary>
        public virtual string ValidationGroup
        {
            get
            {
                return ViewState.GetValue("ValidationGroup", string.Empty);
            }
            set
            {
                ViewState.SetValue("ValidationGroup", value, string.Empty);
            }
        }

        /// <summary>
        /// Register a list of JavaScript methods that are executed when the selection from the list control changes on the client.
        /// </summary>
        public List<string> OnClientSelectionChanged
        {
            get
            {
                return Options.OnClientSelectionChanged;
            }
        }

        /// <summary>
        /// When the tree view in drop down has multiple level nodes, and the initial selected item is a child node.
        /// we need expand its parent nodes to make it selected.
        /// </summary>
        public string ExpandPath
        {
            get
            {
                return ClientAPI.GetClientVariable(Page, ClientID + "_expandPath");
            }
            set
            {
                ClientAPI.RegisterClientVariable(Page, ClientID + "_expandPath", value, true);
            }
        }

        #endregion

        #region Event Handlers

        protected override void CreateChildControls()
        {
            Controls.Clear();

            var selectedItemPanel = new Panel { CssClass = "selected-item" };

            _selectedValue = new HtmlAnchor { HRef = "javascript:void(0);", Title = LocalizeString("DropDownList.SelectedItemExpandTooltip") };
            _selectedValue.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "selected-value");
            _selectedValue.ViewStateMode = ViewStateMode.Disabled;
            selectedItemPanel.Controls.Add(_selectedValue);
            Controls.Add(selectedItemPanel);

            _stateControl = new DnnGenericHiddenField<DnnDropDownListState> { ID = "state" };
            _stateControl.ValueChanged += (sender, args) => OnSelectionChanged(EventArgs.Empty);
            Controls.Add(_stateControl);

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            StateControl.Value = ""; // for state persistence (stateControl)
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }

        protected override void OnPreRender(EventArgs e)
        {
            RegisterClientScript(Page, Skin);

            this.AddCssClass("dnnDropDownList");

            base.OnPreRender(e);

            RegisterStartupScript();
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var eventHandler = (EventHandler)Events[EventSelectionChanged];
            if (eventHandler == null)
            {
                return;
            }
            eventHandler(this, e);
        }

        #endregion

        #region Private Methods

        private static string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.SharedResourceFile);
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

        private string GetPostBackScript()
        {
            var script = string.Empty;
            if (HasAttributes)
            {
                script = Attributes["onchange"];
                if (script != null)
                {
                    Attributes.Remove("onchange");
                }
            }
            var options = new PostBackOptions(this, string.Empty);
            if (CausesValidation)
            {
                options.PerformValidation = true;
                options.ValidationGroup = ValidationGroup;
            }
            if (Page.Form != null)
            {
                options.AutoPostBack = true;
                options.TrackFocus = true;
            }
            return script.Append(Page.ClientScript.GetPostBackEventReference(options), "; ");
        }

        private void RegisterStartupScript()
        {
            Options.InternalStateFieldId = StateControl.ClientID;

            if (SelectedItem == null && UseUndefinedItem)
            {
                SelectedItem = UndefinedItem;
            }

            Options.InitialState = new DnnDropDownListState
            {
                SelectedItem = StateControl.TypedValue != null ? StateControl.TypedValue.SelectedItem : null
            };

            SelectedValue.InnerText = (SelectedItem != null) ? SelectedItem.Text : Options.SelectItemDefaultText;

            Options.Disabled = !Enabled;

            var optionsAsJsonString = Json.Serialize(Options);

            var methods = new JavaScriptObjectDictionary();
            if (AutoPostBack)
            {
                methods.AddMethodBody("onSelectionChangedBackScript", GetPostBackScript());
            }

            var methodsAsJsonString = methods.ToJsonString();

            var script = string.Format("dnn.createDropDownList('#{0}', {1}, {2});{3}", ClientID, optionsAsJsonString, methodsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), ClientID + "DnnDropDownList", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "DnnDropDownList", script, true);
            }
        }

        #endregion

    }

}
