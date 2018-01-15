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

using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNMultiStateBoxColumn
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNMultiStateBoxColumn control provides a DNNMultiState Box column for a Data Grid
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DNNMultiStateBoxColumn : TemplateColumn
    {
        private bool mAutoPostBack = true;
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;
        private string mImagePath = "";
        private string mSelectedStateKey = "";
        private DNNMultiStateCollection mStates;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs the DNNMultiStateBoxColumn
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DNNMultiStateBoxColumn() : this(false)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs the MultiStateBoxColumn, with an optional AutoPostBack (where each change
        /// of state of the control causes a Post Back)
        /// </summary>
        /// <param name="autoPostBack">Optional set the control to postback</param>
        /// -----------------------------------------------------------------------------
        public DNNMultiStateBoxColumn(bool autoPostBack)
        {
            AutoPostBack = autoPostBack;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the column fires a postback when the control changes
        /// </summary>
        /// <value>A Boolean</value>
        /// -----------------------------------------------------------------------------
        public bool AutoPostBack
        {
            get
            {
                return mAutoPostBack;
            }
            set
            {
                mAutoPostBack = value;
            }
        }

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
        /// changed
        /// </summary>
        /// <value>A Boolean</value>
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
        /// An flag that indicates whether the control is enabled (this is overridden if
        /// the EnabledField is set)
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a DNNMultiStateBoxColumnTemplate
        /// </summary>
        /// <returns>A DNNMultiStateBoxColumnTemplate</returns>
        /// -----------------------------------------------------------------------------
        private DNNMultiStateBoxColumnTemplate CreateTemplate(ListItemType type)
        {
            bool isDesignMode = false;
            if (HttpContext.Current == null)
            {
                isDesignMode = true;
            }
            var template = new DNNMultiStateBoxColumnTemplate(type);
            if (type != ListItemType.Header)
            {
                template.AutoPostBack = AutoPostBack;
            }
            template.DataField = DataField;
            template.Enabled = Enabled;
            template.EnabledField = EnabledField;
            template.ImagePath = ImagePath;
            foreach (DNNMultiState objState in States)
            {
                template.States.Add(objState);
            }
            template.SelectedStateKey = SelectedStateKey;
            if (type == ListItemType.Header)
            {
                template.Text = HeaderText;
                template.AutoPostBack = true;
            }
            template.DesignMode = isDesignMode;
            return template;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialises the Column
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Initialize()
        {
            ItemTemplate = CreateTemplate(ListItemType.Item);
            EditItemTemplate = CreateTemplate(ListItemType.EditItem);
            HeaderTemplate = CreateTemplate(ListItemType.Header);
            if (HttpContext.Current == null)
            {
                HeaderStyle.Font.Names = new[] {"Tahoma, Verdana, Arial"};
                HeaderStyle.Font.Size = new FontUnit("10pt");
                HeaderStyle.Font.Bold = true;
            }
            ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        }
    }
}
