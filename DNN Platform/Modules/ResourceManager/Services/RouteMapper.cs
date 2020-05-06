// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.Api;

namespace Dnn.Modules.ResourceManager.Services
{
    public class RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("ResourceManager", "default", "{controller}/{action}", new[] { "Dnn.Modules.ResourceManager.Services" });
        }
    }
}