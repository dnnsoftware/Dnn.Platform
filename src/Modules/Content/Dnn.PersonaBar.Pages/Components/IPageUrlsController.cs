using System.Collections.Generic;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IPageUrlsController
    {
        IEnumerable<Url> GetPageUrls(TabInfo tab, int portalId);
    }
}