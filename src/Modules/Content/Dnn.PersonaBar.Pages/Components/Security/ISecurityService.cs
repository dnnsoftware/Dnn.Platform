using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Tabs;
using Newtonsoft.Json.Linq;

namespace Dnn.PersonaBar.Pages.Components.Security
{
    public interface ISecurityService
    {
        bool IsVisible(MenuItem menuItem);

        JObject GetCurrentPagePermissions();

        bool IsPageAdminUser();

        bool IsUserAllowed(string permission);

        bool CanManagePage(TabInfo tab);
        bool CanDeletePage(TabInfo getTabById);
        bool CanAdminPage(TabInfo getTabById);
    }
}
