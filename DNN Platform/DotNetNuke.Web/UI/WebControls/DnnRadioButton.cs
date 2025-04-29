// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

public class DnnRadioButton : RadioButton, ILocalizable
{
    private bool localize = true;

    /// <summary>Initializes a new instance of the <see cref="DnnRadioButton"/> class.</summary>
    public DnnRadioButton()
    {
        this.CssClass = "SubHead dnnLabel";
    }

    /// <inheritdoc/>
    public bool Localize
    {
        get
        {
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
                this.ToolTip = Localization.GetString(this.ToolTip, this.LocalResourceFile);
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                this.Text = Localization.GetString(this.Text, this.LocalResourceFile);

                if (string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = Localization.GetString(this.Text + ".ToolTip", this.LocalResourceFile);
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
    }

    /// <inheritdoc/>
    protected override void Render(HtmlTextWriter writer)
    {
        this.LocalizeStrings();
        base.Render(writer);
    }
}
