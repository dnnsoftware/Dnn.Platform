using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.UI.Components.Controllers
{
    public interface IAdminMenuController
    {
        void CreateLinkMenu(TabInfo tab);
        void DeleteLinkMenu(TabInfo tab);
    }
}
