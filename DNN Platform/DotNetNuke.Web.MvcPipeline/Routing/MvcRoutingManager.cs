// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Web.Configuration;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Commons;

    /// <summary>
    /// Provides routing registration and mapping for DNN MVC modules and pages.
    /// </summary>
    public sealed class MvcRoutingManager : IRoutingManager, IMapRoute
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MvcRoutingManager));
        private readonly Dictionary<string, int> moduleUsage = new Dictionary<string, int>();
        private readonly RouteCollection routes;
        private readonly PortalAliasMvcRouteManager portalAliasMvcRouteManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcRoutingManager"/> class.
        /// </summary>
        public MvcRoutingManager()
            : this(RouteTable.Routes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcRoutingManager"/> class for testing.
        /// </summary>
        /// <param name="routes">The route collection to register routes in.</param>
        internal MvcRoutingManager(RouteCollection routes)
        {
            this.routes = routes;
            this.portalAliasMvcRouteManager = new PortalAliasMvcRouteManager();
            this.TypeLocator = new TypeLocator();
        }

        /// <summary>
        /// Gets or sets the type locator used to discover MVC route mappers.
        /// </summary>
        internal ITypeLocator TypeLocator { get; set; }

        /// <summary>
        /// Maps a route for a module folder using the specified name, URL, and namespaces.
        /// </summary>
        /// <param name="moduleFolderName">The module folder name (area).</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="url">The route URL pattern.</param>
        /// <param name="namespaces">The controller namespaces associated with the route.</param>
        /// <returns>The created <see cref="Route"/>.</returns>
        public Route MapRoute(string moduleFolderName, string routeName, string url, string[] namespaces)
        {
            return this.MapRoute(moduleFolderName, routeName, url, null /* defaults */, null /* constraints */, namespaces);
        }

        /// <inheritdoc/>
        public Route MapRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces)
        {
            return this.MapRoute(moduleFolderName, routeName, url, defaults, null /* constraints */, namespaces);
        }

        /// <inheritdoc/>
        public Route MapRoute(string moduleFolderName, string routeName, string url, object defaults, object constraints, string[] namespaces)
        {
            if (namespaces == null || namespaces.Length == 0 || string.IsNullOrEmpty(namespaces[0]))
            {
                throw new ArgumentException(Localization.GetExceptionMessage(
                    "ArgumentCannotBeNullOrEmpty",
                    "The argument '{0}' cannot be null or empty.",
                    "namespaces"));
            }

            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            url = url.Trim('/', '\\');

            var prefixCounts = this.portalAliasMvcRouteManager.GetRoutePrefixCounts();
            Route route = null;

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            foreach (var count in prefixCounts)
            {
                var fullRouteName = this.portalAliasMvcRouteManager.GetRouteName(moduleFolderName, routeName, count);
                var routeUrl = this.portalAliasMvcRouteManager.GetRouteUrl(moduleFolderName, url, count);
                route = MapRouteWithNamespace(fullRouteName, moduleFolderName, routeUrl, defaults, constraints, namespaces);
                this.routes.Add(route);
                Logger.Trace("Mapping route: " + fullRouteName + " Area=" + moduleFolderName + " @ " + routeUrl);
            }

            return route;
        }

        /// <inheritdoc/>
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

        internal static bool IsValidServiceRouteMapper(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IMvcRouteMapper).IsAssignableFrom(t);
        }

        private static bool IsTracingEnabled()
        {
            var configValue = Config.GetSetting("EnableServicesFrameworkTracing");

            return !string.IsNullOrEmpty(configValue) && Convert.ToBoolean(configValue);
        }

        private static Route MapRouteWithNamespace(string name, string area, string url, object defaults, object constraints, string[] namespaces)
        {
            var route = new Route(url, new DnnMvcPageRouteHandler())
            {
                Defaults = CreateRouteValueDictionaryUncached(defaults),
                Constraints = CreateRouteValueDictionaryUncached(constraints),
            };
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            route.DataTokens.Add("area", area);
            ConstraintValidation.Validate(route);
            if ((namespaces != null) && (namespaces.Length > 0))
            {
                route.SetNameSpaces(namespaces);
            }

            route.SetName(name);
            return route;
        }

        private static RouteValueDictionary CreateRouteValueDictionaryUncached(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            return dictionary != null ? new RouteValueDictionary(dictionary) : TypeHelper.ObjectToDictionary(values);
        }

        private void RegisterSystemRoutes()
        {
            var dataTokens = new RouteValueDictionary();
            var ns = new string[] { "DotNetNuke.Web.MvcWebsite.Controllers" };
            dataTokens["Namespaces"] = ns;

            var route = new Route(
                "DesktopModules/{controller}/{action}/{tabid}/{language}",
                new RouteValueDictionary(new { action = "Index", tabid = UrlParameter.Optional, language = UrlParameter.Optional }),
                null, // No constraints
                dataTokens,
                new DnnMvcPageRouteHandler());

            this.routes.Add(route);
        }

        private void LocateServicesAndMapRoutes()
        {
            this.RegisterSystemRoutes();
            this.ClearCachedRouteData();

            this.moduleUsage.Clear();
            foreach (var routeMapper in this.GetServiceRouteMappers())
            {
                try
                {
                    routeMapper.RegisterRoutes(this);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("{0}.RegisterRoutes threw an exception.  {1}\r\n{2}", routeMapper.GetType().FullName, e.Message, e.StackTrace);
                }
            }
        }

        private void ClearCachedRouteData()
        {
            this.portalAliasMvcRouteManager.ClearCachedData();
        }

        private IEnumerable<IMvcRouteMapper> GetServiceRouteMappers()
        {
            IEnumerable<Type> types = this.GetAllServiceRouteMapperTypes();

            foreach (var routeMapperType in types)
            {
                IMvcRouteMapper routeMapper;
                try
                {
                    routeMapper = Activator.CreateInstance(routeMapperType) as IMvcRouteMapper;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while registering service routes.  {1}", routeMapperType.FullName, e.Message);
                    routeMapper = null;
                }

                if (routeMapper != null)
                {
                    yield return routeMapper;
                }
            }
        }

        private IEnumerable<Type> GetAllServiceRouteMapperTypes()
        {
            return this.TypeLocator.GetAllMatchingTypes(IsValidServiceRouteMapper);
        }
    }
}
