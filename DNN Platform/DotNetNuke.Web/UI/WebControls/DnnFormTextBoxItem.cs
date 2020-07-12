// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    public class DnnFormTextBoxItem : DnnFormItemBase
    {
        private TextBox _textBox;

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

        /// <summary>
        /// Gets or sets a value indicating whether do not output field's value after post back when text mode set to password mode.
        /// </summary>
        public bool ClearContentInPasswordMode { get; set; }

        protected override WebControl CreateControlInternal(Control container)
        {
            this._textBox = new TextBox { ID = this.ID + "_TextBox" };

            this._textBox.Rows = this.Rows;
            this._textBox.Columns = this.Columns;
            this._textBox.TextMode = this.TextMode;
            this._textBox.CssClass = this.TextBoxCssClass;
            this._textBox.AutoCompleteType = this.AutoCompleteType;
            this._textBox.TextChanged += this.TextChanged;
            this._textBox.Attributes.Add("aria-label", this.DataField);

            // Load from ControlState
            this._textBox.Text = Convert.ToString(this.Value);
            if (this.TextMode == TextBoxMode.Password)
            {
                this._textBox.Attributes.Add("autocomplete", "off");
            }

            if (this.MaxLength > 0)
            {
                this._textBox.MaxLength = this.MaxLength;
            }

            container.Controls.Add(this._textBox);

            return this._textBox;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.TextMode == TextBoxMode.Password && !this.ClearContentInPasswordMode)
            {
                this._textBox.Attributes.Add("value", Convert.ToString(this.Value));
            }
        }

        private void TextChanged(object sender, EventArgs e)
        {
            this.UpdateDataSource(this.Value, this._textBox.Text, this.DataField);
        }
    }
}
