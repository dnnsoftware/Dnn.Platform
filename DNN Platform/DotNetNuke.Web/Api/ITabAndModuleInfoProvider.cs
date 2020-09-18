// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Net.Http;

    using DotNetNuke.Entities.Modules;

    public interface ITabAndModuleInfoProvider
    {
        bool TryFindTabId(HttpRequestMessage request, out int tabId);

        bool TryFindModuleId(HttpRequestMessage request, out int moduleId);

        bool TryFindModuleInfo(HttpRequestMessage request, out ModuleInfo moduleInfo);
    }
}
