#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTagCloud : RadTagCloud
    {
        protected void OnItemDataBound(DnnTagCloudItem item)
        {
            base.OnItemDataBound(item);
        }
    }
}
