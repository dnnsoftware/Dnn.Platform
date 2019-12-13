#region Usings

using System;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridItemSelectedEventArgs : EventArgs
    {
        private readonly GridItemCollection _SelectedItems;

        #region "Constructors"

        public DnnGridItemSelectedEventArgs(GridItemCollection selectedItems)
        {
            _SelectedItems = selectedItems;
        }

        #endregion

        #region "Public Properties"

        public GridItemCollection SelectedItems
        {
            get
            {
                return _SelectedItems;
            }
        }

        #endregion
    }
}
