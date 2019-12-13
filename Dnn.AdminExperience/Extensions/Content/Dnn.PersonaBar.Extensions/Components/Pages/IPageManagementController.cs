using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IPageManagementController
    {
        string GetCreatedInfo(TabInfo tab);

        string GetTabHierarchy(TabInfo tab);

        string GetTabUrl(TabInfo tab);

        /// <summary>
        /// Returns true if tab has children, false otherwise
        /// </summary>
        /// <param name="tabInfo">Tab info object</param>
        /// <returns>Returns true if tab has children, false otherwise</returns>
        bool TabHasChildren(TabInfo tabInfo);
    }
}
