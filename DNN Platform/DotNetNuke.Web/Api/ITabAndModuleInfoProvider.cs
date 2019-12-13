// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Net.Http;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Api
{
    public interface ITabAndModuleInfoProvider
    {
        bool TryFindTabId(HttpRequestMessage request, out int tabId);
        bool TryFindModuleId(HttpRequestMessage request, out int moduleId);
        bool TryFindModuleInfo(HttpRequestMessage request, out ModuleInfo moduleInfo);
    }
}
