#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
            //register routes is ONLY called from within DNN application initialization
            //which is well protected from races
            //allowing us to not worry about multi-threading threats here
            //if (!HttpConfiguration.Configuration.MessageHandlers.Any(x => x is BasicAuthMessageHandler))
            {
                ////Everything in this block is run one time at startup

                ////dnnContext message handler
                ////this must run before any auth message handlers
                //HttpConfiguration.Configuration.MessageHandlers.Add(new DnnContextMessageHandler());

                //RegisterAuthenticationHandlers();
                ////this must run after aall other auth message handlers
                //var handler = new WebFormsAuthMessageHandler();
                //HttpConfiguration.Configuration.MessageHandlers.Add(handler);
                //DnnAuthorizeAttribute.AppendToDefaultAuthTypes(handler.AuthScheme);

                ////media type formatter for text/html, text/plain
                //HttpConfiguration.Configuration.Formatters.Add(new StringPassThroughMediaTypeFormatter());

                ////controller selector that respects namespaces
                //HttpConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new DnnHttpControllerSelector(HttpConfiguration.Configuration));

                ////tracwriter for dotnetnuke.instrumentation
                //HttpConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new TraceWriter(IsTracingEnabled()));

                ////replace the default action filter provider with our own
                //HttpConfiguration.Configuration.Services.Add(typeof(IFilterProvider), new DnnActionFilterProvider());
                //var defaultprovider = HttpConfiguration.Configuration.Services.GetFilterProviders().Where(x => x is ActionDescriptorFilterProvider);
                //HttpConfiguration.Configuration.Services.Remove(typeof(IFilterProvider), defaultprovider);

                //add standard tab and module id provider
                //HttpConfiguration .AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            }
            GlobalConfiguration.Configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            using (_routes.GetWriteLock())
            {
                //_routes.Clear();
                LocateServicesAndMapRoutes();
            }
            Logger.TraceFormat("Registered a total of {0} routes", _routes.Count);
        }

        //private static void RegisterAuthenticationHandlers()
        //{
        //    //authentication message handlers from web.config file
        //    var authSvcCfg = AuthServicesConfiguration.GetConfig();
        //    if (authSvcCfg?.MessageHandlers == null || authSvcCfg.MessageHandlers.Count <= 0)
        //        return;

        //    var registeredSchemes = new List<string>();
        //    foreach (var handlerEntry in authSvcCfg.MessageHandlers.Cast<MessageHandlerEntry>())
        //    {
        //        if (!handlerEntry.Enabled)
        //        {
        //            Logger.Trace("The following handler is disabled " + handlerEntry.ClassName);
        //            continue;
        //        }

        //        try
        //        {
        //            var type = Reflection.CreateType(handlerEntry.ClassName, false);
        //            var handler = Activator.CreateInstance(type, handlerEntry.DefaultInclude, handlerEntry.ForceSsl) as AuthMessageHandlerBase;
        //            if (handler == null)
        //            {
        //                throw new Exception("The handler is not a descendant of AuthMessageHandlerBase abstract class");
        //            }

        //            var schemeName = handler.AuthScheme.ToUpperInvariant();
        //            if (registeredSchemes.Contains(schemeName))
        //            {
        //                Logger.Trace($"The following handler scheme '{handlerEntry.ClassName}' is already added and will be skipped");
        //                continue;
        //            }

        //            HttpConfiguration.Configuration.MessageHandlers.Add(handler);
        //            registeredSchemes.Add(schemeName);
        //            Logger.Trace($"Instantiated/Activated instance of {handler.AuthScheme}, class: {handler.GetType().FullName}");

        //            if (handlerEntry.DefaultInclude)
        //            {
        //                DnnAuthorizeAttribute.AppendToDefaultAuthTypes(handler.AuthScheme);
        //            }
        //            if (handler.BypassAntiForgeryToken)
        //            {
        //                ValidateAntiForgeryTokenAttribute.AppendToBypassAuthTypes(handler.AuthScheme);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error("Cannot instantiate/activate instance of " + handlerEntry.ClassName + Environment.NewLine + ex);
        //        }
        //    }
        //}

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