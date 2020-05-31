// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
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
                return ViewState.GetValue("TextBoxCssClass", string.Empty);
            }
            set
            {
                ViewState.SetValue("TextBoxCssClass", value, string.Empty);
            }
        }

        public TextBoxMode TextMode { get; set; }

		/// <summary>
		/// do not output field's value after post back when text mode set to password mode.
		/// </summary>
	    public bool ClearContentInPasswordMode { get; set; }

        private void TextChanged(object sender, EventArgs e)
        {
            UpdateDataSource(Value, _textBox.Text, DataField);
        }

        protected override WebControl CreateControlInternal(Control container)
        {

            _textBox = new TextBox { ID = ID + "_TextBox" };

            _textBox.Rows = Rows;
            _textBox.Columns = Columns;
            _textBox.TextMode = TextMode;
            _textBox.CssClass = TextBoxCssClass;
            _textBox.AutoCompleteType = AutoCompleteType;
            _textBox.TextChanged += TextChanged;
            _textBox.Attributes.Add("aria-label", DataField);

            //Load from ControlState
            _textBox.Text = Convert.ToString(Value);
            if (TextMode == TextBoxMode.Password)
            {
                _textBox.Attributes.Add("autocomplete", "off");
            }
            if (MaxLength > 0)
            {
                _textBox.MaxLength = MaxLength;
            }

            container.Controls.Add(_textBox);

            return _textBox;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (TextMode == TextBoxMode.Password && !ClearContentInPasswordMode)
            {
                _textBox.Attributes.Add("value", Convert.ToString(Value));
            }
        }

    }

}

