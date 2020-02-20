// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    public sealed class CoreMessagingRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("CoreMessaging", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.CoreMessaging.Services" });
        }
    }
}
