using System.Collections;
using System.Collections.Generic;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IPagesController
    {
        bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage);

        TabInfo GetPageDetails(int pageId);
        
        IEnumerable<ModuleInfo> GetModules(int pageId);
        PagePermissions GetPermissionsData(int pageId);
        bool ValidatePageSettingsData(PageSettings pageSettings, TabInfo tab, out string invalidField, out string errorMessage);
        int AddTab(PageSettings pageSettings);
        int UpdateTab(TabInfo tab, PageSettings pageSettings);
    }
}