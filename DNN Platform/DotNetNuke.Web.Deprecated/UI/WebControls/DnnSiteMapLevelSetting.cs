#region Usings

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnSiteMapLevelSetting : SiteMapLevelSetting
    {
        public DnnSiteMapLevelSetting()
        {
        }

        public DnnSiteMapLevelSetting(int level) : base(level)
        {
        }

        public DnnSiteMapLevelSetting(int level, SiteMapLayout layout) : base(level, layout)
        {
        }

        public DnnSiteMapLevelSetting(SiteMapLayout layout) : base(layout)
        {
        }
    }
}
