// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
