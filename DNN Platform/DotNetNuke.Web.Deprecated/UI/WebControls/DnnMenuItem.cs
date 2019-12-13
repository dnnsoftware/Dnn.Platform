#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnMenuItem : RadMenuItem
    {
        public DnnMenuItem()
        {
        }

        public DnnMenuItem(string text) : base(text)
        {
        }

        public DnnMenuItem(string text, string navigateUrl) : base(text, navigateUrl)
        {
        }
    }
}
