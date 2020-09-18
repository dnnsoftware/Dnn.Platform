// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;

    using Telerik.Web.UI;

    public class DnnGridItemSelectedEventArgs : EventArgs
    {
        private readonly GridItemCollection _SelectedItems;

        public DnnGridItemSelectedEventArgs(GridItemCollection selectedItems)
        {
            this._SelectedItems = selectedItems;
        }

        public GridItemCollection SelectedItems
        {
            get
            {
                return this._SelectedItems;
            }
        }
    }
}
