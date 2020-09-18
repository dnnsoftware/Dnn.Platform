// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components
{
    using System.Collections.Generic;
    using System.Text;

    using Dnn.PersonaBar.Recyclebin.Components.Dto;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    public interface IRecyclebinController
    {
        string LocalizeString(string key);

        string GetTabStatus(TabInfo tab);

        void DeleteTabs(IEnumerable<PageItem> tabs, StringBuilder errors, bool deleteDescendants = false);

        void DeleteTabs(IEnumerable<TabInfo> tabs, StringBuilder errors, bool deleteDescendants = false);

        void DeleteModules(IEnumerable<ModuleItem> modules, StringBuilder errors);

        void DeleteModules(IEnumerable<ModuleInfo> modules, StringBuilder errors);

        void DeleteUsers(IEnumerable<UserItem> users);

        void DeleteUsers(IEnumerable<UserInfo> users);

        bool RestoreTab(TabInfo tab, out string resultmessage);

        bool RestoreModule(int moduleId, int tabId, out string errorMessage);

        bool RestoreUser(UserInfo user, out string resultmessage);

        List<TabInfo> GetDeletedTabs(out int totalRecords, int pageIndex = -1, int pageSize = -1);

        List<ModuleInfo> GetDeletedModules(out int totalRecords, int pageIndex = -1, int pageSize = -1);

        List<UserInfo> GetDeletedUsers(out int totalRecords, int pageIndex = -1, int pageSize = -1);
    }
}
