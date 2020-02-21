// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Pages.Services.Dto;
using DotNetNuke.Entities.Tabs;
using Newtonsoft.Json.Linq;

namespace Dnn.PersonaBar.Pages.Components.Security
{
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
