// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI.WebControls;

    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public delegate void DNNDataGridCheckedColumnEventHandler(object sender, DNNDataGridCheckChangedEventArgs e);

    /// <summary>
    /// The DNNDataGridCheckChangedEventArgs class is a custom EventArgs class for
    /// handling Event Args from the CheckChanged event in a CheckBox Column.
    /// </summary>
    public class DNNDataGridCheckChangedEventArgs : DataGridItemEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="DNNDataGridCheckChangedEventArgs"/> class.</summary>
        /// <param name="item">The data grid item.</param>
        /// <param name="isChecked">Whether it's checked.</param>
        /// <param name="field">The field name.</param>
        public DNNDataGridCheckChangedEventArgs(DataGridItem item, bool isChecked, string field)
            : this(item, isChecked, field, false)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DNNDataGridCheckChangedEventArgs"/> class.</summary>
        /// <param name="item">The data grid item.</param>
        /// <param name="isChecked">Whether it's checked.</param>
        /// <param name="field">The field name.</param>
        /// <param name="isAll">Whether this is the "All" item for the column.</param>
        public DNNDataGridCheckChangedEventArgs(DataGridItem item, bool isChecked, string field, bool isAll)
            : base(item)
        {
            this.Checked = isChecked;
            this.IsAll = isAll;
            this.Field = field;
        }

        public bool Checked { get; set; }

        public CheckBoxColumn Column { get; set; }

        public string Field { get; set; }

        public bool IsAll { get; set; }
    }
}
