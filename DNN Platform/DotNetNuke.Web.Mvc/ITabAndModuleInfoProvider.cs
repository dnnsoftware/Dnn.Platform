﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Web;

    using DotNetNuke.Entities.Modules;

    public interface ITabAndModuleInfoProvider
    {
        bool TryFindTabId(HttpRequestBase request, out int tabId);

        bool TryFindModuleId(HttpRequestBase request, out int moduleId);

        bool TryFindModuleInfo(HttpRequestBase request, out ModuleInfo moduleInfo);
    }
}
