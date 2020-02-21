// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.WebControls
{
    public delegate void DNNDataGridCheckedColumnEventHandler(object sender, DNNDataGridCheckChangedEventArgs e);

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNDataGrid
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNDataGridCheckChangedEventArgs class is a cusom EventArgs class for
    /// handling Event Args from the CheckChanged event in a CheckBox Column
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DNNDataGridCheckChangedEventArgs : DataGridItemEventArgs
    {
        public DNNDataGridCheckChangedEventArgs(DataGridItem item, bool isChecked, string field) : this(item, isChecked, field, false)
        {
        }

        public DNNDataGridCheckChangedEventArgs(DataGridItem item, bool isChecked, string field, bool isAll) : base(item)
        {
            Checked = isChecked;
            IsAll = isAll;
            Field = field;
        }

        public bool Checked { get; set; }

        public CheckBoxColumn Column { get; set; }

        public string Field { get; set; }

        public bool IsAll { get; set; }
    }
}
