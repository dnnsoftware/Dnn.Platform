// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.Mvc.Routing;

namespace Dnn.ContactList.Mvc
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute("ContactList", "ContactList", "{controller}/{action}", new[]
            {"Dnn.ContactList.Mvc.Controllers"});
        }
    }
}
