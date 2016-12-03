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


        //void HardDeleteTab(TabInfo tab, bool deleteDescendants);

        //void HardDeleteModule(ModuleInfo module);

        bool RestoreTab(TabInfo tab, out string resultmessage);

        bool RestoreModule(int moduleId, int tabId, out string errorMessage);

        List<TabInfo> GetDeletedTabs();

        List<ModuleInfo> GetDeletedModules();
    }
}