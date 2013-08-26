// DotNetNuke® - http://www.dotnetnuke.com
//
// Copyright (c) 2002-2013 DotNetNuke Corporation
// All rights reserved.

using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.SubscriptionsMgmt.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        #region Implementation of IServiceRouteMapper

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("SubscriptionsMgmt",
                                         "default",
                                         "{controller}/{action}",
										 new[] { "DotNetNuke.Modules.SubscriptionsMgmt.Services" });
        }

        #endregion
    }
}
