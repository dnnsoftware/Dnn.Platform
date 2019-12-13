// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("DigitalAssets", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.DigitalAssets.Services" });
        }
    }
}
