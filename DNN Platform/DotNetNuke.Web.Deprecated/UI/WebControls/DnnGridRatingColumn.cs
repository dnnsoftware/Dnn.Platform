// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;
    using Telerik.Web.UI;

    public class DnnGridRatingColumn : GridRatingColumn
    {
        public string LocalResourceFile
        {
            get
            {
                return Utilities.GetLocalResourceFile(this.Owner.OwnerGrid.Parent);
            }
        }

        public override GridColumn Clone()
        {
            DnnGridRatingColumn dnnGridColumn = new DnnGridRatingColumn();

            // you should override CopyBaseProperties if you have some column specific properties
            dnnGridColumn.CopyBaseProperties(this);

            return dnnGridColumn;
        }

        public override void InitializeCell(TableCell cell, int columnIndex, GridItem inItem)
        {
            base.InitializeCell(cell, columnIndex, inItem);
            if (inItem is GridHeaderItem && !string.IsNullOrEmpty(this.HeaderText))
            {
                cell.Text = Localization.GetString(string.Format("{0}.Header", this.HeaderText), this.LocalResourceFile);
            }
        }
    }
}
