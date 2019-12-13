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
