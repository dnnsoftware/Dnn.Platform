// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Web.Http.Routing;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Extension methods on <see cref="UrlHelper"/> for generating web API links.</summary>
    public static partial class UrlHelperExtensions
    {
        /// <summary>Generate WebAPI Links compatible with DNN Services Framework.</summary>
        /// <param name="urlHelper">The <see cref="UrlHelper"/>.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <returns>a URL.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IPortalAliasService")]
        public static partial string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues)
            => urlHelper.DnnLink(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), moduleFolderName, routeName, routeValues);

        /// <summary>Generate WebAPI Links compatible with DNN Services Framework.</summary>
        /// <param name="urlHelper">The <see cref="UrlHelper"/>.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <returns>a URL.</returns>
        public static string DnnLink(this UrlHelper urlHelper, IPortalAliasService portalAliasService, string moduleFolderName, string routeName, object routeValues)
            => urlHelper.DnnLink(portalAliasService, moduleFolderName, routeName, routeValues, (IPortalAliasInfo)PortalSettings.Current.PortalAlias);

        /// <summary>Generate WebAPI Links compatible with DNN Services Framework.</summary>
        /// <param name="urlHelper">The <see cref="UrlHelper"/>.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the URL.</param>
        /// <returns>a URL.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IPortalAliasService")]
        public static partial string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues, PortalAliasInfo portalAliasInfo)
            => urlHelper.DnnLink(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), moduleFolderName, routeName, routeValues, portalAliasInfo);

        /// <summary>Generate WebAPI Links compatible with DNN Services Framework.</summary>
        /// <param name="urlHelper">The <see cref="UrlHelper"/>.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the URL.</param>
        /// <returns>a URL.</returns>
        public static string DnnLink(this UrlHelper urlHelper, IPortalAliasService portalAliasService, string moduleFolderName, string routeName, object routeValues, PortalAliasInfo portalAliasInfo)
            => urlHelper.DnnLink(portalAliasService, moduleFolderName, routeName, routeValues, (IPortalAliasInfo)portalAliasInfo);

        /// <summary>Generate WebAPI Links compatible with DNN Services Framework.</summary>
        /// <param name="urlHelper">The UrlHelper.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the URL.</param>
        /// <returns>a URL.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IPortalAliasService")]
        public static partial string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues, IPortalAliasInfo portalAliasInfo)
            => urlHelper.DnnLink(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), moduleFolderName, routeName, routeValues, portalAliasInfo);

        /// <summary>Generate WebAPI Links compatible with DNN Services Framework.</summary>
        /// <param name="urlHelper">The <see cref="UrlHelper"/>.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the URL.</param>
        /// <returns>a URL.</returns>
        public static string DnnLink(this UrlHelper urlHelper, IPortalAliasService portalAliasService, string moduleFolderName, string routeName, object routeValues, IPortalAliasInfo portalAliasInfo)
        {
            var parm = new PortalAliasRouteManager(portalAliasService);
            var fullName = parm.GetRouteName(moduleFolderName, routeName, (IPortalAliasInfo)PortalSettings.Current.PortalAlias);
            var allRouteValues = PortalAliasRouteManager.GetAllRouteValues(portalAliasInfo, routeValues);

            return urlHelper.Link(fullName, allRouteValues);
        }
    }
}
