using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnFormPagesItem : DnnFormComboBoxItem
    {
        public DnnFormPagesItem()
        {
            ListSource = TabController.GetPortalTabs(PortalSettings.PortalId, Null.NullInteger, true, "<" + Localization.GetString("None_Specified") + ">", true, false, true, true, false);
            ListTextField = "TabName";
            ListValueField = "TabID";
        }
    }
}
