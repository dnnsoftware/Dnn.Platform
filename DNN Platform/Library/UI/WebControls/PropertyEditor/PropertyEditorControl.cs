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

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.UI.WebControls
// ReSharper restore CheckNamespace
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PropertyEditorControl control provides a way to display and edit any 
    /// properties of any Info class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class PropertyEditorControl : WebControl, INamingContainer
    {
		#region Private Members

        private bool _itemChanged;
        private Hashtable _sections;
        #endregion
		
		#region Constructors

        public PropertyEditorControl()
        {
            VisibilityStyle = new Style();
            ItemStyle = new Style();
            LabelStyle = new Style();
            HelpStyle = new Style();
            GroupHeaderStyle = new Style();
            ErrorStyle = new Style();
            EditControlStyle = new Style();
            Fields = new ArrayList();
            ShowRequired = true;
            LabelMode = LabelMode.Left;
            HelpDisplayMode = HelpDisplayMode.Always;
            Groups = Null.NullString;
            AutoGenerate = true;
        }
		
		#endregion

		#region Protected Members

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Underlying DataSource
        /// </summary>
        /// <value>An IEnumerable Boolean</value>
        /// -----------------------------------------------------------------------------
        protected virtual IEnumerable UnderlyingDataSource
        {
            get { return GetProperties(); }
        }
		
		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the editor Autogenerates its editors
        /// </summary>
        /// <value>The DataSource object</value>
        /// -----------------------------------------------------------------------------
        [Category("Behavior")]
        public bool AutoGenerate { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the DataSource that is bound to this control
        /// </summary>
        /// <value>The DataSource object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false), Category("Data")]
        public object DataSource { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Edit Mode of the Editor
        /// </summary>
        /// <value>The mode of the editor</value>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public PropertyEditorMode EditMode { get; set; }

        public EditorDisplayMode DisplayMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets a flag indicating whether the Validators should use client-side
        /// validation
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        [Category("Behavior")]
        public bool EnableClientValidation { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the grouping mode
        /// </summary>
        /// <value>A GroupByMode enum</value>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public GroupByMode GroupByMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the grouping order
        /// </summary>
        /// <value>A comma-delimited list of categories/groups</value>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public string Groups { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the control displays Help
        /// </summary>
        /// <value>A HelpDisplayMode enum</value>
        /// -----------------------------------------------------------------------------
        public HelpDisplayMode HelpDisplayMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether any of the properties have been changed
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                return Fields.Cast<FieldEditorControl>().Any(editor => editor.Visible && editor.IsDirty);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether all of the properties are Valid
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                return Fields.Cast<FieldEditorControl>().All(editor => !editor.Visible || editor.IsValid);
            }
        }

        public LabelMode LabelMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Local Resource File for the Control
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string LocalResourceFile { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Url of the Required Image
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string RequiredUrl { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// gets and sets whether the Required icon is used
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool ShowRequired { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// gets and sets whether the Visibility control is used
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Category("Appearance")]
        public bool ShowVisibility { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether to sort properties. 
        /// </summary>
        /// <value>The Sort Mode of the editor</value>
        /// <remarks>
        /// By default all properties will be sorted 
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
        /// <value>A collection of FieldEditorControl objects</value>
        /// -----------------------------------------------------------------------------
        [Category("Behavior"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ArrayList Fields { get; private set; }

		#region "Style Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Field Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Edit Control.")]
        public Style EditControlStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the width of the Edit Control Column
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Appearance"), Description("Set the Width for the Edit Control.")]
        public Unit EditControlWidth { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Error Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Error Text.")]
        public Style ErrorStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Group Header Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Group Header Control.")]
        public Style GroupHeaderStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether to add a &lt;hr&gt; to the Group Header
        /// </summary>
        /// <value>A boolean</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Appearance"), Description("Set whether to include a rule <hr> in the Group Header.")]
        public bool GroupHeaderIncludeRule { get; set; }

        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Help Text.")]
        public Style HelpStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Label Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof(ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Label Text")]
        public Style ItemStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Label Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Label Text")]
        public Style LabelStyle { get; private set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the width of the Label Column
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Appearance"), Description("Set the Width for the Label Control.")]
        public Unit LabelWidth { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the value of the Visibility Style
        /// </summary>
        /// <value>A Style object</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)), PersistenceMode(PersistenceMode.InnerProperty), Description("Set the Style for the Visibility Control")]
        public Style VisibilityStyle { get; private set; }
		
		#endregion

		#endregion

		#region Events

        public event PropertyChangedEventHandler ItemAdded;
        public event EditorCreatedEventHandler ItemCreated;
        public event PropertyChangedEventHandler ItemDeleted;

		#endregion

		#region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetProperties returns an array of <see cref="System.Reflection.PropertyInfo">PropertyInfo</see>
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
            if (DataSource != null)
            {
				//TODO:  We need to add code to support using the cache in the future
                const BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                var properties = DataSource.GetType().GetProperties(bindings);

                //Apply sort method
                switch (SortMode)
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
            editor.ControlStyle.CopyFrom(ItemStyle);
            editor.LabelStyle.CopyFrom(LabelStyle);
            editor.HelpStyle.CopyFrom(HelpStyle);
            editor.ErrorStyle.CopyFrom(ErrorStyle);
            editor.VisibilityStyle.CopyFrom(VisibilityStyle);
            editor.EditControlStyle.CopyFrom(EditControlStyle);
            if (editor.EditControlWidth == Unit.Empty)
            {
                editor.EditControlWidth = EditControlWidth;
            }
            editor.LocalResourceFile = LocalResourceFile;
            editor.RequiredUrl = RequiredUrl;
            editor.ShowRequired = ShowRequired;
            editor.ShowVisibility = ShowVisibility;
            editor.User = User;
            editor.Width = Width;
            editor.ItemAdded += CollectionItemAdded;
            editor.ItemChanged += ListItemChanged;
            editor.ItemCreated += EditorItemCreated;
            editor.ItemDeleted += CollectionItemDeleted;

            editor.DataBind();
            container.Controls.Add(editor);
        }

		#endregion

		#region Protected Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddEditorRow builds a sigle editor row and adds it to the Table, using the
        /// specified adapter
        /// </summary>
		/// <param name="table">The Table Control to add the row to</param>
        /// <param name="name">The name of property being added</param>
        /// <param name="adapter">An IEditorInfoAdapter</param>
        /// -----------------------------------------------------------------------------
        protected void AddEditorRow(Table table, string name, IEditorInfoAdapter adapter)
        {
            var row = new TableRow();
            table.Rows.Add(row);

            var cell = new TableCell();
            row.Cells.Add(cell);

            //Create a FieldEditor for this Row
            var editor = new FieldEditorControl
                             {
                                 DataSource = DataSource,
                                 EditorInfoAdapter = adapter,
                                 DataField = name,
                                 EditorDisplayMode = DisplayMode,
                                 EnableClientValidation = EnableClientValidation,
                                 EditMode = EditMode,
                                 HelpDisplayMode = HelpDisplayMode,
                                 LabelMode = LabelMode,
                                 LabelWidth = LabelWidth
                             };
            AddEditorRow(editor, cell);

            Fields.Add(editor);
        }

        protected void AddEditorRow(WebControl container, string name, IEditorInfoAdapter adapter)
        {
            var editor = new FieldEditorControl
            {
                DataSource = DataSource,
                EditorInfoAdapter = adapter,
                DataField = name,
                EditorDisplayMode = DisplayMode,
                EnableClientValidation = EnableClientValidation,
                EditMode = EditMode,
                HelpDisplayMode = HelpDisplayMode,
                LabelMode = LabelMode,
                LabelWidth = LabelWidth
            };
            AddEditorRow(editor, container);

            Fields.Add(editor);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddEditorRow builds a sigle editor row and adds it to the Table
        /// </summary>
        /// <remarks>This method is protected so that classes that inherit from
        /// PropertyEditor can modify how the Row is displayed</remarks>
		/// <param name="table">The Table Control to add the row to</param>
		/// <param name="obj">Row Data Info.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void AddEditorRow(Table table, object obj)
        {
            var objProperty = (PropertyInfo) obj;
            AddEditorRow(table, objProperty.Name, new StandardEditorInfoAdapter(DataSource, objProperty.Name));
        }

        protected virtual void AddEditorRow(Panel container, object obj)
        {
            var objProperty = (PropertyInfo)obj;
            AddEditorRow(container, objProperty.Name, new StandardEditorInfoAdapter(DataSource, objProperty.Name));
        }

        protected virtual void AddEditorRow(object obj)
        {
            var objProperty = (PropertyInfo)obj;
            AddEditorRow(this, objProperty.Name, new StandardEditorInfoAdapter(DataSource, objProperty.Name));
        }

        protected virtual void AddFields()
        {
            foreach (FieldEditorControl editor in Fields)
            {
                editor.DataSource = DataSource;
                editor.EditorInfoAdapter = new StandardEditorInfoAdapter(DataSource, editor.DataField);
                editor.EditorDisplayMode = DisplayMode;
                editor.EnableClientValidation = EnableClientValidation;
                if (editor.EditMode != PropertyEditorMode.View)
                {
                    editor.EditMode = EditMode;
                }
                editor.HelpDisplayMode = HelpDisplayMode;
                if (editor.LabelMode == LabelMode.None)
                {
                    editor.LabelMode = LabelMode;
                }
                if (editor.LabelWidth == Unit.Empty)
                {
                    editor.LabelWidth = LabelWidth;
                }

                AddEditorRow(editor, this);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddFields adds the fields that have beend defined in design mode (Autogenerate=false)
        /// </summary>
        /// <param name="tbl">The Table Control to add the row to</param>
        /// -----------------------------------------------------------------------------
        protected virtual void AddFields(Table tbl)
        {
            foreach (FieldEditorControl editor in Fields)
            {
                var row = new TableRow();
                tbl.Rows.Add(row);
                var cell = new TableCell();
                row.Cells.Add(cell);

                editor.DataSource = DataSource;
                editor.EditorInfoAdapter = new StandardEditorInfoAdapter(DataSource, editor.DataField);
                editor.EditorDisplayMode = DisplayMode;
                editor.EnableClientValidation = EnableClientValidation;
                if (editor.EditMode != PropertyEditorMode.View)
                {
                    editor.EditMode = EditMode;
                }
                editor.HelpDisplayMode = HelpDisplayMode;
                if (editor.LabelMode == LabelMode.None)
                {
                    editor.LabelMode = LabelMode;
                }
                if (editor.LabelWidth == Unit.Empty)
                {
                    editor.LabelWidth = LabelWidth;
                }

                AddEditorRow(editor, cell);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddHeader builds a group header
        /// </summary>
        /// <remarks>This method is protected so that classes that inherit from
        /// PropertyEditor can modify how the Header is displayed</remarks>
        /// <param name="tbl">The Table Control that contains the group</param>
        /// <param name="header">Table Header.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void AddHeader(Table tbl, string header)
        {
            var panel = new Panel();
            var icon = new Image {ID = "ico" + header, EnableViewState = false};

            var spacer = new Literal {Text = " ", EnableViewState = false};

            var label = new Label {ID = "lbl" + header};
            label.Attributes["resourcekey"] = ID + "_" + header + ".Header";
            label.Text = header;
            label.EnableViewState = false;
            label.ControlStyle.CopyFrom(GroupHeaderStyle);

            panel.Controls.Add(icon);
            panel.Controls.Add(spacer);
            panel.Controls.Add(label);

            if (GroupHeaderIncludeRule)
            {
                panel.Controls.Add(new LiteralControl("<hr noshade=\"noshade\" size=\"1\"/>"));
            }
            Controls.Add(panel);
			
			//Get the Hashtable
            if (_sections == null)
            {
                _sections = new Hashtable();
            }
            _sections[icon] = tbl;
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

            Controls.Clear();
            if (!String.IsNullOrEmpty(Groups))
            {
                arrGroups = Groups.Split(',');
            }
            else if (GroupByMode != GroupByMode.None)
            {
                arrGroups = GetGroups(UnderlyingDataSource);
            }

            if (!AutoGenerate)
            {
				//Create a new table
                if (DisplayMode == EditorDisplayMode.Div)
                {
                    AddFields();
                }
                else
                {
					//Add the Table to the Controls Collection
                    table = new Table { ID = "tbl" };
                    AddFields(table);
                    Controls.Add(table);
                }
            }
            else
            {
                Fields.Clear();
                if (arrGroups != null && arrGroups.Length > 0)
                {
                    foreach (string strGroup in arrGroups)
                    {
                        if (GroupByMode == GroupByMode.Section)
                        {
                            if (DisplayMode == EditorDisplayMode.Div)
                            {
								var groupData = UnderlyingDataSource.Cast<object>().Where(obj => GetCategory(obj) == strGroup.Trim() && GetRowVisibility(obj));
	                            if (groupData.Count() > 0)
	                            {
		                            //Add header
		                            var header = new HtmlGenericControl("h2");
		                            header.Attributes.Add("class", "dnnFormSectionHead");
		                            header.Attributes.Add("id", strGroup);
		                            Controls.Add(header);

                                    var localizedGroupName = Localization.GetString("ProfileProperties_" + strGroup + ".Header", LocalResourceFile);
		                            if (string.IsNullOrEmpty(localizedGroupName))
		                            {
			                            localizedGroupName = strGroup;
		                            }
		                            var link = new HyperLink() { Text = localizedGroupName, NavigateUrl = "#" };
		                            header.Controls.Add(link);

		                            //fieldset to hold properties in group
		                            var fieldset = new HtmlGenericControl("fieldset");
		                            var container = new Panel();
		                            fieldset.Controls.Add(container);

		                            foreach (object obj in groupData)
		                            {
										AddEditorRow(container, obj);
		                            }
		                            Controls.Add(fieldset);
	                            }
                            }
                            else
                            {
                                //Create a new table
                                table = new Table { ID = "tbl" + strGroup };
                                foreach (object obj in UnderlyingDataSource)
                                {
                                    if (GetCategory(obj) == strGroup.Trim())
                                    {
                                        //Add the Editor Row to the Table
                                        if (GetRowVisibility(obj))
                                        {
                                            if (table.Rows.Count == 0)
                                            {
                                                //Add a Header
                                                AddHeader(table, strGroup);
                                            }

                                            AddEditorRow(table, obj);
                                        }
                                    }
                                }

                                //Add the Table to the Controls Collection (if it has any rows)
                                if (table.Rows.Count > 0)
                                {
                                    Controls.Add(table);
                                }
                            }
                        }
                    }
                }
                else
                {
					//Create a new table
                    if (DisplayMode == EditorDisplayMode.Div)
                    {
                        foreach (object obj in UnderlyingDataSource)
                        {
							//Add the Editor Row to the Table
                            if (GetRowVisibility(obj))
                            {
                                AddEditorRow(obj);
                            }
                        }
                    }
                    else
                    {
                        table = new Table { ID = "tbl" };
                        foreach (object obj in UnderlyingDataSource)
                        {
                            if (GetRowVisibility(obj))
                            {
                                AddEditorRow(table, obj);
                            }
                        }
						
						//Add the Table to the Controls Collection
                        Controls.Add(table);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCategory gets the Category of an object
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual string GetCategory(object obj)
        {
            var objProperty = (PropertyInfo) obj;
            var categoryString = Null.NullString;

            //Get Category Field
            var categoryAttributes = objProperty.GetCustomAttributes(typeof (CategoryAttribute), true);
            if (categoryAttributes.Length > 0)
            {
                var category = (CategoryAttribute) categoryAttributes[0];
                categoryString = category.Category;
            }
            return categoryString;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetGroups gets an array of Groups/Categories from the DataSource
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual string[] GetGroups(IEnumerable arrObjects)
        {
            var arrGroups = new ArrayList();

            foreach (PropertyInfo objProperty in arrObjects)
            {
                var categoryAttributes = objProperty.GetCustomAttributes(typeof (CategoryAttribute), true);
                if (categoryAttributes.Length > 0)
                {
                    var category = (CategoryAttribute) categoryAttributes[0];

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
        /// GetRowVisibility determines the Visibility of a row in the table
        /// </summary>
        /// <param name="obj">The property</param>
        /// -----------------------------------------------------------------------------
        protected virtual bool GetRowVisibility(object obj)
        {
            var objProperty = (PropertyInfo) obj;

            bool isVisible = true;
            object[] browsableAttributes = objProperty.GetCustomAttributes(typeof (BrowsableAttribute), true);
            if (browsableAttributes.Length > 0)
            {
                var browsable = (BrowsableAttribute) browsableAttributes[0];
                if (!browsable.Browsable)
                {
                    isVisible = false;
                }
            }
            if (!isVisible && EditMode == PropertyEditorMode.Edit)
            {
				//Check if property is required - as this will need to override visibility
                object[] requiredAttributes = objProperty.GetCustomAttributes(typeof (RequiredAttribute), true);
                if (requiredAttributes.Length > 0)
                {
                    var required = (RequiredAttribute) requiredAttributes[0];
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
        /// Runs when an item is added to a collection type property
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
            if (ItemAdded != null)
            {
                ItemAdded(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an Editor is Created
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemCreated(PropertyEditorItemEventArgs e)
        {
            if (ItemCreated != null)
            {
                ItemCreated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is removed from a collection type property
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
            if (ItemDeleted != null)
            {
                ItemDeleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs just before the control is rendered
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            if (_itemChanged)
            {
				//Rebind the control to the DataSource to make sure that the dependent
                //editors are updated
                DataBind();
            }
            if (String.IsNullOrEmpty(CssClass))
            {
                CssClass = "dnnForm";
            }
			
			//Find the Min/Max buttons
            if (GroupByMode == GroupByMode.Section && (_sections != null))
            {
                foreach (DictionaryEntry key in _sections)
                {
                    var tbl = (Table) key.Value;
                    var icon = (Image) key.Key;
                    DNNClientAPI.EnableMinMax(icon, tbl, false, IconController.IconURL("Minus", "12X15"), IconController.IconURL("Plus","12X15"), DNNClientAPI.MinMaxPersistanceType.Page);
                }
            }
            base.OnPreRender(e);
        }
		
		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Binds the controls to the DataSource
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            //Invoke OnDataBinding so DataBinding Event is raised
            base.OnDataBinding(EventArgs.Empty);

            //Clear Existing Controls
            Controls.Clear();

            //Clear Child View State as controls will be loaded from DataSource
            ClearChildViewState();

            //Start Tracking ViewState
            TrackViewState();

            //Create the Editor
            CreateEditor();

            //Set flag so CreateChildConrols should not be invoked later in control's lifecycle
            ChildControlsCreated = true;
        }

		#endregion

		#region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is added to a collection type property
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void CollectionItemAdded(object sender, PropertyEditorEventArgs e)
        {
            OnItemAdded(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an item is removed from a collection type property
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void CollectionItemDeleted(object sender, PropertyEditorEventArgs e)
        {
            OnItemDeleted(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an Editor Is Created
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void EditorItemCreated(object sender, PropertyEditorItemEventArgs e)
        {
            OnItemCreated(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Runs when an Item in the List Is Changed
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void ListItemChanged(object sender, PropertyEditorEventArgs e)
        {
            _itemChanged = true;
        }
		
		#endregion
    }
}
