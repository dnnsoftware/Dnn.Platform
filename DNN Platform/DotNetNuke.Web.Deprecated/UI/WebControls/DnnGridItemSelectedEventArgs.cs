﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this._SelectedItems = selectedItems;
        }

        #endregion

        #region "Public Properties"

        public GridItemCollection SelectedItems
        {
            get
            {
                return this._SelectedItems;
            }
        }

        #endregion
    }
}
