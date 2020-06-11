﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      CheckBoxColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CheckBoxColumnTemplate provides a Template for the CheckBoxColumn
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class CheckBoxColumnTemplate : ITemplate
    {
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;
        private bool mHeaderCheckBox = true;
        private ListItemType mItemType = ListItemType.Item;
        private string mText = "";

        public CheckBoxColumnTemplate() : this(ListItemType.Item)
        {
        }

        public CheckBoxColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
        }

 /// -----------------------------------------------------------------------------
 /// <summary>
 /// Gets and sets whether the column fires a postback when any check box is
 /// changed
 /// </summary>
 /// <value>A Boolean</value>
 /// -----------------------------------------------------------------------------
        public bool AutoPostBack { get; set; }

 /// -----------------------------------------------------------------------------
 /// <summary>
 /// Gets and sets whether the checkbox is checked (unless DataBound)
 /// </summary>
 /// <value>A Boolean</value>
 /// -----------------------------------------------------------------------------
        public bool Checked { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Data Field that the column should bind to
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string DataField
        {
            get
            {
                return this.mDataField;
            }
            set
            {
                this.mDataField = value;
            }
        }

 /// -----------------------------------------------------------------------------
 /// <summary>
 /// Gets or sets the Design Mode of the Column
 /// </summary>
 /// <value>A Boolean</value>
 /// -----------------------------------------------------------------------------
        public bool DesignMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// An flag that indicates whether the hcekboxes are enabled (this is overridden if
        /// the EnabledField is set
        /// changed
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool Enabled
        {
            get
            {
                return this.mEnabled;
            }
            set
            {
                this.mEnabled = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Data Field that determines whether the checkbox is Enabled
        /// changed
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string EnabledField
        {
            get
            {
                return this.mEnabledField;
            }
            set
            {
                this.mEnabledField = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// A flag that indicates whether there is a checkbox in the Header that sets all
        /// the checkboxes
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool HeaderCheckBox
        {
            get
            {
                return this.mHeaderCheckBox;
            }
            set
            {
                this.mHeaderCheckBox = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The type of Template to Create
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public ListItemType ItemType
        {
            get
            {
                return this.mItemType;
            }
            set
            {
                this.mItemType = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Text to display in a Header Template
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        public string Text
        {
            get
            {
                return this.mText;
            }
            set
            {
                this.mText = value;
            }
        }

        #region ITemplate Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstantiateIn is called when the Template is instantiated by the parent control
        /// </summary>
        /// <param name="container">The container control</param>
        /// -----------------------------------------------------------------------------
        public void InstantiateIn(Control container)
        {
            if (!String.IsNullOrEmpty(this.Text))
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

        #endregion

        public event DNNDataGridCheckedColumnEventHandler CheckedChanged;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Called when the template item is Data Bound
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Item_DataBinding(object sender, EventArgs e)
        {
            var box = (CheckBox) sender;
            var container = (DataGridItem) box.NamingContainer;
            if (!String.IsNullOrEmpty(this.DataField) && this.ItemType != ListItemType.Header)
            {
                if (this.DesignMode)
                {
                    box.Checked = false;
                }
                else
                {
                    box.Checked = Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.DataField));
                }
            }
            else
            {
                box.Checked = this.Checked;
            }
            if (!String.IsNullOrEmpty(this.EnabledField))
            {
                if (this.DesignMode)
                {
                    box.Enabled = false;
                }
                else
                {
                    box.Enabled = Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.EnabledField));
                }
            }
            else
            {
                box.Enabled = this.Enabled;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Centralised Event that is raised whenever a check box's state is modified
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void OnCheckChanged(object sender, EventArgs e)
        {
            var box = (CheckBox) sender;
            var container = (DataGridItem) box.NamingContainer;
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
