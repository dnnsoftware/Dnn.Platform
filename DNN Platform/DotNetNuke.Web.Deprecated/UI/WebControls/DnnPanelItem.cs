#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnPanelItem : RadPanelItem
    {
        public DnnPanelItem()
        {
        }

        public DnnPanelItem(string text) : base(text)
        {
        }

        public DnnPanelItem(string text, string navigateUrl) : base(text, navigateUrl)
        {
        }
    }
}
