// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
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

    /// <summary>
    /// The PropertyLabelControl control provides a standard UI component for displaying
    /// a label for a property. It contains a Label and Help Text and can be Data Bound.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [ToolboxData("<{0}:PropertyLabelControl runat=server></{0}:PropertyLabelControl>")]
    public class PropertyLabelControl : WebControl
    {
        protected LinkButton cmdHelp;
        protected HtmlGenericControl label;
        protected Label lblHelp;
        protected Label lblLabel;
        protected Panel pnlTooltip;
        protected Panel pnlHelp;
        private string _ResourceKey;

        public PropertyLabelControl()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Label Style.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Help Text.")]
        public Style HelpStyle
        {
            get
            {
                this.EnsureChildControls();
                return this.pnlHelp.ControlStyle;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the value of the Label Style.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        /// -----------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Label Text")]
        public Style LabelStyle
        {
            get
            {
                this.EnsureChildControls();
                return this.lblLabel.ControlStyle;
            }
        }

        /// <summary>
        /// Gets or sets and Sets the Caption Text if no ResourceKey is provided.
        /// </summary>
        /// <value>A string representing the Caption.</value>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("Property")]
        [Description("Enter Caption for the control.")]
        public string Caption
        {
            get
            {
                this.EnsureChildControls();
                return this.lblLabel.Text;
            }

            set
            {
                this.EnsureChildControls();
                this.lblLabel.Text = value;
            }
        }

        public string AssociatedControlId
        {
            get
            {
                this.EnsureChildControls();
                return this.lblLabel.AssociatedControlID;
            }

            set
            {
                this.EnsureChildControls();
                this.lblLabel.AssociatedControlID = value;
            }
        }

        /// <summary>
        /// Gets or sets and Sets the related Edit Control.
        /// </summary>
        /// <value>A Control.</value>
        [Browsable(false)]
        public Control EditControl { get; set; }

        /// <summary>
        /// Gets or sets text is value of the Label Text if no ResourceKey is provided.
        /// </summary>
        /// <value>A string representing the Text.</value>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("Enter Help Text for the control.")]
        public string HelpText
        {
            get
            {
                this.EnsureChildControls();
                return this.lblHelp.Text;
            }

            set
            {
                this.EnsureChildControls();
                this.lblHelp.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets resourceKey is the root localization key for this control.
        /// </summary>
        /// <value>A string representing the Resource Key.</value>
        /// <remarks>This control will "standardise" the resource key names, so for instance
        /// if the resource key is "Control", Control.Text is the label text key, Control.Help
        /// is the label help text, Control.ErrorMessage is the Validation Error Message for the
        /// control.
        /// </remarks>
        [Browsable(true)]
        [Category("Localization")]
        [DefaultValue("")]
        [Description("Enter the Resource key for the control.")]
        public string ResourceKey
        {
            get
            {
                return this._ResourceKey;
            }

            set
            {
                this._ResourceKey = value;

                this.EnsureChildControls();

                // Localize the Label and the Help text
                this.lblHelp.Attributes["resourcekey"] = this._ResourceKey + ".Help";
                this.lblLabel.Attributes["resourcekey"] = this._ResourceKey + ".Text";
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Set whether the Help icon is displayed.")]
        public bool ShowHelp
        {
            get
            {
                this.EnsureChildControls();
                return this.cmdHelp.Visible;
            }

            set
            {
                this.EnsureChildControls();
                this.cmdHelp.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets and sets the value of the Field that is bound to the Label.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Label's Text property.")]
        public string DataField { get; set; }

        /// <summary>
        /// Gets or sets and sets the DataSource that is bound to this control.
        /// </summary>
        /// <value>The DataSource object.</value>
        [Browsable(false)]
        public object DataSource { get; set; }

        public bool Required { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>
        /// CreateChildControls creates the control collection.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.CssClass += "dnnLabel";

            this.label = new HtmlGenericControl { TagName = "label" };

            if (!this.DesignMode)
            {
                this.cmdHelp = new LinkButton { ID = this.ID + "_cmdHelp", CssClass = "dnnFormHelp", CausesValidation = false, EnableViewState = false, TabIndex = -1 };
                this.cmdHelp.Attributes.Add("aria-label", "Help");
                this.lblLabel = new Label { ID = this.ID + "_label", EnableViewState = false };

                this.label.Controls.Add(this.lblLabel);

                this.Controls.Add(this.label);
                this.Controls.Add(this.cmdHelp);
            }

            this.pnlTooltip = new Panel { CssClass = "dnnTooltip" };

            this.pnlHelp = new Panel { ID = this.ID + "_pnlHelp", EnableViewState = false, CssClass = "dnnFormHelpContent dnnClear" };

            this.pnlTooltip.Controls.Add(this.pnlHelp);

            this.lblHelp = new Label { ID = this.ID + "_lblHelp", EnableViewState = false };
            this.pnlHelp.Controls.Add(this.lblHelp);

            var aHelpPin = new HyperLink();
            aHelpPin.CssClass = "pinHelp";
            aHelpPin.Attributes.Add("href", "#");
            aHelpPin.Attributes.Add("aria-label", "Pin");
            this.pnlHelp.Controls.Add(aHelpPin);

            // Controls.Add(label);
            this.Controls.Add(this.pnlTooltip);
        }

        /// <summary>
        /// OnDataBinding runs when the Control is being Data Bound (It is triggered by
        /// a call to Control.DataBind().
        /// </summary>
        protected override void OnDataBinding(EventArgs e)
        {
            // If there is a DataSource bind the relevent Properties
            if (this.DataSource != null)
            {
                this.EnsureChildControls();
                if (!string.IsNullOrEmpty(this.DataField))
                {
                    // DataBind the Label (via the Resource Key)
                    var dataRow = (DataRowView)this.DataSource;
                    if (this.ResourceKey == string.Empty)
                    {
                        this.ResourceKey = Convert.ToString(dataRow[this.DataField]);
                    }

                    if (this.DesignMode)
                    {
                        this.label.InnerText = Convert.ToString(dataRow[this.DataField]);
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
        }

        /// <summary>
        /// OnLoad runs just before the Control is rendered, and makes sure that any
        /// properties are set properly before the control is rendered.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            // Make sure the Child Controls are created before assigning any properties
            this.EnsureChildControls();

            if (this.Required)
            {
                this.lblLabel.CssClass += " dnnFormRequired";
            }

            // DNNClientAPI.EnableMinMax(cmdHelp, pnlHelp, true, DNNClientAPI.MinMaxPersistanceType.None);
            if (this.EditControl != null)
            {
                this.label.Attributes.Add("for", this.EditControl is EditControl ? ((EditControl)this.EditControl).EditControlClientId : this.EditControl.ClientID);
            }

            // make sure the help container have the default css class to active js handler.
            if (!this.pnlHelp.ControlStyle.CssClass.Contains("dnnClear"))
            {
                this.pnlHelp.ControlStyle.CssClass = string.Format("dnnClear {0}", this.pnlHelp.ControlStyle.CssClass);
            }

            if (!this.pnlHelp.ControlStyle.CssClass.Contains("dnnFormHelpContent"))
            {
                this.pnlHelp.ControlStyle.CssClass = string.Format("dnnFormHelpContent {0}", this.pnlHelp.ControlStyle.CssClass);
            }
        }
    }
}
