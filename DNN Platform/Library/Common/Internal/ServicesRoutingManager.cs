// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using System;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Manages http routes for services (WebAPI, MVC, etc).</summary>
    public static class ServicesRoutingManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesRoutingManager));

        /// <summary>Registers all the service routes.</summary>
        public static void RegisterServiceRoutes()
        {
            const string unableToRegisterServiceRoutes = "Unable to register service routes";

            try
            {
                foreach (IRoutingManager routingManager in Globals.GetCurrentServiceProvider().GetServices(typeof(IRoutingManager)))
                {
                    routingManager.RegisterRoutes();
                }
            }
            catch (Exception e)
            {
                Logger.Error(unableToRegisterServiceRoutes, e);
            }
        }

        /// <summary>Re-registers all the routes while the site is still running.</summary>
        public static void ReRegisterServiceRoutesWhileSiteIsRunning()
        {
            // by clearing a "fake" key on the caching provider we can echo this
            // command to all the members of a web farm
            // the caching provider will call to make the actual registration of new routes
            CachingProvider.Instance().Clear("ServiceFrameworkRoutes", "-1");
        }
    }
}
