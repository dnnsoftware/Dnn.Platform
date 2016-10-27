using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IPagesController
    {
        bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage);

        PageSettings GetPageDetails(int pageId);
    }
}