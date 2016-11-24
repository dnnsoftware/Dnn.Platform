using Dnn.PersonaBar.UI.Components.Controllers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.UI.Components
{
    public class BusinessController : IUpgradeable
    {
        public string UpgradeModule(string version)
        {
            switch (version)
            {
                case "01.00.00":
                    UpdateControlPanel();
                    CreateAdminLinks();
                    break;
            }

            return "Success";
        }

        private void CreateAdminLinks()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                CreatePageLinks(portal.PortalID, "Admin");
            }

            CreatePageLinks(Null.NullInteger, "Host");
        }

        private void CreatePageLinks(int portalId, string parentPath)
        {
            var parentTab = TabController.GetTabByTabPath(portalId, "//" + parentPath, string.Empty);
            if (parentTab == Null.NullInteger)
            {
                return;
            }

            var adminTabs = TabController.GetTabsByParent(parentTab, portalId);
            foreach (var tab in adminTabs)
            {
                AdminMenuController.Instance.CreateLinkMenu(tab);
            }
        }

        private void UpdateControlPanel()
        {
            HostController.Instance.Update("ControlPanel", "DesktopModules/admin/Dnn.PersonaBar/UserControls/PersonaBarContainer.ascx");
        }
    }
}
