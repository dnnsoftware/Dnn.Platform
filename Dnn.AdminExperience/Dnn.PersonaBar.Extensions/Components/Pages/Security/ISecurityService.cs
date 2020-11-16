// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Security
{
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Entities.Tabs;
    using Newtonsoft.Json.Linq;

    public interface ISecurityService
    {
        bool IsVisible(MenuItem menuItem);

        JObject GetCurrentPagePermissions();

        JObject GetPagePermissions(TabInfo tab);

        bool IsPageAdminUser();

        bool CanManagePage(int tabId);

        bool CanDeletePage(int tabId);

        bool CanAdminPage(int tabId);

        bool CanAddPage(int tabId);

        bool CanCopyPage(int tabId);

        bool CanExportPage(int tabId);

        bool CanViewPageList(int menuId);

        bool CanSavePageDetails(PageSettings pageSettings);

        bool IsAdminHostSystemPage();
    }
}
