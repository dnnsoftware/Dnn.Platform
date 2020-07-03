// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.WebControls

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PropertyEditorControl control provides a way to display and edit any
    /// properties of any Info class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PropertyEditorControl : WebControl, INamingContainer
    {
        private bool _itemChanged;
        private Hashtable _sections;

        public PropertyEditorControl()
        {
            this.VisibilityStyle = new Style();
            this.ItemStyle = new Style();
            this.LabelStyle = new Style();
            this.HelpStyle = new Style();
            this.GroupHeaderStyle = new Style();
            this.ErrorStyle = new Style();
            this.EditControlStyle = new Style();
            this.Fields = new ArrayList();
            this.ShowRequired = true;
            this.LabelMode = LabelMode.Left;
            this.HelpDisplayMode = HelpDisplayMode.Always;
            this.Groups = Null.NullString;
            this.AutoGenerate = true;
        }

        public event PropertyChangedEventHandler ItemAdded;

        public event EditorCreatedEventHandler ItemCreated;

        public event PropertyChangedEventHandler ItemDeleted;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether any of the properties have been changed.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                return this.Fields.Cast<FieldEditorControl>().Any(editor => editor.Visible && editor.IsDirty);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether all of the properties are Valid.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                return this.Fields.Cast<FieldEditorControl>().All(editor => !editor.Visible || editor.IsValid);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the editor Autogenerates its editors.
        /// </summary>
        /// <value>The DataSource object.</value>
        /// -----------------------------------------------------------------------------
        [Category("Behavior")]
        public bool AutoGenerate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the DataSource that is bound to this control.
        /// </summary>
        /// <value>The DataSource object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        [Category("Data")]
        public object DataSource { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Edit Mode of the Editor.
        /// </summary>
        /// <value>The mode of the editor.</value>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public PropertyEditorMode EditMode { get; set; }

        public EditorDisplayMode DisplayMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets a flag indicating whether the Validators should use client-side
        /// validation.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        [Category("Behavior")]
        public bool EnableClientValidation { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the grouping mode.
        /// </summary>
        /// <value>A GroupByMode enum.</value>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public GroupByMode GroupByMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the grouping order.
        /// </summary>
        /// <value>A comma-delimited list of categories/groups.</value>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public string Groups { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets whether the control displays Help.
        /// </summary>
        /// <value>A HelpDisplayMode enum.</value>
        /// -----------------------------------------------------------------------------
        public HelpDisplayMode HelpDisplayMode { get; set; }

        public LabelMode LabelMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Local Resource File for the Control.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Url of the Required Image.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string RequiredUrl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Required icon is used.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool ShowRequired { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the Visibility control is used.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public bool ShowVisibility { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets whether to sort properties.
        /// </summary>
        /// <value>The Sort Mode of the editor.</value>
        /// <remarks>
        /// By default all properties will be sorted.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public PropertySortType SortMode { get; set; }

        public UserInfo User { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of fields to display if AutoGenerate is false. Or the
        /// collection of fields generated if AutoGenerate is true.
        /// </summary>
        /// <value>A collection of FieldEditorControl objects.</value>
        /// -----------------------------------------------------------------------------
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ArrayList Fields { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Field Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Edit Control.")]
        public Style EditControlStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the width of the Edit Control Column.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set the Width for the Edit Control.")]
        public Unit EditControlWidth { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Error Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Error Text.")]
        public Style ErrorStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Group Header Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Group Header Control.")]
        public Style GroupHeaderStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether to add a &lt;hr&gt; to the Group Header.
        /// </summary>
        /// <value>A boolean.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set whether to include a rule <hr> in the Group Header.")]
        public bool GroupHeaderIncludeRule { get; set; }

        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Help Text.")]
        public Style HelpStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Label Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Label Text")]
        public Style ItemStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Label Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Label Text")]
        public Style LabelStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the width of the Label Column.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set the Width for the Label Control.")]
        public Unit LabelWidth { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Visibility Style.
        /// </summary>
        /// <value>A Style object.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Visibility Control")]
        public Style VisibilityStyle { get; private set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Underlying DataSource.
        /// </summary>
        /// <value>An IEnumerable Boolean.</value>
        /// -----------------------------------------------------------------------------
        protected virtual IEnumerable UnderlyingDataSource
        {
            get { return this.GetProperties(); }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Binds the controls to the DataSource.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            // Invoke OnDataBinding so DataBinding Event is raised
            this.OnDataBinding(EventArgs.Empty);

            // Clear Existing Controls
            this.Controls.Clear();

            // Clear Child View State as controls will be loaded from DataSource
            this.ClearChildViewState();

            // Start Tracking ViewState
            this.TrackViewState();

            // Create the Editor
            this.CreateEditor();

            // Set flag so CreateChildConrols should not be invoked later in control's lifecycle
            this.ChildControlsCreated = true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddEditorRow builds a sigle editor row and adds it to the Table, using the
        /// specified adapter.
        /// </summary>
        /// <param name="table">The Table Control to add the row to.</param>
        /// <param name="name">The name of property being added.</param>
        /// <param name="adapter">An IEditorInfoAdapter.</param>
        /// -----------------------------------------------------------------------------
        protected void AddEditorRow(Table table, string name, IEditorInfoAdapter adapter)
        {
            var row = new TableRow();
            table.Rows.Add(row);

            var cell = new TableCell();
            row.Cells.Add(cell);

            // Create a FieldEditor for this Row
            var editor = new FieldEditorControl
            {
                DataSource = this.DataSource,
                EditorInfoAdapter = adapter,
                DataField = name,
                EditorDisplayMode = this.DisplayMode,
                EnableClientValidation = this.EnableClientValidation,
                EditMode = this.EditMode,
                HelpDisplayMode = this.HelpDisplayMode,
                LabelMode = this.LabelMode,
                LabelWidth = this.LabelWidth,
            };
            this.AddEditorRow(editor, cell);

            this.Fields.Add(editor);
        }

        protected void AddEditorRow(WebControl container, string name, IEditorInfoAdapter adapter)
        {
            var editor = new FieldEditorControl
            {
                DataSource = this.DataSource,
                EditorInfoAdapter = adapter,
                DataField = name,
                EditorDisplayMode = this.DisplayMode,
                EnableClientValidation = this.EnableClientValidation,
                EditMode = this.EditMode,
                HelpDisplayMode = this.HelpDisplayMode,
                LabelMode = this.LabelMode,
                LabelWidth = this.LabelWidth,
            };
            this.AddEditorRow(editor, container);

            this.Fields.Add(editor);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddEditorRow builds a sigle editor row and adds it to the Table.
        /// </summary>
        /// <remarks>This method is protected so that classes that inherit from
        /// PropertyEditor can modify how the Row is displayed.</remarks>
        /// <param name="table">The Table Control to add the row to.</param>
        /// <param name="obj">Row Data Info.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void AddEditorRow(Table table, object obj)
        {
            var objProperty = (PropertyInfo)obj;
            this.AddEditorRow(table, objProperty.Name, new StandardEditorInfoAdapter(this.DataSource, objProperty.Name));
        }

        protected virtual void AddEditorRow(Panel container, object obj)
        {
            var objProperty = (PropertyInfo)obj;
            this.AddEditorRow(container, objProperty.Name, new StandardEditorInfoAdapter(this.DataSource, objProperty.Name));
        }

        protected virtual void AddEditorRow(object obj)
        {
            var objProperty = (PropertyInfo)obj;
            this.AddEditorRow(this, objProperty.Name, new StandardEditorInfoAdapter(this.DataSource, objProperty.Name));
        }

        protected virtual void AddFields()
        {
            foreach (FieldEditorControl editor in this.Fields)
            {
                editor.DataSource = this.DataSource;
                editor.EditorInfoAdapter = new StandardEditorInfoAdapter(this.DataSource, editor.DataField);
                editor.EditorDisplayMode = this.DisplayMode;
                editor.EnableClientValidation = this.EnableClientValidation;
                if (editor.EditMode != PropertyEditorMode.View)
                {
                    editor.EditMode = this.EditMode;
                }

                editor.HelpDisplayMode = this.HelpDisplayMode;
                if (editor.LabelMode == LabelMode.None)
                {
                    editor.LabelMode = this.LabelMode;
                }

                if (editor.LabelWidth == Unit.Empty)
                {
                    editor.LabelWidth = this.LabelWidth;
                }

                this.AddEditorRow(editor, this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddFields adds the fields that have beend defined in design mode (Autogenerate=false).
        /// </summary>
        /// <param name="tbl">The Table Control to add the row to.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void AddFields(Table tbl)
        {
            foreach (FieldEditorControl editor in this.Fields)
            {
                var row = new TableRow();
                tbl.Rows.Add(row);
                var cell = new TableCell();
                row.Cells.Add(cell);

                editor.DataSource = this.DataSource;
                editor.EditorInfoAdapter = new StandardEditorInfoAdapter(this.DataSource, editor.DataField);
                editor.EditorDisplayMode = this.DisplayMode;
                editor.EnableClientValidation = this.EnableClientValidation;
                if (editor.EditMode != PropertyEditorMode.View)
                {
                    editor.EditMode = this.EditMode;
                }

                editor.HelpDisplayMode = this.HelpDisplayMode;
                if (editor.LabelMode == LabelMode.None)
                {
                    editor.LabelMode = this.LabelMode;
                }

                if (editor.LabelWidth == Unit.Empty)
                {
                    editor.LabelWidth = this.LabelWidth;
                }

                this.AddEditorRow(editor, cell);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddHeader builds a group header.
        /// </summary>
        /// <remarks>This method is protected so that classes that inherit from
        /// PropertyEditor can modify how the Header is displayed.</remarks>
        /// <param name="tbl">The Table Control that contains the group.</param>
        /// <param name="header">Table Header.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void AddHeader(Table tbl, string header)
        {
            var panel = new Panel();
            var icon = new Image { ID = "ico" + header, EnableViewState = false };

            var spacer = new Literal { Text = " ", EnableViewState = false };

            var label = new Label { ID = "lbl" + header };
            label.Attributes["resourcekey"] = this.ID + "_" + header + ".Header";
            label.Text = header;
            label.EnableViewState = false;
            label.ControlStyle.CopyFrom(this.GroupHeaderStyle);

            panel.Controls.Add(icon);
            panel.Controls.Add(spacer);
            panel.Controls.Add(label);

            if (this.GroupHeaderIncludeRule)
            {
                panel.Controls.Add(new LiteralControl("<hr noshade=\"noshade\" size=\"1\"/>"));
            }

            this.Controls.Add(panel);

            // Get the Hashtable
            if (this._sections == null)
            {
                this._sections = new Hashtable();
            }

            this._sections[icon] = tbl;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateEditor creates the control collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void CreateEditor()
        {
            Table table;
            string[] arrGroups = null;

            this.Controls.Clear();
            if (!string.IsNullOrEmpty(this.Groups))
            {
                arrGroups = this.Groups.Split(',');
            }
            else if (this.GroupByMode != GroupByMode.None)
            {
                arrGroups = this.GetGroups(this.UnderlyingDataSource);
            }

            if (!this.AutoGenerate)
            {
                // Create a new table
                if (this.DisplayMode == EditorDisplayMode.Div)
                {
                    this.AddFields();
                }
                else
                {
                    // Add the Table to the Controls Collection
                    table = new Table { ID = "tbl" };
                    this.AddFields(table);
                    this.Controls.Add(table);
                }
            }
            else
            {
                this.Fields.Clear();
                if (arrGroups != null && arrGroups.Length > 0)
                {
                    foreach (string strGroup in arrGroups)
                    {
                        if (this.GroupByMode == GroupByMode.Section)
                        {
                            if (this.DisplayMode == EditorDisplayMode.Div)
                            {
                                var groupData = this.UnderlyingDataSource.Cast<object>().Where(obj => this.GetCategory(obj) == strGroup.Trim() && this.GetRowVisibility(obj));
                                if (groupData.Count() > 0)
                                {
                                    // Add header
                                    var header = new HtmlGenericControl("h2");
                                    header.Attributes.Add("class", "dnnFormSectionHead");
                                    header.Attributes.Add("id", strGroup);
                                    this.Controls.Add(header);

                                    var localizedGroupName = Localization.GetString("ProfileProperties_" + strGroup + ".Header", this.LocalResourceFile);
                                    if (string.IsNullOrEmpty(localizedGroupName))
                                    {
                                        localizedGroupName = strGroup;
                                    }

                                    var link = new HyperLink() { Text = localizedGroupName, NavigateUrl = "#" };
                                    header.Controls.Add(link);

                                    // fieldset to hold properties in group
                                    var fieldset = new HtmlGenericControl("fieldset");
                                    var container = new Panel();
                                    fieldset.Controls.Add(container);

                                    foreach (object obj in groupData)
                                    {
                                        this.AddEditorRow(container, obj);
                                    }

                                    this.Controls.Add(fieldset);
                                }
                            }
                            else
                            {
                                // Create a new table
                                table = new Table { ID = "tbl" + strGroup };
                                foreach (object obj in this.UnderlyingDataSource)
                                {
                                    if (this.GetCategory(obj) == strGroup.Trim())
                                    {
                                        // Add the Editor Row to the Table
                                        if (this.GetRowVisibility(obj))
                                        {
                                            if (table.Rows.Count == 0)
                                            {
                                                // Add a Header
                                                this.AddHeader(table, strGroup);
                                            }

                                            this.AddEditorRow(table, obj);
                                        }
                                    }
                                }

                                // Add the Table to the Controls Collection (if it has any rows)
                                if (table.Rows.Count > 0)
                                {
                                    this.Controls.Add(table);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Create a new table
                    if (this.DisplayMode == EditorDisplayMode.Div)
                    {
                        foreach (object obj in this.UnderlyingDataSource)
                        {
                            // Add the Editor Row to the Table
                            if (this.GetRowVisibility(obj))
                            {
                                this.AddEditorRow(obj);
                            }
                        }
                    }
                    else
                    {
                        table = new Table { ID = "tbl" };
                        foreach (object obj in this.UnderlyingDataSource)
                        {
                            if (this.GetRowVisibility(obj))
                            {
                                this.AddEditorRow(table, obj);
                            }
                        }

                        // Add the Table to the Controls Collection
                        this.Controls.Add(table);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCategory gets the Category of an object.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected virtual string GetCategory(object obj)
        {
            var objProperty = (PropertyInfo)obj;
            var categoryString = Null.NullString;

            // Get Category Field
            var categoryAttributes = objProperty.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (categoryAttributes.Length > 0)
            {
                var category = (CategoryAttribute)categoryAttributes[0];
                categoryString = category.Category;
            }

            return categoryString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetGroups gets an array of Groups/Categories from the DataSource.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected virtual string[] GetGroups(IEnumerable arrObjects)
        {
            var arrGroups = new ArrayList();

            foreach (PropertyInfo objProperty in arrObjects)
            {
                var categoryAttributes = objProperty.GetCustomAttributes(typeof(CategoryAttribute), true);
                if (categoryAttributes.Length > 0)
                {
                    var category = (CategoryAttribute)categoryAttributes[0];

                    if (!arrGroups.Contains(category.Category))
                    {
                        arrGroups.Add(category.Category);
                    }
                }
            }

            var strGroups = new string[arrGroups.Count];
            for (int i = 0; i <= arrGroups.Count - 1; i++)
            {
                strGroups[i] = Convert.ToString(arrGroups[i]);
            }

            return strGroups;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetRowVisibility determines the Visibility of a row in the table.
        /// </summary>
        /// <param name="obj">The property.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        protected virtual bool GetRowVisibility(object obj)
        {
            var objProperty = (PropertyInfo)obj;

            bool isVisible = true;
            object[] browsableAttributes = objProperty.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (browsableAttributes.Length > 0)
            {
                var browsable = (BrowsableAttribute)browsableAttributes[0];
                if (!browsable.Browsable)
                {
                    isVisible = false;
                }
            }

            if (!isVisible && this.EditMode == PropertyEditorMode.Edit)
            {
                // Check if property is required - as this will need to override visibility
                object[] requiredAttributes = objProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
                if (requiredAttributes.Length > 0)
                {
                    var required = (RequiredAttribute)requiredAttributes[0];
                    if (required.Required)
                    {
                        isVisible = true;
                    }
                }
            }

            return isVisible;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is added to a collection type property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
            if (this.ItemAdded != null)
            {
                this.ItemAdded(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an Editor is Created.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemCreated(PropertyEditorItemEventArgs e)
        {
            if (this.ItemCreated != null)
            {
                this.ItemCreated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is removed from a collection type property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
            if (this.ItemDeleted != null)
            {
                this.ItemDeleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs just before the control is rendered.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            if (this._itemChanged)
            {
                // Rebind the control to the DataSource to make sure that the dependent
                // editors are updated
                this.DataBind();
            }

            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "dnnForm";
            }

            // Find the Min/Max buttons
            if (this.GroupByMode == GroupByMode.Section && (this._sections != null))
            {
                foreach (DictionaryEntry key in this._sections)
                {
                    var tbl = (Table)key.Value;
                    var icon = (Image)key.Key;
                    DNNClientAPI.EnableMinMax(icon, tbl, false, IconController.IconURL("Minus", "12X15"), IconController.IconURL("Plus", "12X15"), DNNClientAPI.MinMaxPersistanceType.Page);
                }
            }

            base.OnPreRender(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is added to a collection type property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void CollectionItemAdded(object sender, PropertyEditorEventArgs e)
        {
            this.OnItemAdded(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is removed from a collection type property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void CollectionItemDeleted(object sender, PropertyEditorEventArgs e)
        {
            this.OnItemDeleted(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an Editor Is Created.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void EditorItemCreated(object sender, PropertyEditorItemEventArgs e)
        {
            this.OnItemCreated(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an Item in the List Is Changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void ListItemChanged(object sender, PropertyEditorEventArgs e)
        {
            this._itemChanged = true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetProperties returns an array of <see cref="System.Reflection.PropertyInfo">PropertyInfo</see>.
        /// </summary>
        /// <returns>An array of <see cref="System.Reflection.PropertyInfo">PropertyInfo</see> objects
        /// for the current DataSource object.</returns>
        /// <remarks>
        /// GetProperties will return an array of public properties for the current DataSource
        /// object.  The properties will be sorted according to the SortMode property.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private IEnumerable<PropertyInfo> GetProperties()
        {
            if (this.DataSource != null)
            {
                // TODO:  We need to add code to support using the cache in the future
                const BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                var properties = this.DataSource.GetType().GetProperties(bindings);

                // Apply sort method
                switch (this.SortMode)
                {
                    case PropertySortType.Alphabetical:
                        Array.Sort(properties, new PropertyNameComparer());
                        break;
                    case PropertySortType.Category:
                        Array.Sort(properties, new PropertyCategoryComparer());
                        break;
                    case PropertySortType.SortOrderAttribute:
                        Array.Sort(properties, new PropertySortOrderComparer());
                        break;
                }

                return properties;
            }

            return null;
        }

        private void AddEditorRow(FieldEditorControl editor, WebControl container)
        {
            editor.ControlStyle.CopyFrom(this.ItemStyle);
            editor.LabelStyle.CopyFrom(this.LabelStyle);
            editor.HelpStyle.CopyFrom(this.HelpStyle);
            editor.ErrorStyle.CopyFrom(this.ErrorStyle);
            editor.VisibilityStyle.CopyFrom(this.VisibilityStyle);
            editor.EditControlStyle.CopyFrom(this.EditControlStyle);
            if (editor.EditControlWidth == Unit.Empty)
            {
                editor.EditControlWidth = this.EditControlWidth;
            }

            editor.LocalResourceFile = this.LocalResourceFile;
            editor.RequiredUrl = this.RequiredUrl;
            editor.ShowRequired = this.ShowRequired;
            editor.ShowVisibility = this.ShowVisibility;
            editor.User = this.User;
            editor.Width = this.Width;
            editor.ItemAdded += this.CollectionItemAdded;
            editor.ItemChanged += this.ListItemChanged;
            editor.ItemCreated += this.EditorItemCreated;
            editor.ItemDeleted += this.CollectionItemDeleted;

            editor.DataBind();
            container.Controls.Add(editor);
        }
    }
}
