// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DnnFormRadioButtonListItem : DnnFormListItemBase
    {
        private RadioButtonList radioButtonList;

        /// <inheritdoc/>
        protected override void BindList()
        {
            if (this.radioButtonList != null)
            {
                string selectedValue = !this.radioButtonList.Page.IsPostBack
                    ? Convert.ToString(this.Value, CultureInfo.InvariantCulture)
                    : this.radioButtonList.Page.Request.Form[this.radioButtonList.UniqueID];

                if (this.ListSource is Dictionary<string, string> items)
                {
                    foreach (var item in items)
                    {
                        var listItem = new ListItem(item.Key, item.Value);
                        listItem.Attributes.Add("onClick", this.OnClientClicked);

                        this.radioButtonList.Items.Add(listItem);
                    }
                }
                else
                {
                    this.radioButtonList.DataTextField = this.ListTextField;
                    this.radioButtonList.DataValueField = this.ListValueField;
                    this.radioButtonList.DataSource = this.ListSource;

                    this.radioButtonList.DataBind();
                }

                if (string.IsNullOrEmpty(selectedValue))
                {
                    selectedValue = this.DefaultValue;
                }

                // Reset SelectedValue
                if (this.radioButtonList.Items.FindByValue(selectedValue) != null)
                {
                    this.radioButtonList.Items.FindByValue(selectedValue).Selected = true;
                }

                if (selectedValue != Convert.ToString(this.Value, CultureInfo.InvariantCulture))
                {
                    this.UpdateDataSource(this.Value, selectedValue, this.DataField);
                }
            }
        }

        /// <inheritdoc/>
        protected override WebControl CreateControlInternal(Control container)
        {
            this.radioButtonList = new RadioButtonList { ID = this.ID + "_RadioButtonList", RepeatColumns = 1, RepeatDirection = RepeatDirection.Vertical, RepeatLayout = RepeatLayout.Flow };

            container.Controls.Add(this.radioButtonList);

            if (this.ListSource != null)
            {
                this.BindList();
            }

            return this.radioButtonList;
        }
    }
}
