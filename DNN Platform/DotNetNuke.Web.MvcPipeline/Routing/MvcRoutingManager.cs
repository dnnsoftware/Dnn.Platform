// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    public sealed class MvcRoutingManager: IRoutingManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MvcRoutingManager));
        private readonly RouteCollection routes;

        public MvcRoutingManager()
            : this(RouteTable.Routes)
        {
        }

        internal MvcRoutingManager(RouteCollection routes)
        {
            this.routes = routes;
        }

        public void RegisterRoutes()
        {
            // add standard tab and module id provider
            GlobalConfiguration.Configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            using (this.routes.GetWriteLock())
            {
                // routes.Clear(); -- don't use; it will remove original WEP API maps
                this.LocateServicesAndMapRoutes();
            }

            Logger.TraceFormat("Registered a total of {0} routes", this.routes.Count);
        }

        private static bool IsTracingEnabled()
        {
            var configValue = Config.GetSetting("EnableServicesFrameworkTracing");

            return !string.IsNullOrEmpty(configValue) && Convert.ToBoolean(configValue);
        }

        private void RegisterSystemRoutes()
        {
            var route = new Route(
                "DesktopModules/{controller}/{action}/{tabid}/{language}",
                new RouteValueDictionary(new { action = "Index", tabid = UrlParameter.Optional, language = UrlParameter.Optional }),
                new DnnMvcPageRouteHandler());

            // route.DataTokens = new RouteValueDictionary();
            // ConstraintValidation.Validate(route);
            // route.SetNameSpaces(new string[] { "DotNetNuke.Framework.Controllers" });
            // route.SetName("Default");
            this.routes.Add(route);
        }

        private void LocateServicesAndMapRoutes()
        {
            this.RegisterSystemRoutes();
        }
    }
}
