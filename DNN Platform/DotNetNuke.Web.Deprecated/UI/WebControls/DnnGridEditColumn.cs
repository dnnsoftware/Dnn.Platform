// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using Telerik.Web.UI;

    public class DnnGridEditColumn : GridEditCommandColumn
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
            DnnGridEditColumn dnnGridColumn = new DnnGridEditColumn();

            // you should override CopyBaseProperties if you have some column specific properties
            dnnGridColumn.CopyBaseProperties(this);

            return dnnGridColumn;
        }
    }
}
