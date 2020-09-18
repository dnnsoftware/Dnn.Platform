// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using Telerik.Web.UI;

    public class DnnFormNumericTextBoxItem : DnnFormItemBase
    {
        // private DnnNumericTextBox _textBox;
        private TextBox _textBox;

        public DnnFormNumericTextBoxItem()
        {
            this.TextBoxWidth = new Unit(100);
            this.ShowSpinButtons = true;
            this.Type = NumericType.Number;
            this.DecimalDigits = 0;
        }

        public int DecimalDigits { get; set; }

        public bool ShowSpinButtons { get; set; }

        public Unit TextBoxWidth { get; set; }

        public NumericType Type { get; set; }

        protected override WebControl CreateControlInternal(Control container)
        {
            // _textBox = new DnnNumericTextBox {EmptyMessage = LocalizeString(ResourceKey + ".Hint"), ID = ID + "_TextBox", Width = TextBoxWidth };
            this._textBox = new TextBox();
            this._textBox.CssClass = "DnnNumericTextBox";

            // _textBox.Style.Add("float", "none");
            // _textBox.EmptyMessageStyle.CssClass += "dnnformHint";
            // _textBox.Type = Type;
            // _textBox.NumberFormat.DecimalDigits = DecimalDigits;
            // _textBox.ShowSpinButtons = ShowSpinButtons;
            this._textBox.TextChanged += this.TextChanged;

            // Load from ControlState
            this._textBox.Text = Convert.ToString(this.Value);

            container.Controls.Add(this._textBox);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            var initalizeScript = "<script type='text/javascript'>$(function(){$('.DnnNumericTextBox').dnnSpinner({type: 'range', defaultVal:" + this._textBox.Text + ", typedata: { min: 1, interval: 1, max: 2147482624 }});});</script>";
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "DnnFormNumericTextBoxItem", initalizeScript);

            return this._textBox;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.FormMode = DnnFormMode.Short;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            this.UpdateDataSource(this.Value, this._textBox.Text, this.DataField);
        }
    }
}
