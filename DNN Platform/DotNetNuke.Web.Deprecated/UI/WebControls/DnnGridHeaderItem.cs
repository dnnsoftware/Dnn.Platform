#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridHeaderItem : GridHeaderItem
    {
        public DnnGridHeaderItem(GridTableView ownerTableView, int itemIndex, int dataSetIndex) : base(ownerTableView, itemIndex, dataSetIndex)
        {
        }
    }
}
