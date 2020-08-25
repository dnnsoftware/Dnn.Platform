// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNMultiStateBoxColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNMultiStateBoxColumnTemplate provides a Template for the DNNMultiStateBoxColumn.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DNNMultiStateBoxColumnTemplate : ITemplate
    {
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;
        private string mImagePath = string.Empty;
        private ListItemType mItemType = ListItemType.Item;
        private string mSelectedStateKey = string.Empty;
        private DNNMultiStateCollection mStates;
        private string mText = string.Empty;

        public DNNMultiStateBoxColumnTemplate()
            : this(ListItemType.Item)
        {
        }

        public DNNMultiStateBoxColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the column fires a postback when the control changes.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool AutoPostBack { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the selected state of the DNNMultiStateBox (unless DataBound).
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public string SelectedStateKey
        {
            get
            {
                return this.mSelectedStateKey;
            }

            set
            {
                this.mSelectedStateKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Data Field that the column should bind to.
        /// </summary>
        /// <value>A String.</value>
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
        /// Gets or sets a value indicating whether gets or sets the Design Mode of the Column.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool DesignMode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether an flag that indicates whether the control is enabled (this is overridden if
        /// the EnabledField is set
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
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
        /// Gets or sets the Data Field that determines whether the control is Enabled
        /// changed.
        /// </summary>
        /// <value>A String.</value>
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
        /// Gets or sets the type of Template to Create.
        /// </summary>
        /// <value>A String.</value>
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
        /// Gets or sets the Text to display in a Header Template.
        /// </summary>
        /// <value>A String.</value>
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the image path of the DNNMultiStateBox.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public string ImagePath
        {
            get
            {
                return this.mImagePath;
            }

            set
            {
                this.mImagePath = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the state collection of the DNNMultiStateBox.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public DNNMultiStateCollection States
        {
            get
            {
                if (this.mStates == null)
                {
                    this.mStates = new DNNMultiStateCollection(new DNNMultiStateBox());
                }

                return this.mStates;
            }

            set
            {
                this.mStates = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// InstantiateIn is called when the Template is instantiated by the parent control.
        /// </summary>
        /// <param name="container">The container control.</param>
        /// -----------------------------------------------------------------------------
        public void InstantiateIn(Control container)
        {
            if (!string.IsNullOrEmpty(this.Text))
            {
                container.Controls.Add(new LiteralControl(this.Text + "<br/>"));
            }

            if (this.ItemType != ListItemType.Header)
            {
                var box = new DNNMultiStateBox();
                box.AutoPostBack = this.AutoPostBack;
                box.ImagePath = this.ImagePath;
                foreach (DNNMultiState objState in this.States)
                {
                    box.States.Add(objState);
                }

                box.DataBinding += this.Item_DataBinding;
                container.Controls.Add(box);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Called when the template item is Data Bound.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Item_DataBinding(object sender, EventArgs e)
        {
            var box = (DNNMultiStateBox)sender;
            var container = (DataGridItem)box.NamingContainer;
            if (!string.IsNullOrEmpty(this.DataField) && this.ItemType != ListItemType.Header)
            {
                if (this.DesignMode)
                {
                    box.SelectedStateKey = string.Empty;
                }
                else
                {
                    box.SelectedStateKey = Convert.ToString(DataBinder.Eval(container.DataItem, this.DataField));
                }
            }
            else
            {
                box.SelectedStateKey = this.SelectedStateKey;
            }

            if (!string.IsNullOrEmpty(this.EnabledField))
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
    }
}
