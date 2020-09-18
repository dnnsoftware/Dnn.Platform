// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Mvc.Common;

    public sealed class MvcRoutingManager : IMapRoute
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MvcRoutingManager));
        private readonly Dictionary<string, int> _moduleUsage = new Dictionary<string, int>();
        private readonly RouteCollection _routes;
        private readonly PortalAliasMvcRouteManager _portalAliasMvcRouteManager;

        public MvcRoutingManager()
            : this(RouteTable.Routes)
        {
        }

        internal MvcRoutingManager(RouteCollection routes)
        {
            this._routes = routes;
            this._portalAliasMvcRouteManager = new PortalAliasMvcRouteManager();
            this.TypeLocator = new TypeLocator();
        }

        internal ITypeLocator TypeLocator { get; set; }

        public Route MapRoute(string moduleFolderName, string routeName, string url, string[] namespaces)
        {
            return this.MapRoute(moduleFolderName, routeName, url, null /* defaults */, null /* constraints */, namespaces);
        }

        public Route MapRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces)
        {
            return this.MapRoute(moduleFolderName, routeName, url, defaults, null /* constraints */, namespaces);
        }

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

            var prefixCounts = this._portalAliasMvcRouteManager.GetRoutePrefixCounts();
            Route route = null;

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            foreach (var count in prefixCounts)
            {
                var fullRouteName = this._portalAliasMvcRouteManager.GetRouteName(moduleFolderName, routeName, count);
                var routeUrl = this._portalAliasMvcRouteManager.GetRouteUrl(moduleFolderName, url, count);
                route = MapRouteWithNamespace(fullRouteName, routeUrl, defaults, constraints, namespaces);
                this._routes.Add(route);
                Logger.Trace("Mapping route: " + fullRouteName + " @ " + routeUrl);
            }

            return route;
        }

        public void RegisterRoutes()
        {
            // add standard tab and module id provider
            GlobalConfiguration.Configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            using (this._routes.GetWriteLock())
            {
                // _routes.Clear(); -- don't use; it will remove original WEP API maps
                this.LocateServicesAndMapRoutes();
            }

            Logger.TraceFormat("Registered a total of {0} routes", this._routes.Count);
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

        private static void RegisterSystemRoutes()
        {
            // _routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        private static Route MapRouteWithNamespace(string name, string url, object defaults, object constraints, string[] namespaces)
        {
            var route = new Route(url, new DnnMvcRouteHandler())
            {
                Defaults = CreateRouteValueDictionaryUncached(defaults),
                Constraints = CreateRouteValueDictionaryUncached(constraints),
            };
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

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

        private void LocateServicesAndMapRoutes()
        {
            RegisterSystemRoutes();
            this.ClearCachedRouteData();

            this._moduleUsage.Clear();
            foreach (var routeMapper in this.GetServiceRouteMappers())
            {
                try
                {
                    routeMapper.RegisterRoutes(this);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("{0}.RegisterRoutes threw an exception.  {1}\r\n{2}", routeMapper.GetType().FullName,
                                 e.Message, e.StackTrace);
                }
            }
        }

        private void ClearCachedRouteData()
        {
            this._portalAliasMvcRouteManager.ClearCachedData();
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
                    Logger.ErrorFormat("Unable to create {0} while registering service routes.  {1}", routeMapperType.FullName,
                                 e.Message);
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
