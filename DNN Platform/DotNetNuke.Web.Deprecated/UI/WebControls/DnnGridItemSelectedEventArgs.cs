// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
