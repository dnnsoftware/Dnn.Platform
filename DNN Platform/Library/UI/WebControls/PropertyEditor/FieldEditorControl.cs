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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.WebControls
{
	public enum EditorDisplayMode
	{
		Div,
		Table
	}

	public enum HelpDisplayMode
	{
		Never,
		EditOnly,
		Always
	}

	/// -----------------------------------------------------------------------------
	/// Project:    DotNetNuke
	/// Namespace:  DotNetNuke.UI.WebControls
	/// Class:      FieldEditorControl
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The FieldEditorControl control provides a Control to display Profile
	/// Properties.
	/// </summary>
	/// <remarks>
	/// </remarks>
	/// -----------------------------------------------------------------------------
	[ToolboxData("<{0}:FieldEditorControl runat=server></{0}:FieldEditorControl>")]
	public class FieldEditorControl : WebControl, INamingContainer
	{
		#region Private Members

		private readonly List<IValidator> Validators = new List<IValidator>();
		private IEditorInfoAdapter _EditorInfoAdapter;
		private bool _IsValid = true;
		private StandardEditorInfoAdapter _StdAdapter;
		private bool _Validated;
		
		#endregion
		
		#region Constructors

		public FieldEditorControl()
		{
			ValidationExpression = Null.NullString;
			ShowRequired = true;
			LabelMode = LabelMode.None;
			EditorTypeName = Null.NullString;
			EditorDisplayMode = EditorDisplayMode.Div;
			HelpDisplayMode = HelpDisplayMode.Always;
			VisibilityStyle = new Style();
			LabelStyle = new Style();
			HelpStyle = new Style();
			EditControlStyle = new Style();
			ErrorStyle = new Style();
            ViewStateMode = ViewStateMode.Disabled;
		}

		#endregion

		#region Protected Properties

		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

		#endregion

		#region Public Properties

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the DataSource that is bound to this control
		/// </summary>
		/// <value>The DataSource object</value>
		/// -----------------------------------------------------------------------------
		[Browsable(false)]
		public object DataSource { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the value of the Field/property that this control displays
		/// </summary>
		/// <value>A string representing the Name of the Field</value>
		/// -----------------------------------------------------------------------------
		[Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the Control.")]
		public string DataField { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets whether the control uses Divs or Tables
		/// </summary>
		/// <value>An EditorDisplayMode enum</value>
		/// -----------------------------------------------------------------------------
		public EditorDisplayMode EditorDisplayMode { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the Edit Mode of the Editor
		/// </summary>
		/// <value>The mode of the editor</value>
		/// -----------------------------------------------------------------------------
		public PropertyEditorMode EditMode { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the Edit Control associated with the Editor
		/// </summary>
		/// -----------------------------------------------------------------------------
		public EditControl Editor { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the Factory used to create the Control
		/// </summary>
		/// <value>The mode of the editor</value>
		/// -----------------------------------------------------------------------------
		public IEditorInfoAdapter EditorInfoAdapter
		{
			get
			{
				if (_EditorInfoAdapter == null)
				{
					if (_StdAdapter == null)
					{
						_StdAdapter = new StandardEditorInfoAdapter(DataSource, DataField);
					}
					return _StdAdapter;
				}
				else
				{
					return _EditorInfoAdapter;
				}
			}
			set
			{
				_EditorInfoAdapter = value;
			}
		}
		
		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the Editor Type to use
		/// </summary>
		/// <value>The typename of the editor</value>
		/// -----------------------------------------------------------------------------

		public string EditorTypeName { get; set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets a flag indicating whether the Validators should use client-side
		/// validation
		/// </summary>
		/// <value>A Boolean</value>
		/// -----------------------------------------------------------------------------
		public bool EnableClientValidation { get; set; }

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
		public bool IsDirty { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets whether all of the properties are Valid
		/// </summary>
		/// <value>A Boolean</value>
		/// -----------------------------------------------------------------------------
		public bool IsValid
		{
			get
			{
				if (!_Validated)
				{
					Validate();
				}
				return _IsValid;
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

		public bool Required { get; set; }

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
		public bool ShowVisibility { get; set; }

        public UserInfo User { get; set; }

		public string ValidationExpression { get; set; }

		#region Style Properties

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the value of the Field Style
		/// </summary>
		/// <value>A Style object</value>
		/// -----------------------------------------------------------------------------
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Edit Control.")]
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
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Error Text.")]
		public Style ErrorStyle { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the value of the Label Style
		/// </summary>
		/// <value>A Style object</value>
		/// -----------------------------------------------------------------------------
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Help Text.")]
		public Style HelpStyle { get; private set; }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets the value of the Label Style
		/// </summary>
		/// <value>A Style object</value>
		/// -----------------------------------------------------------------------------
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Label Text")]
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
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter(typeof (ExpandableObjectConverter)), Description("Set the Style for the Visibility Control")]
		public Style VisibilityStyle { get; private set; }
		
		#endregion

		#endregion

		#region Events

		public event PropertyChangedEventHandler ItemAdded;
		public event PropertyChangedEventHandler ItemChanged;
		public event EditorCreatedEventHandler ItemCreated;
		public event PropertyChangedEventHandler ItemDeleted;
	
		#endregion

		#region Private Methods

		/// <summary>
		/// BuildDiv creates the Control as a Div
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		private void BuildDiv(EditorInfo editInfo)
		{
			var propLabel = new PropertyLabelControl();
            propLabel.ViewStateMode = ViewStateMode.Disabled;

			var propEditor = BuildEditor(editInfo);
			var visibility = BuildVisibility(editInfo);

			if (editInfo.LabelMode != LabelMode.None)
			{
				propLabel = BuildLabel(editInfo);
				propLabel.EditControl = propEditor;
			}

			var strValue = editInfo.Value as string; 
			if (ShowRequired && editInfo.Required && (editInfo.EditMode == PropertyEditorMode.Edit || (editInfo.Required && string.IsNullOrEmpty(strValue))))
			{
                propLabel.Required = true;
			}

			if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Top)
			{
				Controls.Add(propLabel);
				Controls.Add(propEditor);
				if (visibility != null)
				{
					Controls.Add(visibility);
				}
			}
			else
			{
				Controls.Add(propEditor);
				if (visibility != null)
				{
					Controls.Add(visibility);
				}
				if ((propLabel != null))
				{
					Controls.Add(propLabel);
				}
			}
			
			//Build the Validators
			BuildValidators(editInfo, propEditor.ID);
			if (Validators.Count > 0)
			{
				//Add the Validators to the editor cell
				foreach (BaseValidator validator in Validators)
				{
					validator.Width = Width;
					Controls.Add(validator);
				}
			}
		}

		/// <summary>
		/// BuildEditor creates the editor part of the Control
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		private EditControl BuildEditor(EditorInfo editInfo)
		{
			EditControl propEditor = EditControlFactory.CreateEditControl(editInfo);
            propEditor.ViewStateMode = ViewStateMode.Enabled;
			propEditor.ControlStyle.CopyFrom(EditControlStyle);
			propEditor.LocalResourceFile = LocalResourceFile;
		    propEditor.User = User;
			if (editInfo.ControlStyle != null)
			{
				propEditor.ControlStyle.CopyFrom(editInfo.ControlStyle);
			}
			propEditor.ItemAdded += CollectionItemAdded;
			propEditor.ItemDeleted += CollectionItemDeleted;
			propEditor.ValueChanged += ValueChanged;
			if (propEditor is DNNListEditControl)
			{
				var listEditor = (DNNListEditControl) propEditor;
				listEditor.ItemChanged += ListItemChanged;
			}
			Editor = propEditor;

			return propEditor;
		}

		/// <summary>
		/// BuildLabel creates the label part of the Control
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		private PropertyLabelControl BuildLabel(EditorInfo editInfo)
		{
			var propLabel = new PropertyLabelControl {ID = editInfo.Name + "_Label"};
		    propLabel.HelpStyle.CopyFrom(HelpStyle);
			propLabel.LabelStyle.CopyFrom(LabelStyle);
			var strValue = editInfo.Value as string;
			switch (HelpDisplayMode)
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
				propLabel.Width = LabelWidth;
			}
			return propLabel;
		}

		/// <summary>
		/// BuildValidators creates the validators part of the Control
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		private Image BuildRequiredIcon(EditorInfo editInfo)
		{
			Image img = null;
			var strValue = editInfo.Value as string;
			if (ShowRequired && editInfo.Required && (editInfo.EditMode == PropertyEditorMode.Edit || (editInfo.Required && string.IsNullOrEmpty(strValue))))
			{
				img = new Image();
				if (String.IsNullOrEmpty(RequiredUrl) || RequiredUrl == Null.NullString)
				{
					img.ImageUrl = "~/images/required.gif";
				}
				else
				{
					img.ImageUrl = RequiredUrl;
				}
				img.Attributes.Add("resourcekey", editInfo.ResourceKey + ".Required");
			}
			return img;
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// BuildTable creates the Control as a Table
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		/// -----------------------------------------------------------------------------
		private void BuildTable(EditorInfo editInfo)
		{
			var tbl = new Table();
			var labelCell = new TableCell();
			var editorCell = new TableCell();

			//Build Label Cell
			labelCell.VerticalAlign = VerticalAlign.Top;
			labelCell.Controls.Add(BuildLabel(editInfo));
			if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Right)
			{
				labelCell.Width = LabelWidth;
			}
			//Build Editor Cell
			editorCell.VerticalAlign = VerticalAlign.Top;
			EditControl propEditor = BuildEditor(editInfo);
			Image requiredIcon = BuildRequiredIcon(editInfo);
			editorCell.Controls.Add(propEditor);
			if (requiredIcon != null)
			{
				editorCell.Controls.Add(requiredIcon);
			}
			if (editInfo.LabelMode == LabelMode.Left || editInfo.LabelMode == LabelMode.Right)
			{
				editorCell.Width = EditControlWidth;
			}
			VisibilityControl visibility = BuildVisibility(editInfo);
			if (visibility != null)
			{
				editorCell.Controls.Add(new LiteralControl("&nbsp;&nbsp;"));
				editorCell.Controls.Add(visibility);
			}
			
			//Add cells to table
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
			
			//Build the Validators
			BuildValidators(editInfo, propEditor.ID);

			var validatorsRow = new TableRow();
			var validatorsCell = new TableCell();
			validatorsCell.ColumnSpan = 2;
			//Add the Validators to the editor cell
			foreach (BaseValidator validator in Validators)
			{
				validatorsCell.Controls.Add(validator);
			}
			validatorsRow.Cells.Add(validatorsCell);
			tbl.Rows.Add(validatorsRow);

			//Add the Table to the Controls Collection
			Controls.Add(tbl);
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// BuildValidators creates the validators part of the Control
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		/// <param name="targetId">Target Control Id.</param>
		/// -----------------------------------------------------------------------------
		private void BuildValidators(EditorInfo editInfo, string targetId)
		{
			Validators.Clear();

			//Add Required Validators
			if (editInfo.Required)
			{
				var reqValidator = new RequiredFieldValidator();
				reqValidator.ID = editInfo.Name + "_Req";
				reqValidator.ControlToValidate = targetId;
				reqValidator.Display = ValidatorDisplay.Dynamic;
				reqValidator.ControlStyle.CopyFrom(ErrorStyle);
			    if(String.IsNullOrEmpty(reqValidator.CssClass))
			    {
			        reqValidator.CssClass = "dnnFormMessage dnnFormError";
			    }
				reqValidator.EnableClientScript = EnableClientValidation;
				reqValidator.Attributes.Add("resourcekey", editInfo.ResourceKey + ".Required");
				reqValidator.ErrorMessage = editInfo.Name + " is Required";
				Validators.Add(reqValidator);
			}
			
			//Add Regular Expression Validators
			if (!String.IsNullOrEmpty(editInfo.ValidationExpression))
			{
				var regExValidator = new RegularExpressionValidator();
				regExValidator.ID = editInfo.Name + "_RegEx";
				regExValidator.ControlToValidate = targetId;
				regExValidator.ValidationExpression = editInfo.ValidationExpression;
				regExValidator.Display = ValidatorDisplay.Dynamic;
				regExValidator.ControlStyle.CopyFrom(ErrorStyle);
			    if(String.IsNullOrEmpty(regExValidator.CssClass))
			    {
                    regExValidator.CssClass = "dnnFormMessage dnnFormError";
			    }
				regExValidator.EnableClientScript = EnableClientValidation;
				regExValidator.Attributes.Add("resourcekey", editInfo.ResourceKey + ".Validation");
				regExValidator.ErrorMessage = editInfo.Name + " is Invalid";
				Validators.Add(regExValidator);
			}
		}

		/// <summary>
		/// BuildVisibility creates the visibility part of the Control
		/// </summary>
		/// <param name="editInfo">The EditorInfo object for this control</param>
		private VisibilityControl BuildVisibility(EditorInfo editInfo)
		{
			VisibilityControl visControl = null;

			if (ShowVisibility)
			{
			    visControl = new VisibilityControl
			                     {
			                         ID = "_visibility", 
                                     Name = editInfo.Name, 
                                     User = User, 
                                     Value = editInfo.ProfileVisibility
			                     };
			    visControl.ControlStyle.CopyFrom(VisibilityStyle);
				visControl.VisibilityChanged += VisibilityChanged;
			}
			return visControl;
		}

	    #endregion

		#region Protected Methods

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// CreateEditor creates the control collection for this control
		/// </summary>
		/// -----------------------------------------------------------------------------
		protected virtual void CreateEditor()
		{
			EditorInfo editInfo = EditorInfoAdapter.CreateEditControl();

		    ID = editInfo.Name;

			if (editInfo != null)
			{
                editInfo.User = User;

                if (editInfo.EditMode == PropertyEditorMode.Edit)
				{
					editInfo.EditMode = EditMode;
				}
				
				//Get the Editor Type to use (if specified)
				if (!string.IsNullOrEmpty(EditorTypeName))
				{
					editInfo.Editor = EditorTypeName;
				}
				
				//Get the Label Mode to use (if specified)
				if (LabelMode != LabelMode.Left)
				{
					editInfo.LabelMode = LabelMode;
				}
				
				//if Required is specified set editors property
				if (Required)
				{
					editInfo.Required = Required;
				}
				
				//Get the ValidationExpression to use (if specified)
				if (!string.IsNullOrEmpty(ValidationExpression))
				{
					editInfo.ValidationExpression = ValidationExpression;
				}
				
				//Raise the ItemCreated Event
				OnItemCreated(new PropertyEditorItemEventArgs(editInfo));

				Visible = editInfo.Visible;

				if (EditorDisplayMode == EditorDisplayMode.Div)
				{
					BuildDiv(editInfo);
				}
				else
				{
					BuildTable(editInfo);
				}
			}
		}

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
		/// Runs when the Editor is Created
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

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (String.IsNullOrEmpty(CssClass))
			{
				CssClass = "dnnFormItem";
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Runs when the Value of a Property changes
		/// </summary>
		/// -----------------------------------------------------------------------------
		protected virtual void ValueChanged(object sender, PropertyEditorEventArgs e)
		{
			IsDirty = EditorInfoAdapter.UpdateValue(e);
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Runs when the Visibility of a Property changes
		/// </summary>
		/// -----------------------------------------------------------------------------
		protected virtual void VisibilityChanged(object sender, PropertyEditorEventArgs e)
		{
			IsDirty = EditorInfoAdapter.UpdateVisibility(e);
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

			//Create the editor
			CreateEditor();

			//Set flag so CreateChildConrols should not be invoked later in control's lifecycle
			ChildControlsCreated = true;
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Validates the data, and sets the IsValid Property
		/// </summary>
		/// -----------------------------------------------------------------------------
		public virtual void Validate()
		{
			_IsValid = Editor.IsValid;

			if (_IsValid)
			{
				IEnumerator valEnumerator = Validators.GetEnumerator();
				while (valEnumerator.MoveNext())
				{
					var validator = (IValidator) valEnumerator.Current;
					validator.Validate();
					if (!validator.IsValid)
					{
						_IsValid = false;
						break;
					}
				}
				_Validated = true;
			}
		}
		
		#endregion

		#region Event Handlers

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Runs when an Item in the List Is Changed
		/// </summary>
		/// <remarks>Raises an ItemChanged event.</remarks>
		/// -----------------------------------------------------------------------------
		protected virtual void ListItemChanged(object sender, PropertyEditorEventArgs e)
		{
			if (ItemChanged != null)
			{
				ItemChanged(this, e);
			}
		}
		
		#endregion
	}
}
