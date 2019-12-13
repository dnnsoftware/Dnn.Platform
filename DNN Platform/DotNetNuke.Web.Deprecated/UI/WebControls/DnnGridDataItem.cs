#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridDataItem : GridDataItem
    {
        public DnnGridDataItem(GridTableView ownerTableView, int itemIndex, int dataSetIndex) : base(ownerTableView, itemIndex, dataSetIndex)
        {
        }

        public DnnGridDataItem(GridTableView ownerTableView, int itemIndex, int dataSetIndex, GridItemType itemType) : base(ownerTableView, itemIndex, dataSetIndex, itemType)
        {
        }
    }
}
