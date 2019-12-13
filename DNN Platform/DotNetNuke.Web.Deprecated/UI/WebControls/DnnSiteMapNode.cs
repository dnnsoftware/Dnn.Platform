#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnSiteMapNode : RadSiteMapNode
    {
        public DnnSiteMapNode()
        {
        }

        public DnnSiteMapNode(string text, string navigateUrl) : base(text, navigateUrl)
        {
        }
    }
}
