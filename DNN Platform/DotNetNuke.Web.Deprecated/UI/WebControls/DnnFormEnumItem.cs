// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Linq;

    using DotNetNuke.Services.Localization;

    public class DnnFormEnumItem : DnnFormComboBoxItem
    {
        private string _enumType;

        public string EnumType
        {
            get
            {
                return this._enumType;
            }

            set
            {
                this._enumType = value;

                // ReSharper disable AssignNullToNotNullAttribute
                this.ListSource = (from object enumValue in Enum.GetValues(Type.GetType(this._enumType))
                                   select new { Name = Localization.GetString(Enum.GetName(Type.GetType(this._enumType), enumValue)) ?? Enum.GetName(Type.GetType(this._enumType), enumValue), Value = (int)enumValue })
                                .ToList();

                // ReSharper restore AssignNullToNotNullAttribute
            }
        }

        protected override void BindList()
        {
            this.ListTextField = "Name";
            this.ListValueField = "Value";

            BindListInternal(this.ComboBox, Convert.ToInt32(this.Value), this.ListSource, this.ListTextField, this.ListValueField);
        }
    }
}
