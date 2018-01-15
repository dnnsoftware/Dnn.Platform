#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
            ItemType = itemType;
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
                return mDataField;
            }
            set
            {
                mDataField = value;
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
                return mEnabled;
            }
            set
            {
                mEnabled = value;
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
                return mEnabledField;
            }
            set
            {
                mEnabledField = value;
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
                return mHeaderCheckBox;
            }
            set
            {
                mHeaderCheckBox = value;
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
                return mItemType;
            }
            set
            {
                mItemType = value;
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
                return mText;
            }
            set
            {
                mText = value;
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
            if (!String.IsNullOrEmpty(Text))
            {
                container.Controls.Add(new LiteralControl(Text + "<br/>"));
            }
            if (ItemType != ListItemType.Header || (ItemType == ListItemType.Header && HeaderCheckBox))
            {
                var box = new CheckBox();
                box.AutoPostBack = AutoPostBack;
                box.DataBinding += Item_DataBinding;
                box.CheckedChanged += OnCheckChanged;
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
            if (!String.IsNullOrEmpty(DataField) && ItemType != ListItemType.Header)
            {
                if (DesignMode)
                {
                    box.Checked = false;
                }
                else
                {
                    box.Checked = Convert.ToBoolean(DataBinder.Eval(container.DataItem, DataField));
                }
            }
            else
            {
                box.Checked = Checked;
            }
            if (!String.IsNullOrEmpty(EnabledField))
            {
                if (DesignMode)
                {
                    box.Enabled = false;
                }
                else
                {
                    box.Enabled = Convert.ToBoolean(DataBinder.Eval(container.DataItem, EnabledField));
                }
            }
            else
            {
                box.Enabled = Enabled;
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
                evntArgs = new DNNDataGridCheckChangedEventArgs(container, box.Checked, DataField, true);
            }
            else
            {
                evntArgs = new DNNDataGridCheckChangedEventArgs(container, box.Checked, DataField, false);
            }
            if (CheckedChanged != null)
            {
                CheckedChanged(sender, evntArgs);
            }
        }
    }
}
