// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DnnFormRadioButtonListItem : DnnFormListItemBase
    {
        private RadioButtonList _radioButtonList;

        protected override void BindList()
        {
            if (this._radioButtonList != null)
            {
                string selectedValue = !this._radioButtonList.Page.IsPostBack ? Convert.ToString(this.Value) : this._radioButtonList.Page.Request.Form[this._radioButtonList.UniqueID];

                if (this.ListSource is Dictionary<string, string>)
                {
                    var items = this.ListSource as Dictionary<string, string>;
                    foreach (var item in items)
                    {
                        var listItem = new ListItem(item.Key, item.Value);
                        listItem.Attributes.Add("onClick", this.OnClientClicked);

                        this._radioButtonList.Items.Add(listItem);
                    }
                }
                else
                {
                    this._radioButtonList.DataTextField = this.ListTextField;
                    this._radioButtonList.DataValueField = this.ListValueField;
                    this._radioButtonList.DataSource = this.ListSource;

                    this._radioButtonList.DataBind();
                }

                if (string.IsNullOrEmpty(selectedValue))
                {
                    selectedValue = this.DefaultValue;
                }

                // Reset SelectedValue
                if (this._radioButtonList.Items.FindByValue(selectedValue) != null)
                {
                    this._radioButtonList.Items.FindByValue(selectedValue).Selected = true;
                }

                if (selectedValue != Convert.ToString(this.Value))
                {
                    this.UpdateDataSource(this.Value, selectedValue, this.DataField);
                }
            }
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            this._radioButtonList = new RadioButtonList { ID = this.ID + "_RadioButtonList", RepeatColumns = 1, RepeatDirection = RepeatDirection.Vertical, RepeatLayout = RepeatLayout.Flow };

            container.Controls.Add(this._radioButtonList);

            if (this.ListSource != null)
            {
                this.BindList();
            }

            return this._radioButtonList;
        }
    }
}
