// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Api.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Filters;
    using System.Web.Http.Tracing;
    using System.Web.Routing;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api.Auth;
    using DotNetNuke.Web.Api.Internal.Auth;
    using DotNetNuke.Web.ConfigSection;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Allows registering web API routes.</summary>
    public sealed class ServicesRoutingManager : IMapRoute, IRoutingManager
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesRoutingManager));
        private readonly IServiceProvider serviceProvider;
        private readonly RouteCollection routes;
        private readonly PortalAliasRouteManager portalAliasRouteManager;

        /// <summary>Initializes a new instance of the <see cref="ServicesRoutingManager"/> class.</summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalAliasService. Scheduled removal in v12.0.0.")]
        public ServicesRoutingManager(IServiceProvider serviceProvider)
            : this(serviceProvider, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServicesRoutingManager"/> class.</summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        public ServicesRoutingManager(IServiceProvider serviceProvider, IPortalAliasService portalAliasService)
            : this(serviceProvider, portalAliasService, RouteTable.Routes)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServicesRoutingManager"/> class.</summary>
        /// <param name="serviceProvider">The dependency injection container.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="routes">The route collection.</param>
        internal ServicesRoutingManager(IServiceProvider serviceProvider, IPortalAliasService portalAliasService, RouteCollection routes)
        {
            this.serviceProvider = serviceProvider;
            this.routes = routes;
            portalAliasService ??= Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
            this.portalAliasRouteManager = new PortalAliasRouteManager(portalAliasService);
            this.TypeLocator = new TypeLocator();
        }

        /// <summary>Gets or sets the type locator.</summary>
        internal ITypeLocator TypeLocator { get; set; }

        /// <inheritdoc />
        public IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, object constraints, string[] namespaces)
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

            var prefixCounts = this.portalAliasRouteManager.GetRoutePrefixCounts();
            var mappedRoutes = new List<Route>();

            foreach (var count in prefixCounts)
            {
                var fullRouteName = this.portalAliasRouteManager.GetRouteName(moduleFolderName, routeName, count);
                var routeUrl = this.portalAliasRouteManager.GetRouteUrl(moduleFolderName, url, count);
                var route = this.MapHttpRouteWithNamespace(fullRouteName, routeUrl, defaults, constraints, namespaces);
                mappedRoutes.Add(route);
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Mapping route: " + fullRouteName + " @ " + routeUrl);
                }

                // compatible with old service path: DesktopModules/{namespace}/API/{controller}/{action}.
                var oldRouteName = $"{fullRouteName}-old";

#pragma warning disable CS0618 // Type or member is obsolete
                var oldRouteUrl = PortalAliasRouteManager.GetOldRouteUrl(moduleFolderName, url, count);
