#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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