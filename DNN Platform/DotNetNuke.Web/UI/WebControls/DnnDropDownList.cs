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

        private static readonly object EventSelectionChanged = new object();

        private readonly Lazy<DnnDropDownListOptions> _options =
            new Lazy<DnnDropDownListOptions>(() => new DnnDropDownListOptions());

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

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            var eventHandler = (EventHandler)Events[EventSelectionChanged];
            if (eventHandler == null)
            {
                return;
            }
            eventHandler(this, e);
        }

        internal DnnDropDownListOptions Options
        {
            get
            {
                return _options.Value;
            }
        }

        private DnnGenericHiddenField<DnnDropDownListState> _stateControl;
        protected DnnGenericHiddenField<DnnDropDownListState> StateControl
        {
            get
            {
                EnsureChildControls();
                return _stateControl;
            }
        }

        private HtmlAnchor _selectedValue;
        private HtmlAnchor SelectedValue
        {
            get
            {
                EnsureChildControls();
                return _selectedValue;
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                EnsureChildControls();
                return base.Controls;
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            var selectedItemPanel = new Panel { CssClass = "selected-item" };

            _selectedValue = new HtmlAnchor { HRef = "javascript:void(0);", Title = LocalizeString("DropDownList.SelectedItemExpandTooltip") };
            _selectedValue.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "selected-value");
            selectedItemPanel.Controls.Add(_selectedValue);
            Controls.Add(selectedItemPanel);

            _stateControl = new DnnGenericHiddenField<DnnDropDownListState> { ID = "state" };
            _stateControl.ValueChanged += (sender, args) => OnSelectionChanged(EventArgs.Empty);
            Controls.Add(_stateControl);

        }

/*
        private Control CreateItemListLayout()
        {
            var dropDownListPanel = new Panel { CssClass = "dt-container" };
            var header = new Panel { CssClass = "dt-header" };

            var sortButton = new HtmlAnchor
            {
                HRef = "javascript:void(0);",
                Title = LocalizeString("DropDownList.SortAscendingButtonTooltip"),
            };

            var sortTitle = new Literal { Text = string.Format(@"<span>{0}</span>", HttpUtility.HtmlEncode(LocalizeString("DropDownList.SortAscendingButtonTitle"))) };
            sortButton.Controls.Add(sortTitle);

            sortButton.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "sort-button");
            header.Controls.Add(sortButton);

            var searchPanel = new Panel { CssClass = "search-container" };

            var searchInputContainer = new Panel { CssClass = "search-input-container" };
            var searchInput = new HtmlInputText();
            searchInput.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "search-input");
            searchInput.Attributes.Add(HtmlTextWriterAttribute.AutoComplete.ToString(), "off");
            searchInput.Attributes.Add(HtmlTextWriterAttribute.Maxlength.ToString(), "200");
            searchInput.Attributes.Add("placeholder", LocalizeString("DropDownList.SearchInputPlaceHolder"));
            searchInputContainer.Controls.Add(searchInput);

            searchPanel.Controls.Add(searchInputContainer);

            var clearButton = new HtmlAnchor { HRef = "javascript:void(0);" };
            clearButton.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "clear-button");
            clearButton.Attributes.Add(HtmlTextWriterAttribute.Title.ToString(), LocalizeString("DropDownList.ClearButtonTooltip"));
            clearButton.Style.Add(HtmlTextWriterStyle.Display, "none");
            searchPanel.Controls.Add(clearButton);

            var searchButton = new HtmlAnchor { HRef = "javascript:void(0);" };
            searchButton.Attributes.Add(HtmlTextWriterAttribute.Class.ToString(), "search-button");
            searchButton.Attributes.Add(HtmlTextWriterAttribute.Title.ToString(), LocalizeString("DropDownList.SearchButtonTooltip"));
            searchPanel.Controls.Add(searchButton);

            header.Controls.Add(searchPanel);

            dropDownListPanel.Controls.Add(header);

            var contentPanel = new Panel { CssClass = "dt-content" };
            var treePanel = new Panel { CssClass = "dt-tree" };
            contentPanel.Controls.Add(treePanel);

            dropDownListPanel.Controls.Add(contentPanel);

            var footer = new Panel { CssClass = "dt-footer" };
            var resultText = new Literal { Text = string.Format(@"<span class=""{0}""><b></b>{1}</span>", "result", HttpUtility.HtmlEncode(" " + LocalizeString("DropDownList.Results"))) };
            footer.Controls.Add(resultText);

            var resizer = new Panel { CssClass = "resizer" };
            footer.Controls.Add(resizer);

            dropDownListPanel.Controls.Add(footer);

            return dropDownListPanel;
        }
*/

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            StateControl.Value = ""; // for state persistence (stateControl)
        }

        private static string LocalizeString(string key)
        {
            return Localization.GetString(key, Localization.SharedResourceFile);
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

        private void RegisterClientScript(string skin)
        {
            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/components/DropDownList/dnn.DropDownList.css");
            if (!string.IsNullOrEmpty(skin))
            {
                ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/components/DropDownList/dnn.DropDownList." + skin + ".css");
            }
            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Shared/scripts/jquery/dnn.jScrollBar.css");

            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.DataStructures.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/jquery/jquery.mousewheel.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/jquery/dnn.jScrollBar.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/TreeView/dnn.TreeView.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/TreeView/dnn.DynamicTreeView.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/Components/DropDownList/dnn.DropDownList.js");
        }

        private string GetPostBackScript()
        {
            var script = "";
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

        protected override void OnPreRender(EventArgs e)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            RegisterClientScript(Skin);

            this.AddCssClass("dnnDropDownList");

            base.OnPreRender(e);

            RegisterStartupScript();
        }

        private void RegisterStartupScript()
        {
            Options.SelectedItemCss = "selected-item";
            Options.InternalStateFieldId = StateControl.ClientID;

            if (SelectedItem == null && UseUndefinedItem)
            {
                SelectedItem = UndefinedItem;
            }

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

    }

}
