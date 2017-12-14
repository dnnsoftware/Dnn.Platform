#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Services.Localization;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Mvc.Common;
using System.Web.Http;

namespace DotNetNuke.Web.Mvc.Routing
{
    public sealed class MvcRoutingManager : IMapRoute
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (MvcRoutingManager));
        private readonly Dictionary<string, int> _moduleUsage = new Dictionary<string, int>();
        private readonly RouteCollection _routes;
        private readonly PortalAliasMvcRouteManager _portalAliasMvcRouteManager;

        public MvcRoutingManager() : this(RouteTable.Routes)
        {
        }

        internal MvcRoutingManager(RouteCollection routes)
        {
            _routes = routes;
            _portalAliasMvcRouteManager = new PortalAliasMvcRouteManager();
            TypeLocator = new TypeLocator();
        }

           internal ITypeLocator TypeLocator { get; set; }

        #region IMapRoute Members

        public Route MapRoute(string moduleFolderName, string routeName, string url, string[] namespaces)
        {
            return MapRoute(moduleFolderName, routeName, url, null /* defaults */, null /* constraints */, namespaces);
        }

        public Route MapRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces)
        {
            return MapRoute(moduleFolderName, routeName, url, defaults, null /* constraints */, namespaces);
        }

        public Route MapRoute(string moduleFolderName, string routeName, string url, object defaults, object constraints, string[] namespaces)
        {
            if (namespaces == null || namespaces.Length == 0 || String.IsNullOrEmpty(namespaces[0]))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty",
                                                                             "The argument '{0}' cannot be null or empty.",
                                                                             "namespaces"));
            }

            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            url = url.Trim('/', '\\');

            var prefixCounts = _portalAliasMvcRouteManager.GetRoutePrefixCounts();
            Route route = null;

            if (url == null)
                throw new ArgumentNullException(nameof(url));

            foreach (var count in prefixCounts)
            {
                var fullRouteName = _portalAliasMvcRouteManager.GetRouteName(moduleFolderName, routeName, count);
                var routeUrl = _portalAliasMvcRouteManager.GetRouteUrl(moduleFolderName, url, count);
                route = MapRouteWithNamespace(fullRouteName, routeUrl, defaults, constraints, namespaces);
                _routes.Add(route);
                Logger.Trace("Mapping route: " + fullRouteName + " @ " + routeUrl);
            }

            return route;
        }

        #endregion

        public void RegisterRoutes()
        {
            //add standard tab and module id provider
            GlobalConfiguration.Configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            using (_routes.GetWriteLock())
            {
                //_routes.Clear(); -- don't use; it will remove original WEP API maps
                LocateServicesAndMapRoutes();
            }
            Logger.TraceFormat("Registered a total of {0} routes", _routes.Count);
        }

        private static bool IsTracingEnabled()
        {
            var configValue = Config.GetSetting("EnableServicesFrameworkTracing");

            return !string.IsNullOrEmpty(configValue) && Convert.ToBoolean(configValue);
        }

        private void LocateServicesAndMapRoutes()
        {
            RegisterSystemRoutes();
            ClearCachedRouteData();

            _moduleUsage.Clear();
            foreach (var routeMapper in GetServiceRouteMappers())
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
            _portalAliasMvcRouteManager.ClearCachedData();
        }

        private static void RegisterSystemRoutes()
        {
            //_routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        private IEnumerable<IMvcRouteMapper> GetServiceRouteMappers()
        {
            IEnumerable<Type> types = GetAllServiceRouteMapperTypes();

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
            return TypeLocator.GetAllMatchingTypes(IsValidServiceRouteMapper);
        }

        internal static bool IsValidServiceRouteMapper(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IMvcRouteMapper).IsAssignableFrom(t);
        }

        private static Route MapRouteWithNamespace(string name, string url, object defaults, object constraints, string[] namespaces)
        {
            var route = new Route(url, new DnnMvcRouteHandler())
            {
                Defaults = CreateRouteValueDictionaryUncached(defaults),
                Constraints = CreateRouteValueDictionaryUncached(constraints)
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
    }
}