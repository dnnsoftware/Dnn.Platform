// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
