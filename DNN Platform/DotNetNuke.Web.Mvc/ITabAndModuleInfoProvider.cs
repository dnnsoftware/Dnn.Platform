// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvc
{
    public interface ITabAndModuleInfoProvider
    {
        bool TryFindTabId(HttpRequestBase request, out int tabId);
        bool TryFindModuleId(HttpRequestBase request, out int moduleId);
        bool TryFindModuleInfo(HttpRequestBase request, out ModuleInfo moduleInfo);
    }
}
