#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Tracing;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api.Auth;
using DotNetNuke.Web.Api.Internal.Auth;
using DotNetNuke.Web.ConfigSection;

namespace DotNetNuke.Web.Api.Internal
{
    public sealed class ServicesRoutingManager : IMapRoute
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ServicesRoutingManager));
        private readonly Dictionary<string, int> _moduleUsage = new Dictionary<string, int>();
        private readonly RouteCollection _routes;
        private readonly PortalAliasRouteManager _portalAliasRouteManager;

        public ServicesRoutingManager() : this(RouteTable.Routes)
        {
        }

        internal ServicesRoutingManager(RouteCollection routes)
        {
            _routes = routes;
            _portalAliasRouteManager = new PortalAliasRouteManager();
            TypeLocator = new TypeLocator();
        }

        internal ITypeLocator TypeLocator { get; set; }

        #region IMapRoute Members

        public IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, object constraints, string[] namespaces)
        {
            if (namespaces == null || namespaces.Length == 0 || String.IsNullOrEmpty(namespaces[0]))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("ArgumentCannotBeNullOrEmpty",
                                                                             "The argument '{0}' cannot be null or empty.",
                                                                             "namespaces"));
            }

            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

            url = url.Trim('/', '\\');

            IEnumerable<int> prefixCounts = _portalAliasRouteManager.GetRoutePrefixCounts();
            var routes = new List<Route>();

            foreach (int count in prefixCounts)
            {
                string fullRouteName = _portalAliasRouteManager.GetRouteName(moduleFolderName, routeName, count);
                string routeUrl = _portalAliasRouteManager.GetRouteUrl(moduleFolderName, url, count);
                Route route = MapHttpRouteWithNamespace(fullRouteName, routeUrl, defaults, constraints, namespaces);
                routes.Add(route);
                if (Logger.IsTraceEnabled)
                    Logger.Trace("Mapping route: " + fullRouteName + " @ " + routeUrl);

                //compatible with old service path: DesktopModules/{namespace}/API/{controller}/{action}.
                var oldRouteName = $"{fullRouteName}-old";
                var oldRouteUrl = PortalAliasRouteManager.GetOldRouteUrl(moduleFolderName, url, count);
                var oldRoute = MapHttpRouteWithNamespace(oldRouteName, oldRouteUrl, defaults, constraints, namespaces);
                routes.Add(oldRoute);
                if (Logger.IsTraceEnabled)
                    Logger.Trace("Mapping route: " + oldRouteName + " @ " + oldRouteUrl);
            }

            return routes;
        }

        public IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces)
        {
            return MapHttpRoute(moduleFolderName, routeName, url, defaults, null, namespaces);
        }

        public IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, string[] namespaces)
        {
            return MapHttpRoute(moduleFolderName, routeName, url, null, null, namespaces);
        }

        #endregion

        public void RegisterRoutes()
        {
            //register routes is ONLY called from within DNN application initialization
            //which is well protected from races
            //allowing us to not worry about multi-threading threats here
            if (!GlobalConfiguration.Configuration.MessageHandlers.Any(x => x is BasicAuthMessageHandler))
            {
                //Everything in this block is run one time at startup

                //dnnContext message handler
                //this must run before any auth message handlers
                GlobalConfiguration.Configuration.MessageHandlers.Add(new DnnContextMessageHandler());

                RegisterAuthenticationHandlers();
                //this must run after all other auth message handlers
                var handler = new WebFormsAuthMessageHandler();
                GlobalConfiguration.Configuration.MessageHandlers.Add(handler);
                DnnAuthorizeAttribute.AppendToDefaultAuthTypes(handler.AuthScheme);
                //Add Windows Authentication type to make API request works when windows authentication enabled.
                DnnAuthorizeAttribute.AppendToDefaultAuthTypes("Negotiate");

                //media type formatter for text/html, text/plain
                GlobalConfiguration.Configuration.Formatters.Add(new StringPassThroughMediaTypeFormatter());

                //controller selector that respects namespaces
                GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new DnnHttpControllerSelector(GlobalConfiguration.Configuration));
                GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new DnnHttpControllerActivator());

                //tracwriter for dotnetnuke.instrumentation
                GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new TraceWriter(IsTracingEnabled()));

                //replace the default action filter provider with our own
                GlobalConfiguration.Configuration.Services.Add(typeof(IFilterProvider), new DnnActionFilterProvider());
                var defaultprovider = GlobalConfiguration.Configuration.Services.GetFilterProviders().Where(x => x is ActionDescriptorFilterProvider);
                GlobalConfiguration.Configuration.Services.Remove(typeof(IFilterProvider), defaultprovider);

                //add standard tab and module id provider
                GlobalConfiguration.Configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            }

            using (_routes.GetWriteLock())
            {
                _routes.Clear();
                LocateServicesAndMapRoutes();
            }
            Logger.TraceFormat("Registered a total of {0} routes", _routes.Count);
        }

        private static void RegisterAuthenticationHandlers()
        {
            //authentication message handlers from web.config file
            var authSvcCfg = AuthServicesConfiguration.GetConfig();
            if (authSvcCfg?.MessageHandlers == null || authSvcCfg.MessageHandlers.Count <= 0)
                return;

            var registeredSchemes = new List<string>();
            foreach (var handlerEntry in authSvcCfg.MessageHandlers.Cast<MessageHandlerEntry>())
            {
                if (!handlerEntry.Enabled)
                {
                    Logger.Trace("The following handler is disabled " + handlerEntry.ClassName);
                    continue;
                }

                try
                {
                    var type = Reflection.CreateType(handlerEntry.ClassName, false);
                    var handler = Activator.CreateInstance(type, handlerEntry.DefaultInclude, handlerEntry.ForceSsl) as AuthMessageHandlerBase;
                    if (handler == null)
                    {
                        throw new Exception("The handler is not a descendant of AuthMessageHandlerBase abstract class");
                    }

                    var schemeName = handler.AuthScheme.ToUpperInvariant();
                    if (registeredSchemes.Contains(schemeName))
                    {
                        Logger.Trace($"The following handler scheme '{handlerEntry.ClassName}' is already added and will be skipped");
                        continue;
                    }

                    GlobalConfiguration.Configuration.MessageHandlers.Add(handler);
                    registeredSchemes.Add(schemeName);
                    Logger.Trace($"Instantiated/Activated instance of {handler.AuthScheme}, class: {handler.GetType().FullName}");

                    if (handlerEntry.DefaultInclude)
                    {
                        DnnAuthorizeAttribute.AppendToDefaultAuthTypes(handler.AuthScheme);
                    }
                    if (handler.BypassAntiForgeryToken)
                    {
                        ValidateAntiForgeryTokenAttribute.AppendToBypassAuthTypes(handler.AuthScheme);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Cannot instantiate/activate instance of " + handlerEntry.ClassName + Environment.NewLine + ex);
                }
            }
        }

        private static bool IsTracingEnabled()
        {
            var configValue = Config.GetSetting("EnableServicesFrameworkTracing");

            if(!string.IsNullOrEmpty(configValue))
            {
                return Convert.ToBoolean(configValue);
            }

            return false;
        }

        private void LocateServicesAndMapRoutes()
        {
            RegisterSystemRoutes();
            ClearCachedRouteData();

            _moduleUsage.Clear();
            foreach (IServiceRouteMapper routeMapper in GetServiceRouteMappers())
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
            _portalAliasRouteManager.ClearCachedData();
        }

        private void RegisterSystemRoutes()
        {
            //_routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        private IEnumerable<IServiceRouteMapper> GetServiceRouteMappers()
        {
            IEnumerable<Type> types = GetAllServiceRouteMapperTypes();

            foreach (Type routeMapperType in types)
            {
                IServiceRouteMapper routeMapper;
                try
                {
                    routeMapper = Activator.CreateInstance(routeMapperType) as IServiceRouteMapper;
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
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof (IServiceRouteMapper).IsAssignableFrom(t);
        }

        private Route MapHttpRouteWithNamespace(string name, string url, object defaults, object constraints, string[] namespaces)
        {
            Route route = _routes.MapHttpRoute(name, url, defaults, constraints);
            
            if(route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            route.SetNameSpaces(namespaces);
            route.SetName(name);
            return route;
        }
    }
}