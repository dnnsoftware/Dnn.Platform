// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.UI.WebControls;

    public class DnnFormEditControlItem : DnnFormItemBase
    {
        private EditControl control;

        public string ControlType { get; set; }

        protected override WebControl CreateControlInternal(Control container)
        {
            this.control = Reflection.CreateObject(this.ControlType, this.ControlType) as EditControl;

            if (this.control != null)
            {
                this.control.ID = this.ID + "_Control";
                this.control.Name = this.ID;
                this.control.EditMode = PropertyEditorMode.Edit;
                this.control.Required = false;
                this.control.Value = this.Value;
                this.control.OldValue = this.Value;
                this.control.ValueChanged += this.ValueChanged;
                this.control.DataField = this.DataField;

                this.control.CssClass = "dnnFormInput";

                container.Controls.Add(this.control);
            }

            return this.control;
        }

        private void ValueChanged(object sender, PropertyEditorEventArgs e)
        {
            this.UpdateDataSource(this.Value, e.Value, this.DataField);
        }
    }
}
