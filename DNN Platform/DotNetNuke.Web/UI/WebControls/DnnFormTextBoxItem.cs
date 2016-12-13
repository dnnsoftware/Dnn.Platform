#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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

