// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNMultiStateBoxColumn
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNMultiStateBoxColumn control provides a DNNMultiState Box column for a Data Grid.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class DNNMultiStateBoxColumn : TemplateColumn
    {
        private bool mAutoPostBack = true;
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;
        private string mImagePath = string.Empty;
        private string mSelectedStateKey = string.Empty;
        private DNNMultiStateCollection mStates;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DNNMultiStateBoxColumn"/> class.
        /// Constructs the DNNMultiStateBoxColumn.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public DNNMultiStateBoxColumn()
            : this(false)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DNNMultiStateBoxColumn"/> class.
        /// Constructs the MultiStateBoxColumn, with an optional AutoPostBack (where each change
        /// of state of the control causes a Post Back).
        /// </summary>
        /// <param name="autoPostBack">Optional set the control to postback.</param>
        /// -----------------------------------------------------------------------------
        public DNNMultiStateBoxColumn(bool autoPostBack)
        {
            this.AutoPostBack = autoPostBack;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the column fires a postback when the control changes.
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool AutoPostBack
        {
            get
            {
                return this.mAutoPostBack;
            }

            set
            {
                this.mAutoPostBack = value;
            }
        }

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
        /// Gets or sets the Data Field that the column should bind to
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
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
        /// Gets or sets a value indicating whether an flag that indicates whether the control is enabled (this is overridden if
        /// the EnabledField is set)
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
        /// Initialises the Column.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Initialize()
        {
            this.ItemTemplate = this.CreateTemplate(ListItemType.Item);
            this.EditItemTemplate = this.CreateTemplate(ListItemType.EditItem);
            this.HeaderTemplate = this.CreateTemplate(ListItemType.Header);
            if (HttpContext.Current == null)
            {
                this.HeaderStyle.Font.Names = new[] { "Tahoma, Verdana, Arial" };
                this.HeaderStyle.Font.Size = new FontUnit("10pt");
                this.HeaderStyle.Font.Bold = true;
            }

            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a DNNMultiStateBoxColumnTemplate.
        /// </summary>
        /// <returns>A DNNMultiStateBoxColumnTemplate.</returns>
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
                template.AutoPostBack = this.AutoPostBack;
            }

            template.DataField = this.DataField;
            template.Enabled = this.Enabled;
            template.EnabledField = this.EnabledField;
            template.ImagePath = this.ImagePath;
            foreach (DNNMultiState objState in this.States)
            {
                template.States.Add(objState);
            }

            template.SelectedStateKey = this.SelectedStateKey;
            if (type == ListItemType.Header)
            {
                template.Text = this.HeaderText;
                template.AutoPostBack = true;
            }

            template.DesignMode = isDesignMode;
            return template;
        }
    }
}
