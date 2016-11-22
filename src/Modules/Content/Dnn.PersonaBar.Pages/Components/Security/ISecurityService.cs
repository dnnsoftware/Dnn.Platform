using Dnn.PersonaBar.Library.Model;
using Newtonsoft.Json.Linq;

namespace Dnn.PersonaBar.Pages.Components.Security
{
    public interface ISecurityService
    {
        bool IsVisible(MenuItem menuItem);

        JObject GetCurrentPagePermissions();
        bool IsSuperUser();
    }
}
