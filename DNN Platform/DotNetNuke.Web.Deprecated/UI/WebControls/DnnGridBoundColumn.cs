// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridBoundColumn : GridBoundColumn
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
            DnnGridBoundColumn dnnGridColumn = new DnnGridBoundColumn();

            //you should override CopyBaseProperties if you have some column specific properties
            dnnGridColumn.CopyBaseProperties(this);

            return dnnGridColumn;
        }

        public override void InitializeCell(TableCell cell, int columnIndex, GridItem inItem)
        {
            base.InitializeCell(cell, columnIndex, inItem);
            if (inItem is GridHeaderItem && !String.IsNullOrEmpty(HeaderText))
            {
                GridHeaderItem headerItem = inItem as GridHeaderItem;
                string columnName = DataField;
                if (!Owner.AllowSorting)
                {
                    cell.Text = Localization.GetString(string.Format("{0}.Header", HeaderText), LocalResourceFile);
                }
                else
                {
                    LinkButton button = (LinkButton) headerItem[columnName].Controls[0];
                    button.Text = Localization.GetString(string.Format("{0}.Header", HeaderText), LocalResourceFile);
                }
            }
        }

        #endregion
    }
}
