// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

public class DnnButton : Button, ILocalizable
{
    private bool localize = true;

    /// <summary>Initializes a new instance of the <see cref="DnnButton"/> class.</summary>
    public DnnButton()
    {
        this.CssClass = "CommandButton";
        this.DisabledCssClass = "CommandButtonDisabled";
    }

    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue("")]
    [Localizable(true)]
    public string ConfirmMessage
    {
        get
        {
            return this.ViewState["ConfirmMessage"] == null ? string.Empty : this.ViewState["ConfirmMessage"].ToString();
        }

        set
        {
            this.ViewState["ConfirmMessage"] = value;
        }
    }

    [Bindable(true)]
    [Category("Appearance")]
    [DefaultValue("")]
    [Localizable(true)]
    public new string DisabledCssClass
    {
        get
        {
            return this.ViewState["DisabledCssClass"] == null ? string.Empty : this.ViewState["DisabledCssClass"].ToString();
        }

        set
        {
            this.ViewState["DisabledCssClass"] = value;
        }
    }

    /// <inheritdoc/>
    public bool Localize
    {
        get
        {
            if (this.DesignMode)
            {
                return false;
            }

            return this.localize;
        }

        set
        {
            this.localize = value;
        }
    }

    /// <inheritdoc/>
    public string LocalResourceFile { get; set; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
            this.OnClientClick = Utilities.GetOnClientClickConfirm(this, msg);
        }
    }

    /// <inheritdoc/>
    protected override void Render(HtmlTextWriter writer)
    {
        this.LocalizeStrings();
        base.Render(writer);
    }
}
