// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HelpButtonControl is a user control that provides all the server code to display
    /// field level help button.
    /// </summary>
    /// <remarks>
    /// To implement help, the control uses the ClientAPI interface.  In particular
    ///  the javascript function __dnn_Help_OnClick().
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public abstract class HelpButtonControl : UserControl
    {
        protected LinkButton cmdHelp;
        protected Image imgHelp;
        protected Label lblHelp;
        protected Panel pnlHelp;
        private string _HelpKey;
        private string _ResourceKey;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets controlName is the Id of the control that is associated with the label.
        /// </summary>
        /// <value>A string representing the id of the associated control.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string ControlName { get; set; } // Associated Edit Control for this Label

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets helpKey is the Resource Key for the Help Text.
        /// </summary>
        /// <value>A string representing the Resource Key for the Help Text.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string HelpKey
        {
            get
            {
                return this._HelpKey;
            }

            set
            {
                this._HelpKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets helpText is value of the Help Text if no ResourceKey is provided.
        /// </summary>
        /// <value>A string representing the Text.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string HelpText
        {
            get
            {
                return this.lblHelp.Text;
            }

            set
            {
                this.lblHelp.Text = value;
                this.imgHelp.AlternateText = HtmlUtils.Clean(value, false);

                // hide the help icon if the help text is ""
                if (string.IsNullOrEmpty(value))
                {
                    this.imgHelp.Visible = false;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets resourceKey is the Resource Key for the Help Text.
        /// </summary>
        /// <value>A string representing the Resource Key for the Label Text.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string ResourceKey
        {
            get
            {
                return this._ResourceKey;
            }

            set
            {
                this._ResourceKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdHelp.Click += this.cmdHelp_Click;

            try
            {
                DNNClientAPI.EnableMinMax(this.cmdHelp, this.pnlHelp, true, DNNClientAPI.MinMaxPersistanceType.None);
                if (string.IsNullOrEmpty(this._HelpKey))
                {
                    // Set Help Key to the Resource Key plus ".Help"
                    this._HelpKey = this._ResourceKey + ".Help";
                }

                string helpText = Localization.GetString(this._HelpKey, this);
                if (!string.IsNullOrEmpty(helpText))
                {
                    this.HelpText = helpText;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdHelp_Click(object sender, EventArgs e)
        {
            this.pnlHelp.Visible = true;
        }
    }
}
