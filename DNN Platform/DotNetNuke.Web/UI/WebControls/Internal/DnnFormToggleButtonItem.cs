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
using DotNetNuke.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls.Internal
{
    ///<remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnFormToggleButtonItem : DnnFormItemBase
    {
        #region CheckBoxMode enum

        public enum CheckBoxMode
        {
            TrueFalse = 0,
            YN = 1,
            YesNo = 2
        }

        #endregion

        //private DnnRadButton _checkBox;
        private CheckBox _checkBox;

        public DnnFormToggleButtonItem()
        {
            Mode = CheckBoxMode.TrueFalse;
        }

        public CheckBoxMode Mode { get; set; }

        private void CheckedChanged(object sender, EventArgs e)
        {
            string newValue;
            switch (Mode)
            {
                case CheckBoxMode.YN:
                    newValue = (_checkBox.Checked) ? "Y" : "N";
                    break;
                case CheckBoxMode.YesNo:
                    newValue = (_checkBox.Checked) ? "Yes" : "No";
                    break;
                default:
                    newValue = (_checkBox.Checked) ? "true" : "false";
                    break;
            }
            UpdateDataSource(Value, newValue, DataField);
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            //_checkBox = new DnnRadButton {ID = ID + "_CheckBox", ButtonType = RadButtonType.ToggleButton, ToggleType = ButtonToggleType.CheckBox, AutoPostBack = false};
            _checkBox = new CheckBox{ ID = ID + "_CheckBox", AutoPostBack = false };

            _checkBox.CheckedChanged += CheckedChanged;
            container.Controls.Add(_checkBox);

            //Load from ControlState
            if (!_checkBox.Page.IsPostBack)
            {
            }
            switch (Mode)
            {
                case CheckBoxMode.YN:
                case CheckBoxMode.YesNo:
                    var stringValue = Value as string;
                    if (stringValue != null)
                    {
                        _checkBox.Checked = stringValue.ToUpperInvariant().StartsWith("Y");
                    }
                    break;
                default:
                    _checkBox.Checked = Convert.ToBoolean(Value);
                    break;
            }

            return _checkBox;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            FormMode = DnnFormMode.Short;
        }
    }
}