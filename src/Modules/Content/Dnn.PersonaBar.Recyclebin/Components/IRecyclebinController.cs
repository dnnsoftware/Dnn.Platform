﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Recyclebin.Components.Dto;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Recyclebin.Components
{
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

        List<TabInfo> GetDeletedTabs();

        List<ModuleInfo> GetDeletedModules();

        List<UserInfo> GetDeletedUsers();
    }
}