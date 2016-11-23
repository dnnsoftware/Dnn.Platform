using System.Collections.Generic;
using Dnn.PersonaBar.Pages.Components.Dto;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IPageUrlsController
    {
        IEnumerable<Url> GetPageUrls(TabInfo tab, int portalId);
        PageUrlResult CreateCustomUrl(SaveUrlDto dto, TabInfo tab);
        PageUrlResult UpdateCustomUrl(SaveUrlDto dto, TabInfo tab);
        PageUrlResult DeleteCustomUrl(int id, TabInfo tab);
    }
}