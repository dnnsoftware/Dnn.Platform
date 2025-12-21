// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>The CheckBoxColumnTemplate provides a Template for the CheckBoxColumn.</summary>
    public class CheckBoxColumnTemplate : ITemplate
    {
        /// <summary>Initializes a new instance of the <see cref="CheckBoxColumnTemplate"/> class.</summary>
        public CheckBoxColumnTemplate()
            : this(ListItemType.Item)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CheckBoxColumnTemplate"/> class.</summary>
        /// <param name="itemType">The list item type.</param>
        public CheckBoxColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
        }

        public event DNNDataGridCheckedColumnEventHandler CheckedChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the column fires a postback when any check box is
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
        public bool AutoPostBack { get; set; }

        /// <summary>Gets or sets a value indicating whether the checkbox is checked (unless DataBound).</summary>
        /// <value>A Boolean.</value>
        public bool Checked { get; set; }

        /// <summary>Gets or sets the Data Field that the column should bind to.</summary>
        /// <value>A String.</value>
        public string DataField { get; set; } = Null.NullString;

        /// <summary>Gets or sets a value indicating whether the Column is in Design Mode.</summary>
        /// <value>A Boolean.</value>
        public bool DesignMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the checkboxes are enabled (this is overridden if
        /// the EnabledField is set).
        /// </summary>
        /// <value>A Boolean.</value>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the Data Field that determines whether the checkbox is Enabled
        /// changed.
        /// </summary>
        /// <value>A String.</value>
        public string EnabledField { get; set; } = Null.NullString;

        /// <summary>
        /// Gets or sets a value indicating whether there is a checkbox in the Header that sets all
        /// the checkboxes.
        /// </summary>
        /// <value>A Boolean.</value>
        public bool HeaderCheckBox { get; set; } = true;

        /// <summary>Gets or sets the type of Template to Create.</summary>
        /// <value>A String.</value>
        public ListItemType ItemType { get; set; } = ListItemType.Item;

        /// <summary>Gets or sets the Text to display in a Header Template.</summary>
        /// <value>A String.</value>
        public string Text { get; set; } = string.Empty;

        /// <summary>InstantiateIn is called when the Template is instantiated by the parent control.</summary>
        /// <param name="container">The container control.</param>
        public void InstantiateIn(Control container)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                container.Controls.Add(new LiteralControl(this.Text + "<br/>"));
            }

            if (this.ItemType != ListItemType.Header || (this.ItemType == ListItemType.Header && this.HeaderCheckBox))
            {
                var box = new CheckBox();
                box.AutoPostBack = this.AutoPostBack;
                box.DataBinding += this.Item_DataBinding;
                box.CheckedChanged += this.OnCheckChanged;
                container.Controls.Add(box);
            }
        }

        /// <summary>Called when the template item is Data Bound.</summary>
        private void Item_DataBinding(object sender, EventArgs e)
        {
            var box = (CheckBox)sender;
            var container = (DataGridItem)box.NamingContainer;
            if (!string.IsNullOrEmpty(this.DataField) && this.ItemType != ListItemType.Header)
            {
                if (this.DesignMode)
                {
                    box.Checked = false;
                }
                else
                {
                    box.Checked = Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.DataField), CultureInfo.InvariantCulture);
                }
            }
            else
            {
                box.Checked = this.Checked;
            }

            if (!string.IsNullOrEmpty(this.EnabledField))
            {
                if (this.DesignMode)
                {
                    box.Enabled = false;
                }
                else
                {
                    box.Enabled = Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.EnabledField), CultureInfo.InvariantCulture);
                }
            }
            else
            {
                box.Enabled = this.Enabled;
            }
        }

        /// <summary>Centralised Event that is raised whenever a check box's state is modified.</summary>
        private void OnCheckChanged(object sender, EventArgs e)
        {
            var box = (CheckBox)sender;
            var container = (DataGridItem)box.NamingContainer;
            DNNDataGridCheckChangedEventArgs evntArgs;
            if (container.ItemIndex == Null.NullInteger)
            {
                evntArgs = new DNNDataGridCheckChangedEventArgs(container, box.Checked, this.DataField, true);
            }
            else
            {
                evntArgs = new DNNDataGridCheckChangedEventArgs(container, box.Checked, this.DataField, false);
            }

            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(sender, evntArgs);
            }
        }
    }
}
