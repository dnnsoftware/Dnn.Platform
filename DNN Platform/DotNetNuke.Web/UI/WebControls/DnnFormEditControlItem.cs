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

using DotNetNuke.Framework;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormEditControlItem : DnnFormItemBase
    {
        private EditControl control;

        public string ControlType { get; set; }

        protected override WebControl CreateControlInternal(Control container)
        {
            control = Reflection.CreateObject(ControlType, ControlType) as EditControl;

            if (control != null)
            {
                control.ID = ID + "_Control";
                control.Name = ID;
                control.EditMode = PropertyEditorMode.Edit;
                control.Required = false;
                control.Value = Value;
                control.OldValue = Value;
                control.ValueChanged += ValueChanged;
	            control.DataField = DataField;

                control.CssClass = "dnnFormInput";

                container.Controls.Add(control);
            }

            return control;
        }

        void ValueChanged(object sender, PropertyEditorEventArgs e)
        {
            UpdateDataSource(Value, e.Value, DataField);
        }
    }
}