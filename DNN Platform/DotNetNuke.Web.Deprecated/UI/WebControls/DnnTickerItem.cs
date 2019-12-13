#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTickerItem : RadTickerItem
    {
        public DnnTickerItem()
        {
        }

        public DnnTickerItem(string text) : base(text)
        {
        }

        public DnnTickerItem(string text, string navigateUrl) : base(text, navigateUrl)
        {
        }
    }
}
