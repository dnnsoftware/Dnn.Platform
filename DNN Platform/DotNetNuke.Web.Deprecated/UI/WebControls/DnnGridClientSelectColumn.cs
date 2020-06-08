// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridClientSelectColumn : GridClientSelectColumn
    {
        #region "Public Properties"

        public string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(Owner.OwnerGrid.Parent);
            }
        }

        #endregion

        #region "Public Methods"

        public override GridColumn Clone()
        {
            var dnnGridColumn = new DnnGridClientSelectColumn();

            //you should override CopyBaseProperties if you have some column specific properties
            dnnGridColumn.CopyBaseProperties(this);

            return dnnGridColumn;
        }

        public override void InitializeCell(TableCell cell, int columnIndex, GridItem inItem)
        {
            base.InitializeCell(cell, columnIndex, inItem);
            if (inItem is GridHeaderItem && !String.IsNullOrEmpty(HeaderText))
            {
                if (! inItem.OwnerTableView.OwnerGrid.AllowMultiRowSelection)
                {
                    cell.Text = Localization.GetString(string.Format("{0}.Header", HeaderText), LocalResourceFile);
                }
            }
        }

        #endregion
    }
}
