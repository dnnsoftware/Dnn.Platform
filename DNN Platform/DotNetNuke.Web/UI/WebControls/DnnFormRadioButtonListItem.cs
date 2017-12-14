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