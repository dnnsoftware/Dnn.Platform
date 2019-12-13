#region Usings

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormRadioButtonListItem : DnnFormListItemBase
    {
        private RadioButtonList _radioButtonList;

        protected override void BindList()
        {
            if (_radioButtonList != null)
            {
                string selectedValue = !_radioButtonList.Page.IsPostBack ? Convert.ToString(Value) : _radioButtonList.Page.Request.Form[_radioButtonList.UniqueID];

                if (ListSource is Dictionary<string, string>)
                {
                    var items = ListSource as Dictionary<string, string>;
                    foreach (var item in items)
                    {
                        var listItem = new ListItem(item.Key, item.Value);
                        listItem.Attributes.Add("onClick", OnClientClicked);

                        _radioButtonList.Items.Add(listItem);
                    }
                }
                else
                {
                    _radioButtonList.DataTextField = ListTextField;
                    _radioButtonList.DataValueField = ListValueField;
                    _radioButtonList.DataSource = ListSource;

                    _radioButtonList.DataBind();
                }
                if (String.IsNullOrEmpty(selectedValue))
                {
                    selectedValue = DefaultValue;
                }

                //Reset SelectedValue
                if (_radioButtonList.Items.FindByValue(selectedValue) != null)
                {
                    _radioButtonList.Items.FindByValue(selectedValue).Selected = true;
                }

                if (selectedValue != Convert.ToString(Value))
                {
                    UpdateDataSource(Value, selectedValue, DataField);
                }
            }
        }

        protected override WebControl CreateControlInternal(Control container)
        {
            _radioButtonList = new RadioButtonList  { ID = ID + "_RadioButtonList", RepeatColumns = 1, RepeatDirection = RepeatDirection.Vertical, RepeatLayout = RepeatLayout.Flow};

            container.Controls.Add(_radioButtonList);

            if (ListSource != null)
            {
                BindList();
            }

            return _radioButtonList;
        }
    }
}
