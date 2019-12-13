// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Web.Api;

namespace Dnn.AzureConnector.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute("AzureConnector",
                                      "default",
                                      "{controller}/{action}",
                                      new[] { "Dnn.AzureConnector.Services" });
        }
    }
}
