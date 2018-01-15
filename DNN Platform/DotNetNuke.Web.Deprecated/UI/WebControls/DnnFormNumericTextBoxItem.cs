#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormNumericTextBoxItem : DnnFormItemBase
    {
        //private DnnNumericTextBox _textBox;
        private TextBox _textBox;

        public DnnFormNumericTextBoxItem()
        {
            TextBoxWidth = new Unit(100);
            ShowSpinButtons = true;
            Type = NumericType.Number;
            DecimalDigits = 0;
        }

        public int DecimalDigits { get; set; }

        public bool ShowSpinButtons { get; set; }

        public Unit TextBoxWidth { get; set; }

        public NumericType Type { get; set; }

        private void TextChanged(object sender, EventArgs e)
        {
            UpdateDataSource(Value, _textBox.Text, DataField);
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            //_textBox = new DnnNumericTextBox {EmptyMessage = LocalizeString(ResourceKey + ".Hint"), ID = ID + "_TextBox", Width = TextBoxWidth };
            _textBox = new TextBox();
            _textBox.CssClass = "DnnNumericTextBox";
            //_textBox.Style.Add("float", "none");
            //_textBox.EmptyMessageStyle.CssClass += "dnnformHint";
            //_textBox.Type = Type;
            //_textBox.NumberFormat.DecimalDigits = DecimalDigits;
            //_textBox.ShowSpinButtons = ShowSpinButtons;
            _textBox.TextChanged += TextChanged;

            //Load from ControlState
            _textBox.Text = Convert.ToString(Value);

            container.Controls.Add(_textBox);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            var initalizeScript = "<script type='text/javascript'>$(function(){$('.DnnNumericTextBox').dnnSpinner({type: 'range', defaultVal:" + _textBox.Text + ", typedata: { min: 1, interval: 1, max: 2147482624 }});});</script>";
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "DnnFormNumericTextBoxItem", initalizeScript);

            return _textBox;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            FormMode = DnnFormMode.Short;
        }
    }
}