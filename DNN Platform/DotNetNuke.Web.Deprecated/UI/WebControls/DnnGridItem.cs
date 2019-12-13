// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridItem : GridItem
    {
        public DnnGridItem(GridTableView ownerTableView, int itemIndex, int dataSetIndex, GridItemType itemType) : base(ownerTableView, itemIndex, dataSetIndex, itemType)
        {
        }
    }
}
