// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.Groups 
{
    public class ServiceRouteMapper : IServiceRouteMapper 
    {
        public void RegisterRoutes(IMapRoute mapRouteManager) 
        {
            mapRouteManager.MapHttpRoute("SocialGroups", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.Groups" });
        }
    }
}
