// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Using Statements

using DotNetNuke.Web.Api;

#endregion

namespace _OWNER_._MODULE_
{
    public class _MODULE_RouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("_OWNER_._MODULE_", "default", "{controller}/{action}", new[] {"_OWNER_._MODULE_"});
        }
    }
} 