#pragma warning restore CS0618 // Type or member is obsolete

                var oldRoute = this.MapHttpRouteWithNamespace(oldRouteName, oldRouteUrl, defaults, constraints, namespaces);
                mappedRoutes.Add(oldRoute);
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Mapping route: " + oldRouteName + " @ " + oldRouteUrl);
                }
            }

            return mappedRoutes;
        }

        /// <inheritdoc />
        public IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces)
        {
            return this.MapHttpRoute(moduleFolderName, routeName, url, defaults, null, namespaces);
        }

        /// <inheritdoc />
        public IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, string[] namespaces)
        {
            return this.MapHttpRoute(moduleFolderName, routeName, url, null, null, namespaces);
        }

        /// <inheritdoc />
        public void RegisterRoutes()
        {
            // register routes is ONLY called from within DNN application initialization
            // which is well protected from races
            // allowing us to not worry about multi-threading threats here
            if (!GlobalConfiguration.Configuration.MessageHandlers.Any(x => x is BasicAuthMessageHandler))
            {
                // Everything in this block is run one time at startup

                // dnnContext message handler
                // this must run before any auth message handlers
                GlobalConfiguration.Configuration.MessageHandlers.Add(new DnnContextMessageHandler());

                this.RegisterAuthenticationHandlers();

                // this must run after all other auth message handlers
                var handler = new WebFormsAuthMessageHandler();
                GlobalConfiguration.Configuration.MessageHandlers.Add(handler);
                DnnAuthorizeAttribute.AppendToDefaultAuthTypes(handler.AuthScheme);

                // Add Windows Authentication type to make API request works when windows authentication enabled.
                DnnAuthorizeAttribute.AppendToDefaultAuthTypes("Negotiate");

                // media type formatter for text/html, text/plain
                GlobalConfiguration.Configuration.Formatters.Add(new StringPassThroughMediaTypeFormatter());

                // controller selector that respects namespaces
                GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new DnnHttpControllerSelector(GlobalConfiguration.Configuration));
                GlobalConfiguration.Configuration.DependencyResolver = new DnnDependencyResolver(Globals.DependencyProvider);

                // trace writer for dotnetnuke.instrumentation
                GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new TraceWriter(IsTracingEnabled()));

                // replace the default action filter provider with our own
                GlobalConfiguration.Configuration.Services.Add(typeof(IFilterProvider), Globals.DependencyProvider.GetRequiredService<IFilterProvider>());
                var defaultProvider = GlobalConfiguration.Configuration.Services.GetFilterProviders().Where(x => x is ActionDescriptorFilterProvider);
                GlobalConfiguration.Configuration.Services.Remove(typeof(IFilterProvider), defaultProvider);

                // add standard tab and module id provider
                GlobalConfiguration.Configuration.AddTabAndModuleInfoProvider(new StandardTabAndModuleInfoProvider());
            }

            using (this.routes.GetWriteLock())
            {
                this.routes.Clear();
                this.LocateServicesAndMapRoutes();
            }

            Logger.TraceFormat(CultureInfo.InvariantCulture, "Registered a total of {0} routes", this.routes.Count);
        }

        /// <summary>Determines whether the given <paramref name="type"/> is a valid <see cref="IServiceRouteMapper"/>.</summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="type"/> is an accessible concrete class, otherwise <see langword="false"/>.</returns>
        internal static bool IsValidServiceRouteMapper(Type type)
        {
            return type is { IsClass: true, IsAbstract: false, IsVisible: true, } && typeof(IServiceRouteMapper).IsAssignableFrom(type);
        }

        private static bool IsTracingEnabled()
        {
            var configValue = Config.GetSetting("EnableServicesFrameworkTracing");

            if (!string.IsNullOrEmpty(configValue))
            {
                return Convert.ToBoolean(configValue);
            }

            return false;
        }

        private static void RegisterSystemRoutes()
        {
            ////_routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        private void RegisterAuthenticationHandlers()
        {
            // authentication message handlers from web.config file
            var authSvcCfg = AuthServicesConfiguration.GetConfig();
            if (authSvcCfg?.MessageHandlers is not { Count: > 0 })
            {
                return;
            }

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
                    if (ActivatorUtilities.CreateInstance(this.serviceProvider, type, handlerEntry.DefaultInclude, handlerEntry.ForceSsl) is not AuthMessageHandlerBase handler)
                    {
                        throw new InvalidAuthHandlerException("The handler is not a descendant of AuthMessageHandlerBase abstract class");
                    }

                    var schemeName = handler.AuthScheme.ToUpperInvariant();
                    if (registeredSchemes.Contains(schemeName))
                    {
                        Logger.Trace($"The following handler scheme '{handlerEntry.ClassName}' is already added and will be skipped");
                        handler.Dispose();
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

        private void LocateServicesAndMapRoutes()
        {
            RegisterSystemRoutes();
            this.ClearCachedRouteData();

            using var serviceScope = Globals.DependencyProvider.CreateScope();
            foreach (var routeMapper in this.GetServiceRouteMappers(serviceScope.ServiceProvider))
            {
                try
                {
                    routeMapper.RegisterRoutes(this);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(CultureInfo.InvariantCulture, "{0}.RegisterRoutes threw an exception.  {1}\r\n{2}", routeMapper.GetType().FullName, e.Message, e.StackTrace);
                }
            }
        }

        private void ClearCachedRouteData()
        {
            this.portalAliasRouteManager.ClearCachedData();
        }

        private IEnumerable<IServiceRouteMapper> GetServiceRouteMappers(IServiceProvider scopedServiceProvider)
        {
            foreach (var routeMapperType in this.GetAllServiceRouteMapperTypes())
            {
                IServiceRouteMapper routeMapper;
                try
                {
                    routeMapper = ActivatorUtilities.CreateInstance(scopedServiceProvider, routeMapperType) as IServiceRouteMapper;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(CultureInfo.InvariantCulture, "Unable to create {0} while registering service routes.  {1}", routeMapperType.FullName, e.Message);
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

        private Route MapHttpRouteWithNamespace(string name, string url, object defaults, object constraints, string[] namespaces)
        {
            var route = this.routes.MapHttpRoute(name, url, defaults, constraints);
            if (route.DataTokens == null)
            {
                route.DataTokens = new RouteValueDictionary();
            }

            route.SetNameSpaces(namespaces);
            route.SetName(name);
            return route;
        }
    }
}
