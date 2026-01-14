// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// The possible editor display modes.
    /// </summary>
    public enum EditorDisplayMode
    {
        /// <summary>
        /// Displays the control in a div.
        /// </summary>
        Div = 0,

        /// <summary>
        /// Displays the control in a table.
        /// </summary>
        Table = 1,
    }

    /// <summary>
    /// Defines how help is displayed.
    /// </summary>
    public enum HelpDisplayMode
    {
        /// <summary>
        /// Never display the help.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Display help only in edit mode.
        /// </summary>
        EditOnly = 1,

        /// <summary>
        /// Always display help.
        /// </summary>
        Always = 2,
    }

    /// <summary>
    /// The FieldEditorControl control provides a Control to display Profile Properties.
    /// </summary>
    [ToolboxData("<{0}:FieldEditorControl runat=server></{0}:FieldEditorControl>")]
    public class FieldEditorControl : WebControl, INamingContainer
    {
        private readonly IServiceProvider serviceProvider;
        private readonly List<IValidator> validators = new List<IValidator>();
        private IEditorInfoAdapter editorInfoAdapter;
        private bool isValid = true;
        private StandardEditorInfoAdapter stdAdapter;
        private bool validated;

        /// <summary>Initializes a new instance of the <see cref="FieldEditorControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public FieldEditorControl()
            : this(Globals.GetCurrentServiceProvider())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FieldEditorControl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public FieldEditorControl(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.ValidationExpression = Null.NullString;
            this.ShowRequired = true;
            this.LabelMode = LabelMode.None;
            this.EditorTypeName = Null.NullString;
            this.EditorDisplayMode = EditorDisplayMode.Div;
            this.HelpDisplayMode = HelpDisplayMode.Always;
            this.VisibilityStyle = new Style();
            this.LabelStyle = new Style();
            this.HelpStyle = new Style();
            this.EditControlStyle = new Style();
            this.ErrorStyle = new Style();
            this.ViewStateMode = ViewStateMode.Disabled;
        }

        /// <summary>
        /// Occurs when an item is added.
        /// </summary>
        public event PropertyChangedEventHandler ItemAdded;

        /// <summary>
        /// Occurs when an item was changed.
        /// </summary>
        public event PropertyChangedEventHandler ItemChanged;

        /// <summary>
        /// Occurs when an item was created.
        /// </summary>
        public event EditorCreatedEventHandler ItemCreated;

        /// <summary>
        /// Occurs when an item was deleted.
        /// </summary>
        public event PropertyChangedEventHandler ItemDeleted;

        /// <summary>Gets a value indicating whether all of the properties are Valid.</summary>
        public bool IsValid
        {
            get
            {
                if (!this.validated)
                {
                    this.Validate();
                }

                return this.isValid;
            }
        }

        /// <summary>Gets or sets the DataSource that is bound to this control.</summary>
        [Browsable(false)]
        public object DataSource { get; set; }

        /// <summary>Gets or sets the value of the Field/property that this control displays.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Control.")]
        public string DataField { get; set; }

        /// <summary>Gets or sets whether the control uses Divs or Tables.</summary>
        public EditorDisplayMode EditorDisplayMode { get; set; }

        /// <summary>Gets or sets the Edit Mode of the Editor.</summary>
        /// <value>The mode of the editor.</value>
        public PropertyEditorMode EditMode { get; set; }

        /// <summary>Gets the Edit Control associated with the Editor.</summary>
        public EditControl Editor { get; private set; }

        /// <summary>Gets or sets the Factory used to create the Control.</summary>
        /// <value>The mode of the editor.</value>
        public IEditorInfoAdapter EditorInfoAdapter
        {
            get
            {
                if (this.editorInfoAdapter == null)
                {
                    if (this.stdAdapter == null)
                    {
                        this.stdAdapter = new StandardEditorInfoAdapter(this.DataSource, this.DataField);
                    }

                    return this.stdAdapter;
                }
                else
                {
                    return this.editorInfoAdapter;
                }
            }

            set
            {
                this.editorInfoAdapter = value;
            }
        }

        /// <summary>Gets or sets the Editor Type to use.</summary>
        /// <value>The typename of the editor.</value>
        public string EditorTypeName { get; set; }

        /// <summary>Gets or sets a value indicating whether the Validators should use client-side validation.</summary>
        public bool EnableClientValidation { get; set; }

        /// <summary>Gets or sets whether the control displays Help.</summary>
        public HelpDisplayMode HelpDisplayMode { get; set; }

        /// <summary>Gets a value indicating whether any of the properties have been changed.</summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets or sets the label mode.
        /// </summary>
        public LabelMode LabelMode { get; set; }

        /// <summary>Gets or sets the Local Resource File for the Control.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>Gets or sets the Url of the Required Image.</summary>
        public string RequiredUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether the Required icon is used.</summary>
        public bool ShowRequired { get; set; }

        /// <summary>Gets or sets a value indicating whether the Visibility control is used.</summary>
        public bool ShowVisibility { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public UserInfo User { get; set; }

        /// <summary>
        /// Gets or sets the validation expression.
        /// </summary>
        public string ValidationExpression { get; set; }

        /// <summary>Gets the value of the Field Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Edit Control.")]
        public Style EditControlStyle { get; private set; }

        /// <summary>Gets or sets the width of the Edit Control Column.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set the Width for the Edit Control.")]
        public Unit EditControlWidth { get; set; }

        /// <summary>Gets the value of the Error Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Error Text.")]
        public Style ErrorStyle { get; private set; }

        /// <summary>Gets the value of the Label Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Help Text.")]
        public Style HelpStyle { get; private set; }

        /// <summary>Gets the value of the Label Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Label Text")]
        public Style LabelStyle { get; private set; }

        /// <summary>Gets or sets the width of the Label Column.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set the Width for the Label Control.")]
        public Unit LabelWidth { get; set; }

        /// <summary>Gets the value of the Visibility Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Visibility Control")]
        public Style VisibilityStyle { get; private set; }

        /// <inheritdoc/>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>Binds the controls to the DataSource.</summary>
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

            // Create the editor
            this.CreateEditor();

            // Set flag so CreateChildConrols should not be invoked later in control's lifecycle
            this.ChildControlsCreated = true;
        }

        /// <summary>Validates the data, and sets the IsValid Property.</summary>
        public virtual void Validate()
        {
            this.isValid = this.Editor.IsValid;

            if (this.isValid)
            {
                IEnumerator valEnumerator = this.validators.GetEnumerator();
                while (valEnumerator.MoveNext())
                {
                    var validator = (IValidator)valEnumerator.Current;
                    validator.Validate();
                    if (!validator.IsValid)
                    {
                        this.isValid = false;
                        break;
                    }
                }

                this.validated = true;
            }
        }

        /// <summary>CreateEditor creates the control collection for this control.</summary>
        protected virtual void CreateEditor()
        {
            EditorInfo editInfo = this.EditorInfoAdapter.CreateEditControl();

            this.ID = editInfo.Name;

            if (editInfo != null)
            {
                editInfo.User = this.User;

                if (editInfo.EditMode == PropertyEditorMode.Edit)
                {
                    editInfo.EditMode = this.EditMode;
                }

                // Get the Editor Type to use (if specified)
                if (!string.IsNullOrEmpty(this.EditorTypeName))
                {
                    editInfo.Editor = this.EditorTypeName;
                }

                // Get the Label Mode to use (if specified)
                if (this.LabelMode != LabelMode.Left)
                {
                    editInfo.LabelMode = this.LabelMode;
                }

                // if Required is specified set editors property
                if (this.Required)
                {
                    editInfo.Required = this.Required;
                }

                // Get the ValidationExpression to use (if specified)
                if (!string.IsNullOrEmpty(this.ValidationExpression))
                {
                    editInfo.ValidationExpression = this.ValidationExpression;
                }

                // Raise the ItemCreated Event
                this.OnItemCreated(new PropertyEditorItemEventArgs(editInfo));

                this.Visible = editInfo.Visible;

                if (this.EditorDisplayMode == EditorDisplayMode.Div)
                {
                    this.BuildDiv(editInfo);
                }
                else
                {
                    this.BuildTable(editInfo);
                }
            }
        }

        /// <summary>
        /// Runs when an item is added to a collection type property.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        protected virtual void CollectionItemAdded(object sender, PropertyEditorEventArgs e)
        {
            this.OnItemAdded(e);
        }

        /// <summary>
        /// Runs when an item is removed from a collection type property.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        protected virtual void CollectionItemDeleted(object sender, PropertyEditorEventArgs e)
        {
            this.OnItemDeleted(e);
        }

        /// <summary>
        /// Runs when an item is added to a collection type property.
        /// </summary>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
            if (this.ItemAdded != null)
            {
                this.ItemAdded(this, e);
            }
        }

        /// <summary>
        /// Runs when the Editor is Created.
        /// </summary>
        /// <param name="e">The event arguments <see cref="PropertyEditorItemEventArgs"/>.</param>
        protected virtual void OnItemCreated(PropertyEditorItemEventArgs e)
        {
            if (this.ItemCreated != null)
            {
                this.ItemCreated(this, e);
            }
        }

        /// <summary>
        /// Runs when an item is removed from a collection type property.
        /// </summary>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
            if (this.ItemDeleted != null)
            {
                this.ItemDeleted(this, e);
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "dnnFormItem";
            }
        }

        /// <summary>
        /// Runs when the Value of a Property changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        protected virtual void ValueChanged(object sender, PropertyEditorEventArgs e)
        {
            this.IsDirty = this.EditorInfoAdapter.UpdateValue(e);
        }

        /// <summary>
        /// Runs when the Visibility of a Property changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        protected virtual void VisibilityChanged(object sender, PropertyEditorEventArgs e)
        {
            this.IsDirty = this.EditorInfoAdapter.UpdateVisibility(e);
        }

        /// <summary>
        /// Runs when an Item in the List Is Changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments <see cref="PropertyEditorEventArgs"/>.</param>
        /// <remarks>Raises an ItemChanged event.</remarks>
        protected virtual void ListItemChanged(object sender, PropertyEditorEventArgs e)
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, e);
            }
        }

        /// <summary>BuildDiv creates the Control as a Div.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        private void BuildDiv(EditorInfo editInfo)
        {
            var propLabel = new PropertyLabelControl();
            propLabel.ViewStateMode = ViewStateMode.Disabled;

            var propEditor = this.BuildEditor(editInfo);
            var visibility = this.BuildVisibility(editInfo);

            if (editInfo.LabelMode != LabelMode.None)
            {
                propLabel = this.BuildLabel(editInfo);
                propLabel.EditControl = propEditor;
            }

            var strValue = editInfo.Value as string;
            if (this.ShowRequired && editInfo.Required && (editInfo.EditMode == PropertyEditorMode.Edit || (editInfo.Required && string.IsNullOrEmpty(strValue))))
            {
                propLabel.Required = true;
            }

            if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Top)
            {
                this.Controls.Add(propLabel);
                this.Controls.Add(propEditor);
                if (visibility != null)
                {
                    this.Controls.Add(visibility);
                }
            }
            else
            {
                this.Controls.Add(propEditor);
                if (visibility != null)
                {
                    this.Controls.Add(visibility);
                }

                if (propLabel != null)
                {
                    this.Controls.Add(propLabel);
                }
            }

            // Build the Validators
            this.BuildValidators(editInfo, propEditor.ID);
            if (this.validators.Count > 0)
            {
                // Add the Validators to the editor cell
                foreach (BaseValidator validator in this.validators)
                {
                    validator.Width = this.Width;
                    this.Controls.Add(validator);
                }
            }
        }

        /// <summary>BuildEditor creates the editor part of the Control.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        private EditControl BuildEditor(EditorInfo editInfo)
        {
            var propEditor = EditControlFactory.CreateEditControl(this.serviceProvider, editInfo);
            propEditor.ViewStateMode = ViewStateMode.Enabled;
            propEditor.ControlStyle.CopyFrom(this.EditControlStyle);
            propEditor.LocalResourceFile = this.LocalResourceFile;
            propEditor.User = this.User;
            if (editInfo.ControlStyle != null)
            {
                propEditor.ControlStyle.CopyFrom(editInfo.ControlStyle);
            }

            propEditor.ItemAdded += this.CollectionItemAdded;
            propEditor.ItemDeleted += this.CollectionItemDeleted;
            propEditor.ValueChanged += this.ValueChanged;
            if (propEditor is DNNListEditControl listEditor)
            {
                listEditor.ItemChanged += this.ListItemChanged;
            }

            this.Editor = propEditor;

            return propEditor;
        }

        /// <summary>BuildLabel creates the label part of the Control.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        private PropertyLabelControl BuildLabel(EditorInfo editInfo)
        {
            var propLabel = new PropertyLabelControl { ID = editInfo.Name + "_Label" };
            propLabel.HelpStyle.CopyFrom(this.HelpStyle);
            propLabel.LabelStyle.CopyFrom(this.LabelStyle);
            var strValue = editInfo.Value as string;
            switch (this.HelpDisplayMode)
            {
                case HelpDisplayMode.Always:
                    propLabel.ShowHelp = true;
                    break;
                case HelpDisplayMode.EditOnly:
                    if (editInfo.EditMode == PropertyEditorMode.Edit || (editInfo.Required && string.IsNullOrEmpty(strValue)))
                    {
                        propLabel.ShowHelp = true;
                    }
                    else
                    {
                        propLabel.ShowHelp = false;
                    }

                    break;
                case HelpDisplayMode.Never:
                    propLabel.ShowHelp = false;
                    break;
            }

            propLabel.Caption = editInfo.Name;
            propLabel.HelpText = editInfo.Name;
            propLabel.ResourceKey = editInfo.ResourceKey;
            if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Right)
            {
                propLabel.Width = this.LabelWidth;
            }

            return propLabel;
        }

        /// <summary>BuildValidators creates the validators part of the Control.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        private Image BuildRequiredIcon(EditorInfo editInfo)
        {
            Image img = null;
            var strValue = editInfo.Value as string;
            if (this.ShowRequired && editInfo.Required && (editInfo.EditMode == PropertyEditorMode.Edit || (editInfo.Required && string.IsNullOrEmpty(strValue))))
            {
                img = new Image();
                if (string.IsNullOrEmpty(this.RequiredUrl) || this.RequiredUrl == Null.NullString)
                {
                    img.ImageUrl = "~/images/required.gif";
                }
                else
                {
                    img.ImageUrl = this.RequiredUrl;
                }

                img.Attributes.Add("resourcekey", editInfo.ResourceKey + ".Required");
            }

            return img;
        }

        /// <summary>BuildTable creates the Control as a Table.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        private void BuildTable(EditorInfo editInfo)
        {
            var tbl = new Table();
            var labelCell = new TableCell();
            var editorCell = new TableCell();

            // Build Label Cell
            labelCell.VerticalAlign = VerticalAlign.Top;
            labelCell.Controls.Add(this.BuildLabel(editInfo));
            if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Right)
            {
                labelCell.Width = this.LabelWidth;
            }

            // Build Editor Cell
            editorCell.VerticalAlign = VerticalAlign.Top;
            EditControl propEditor = this.BuildEditor(editInfo);
            Image requiredIcon = this.BuildRequiredIcon(editInfo);
            editorCell.Controls.Add(propEditor);
            if (requiredIcon != null)
            {
                editorCell.Controls.Add(requiredIcon);
            }

            if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Right)
            {
                editorCell.Width = this.EditControlWidth;
            }

            VisibilityControl visibility = this.BuildVisibility(editInfo);
            if (visibility != null)
            {
                editorCell.Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
                editorCell.Controls.Add(visibility);
            }

            // Add cells to table
            var editorRow = new TableRow();
            var labelRow = new TableRow();
            if (editInfo.LabelMode == LabelMode.Bottom || editInfo.LabelMode == LabelMode.Top || editInfo.LabelMode == LabelMode.None)
            {
                editorCell.ColumnSpan = 2;
                editorRow.Cells.Add(editorCell);
                if (editInfo.LabelMode == LabelMode.Bottom || editInfo.LabelMode == LabelMode.Top)
                {
                    labelCell.ColumnSpan = 2;
                    labelRow.Cells.Add(labelCell);
                }

                if (editInfo.LabelMode == LabelMode.Top)
                {
                    tbl.Rows.Add(labelRow);
                }

                tbl.Rows.Add(editorRow);
                if (editInfo.LabelMode == LabelMode.Bottom)
                {
                    tbl.Rows.Add(labelRow);
                }
            }
            else if (editInfo.LabelMode == LabelMode.Left)
            {
                editorRow.Cells.Add(labelCell);
                editorRow.Cells.Add(editorCell);
                tbl.Rows.Add(editorRow);
            }
            else if (editInfo.LabelMode == LabelMode.Right)
            {
                editorRow.Cells.Add(editorCell);
                editorRow.Cells.Add(labelCell);
                tbl.Rows.Add(editorRow);
            }

            // Build the Validators
            this.BuildValidators(editInfo, propEditor.ID);

            var validatorsRow = new TableRow();
            var validatorsCell = new TableCell();
            validatorsCell.ColumnSpan = 2;

            // Add the Validators to the editor cell
            foreach (BaseValidator validator in this.validators)
            {
                validatorsCell.Controls.Add(validator);
            }

            validatorsRow.Cells.Add(validatorsCell);
            tbl.Rows.Add(validatorsRow);

            // Add the Table to the Controls Collection
            this.Controls.Add(tbl);
        }

        /// <summary>BuildValidators creates the validators part of the Control.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        /// <param name="targetId">Target Control Id.</param>
        private void BuildValidators(EditorInfo editInfo, string targetId)
        {
            this.validators.Clear();

            // Add Required Validators
            if (editInfo.Required)
            {
                var reqValidator = new RequiredFieldValidator();
                reqValidator.ID = editInfo.Name + "_Req";
                reqValidator.ControlToValidate = targetId;
                reqValidator.Display = ValidatorDisplay.Dynamic;
                reqValidator.ControlStyle.CopyFrom(this.ErrorStyle);
                if (string.IsNullOrEmpty(reqValidator.CssClass))
                {
                    reqValidator.CssClass = "dnnFormMessage dnnFormError";
                }

                reqValidator.EnableClientScript = this.EnableClientValidation;
                reqValidator.Attributes.Add("resourcekey", editInfo.ResourceKey + ".Required");
                reqValidator.ErrorMessage = editInfo.Name + " is Required";
                this.validators.Add(reqValidator);
            }

            // Add Regular Expression Validators
            if (!string.IsNullOrEmpty(editInfo.ValidationExpression))
            {
                var regExValidator = new RegularExpressionValidator();
                regExValidator.ID = editInfo.Name + "_RegEx";
                regExValidator.ControlToValidate = targetId;
                regExValidator.ValidationExpression = editInfo.ValidationExpression;
                regExValidator.Display = ValidatorDisplay.Dynamic;
                regExValidator.ControlStyle.CopyFrom(this.ErrorStyle);
                if (string.IsNullOrEmpty(regExValidator.CssClass))
                {
                    regExValidator.CssClass = "dnnFormMessage dnnFormError";
                }

                regExValidator.EnableClientScript = this.EnableClientValidation;
                regExValidator.Attributes.Add("resourcekey", editInfo.ResourceKey + ".Validation");
                regExValidator.ErrorMessage = editInfo.Name + " is Invalid";
                this.validators.Add(regExValidator);
            }
        }

        /// <summary>BuildVisibility creates the visibility part of the Control.</summary>
        /// <param name="editInfo">The EditorInfo object for this control.</param>
        private VisibilityControl BuildVisibility(EditorInfo editInfo)
        {
            VisibilityControl visControl = null;

            if (this.ShowVisibility)
            {
                visControl = new VisibilityControl
                {
                    ID = "_visibility",
                    Name = editInfo.Name,
                    User = this.User,
                    Value = editInfo.ProfileVisibility,
                };
                visControl.ControlStyle.CopyFrom(this.VisibilityStyle);
                visControl.VisibilityChanged += this.VisibilityChanged;
            }

            return visControl;
        }
    }
}
