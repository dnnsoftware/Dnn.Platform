﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
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

        List<TabInfo> GetDeletedTabs(out int totalRecords, int pageIndex = -1, int pageSize = -1);

        List<ModuleInfo> GetDeletedModules(out int totalRecords, int pageIndex = -1, int pageSize = -1);

        List<UserInfo> GetDeletedUsers(out int totalRecords, int pageIndex = -1, int pageSize = -1);
    }
}