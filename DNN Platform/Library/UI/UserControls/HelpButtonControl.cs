// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    /// <summary>
    /// HelpButtonControl is a user control that provides all the server code to display
    /// field level help button.
    /// </summary>
    /// <remarks>
    /// To implement help, the control uses the ClientAPI interface.  In particular
    ///  the javascript function __dnn_Help_OnClick().
    /// </remarks>
    public abstract class HelpButtonControl : UserControl
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdHelp;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Image imgHelp;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblHelp;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel pnlHelp;

        /// <summary>Gets or sets controlName is the Id of the control that is associated with the label.</summary>
        /// <value>A string representing the id of the associated control.</value>
        public string ControlName { get; set; } // Associated Edit Control for this Label

        /// <summary>Gets or sets helpKey is the Resource Key for the Help Text.</summary>
        /// <value>A string representing the Resource Key for the Help Text.</value>
        public string HelpKey { get; set; }

        /// <summary>Gets or sets helpText is value of the Help Text if no ResourceKey is provided.</summary>
        /// <value>A string representing the Text.</value>
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

        /// <summary>Gets or sets resourceKey is the Resource Key for the Help Text.</summary>
        /// <value>A string representing the Resource Key for the Label Text.</value>
        public string ResourceKey { get; set; }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdHelp.Click += this.cmdHelp_Click;

            try
            {
                DNNClientAPI.EnableMinMax(this.cmdHelp, this.pnlHelp, true, DNNClientAPI.MinMaxPersistanceType.None);
                if (string.IsNullOrEmpty(this.HelpKey))
                {
                    // Set Help Key to the Resource Key plus ".Help"
                    this.HelpKey = this.ResourceKey + ".Help";
                }

                string helpText = Localization.GetString(this.HelpKey, this);
                if (!string.IsNullOrEmpty(helpText))
                {
                    this.HelpText = helpText;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdHelp_Click(object sender, EventArgs e)
        {
            this.pnlHelp.Visible = true;
        }
    }
}
