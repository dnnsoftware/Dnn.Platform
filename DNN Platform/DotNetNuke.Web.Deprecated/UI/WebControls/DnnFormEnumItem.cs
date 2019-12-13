// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormEnumItem : DnnFormComboBoxItem
    {
        private string _enumType;

        public string EnumType
        {
            get
            {
                return _enumType;
            }
            set
            {
                _enumType = value;
                // ReSharper disable AssignNullToNotNullAttribute
                ListSource = (from object enumValue in Enum.GetValues(Type.GetType(_enumType))
                              select new { Name = Localization.GetString(Enum.GetName(Type.GetType(_enumType), enumValue)) ?? Enum.GetName(Type.GetType(_enumType), enumValue), Value = (int)enumValue })
                                .ToList();
                // ReSharper restore AssignNullToNotNullAttribute
            }
        }

        protected override void BindList()
        {
            ListTextField = "Name";
            ListValueField = "Value";

            BindListInternal(ComboBox, Convert.ToInt32(Value), ListSource, ListTextField, ListValueField);
        }
    }
}
