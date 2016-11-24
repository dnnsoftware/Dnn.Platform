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

        bool CanManagePage(TabInfo tab);

        bool CanDeletePage(TabInfo tab);

        bool CanAdminPage(TabInfo tab);

        bool CanAddPage(TabInfo tab);

        bool CanCopyPage(TabInfo tab);

        bool CanExportPage(TabInfo tab);
    }
}
