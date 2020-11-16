// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Project:    DotNetNuke
    /// Class:      CommandButton
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CommandButton Class provides an enhanced Button control for DotNetNuke.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ToolboxData("<{0}:CommandButton runat=server></{0}:CommandButton>")]
    public class CommandButton : WebControl, INamingContainer
    {
        private ImageButton icon;
        private LinkButton link;
        private LiteralControl separator;

        public event EventHandler Click;

        public event CommandEventHandler Command;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Separator between Buttons.
        /// </summary>
        /// <remarks>Defaults to 1 non-breaking spaces.</remarks>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ButtonSeparator
        {
            get
            {
                this.EnsureChildControls();
                return this.separator.Text;
            }

            set
            {
                this.EnsureChildControls();
                this.separator.Text = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the control causes Validation to occur.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool CausesValidation
        {
            get
            {
                this.EnsureChildControls();
                return this.link.CausesValidation;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.CausesValidation = value;
                this.link.CausesValidation = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the command argument for this command button.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string CommandArgument
        {
            get
            {
                this.EnsureChildControls();
                return this.link.CommandArgument;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.CommandArgument = value;
                this.link.CommandArgument = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the command name for this command button.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string CommandName
        {
            get
            {
                this.EnsureChildControls();
                return this.link.CommandName;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.CommandName = value;
                this.link.CommandName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the link is displayed.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayLink
        {
            get
            {
                this.EnsureChildControls();
                return this.link.Visible;
            }

            set
            {
                this.EnsureChildControls();
                this.link.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the icon is displayed.
        /// </summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DisplayIcon
        {
            get
            {
                this.EnsureChildControls();
                return this.icon.Visible;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Image used for the Icon.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ImageUrl
        {
            get
            {
                this.EnsureChildControls();
                if (string.IsNullOrEmpty(this.icon.ImageUrl))
                {
                    this.icon.ImageUrl = Entities.Icons.IconController.IconURL(this.IconKey, this.IconSize, this.IconStyle);
                }

                return this.icon.ImageUrl;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.ImageUrl = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Icon Key to obtain ImageURL.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string IconKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Icon Siz to obtain ImageURL.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string IconSize { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Icon Style to obtain ImageURL.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string IconStyle { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the "onClick" Attribute.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string OnClick
        {
            get
            {
                this.EnsureChildControls();
                return this.link.Attributes["onclick"];
            }

            set
            {
                this.EnsureChildControls();
                if (string.IsNullOrEmpty(value))
                {
                    this.icon.Attributes.Remove("onclick");
                    this.link.Attributes.Remove("onclick");
                }
                else
                {
                    this.icon.Attributes.Add("onclick", value);
                    this.link.Attributes.Add("onclick", value);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets the "OnClientClick" Property.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string OnClientClick
        {
            get
            {
                this.EnsureChildControls();
                return this.link.OnClientClick;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.OnClientClick = value;
                this.link.OnClientClick = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Resource Key used for the Control.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ResourceKey
        {
            get
            {
                this.EnsureChildControls();
                return this.link.Attributes["resourcekey"];
            }

            set
            {
                this.EnsureChildControls();
                if (string.IsNullOrEmpty(value))
                {
                    this.icon.Attributes.Remove("resourcekey");
                    this.link.Attributes.Remove("resourcekey");
                }
                else
                {
                    this.icon.Attributes.Add("resourcekey", value);
                    this.link.Attributes.Add("resourcekey", value);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Text used for the Control.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string Text
        {
            get
            {
                this.EnsureChildControls();
                return this.link.Text;
            }

            set
            {
                this.EnsureChildControls();
                this.link.Text = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the tooltip resource key used for the Control.
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ToolTipKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Validation Group that this control "validates".
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public string ValidationGroup
        {
            get
            {
                this.EnsureChildControls();
                return this.link.ValidationGroup;
            }

            set
            {
                this.EnsureChildControls();
                this.icon.ValidationGroup = value;
                this.link.ValidationGroup = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public void RegisterForPostback()
        {
            AJAX.RegisterPostBackControl(this.link);
            AJAX.RegisterPostBackControl(this.icon);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateChildControls overrides the Base class's method to correctly build the
        /// control based on the configuration.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "CommandButton";
            }

            this.icon = new ImageButton();
            this.icon.Visible = true;
            this.icon.CausesValidation = true;
            this.icon.Click += this.RaiseImageClick;
            this.icon.Command += this.RaiseCommand;
            this.Controls.Add(this.icon);
            this.separator = new LiteralControl();
            this.separator.Text = "&nbsp;";
            this.Controls.Add(this.separator);
            this.link = new LinkButton();
            this.link.Visible = true;
            this.link.CausesValidation = true;
            this.link.Click += this.RaiseClick;
            this.link.Command += this.RaiseCommand;
            this.Controls.Add(this.link);
            if (this.DisplayIcon && !string.IsNullOrEmpty(this.ImageUrl))
            {
                this.icon.EnableViewState = this.EnableViewState;
            }

            if (this.DisplayLink)
            {
                this.link.CssClass = this.CssClass;
                this.link.EnableViewState = this.EnableViewState;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnButtonClick raises the CommandButton control's Click event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnButtonClick(EventArgs e)
        {
            if (this.Click != null)
            {
                this.Click(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnCommand raises the CommandButton control's Command event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected virtual void OnCommand(CommandEventArgs e)
        {
            if (this.Command != null)
            {
                this.Command(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// OnPreRender runs just before the Render phase of the Page Life Cycle.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.EnsureChildControls();
            this.separator.Visible = this.DisplayLink && this.DisplayIcon;

            this.LocalResourceFile = UIUtilities.GetLocalResourceFile(this);

            var tooltipText = string.Empty;
            if (!string.IsNullOrEmpty(this.ToolTipKey))
            {
                tooltipText = Localization.GetString(this.ToolTipKey, this.LocalResourceFile);
            }

            if (string.IsNullOrEmpty(tooltipText) && !string.IsNullOrEmpty(this.ToolTip))
            {
                tooltipText = this.ToolTip;
            }

            if (!string.IsNullOrEmpty(tooltipText))
            {
                this.icon.ToolTip = this.link.ToolTip = this.icon.AlternateText = tooltipText;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaiseImageClick runs when the Image button is clicked.
        /// </summary>
        /// <remarks>It raises a Command Event.
        /// </remarks>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An ImageClickEventArgs object.</param>
        /// -----------------------------------------------------------------------------
        protected void RaiseImageClick(object sender, ImageClickEventArgs e)
        {
            this.OnButtonClick(new EventArgs());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaiseClick runs when one of the contained Link buttons is clciked.
        /// </summary>
        /// <remarks>It raises a Click Event.
        /// </remarks>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An EventArgs object.</param>
        /// -----------------------------------------------------------------------------
        private void RaiseClick(object sender, EventArgs e)
        {
            this.OnButtonClick(e);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// RaiseCommand runs when one of the contained Link buttons is clicked.
        /// </summary>
        /// <remarks>It raises a Command Event.
        /// </remarks>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An CommandEventArgs object.</param>
        /// -----------------------------------------------------------------------------
        private void RaiseCommand(object sender, CommandEventArgs e)
        {
            this.OnCommand(e);
        }
    }
}
