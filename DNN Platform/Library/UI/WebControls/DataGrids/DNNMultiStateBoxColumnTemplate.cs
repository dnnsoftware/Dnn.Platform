// Licensed to the .NET Foundation under one or more agreements.
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
    /// Class:      DNNMultiStateBoxColumnTemplate
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNMultiStateBoxColumnTemplate provides a Template for the DNNMultiStateBoxColumn
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DNNMultiStateBoxColumnTemplate : ITemplate
    {
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;
        private string mImagePath = "";
        private ListItemType mItemType = ListItemType.Item;
        private string mSelectedStateKey = "";
        private DNNMultiStateCollection mStates;
        private string mText = "";

        public DNNMultiStateBoxColumnTemplate() : this(ListItemType.Item)
        {
        }

        public DNNMultiStateBoxColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Gets and sets whether the column fires a postback when the control changes
		/// </summary>
		/// <value>A Boolean</value>
		/// -----------------------------------------------------------------------------
        public bool AutoPostBack { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the selected state of the DNNMultiStateBox (unless DataBound)
        /// </summary>
        /// <value>A Boolean</value>
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
        /// An flag that indicates whether the control is enabled (this is overridden if
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
        /// The Data Field that determines whether the control is Enabled
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the image path of the DNNMultiStateBox
        /// </summary>
        /// <value>A Boolean</value>
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
        /// Gets and sets the state collection of the DNNMultiStateBox
        /// </summary>
        /// <value>A Boolean</value>
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

        #endregion

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Called when the template item is Data Bound
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void Item_DataBinding(object sender, EventArgs e)
        {
            var box = (DNNMultiStateBox)sender;
            var container = (DataGridItem)box.NamingContainer;
            if (!String.IsNullOrEmpty(this.DataField) && this.ItemType != ListItemType.Header)
            {
                if (this.DesignMode)
                {
                    box.SelectedStateKey = "";
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
    }
}
