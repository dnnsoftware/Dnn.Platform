// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Web.UI.WebControls.Extensions;
    using Telerik.Web.UI;

    public class DnnFormComboBoxItem : DnnFormListItemBase
    {
        // public DropDownList ComboBox { get; set; }
        public DnnComboBox ComboBox { get; set; }

        // internal static void BindListInternal(DropDownList comboBox, object value, IEnumerable listSource, string textField, string valueField)
        internal static void BindListInternal(DnnComboBox comboBox, object value, IEnumerable listSource, string textField, string valueField)
        {
            if (comboBox != null)
            {
                string selectedValue = !comboBox.Page.IsPostBack ? Convert.ToString(value) : comboBox.SelectedValue;

                if (listSource is Dictionary<string, string>)
                {
                    var items = listSource as Dictionary<string, string>;
                    foreach (var item in items)
                    {
                        // comboBox.Items.Add(new ListItem(item.Key, item.Value));
                        comboBox.AddItem(item.Key, item.Value);
                    }
                }
                else
                {
                    comboBox.DataTextField = textField;
                    comboBox.DataValueField = valueField;
                    comboBox.DataSource = listSource;

                    comboBox.DataBind();
                }

                // Reset SelectedValue
                // comboBox.Select(selectedValue);
                var selectedItem = comboBox.FindItemByValue(selectedValue);
                if (selectedItem != null)
                {
                    selectedItem.Selected = true;
                }
            }
        }

        protected override void BindList()
        {
            BindListInternal(this.ComboBox, this.Value, this.ListSource, this.ListTextField, this.ListValueField);
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            // ComboBox = new DropDownList { ID = ID + "_ComboBox" };
            this.ComboBox = new DnnComboBox { ID = this.ID + "_ComboBox" };
            this.ComboBox.SelectedIndexChanged += this.IndexChanged;
            container.Controls.Add(this.ComboBox);

            if (this.ListSource != null)
            {
                this.BindList();
            }

            return this.ComboBox;
        }

        private void IndexChanged(object sender, EventArgs e)
        {
            this.UpdateDataSource(this.Value, this.ComboBox.SelectedValue, this.DataField);
        }
    }
}
