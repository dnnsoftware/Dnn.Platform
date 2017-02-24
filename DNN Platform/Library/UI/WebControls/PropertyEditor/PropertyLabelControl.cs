#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.UserControls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

#endregion

namespace DotNetNuke.UI.WebControls
{

	/// <summary>
	/// The PropertyLabelControl control provides a standard UI component for displaying
	/// a label for a property. It contains a Label and Help Text and can be Data Bound.
	/// </summary>
	/// <remarks>
	/// </remarks>
	[ToolboxData("<{0}:PropertyLabelControl runat=server></{0}:PropertyLabelControl>")]
	public class PropertyLabelControl : WebControl
	{

		#region Private Members

		private string _ResourceKey;
		protected LinkButton cmdHelp;
		protected HtmlGenericControl label;
		protected Label lblHelp;
		protected Label lblLabel;
        protected Panel pnlTooltip;
		protected Panel pnlHelp;
		
		#endregion

		#region Protected Members

		public PropertyLabelControl()
		{
			
		}


		protected override HtmlTextWriterTag TagKey
		{
			get
			{
				return HtmlTextWriterTag.Div;
			}
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets and Sets the Caption Text if no ResourceKey is provided
		/// </summary>
		/// <value>A string representing the Caption</value>
		[Browsable(true), Category("Appearance"), DefaultValue("Property"), Description("Enter Caption for the control.")]
		public string Caption
		{
			get
			{
				EnsureChildControls();
				return lblLabel.Text;
			}
			set
			{
				EnsureChildControls();
				lblLabel.Text = value;
			}
		}

		public string AssociatedControlId
		{
			get
			{
				EnsureChildControls();
				return lblLabel.AssociatedControlID;
			}
			set
			{
				EnsureChildControls();
				lblLabel.AssociatedControlID = value;
			}
		}

		/// <summary>
		/// Gets and Sets the related Edit Control
		/// </summary>
		/// <value>A Control</value>
		[Browsable(false)]
		public Control EditControl { get; set; }

		/// <summary>
		/// Text is value of the Label Text if no ResourceKey is provided
		/// </summary>
		/// <value>A string representing the Text</value>
		[Browsable(true), Category("Appearance"), DefaultValue(""), Description("Enter Help Text for the control.")]
		public string HelpText
		{
			get
			{
				EnsureChildControls();
				return lblHelp.Text;
			}
			set
			{
				EnsureChildControls();
				lblHelp.Text = value;
			}
		}

		/// <summary>
		/// ResourceKey is the root localization key for this control
		/// </summary>
		/// <value>A string representing the Resource Key</value>
		/// <remarks>This control will "standardise" the resource key names, so for instance
		/// if the resource key is "Control", Control.Text is the label text key, Control.Help
		/// is the label help text, Control.ErrorMessage is the Validation Error Message for the
		/// control
		/// </remarks>
		[Browsable(true), Category("Localization"), DefaultValue(""), Description("Enter the Resource key for the control.")]
		public string ResourceKey
		{
			get
			{
				return _ResourceKey;
			}
			set
			{
				_ResourceKey = value;

				EnsureChildControls();

				//Localize the Label and the Help text
				lblHelp.Attributes["resourcekey"] = _ResourceKey + ".Help";
				lblLabel.Attributes["resourcekey"] = _ResourceKey + ".Text";
			}
		}

		[Browsable(true), Category("Behavior"), DefaultValue(false), Description("Set whether the Help icon is displayed.")]
		public bool ShowHelp
		{
			get
			{
				EnsureChildControls();
				return cmdHelp.Visible;
			}
			set
			{
				EnsureChildControls();
				cmdHelp.Visible = value;
			}
		}
		
		#region Data Properties

		/// <summary>
		/// Gets and sets the value of the Field that is bound to the Label
		/// </summary>
		/// <value>A string representing the Name of the Field</value>
		[Browsable(true), Category("Data"), DefaultValue(""), Description("Enter the name of the field that is data bound to the Label's Text property.")]
		public string DataField { get; set; }

		/// <summary>
		/// Gets and sets the DataSource that is bound to this control
		/// </summary>
		/// <value>The DataSource object</value>
		[Browsable(false)]
		public object DataSource { get; set; }

		#endregion

		#region Style Properties

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the value of the Label Style
		/// </summary>
		/// <value>A string representing the Name of the Field</value>
		/// -----------------------------------------------------------------------------
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)),
		 Description("Set the Style for the Help Text.")]
		public Style HelpStyle
		{
			get
			{
				EnsureChildControls();
				return pnlHelp.ControlStyle;
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets the value of the Label Style
		/// </summary>
		/// <value>A string representing the Name of the Field</value>
		/// -----------------------------------------------------------------------------
		[Browsable(true), Category("Styles"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof (ExpandableObjectConverter)),
		 Description("Set the Style for the Label Text")]
		public Style LabelStyle
		{
			get
			{
				EnsureChildControls();
				return lblLabel.ControlStyle;
			}
		}

        public bool Required { get; set; }

		#endregion

		#endregion

		#region Protected Methods

		/// <summary>
		/// CreateChildControls creates the control collection.
		/// </summary>
		protected override void CreateChildControls()
		{
            CssClass += "dnnLabel";

            label = new HtmlGenericControl { TagName = "label" };

            if (!DesignMode)
            {
                cmdHelp = new LinkButton { ID = ID + "_cmdHelp", CssClass = "dnnFormHelp", CausesValidation = false, EnableViewState = false, TabIndex = -1 };

                lblLabel = new Label { ID = ID + "_label", EnableViewState = false };

                label.Controls.Add(lblLabel);

                Controls.Add(label);
                Controls.Add(cmdHelp);
            }

            pnlTooltip = new Panel { CssClass = "dnnTooltip" };

            pnlHelp = new Panel { ID = ID + "_pnlHelp", EnableViewState = false, CssClass = "dnnFormHelpContent dnnClear" };

            pnlTooltip.Controls.Add(pnlHelp);
          
            lblHelp = new Label { ID = ID + "_lblHelp", EnableViewState = false };
            pnlHelp.Controls.Add(lblHelp);

            var aHelpPin = new HyperLink();
            aHelpPin.CssClass = "pinHelp";
            aHelpPin.Attributes.Add("href", "#");
            aHelpPin.Attributes.Add("aria-label", "Pin");
            pnlHelp.Controls.Add(aHelpPin);

            //Controls.Add(label);
            Controls.Add(pnlTooltip);
		}

		/// <summary>
		/// OnDataBinding runs when the Control is being Data Bound (It is triggered by
		/// a call to Control.DataBind()
		/// </summary>
		protected override void OnDataBinding(EventArgs e)
		{
			//If there is a DataSource bind the relevent Properties
			if (DataSource != null)
			{
				EnsureChildControls();
				if (!String.IsNullOrEmpty(DataField))
				{
					//DataBind the Label (via the Resource Key)
					var dataRow = (DataRowView) DataSource;
					if (ResourceKey == string.Empty)
					{
						ResourceKey = Convert.ToString(dataRow[DataField]);
					}
					if (DesignMode)
					{
						label.InnerText = Convert.ToString(dataRow[DataField]);
					}
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
		    ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
		}

		/// <summary>
		/// OnLoad runs just before the Control is rendered, and makes sure that any
		/// properties are set properly before the control is rendered
		/// </summary>
		protected override void OnPreRender(EventArgs e)
		{
			//Make sure the Child Controls are created before assigning any properties
			EnsureChildControls();

            if (Required)
            {
                lblLabel.CssClass += " dnnFormRequired";
            }

			//DNNClientAPI.EnableMinMax(cmdHelp, pnlHelp, true, DNNClientAPI.MinMaxPersistanceType.None);
			if (EditControl != null)
			{
				label.Attributes.Add("for", EditControl is EditControl ? ((EditControl)EditControl).EditControlClientId : EditControl.ClientID);
			}

            //make sure the help container have the default css class to active js handler.
            if (!pnlHelp.ControlStyle.CssClass.Contains("dnnClear"))
            {
                pnlHelp.ControlStyle.CssClass = string.Format("dnnClear {0}", pnlHelp.ControlStyle.CssClass);
            }
            if(!pnlHelp.ControlStyle.CssClass.Contains("dnnFormHelpContent"))
            {
                pnlHelp.ControlStyle.CssClass = string.Format("dnnFormHelpContent {0}", pnlHelp.ControlStyle.CssClass);
            }
		}

		#endregion
	}
}
