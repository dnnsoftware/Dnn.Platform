// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.Journal
{
    public class JournalRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Journal", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.Journal" });
        }
    }
}
