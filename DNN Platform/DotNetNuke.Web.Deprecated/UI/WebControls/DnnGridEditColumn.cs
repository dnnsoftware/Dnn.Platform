// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridEditColumn : GridEditCommandColumn
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
            DnnGridEditColumn dnnGridColumn = new DnnGridEditColumn();

            //you should override CopyBaseProperties if you have some column specific properties
            dnnGridColumn.CopyBaseProperties(this);

            return dnnGridColumn;
        }

        #endregion
    }
}
