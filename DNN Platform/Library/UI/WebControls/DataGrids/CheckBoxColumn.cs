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
    /// Class:      CheckBoxColumn
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CheckBoxColumn control provides a Check Box column for a Data Grid.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class CheckBoxColumn : TemplateColumn
    {
        private bool mAutoPostBack = true;
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;
        private bool mHeaderCheckBox = true;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxColumn"/> class.
        /// Constructs the CheckBoxColumn.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public CheckBoxColumn()
            : this(false)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxColumn"/> class.
        /// Constructs the CheckBoxColumn, with an optional AutoPostBack (where each change
        /// of state of a check box causes a Post Back).
        /// </summary>
        /// <param name="autoPostBack">Optional set the checkboxes to postback.</param>
        /// -----------------------------------------------------------------------------
        public CheckBoxColumn(bool autoPostBack)
        {
            this.AutoPostBack = autoPostBack;
        }

        public event DNNDataGridCheckedColumnEventHandler CheckedChanged;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the column fires a postback when any check box is
        /// changed.
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
        /// Gets or sets a value indicating whether gets and sets whether the checkbox is checked (unless DataBound).
        /// </summary>
        /// <value>A Boolean.</value>
        /// -----------------------------------------------------------------------------
        public bool Checked { get; set; }

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
        /// Gets or sets a value indicating whether an flag that indicates whether the checkboxes are enabled (this is overridden if
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
        /// Gets or sets the Data Field that determines whether the checkbox is Enabled
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
        /// Gets or sets a value indicating whether a flag that indicates whether there is a checkbox in the Header that sets all
        /// the checkboxes.
        /// </summary>
        /// <value>A Boolean.</value>
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
        /// Creates a CheckBoxColumnTemplate.
        /// </summary>
        /// <returns>A CheckBoxColumnTemplate.</returns>
        /// -----------------------------------------------------------------------------
        private CheckBoxColumnTemplate CreateTemplate(ListItemType type)
        {
            bool isDesignMode = false;
            if (HttpContext.Current == null)
            {
                isDesignMode = true;
            }

            var template = new CheckBoxColumnTemplate(type);
            if (type != ListItemType.Header)
            {
                template.AutoPostBack = this.AutoPostBack;
            }

            template.Checked = this.Checked;
            template.DataField = this.DataField;
            template.Enabled = this.Enabled;
            template.EnabledField = this.EnabledField;
            template.CheckedChanged += this.OnCheckedChanged;
            if (type == ListItemType.Header)
            {
                template.Text = this.HeaderText;
                template.AutoPostBack = true;
                template.HeaderCheckBox = this.HeaderCheckBox;
            }

            template.DesignMode = isDesignMode;
            return template;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Centralised Event that is raised whenever a check box is changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void OnCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
            // Add the column to the Event Args
            e.Column = this;
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(sender, e);
            }
        }
    }
}
