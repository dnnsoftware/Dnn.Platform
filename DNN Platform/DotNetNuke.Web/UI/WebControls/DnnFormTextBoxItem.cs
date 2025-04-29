// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

public class DnnFormTextBoxItem : DnnFormItemBase
{
    private TextBox textBox;

    public AutoCompleteType AutoCompleteType { get; set; }

    public int MaxLength { get; set; }

    public int Columns { get; set; }

    public int Rows { get; set; }

    public string TextBoxCssClass
    {
        get
        {
            return this.ViewState.GetValue("TextBoxCssClass", string.Empty);
        }

        set
        {
            this.ViewState.SetValue("TextBoxCssClass", value, string.Empty);
        }
    }

    public TextBoxMode TextMode { get; set; }

    /// <summary>Gets or sets a value indicating whether do not output field's value after post back when text mode set to password mode.</summary>
    public bool ClearContentInPasswordMode { get; set; }

    /// <inheritdoc/>
    protected override WebControl CreateControlInternal(Control container)
    {
        this.textBox = new TextBox { ID = this.ID + "_TextBox" };

        this.textBox.Rows = this.Rows;
        this.textBox.Columns = this.Columns;
        this.textBox.TextMode = this.TextMode;
        this.textBox.CssClass = this.TextBoxCssClass;
        this.textBox.AutoCompleteType = this.AutoCompleteType;
        this.textBox.TextChanged += this.TextChanged;
        this.textBox.Attributes.Add("aria-label", this.DataField);

        // Load from ControlState
        this.textBox.Text = Convert.ToString(this.Value);
        if (this.TextMode == TextBoxMode.Password)
        {
            this.textBox.Attributes.Add("autocomplete", "off");
        }

        if (this.MaxLength > 0)
        {
            this.textBox.MaxLength = this.MaxLength;
        }

        container.Controls.Add(this.textBox);

        return this.textBox;
    }

    /// <inheritdoc/>
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (this.TextMode == TextBoxMode.Password && !this.ClearContentInPasswordMode)
        {
            this.textBox.Attributes.Add("value", Convert.ToString(this.Value));
        }
    }

    private void TextChanged(object sender, EventArgs e)
    {
        this.UpdateDataSource(this.Value, this.textBox.Text, this.DataField);
    }
}
