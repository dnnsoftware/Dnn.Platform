// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
