using System.Collections.Generic;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IPagesController
    {
        bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage);
        
        IEnumerable<TabInfo> GetPageList(int parentId = -1, string searchKey = "");

        TabInfo GetPageDetails(int pageId);

        List<int> GetPageHierarchy(int pageId);

        TabInfo MovePage(PageMoveRequest request);

        void DeletePage(PageItem page);

        void EditModeForPage(int pageId, int userId);

        TabInfo SavePageDetails(PageSettings pageSettings);

        IEnumerable<ModuleInfo> GetModules(int pageId);

        PagePermissions GetPermissionsData(int pageId);

        void DeleteTabModule(int pageId, int moduleId);

        /// <summary>
        /// Returns a clean tab relative url based on Advanced Management Url settings
        /// </summary>
        /// <param name="url">Url not cleaned, this could containes blank space or invalid characters</param>
        /// <returns>Cleaned Url</returns>
        string CleanTabUrl(string url);
    }
}