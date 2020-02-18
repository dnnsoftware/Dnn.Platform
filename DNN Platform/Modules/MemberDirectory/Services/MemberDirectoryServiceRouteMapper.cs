// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    public class MemberDirectoryServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("MemberDirectory", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.MemberDirectory.Services" });
        }
    }
}
