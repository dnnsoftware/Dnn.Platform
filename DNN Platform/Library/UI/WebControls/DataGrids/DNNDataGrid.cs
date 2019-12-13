#region Usings

using System;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.UI.WebControls
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.UI.WebControls
    /// Class:      DNNDataGrid
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNDataGrid control provides an Enhanced Data Grid, that supports other
    /// column types
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DNNDataGrid : DataGrid
    {
		#region "Events"

        public event DNNDataGridCheckedColumnEventHandler ItemCheckedChanged;

		#endregion

		#region "Private Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Centralised Event that is raised whenever a check box is changed.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void OnItemCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
            if (ItemCheckedChanged != null)
            {
                ItemCheckedChanged(sender, e);
            }
        }

		#endregion

		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Called when the grid is Data Bound
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnDataBinding(EventArgs e)
        {
            foreach (DataGridColumn column in Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof (CheckBoxColumn)))
                {
					//Manage CheckBox column events
                    var cbColumn = (CheckBoxColumn) column;
                    cbColumn.CheckedChanged += OnItemCheckedChanged;
                }
            }
        }

        protected override void CreateControlHierarchy(bool useDataSource)
        {
            base.CreateControlHierarchy(useDataSource);
        }

        protected override void PrepareControlHierarchy()
        {
            base.PrepareControlHierarchy();
        }
		
		#endregion
    }
}
