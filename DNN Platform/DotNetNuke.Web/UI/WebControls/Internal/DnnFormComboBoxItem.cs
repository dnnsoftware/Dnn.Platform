// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Web.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    public class DnnFormComboBoxItem : DnnFormListItemBase
    {
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnFormComboBoxItem"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormComboBoxItem()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormComboBoxItem"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnFormComboBoxItem(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
            : base(appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
            this.clientResourceController = clientResourceController;
        }

        ////public DropDownList ComboBox { get; set; }

        /// <summary>Gets or sets the combobox.</summary>
        public DnnComboBox ComboBox { get; set; }

        ////internal static void BindListInternal(DropDownList comboBox, object value, IEnumerable listSource, string textField, string valueField)

        /// <summary>Binds the list.</summary>
        /// <param name="comboBox">The combobox.</param>
        /// <param name="value">The selected value.</param>
        /// <param name="listSource">The list source.</param>
        /// <param name="textField">The field of the data source that provides the text content of the list items.</param>
        /// <param name="valueField">The field of the data source that provides the value content of the list items.</param>
        internal static void BindListInternal(DnnComboBox comboBox, object value, IEnumerable listSource, string textField, string valueField)
        {
            if (comboBox != null)
            {
                string selectedValue = !comboBox.Page.IsPostBack ? Convert.ToString(value, CultureInfo.InvariantCulture) : comboBox.SelectedValue;

                if (listSource is Dictionary<string, string> items)
                {
                    foreach (var item in items)
                    {
                        ////comboBox.Items.Add(new ListItem(item.Key, item.Value));
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
                selectedItem?.Selected = true;
            }
        }

        /// <inheritdoc />
        protected override void BindList()
        {
            BindListInternal(this.ComboBox, this.Value, this.ListSource, this.ListTextField, this.ListValueField);
        }

        /// <inheritdoc />
        protected override WebControl CreateControlInternal(Control container)
        {
            ////ComboBox = new DropDownList { ID = ID + "_ComboBox" };
            this.ComboBox = new DnnComboBox(this.AppStatus, this.EventLogger, this.clientResourceController) { ID = this.ID + "_ComboBox", };
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
