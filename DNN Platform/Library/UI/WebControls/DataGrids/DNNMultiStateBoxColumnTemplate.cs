// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            ItemType = itemType;
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
                return mSelectedStateKey;
            }
            set
            {
                mSelectedStateKey = value;
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
                return mEnabled;
            }
            set
            {
                mEnabled = value;
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
                return mEnabledField;
            }
            set
            {
                mEnabledField = value;
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
                return mImagePath;
            }
            set
            {
                mImagePath = value;
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
                if (mStates == null)
                {
                    mStates = new DNNMultiStateCollection(new DNNMultiStateBox());
                }
                return mStates;
            }
            set
            {
                mStates = value;
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
            if (ItemType != ListItemType.Header)
            {
                var box = new DNNMultiStateBox();
                box.AutoPostBack = AutoPostBack;
                box.ImagePath = ImagePath;
                foreach (DNNMultiState objState in States)
                {
                    box.States.Add(objState);
                }
                box.DataBinding += Item_DataBinding;
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
            var box = (DNNMultiStateBox) sender;
            var container = (DataGridItem) box.NamingContainer;
            if (!String.IsNullOrEmpty(DataField) && ItemType != ListItemType.Header)
            {
                if (DesignMode)
                {
                    box.SelectedStateKey = "";
                }
                else
                {
                    box.SelectedStateKey = Convert.ToString(DataBinder.Eval(container.DataItem, DataField));
                }
            }
            else
            {
                box.SelectedStateKey = SelectedStateKey;
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
    }
}
