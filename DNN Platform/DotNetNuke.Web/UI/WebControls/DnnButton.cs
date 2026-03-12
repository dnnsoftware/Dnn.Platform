// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A button control.</summary>
    public class DnnButton : Button, ILocalizable
    {
        private readonly IClientResourceController clientResourceController;
        private bool localize = true;

        /// <summary>Initializes a new instance of the <see cref="DnnButton"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnButton()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnButton"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnButton(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();

            this.CssClass = "CommandButton";
            this.DisabledCssClass = "CommandButtonDisabled";
        }

        /// <summary>Gets or sets a message to display upon clicking the button.</summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ConfirmMessage
        {
            get => this.ViewState["ConfirmMessage"] == null ? string.Empty : this.ViewState["ConfirmMessage"].ToString();
            set => this.ViewState["ConfirmMessage"] = value;
        }

        /// <summary>Gets or sets the CSS class to use when the control is disabled.</summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public new string DisabledCssClass
        {
            get => this.ViewState["DisabledCssClass"] == null ? string.Empty : this.ViewState["DisabledCssClass"].ToString();
            set => this.ViewState["DisabledCssClass"] = value;
        }

        /// <inheritdoc />
        public bool Localize
        {
            get => !this.DesignMode && this.localize;
            set => this.localize = value;
        }

        /// <inheritdoc />
        public string LocalResourceFile { get; set; }

        /// <inheritdoc />
        public virtual void LocalizeStrings()
        {
            if (this.Localize)
            {
                if (!string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = Utilities.GetLocalizedStringFromParent(this.ToolTip, this);
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    string unlocalizedText = this.Text;
                    this.Text = Utilities.GetLocalizedStringFromParent(unlocalizedText, this);
                    if (string.IsNullOrEmpty(this.Text))
                    {
                        this.Text = unlocalizedText;
                    }

                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = Utilities.GetLocalizedStringFromParent(unlocalizedText + ".ToolTip", this);
                        if (string.IsNullOrEmpty(this.ToolTip))
                        {
                            this.ToolTip = unlocalizedText;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!this.Enabled)
            {
                this.CssClass = this.DisabledCssClass;
            }

            if (!string.IsNullOrEmpty(this.ConfirmMessage))
            {
                string msg = this.ConfirmMessage;
                if (this.Localize)
                {
                    msg = Utilities.GetLocalizedStringFromParent(this.ConfirmMessage, this);
                }

                // must be done before render
                this.OnClientClick = Utilities.GetOnClientClickConfirm(this.clientResourceController, this, msg);
            }
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            this.LocalizeStrings();
            base.Render(writer);
        }
    }
}
