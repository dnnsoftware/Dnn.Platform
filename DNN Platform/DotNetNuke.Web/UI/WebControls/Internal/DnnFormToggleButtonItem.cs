﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Web.UI.WebControls;

    /// <remarks>
    /// This control is only for internal use, please don't reference it in any other place as it may be removed in future.
    /// </remarks>
    public class DnnFormToggleButtonItem : DnnFormItemBase
    {
        // private DnnRadButton _checkBox;
        private CheckBox checkBox;

        /// <summary>Initializes a new instance of the <see cref="DnnFormToggleButtonItem"/> class.</summary>
        public DnnFormToggleButtonItem()
        {
            this.Mode = CheckBoxMode.TrueFalse;
        }

        public enum CheckBoxMode
        {
            TrueFalse = 0,
            YN = 1,
            YesNo = 2,
        }

        public CheckBoxMode Mode { get; set; }

        /// <inheritdoc/>
        protected override WebControl CreateControlInternal(Control container)
        {
            // _checkBox = new DnnRadButton {ID = ID + "_CheckBox", ButtonType = RadButtonType.ToggleButton, ToggleType = ButtonToggleType.CheckBox, AutoPostBack = false};
            this.checkBox = new CheckBox { ID = this.ID + "_CheckBox", AutoPostBack = false };

            this.checkBox.CheckedChanged += this.CheckedChanged;
            container.Controls.Add(this.checkBox);

            // Load from ControlState
            if (!this.checkBox.Page.IsPostBack)
            {
            }

            switch (this.Mode)
            {
                case CheckBoxMode.YN:
                case CheckBoxMode.YesNo:
                    var stringValue = this.Value as string;
                    if (stringValue != null)
                    {
                        this.checkBox.Checked = stringValue.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase);
                    }

                    break;
                default:
                    this.checkBox.Checked = Convert.ToBoolean(this.Value);
                    break;
            }

            return this.checkBox;
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.FormMode = DnnFormMode.Short;
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            string newValue;
            switch (this.Mode)
            {
                case CheckBoxMode.YN:
                    newValue = this.checkBox.Checked ? "Y" : "N";
                    break;
                case CheckBoxMode.YesNo:
                    newValue = this.checkBox.Checked ? "Yes" : "No";
                    break;
                default:
                    newValue = this.checkBox.Checked ? "true" : "false";
                    break;
            }

            this.UpdateDataSource(this.Value, newValue, this.DataField);
        }
    }
}
