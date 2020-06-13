// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;

    public static class ServicesRoutingManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesRoutingManager));

        public static void RegisterServiceRoutes()
        {
            const string unableToRegisterServiceRoutes = "Unable to register service routes";

            try
            {
                // new ServicesRoutingManager().RegisterRoutes();
                var instance = Activator.CreateInstance("DotNetNuke.Web", "DotNetNuke.Web.Api.Internal.ServicesRoutingManager");

                var method = instance.Unwrap().GetType().GetMethod("RegisterRoutes");
                method.Invoke(instance.Unwrap(), new object[0]);

                var instanceMvc = Activator.CreateInstance("DotNetNuke.Web.Mvc", "DotNetNuke.Web.Mvc.Routing.MvcRoutingManager");

                var methodMvc = instanceMvc.Unwrap().GetType().GetMethod("RegisterRoutes");
                methodMvc.Invoke(instanceMvc.Unwrap(), new object[0]);
            }
            catch (Exception e)
            {
                Logger.Error(unableToRegisterServiceRoutes, e);
            }
        }

        public static void ReRegisterServiceRoutesWhileSiteIsRunning()
        {
            // by clearing a "fake" key on the caching provider we can echo this
            // command to all the members of a web farm
            // the caching provider will call to make the actual registration of new routes
            CachingProvider.Instance().Clear("ServiceFrameworkRoutes", "-1");
        }
    }
}
