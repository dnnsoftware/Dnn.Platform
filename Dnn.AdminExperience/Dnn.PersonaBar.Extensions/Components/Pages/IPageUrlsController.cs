// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;

    public interface IPageUrlsController
    {
        IEnumerable<Url> GetPageUrls(TabInfo tab, int portalId);
        PageUrlResult CreateCustomUrl(SaveUrlDto dto, TabInfo tab);
        PageUrlResult UpdateCustomUrl(SaveUrlDto dto, TabInfo tab);
        PageUrlResult DeleteCustomUrl(int id, TabInfo tab);
    }
}
