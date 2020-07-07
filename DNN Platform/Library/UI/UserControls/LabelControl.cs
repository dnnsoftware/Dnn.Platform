// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// LabelControl is a user control that provides all the server code to manage a
    /// label, including localization, 508 support and help.
    /// </summary>
    /// <remarks>
    /// To implement help, the control uses the ClientAPI interface.  In particular
    ///  the javascript function __dnn_Help_OnClick().
    /// </remarks>
    public abstract class LabelControl : UserControl
    {
        protected LinkButton cmdHelp;
        protected HtmlGenericControl label;
        protected Label lblHelp;
        protected Label lblLabel;
        protected Panel pnlHelp;
        protected Label lblNoHelpLabel;

        /// <summary>
        /// Gets or sets controlName is the Id of the control that is associated with the label.
        /// </summary>
        /// <value>A string representing the id of the associated control.</value>
        /// <remarks>
        /// </remarks>
        public string ControlName { get; set; }

        /// <summary>
        /// Gets or sets set the associate control id format, combined used with ControlName for controls
        ///  which have child edit control and want that child control focus when click label.
        /// </summary>
        public string AssociateFormat { get; set; }

        /// <summary>
        /// Gets or sets css style applied to the asp:label control.
        /// </summary>
        /// <value>A string representing css class name.</value>
        /// <remarks>
        /// </remarks>
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets helpKey is the Resource Key for the Help Text.
        /// </summary>
        /// <value>A string representing the Resource Key for the Help Text.</value>
        /// <remarks>
        /// </remarks>
        public string HelpKey { get; set; }

        /// <summary>
        /// Gets or sets helpText is value of the Help Text if no ResourceKey is provided.
        /// </summary>
        /// <value>A string representing the Text.</value>
        /// <remarks>
        /// </remarks>
        public string HelpText
        {
            get
            {
                return this.lblHelp.Text;
            }

            set
            {
                this.lblHelp.Text = value;
            }
        }

        public string NoHelpLabelText
        {
            get
            {
                return this.lblNoHelpLabel.Text;
            }

            set
            {
                this.lblNoHelpLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets resourceKey is the Resource Key for the Label Text.
        /// </summary>
        /// <value>A string representing the Resource Key for the Label Text.</value>
        /// <remarks>
        /// </remarks>
        public string ResourceKey { get; set; }

        /// <summary>
        /// Gets or sets suffix is Optional Text that appears after the Localized Label Text.
        /// </summary>
        /// <value>A string representing the Optional Text.</value>
        /// <remarks>
        /// </remarks>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets text is value of the Label Text if no ResourceKey is provided.
        /// </summary>
        /// <value>A string representing the Text.</value>
        /// <remarks>
        /// </remarks>
        public string Text
        {
            get
            {
                return this.lblLabel.Text;
            }

            set
            {
                this.lblLabel.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets width is value of the Label Width.
        /// </summary>
        /// <value>A string representing the Text.</value>
        /// <remarks>
        /// </remarks>
        public Unit Width
        {
            get
            {
                return this.lblLabel.Width;
            }

            set
            {
                this.lblLabel.Width = value;
            }
        }

        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                this.RegisterClientDependencies();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // get the localised text
            if (string.IsNullOrEmpty(this.ResourceKey))
            {
                // Set Resource Key to the ID of the control
                this.ResourceKey = this.ID;
            }

            if (!string.IsNullOrEmpty(this.ResourceKey))
            {
                var localText = Localization.GetString(this.ResourceKey, this);
                if (!string.IsNullOrEmpty(localText))
                {
                    this.Text = localText + this.Suffix;

                    // NoHelpLabelText = Text;
                }
                else
                {
                    this.Text += this.Suffix;

                    // NoHelpLabelText = Text;
                }
            }

            // Set Help Key to the Resource Key plus ".Help"
            if (string.IsNullOrEmpty(this.HelpKey))
            {
                this.HelpKey = this.ResourceKey + ".Help";
            }

            var helpText = Localization.GetString(this.HelpKey, this);
            if ((!string.IsNullOrEmpty(helpText)) || string.IsNullOrEmpty(this.HelpText))
            {
                this.HelpText = helpText;
            }

            if (string.IsNullOrEmpty(this.HelpText))
            {
                this.pnlHelp.Visible = this.cmdHelp.Visible = false;

                // lblHelp.Visible = false;
                // lblNoHelpLabel.Visible = true;
            }

            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.lblLabel.CssClass = this.CssClass;
            }

            // find the reference control in the parents Controls collection
            if (!string.IsNullOrEmpty(this.ControlName))
            {
                var c = this.Parent.FindControl(this.ControlName);
                var clientId = this.ControlName;
                if (c != null)
                {
                    clientId = c.ClientID;
                }

                if (!string.IsNullOrEmpty(this.AssociateFormat))
                {
                    clientId = string.Format(this.AssociateFormat, clientId);
                }

                this.label.Attributes["for"] = clientId;
            }
        }

        private void RegisterClientDependencies()
        {
            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            // ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
        }
    }
}
