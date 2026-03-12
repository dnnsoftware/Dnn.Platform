// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Caching;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Extension methods for <see langword="ViewEngineCollection"/>.</summary>
    public static partial class ViewEngineCollectionExt
    {
        /// <summary>Finds the specified view by using the specified controller context.</summary>
        /// <param name="viewEngineCollection">The view engine collection.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="masterName">The name of the master.</param>
        /// <param name="useCache"><see langword="true"/> to specify that the view engine returns the cached view, if a cached view exists; otherwise, <see langword="false"/>.</param>
        /// <returns>The page view.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial ViewEngineResult FindView(this ViewEngineCollection viewEngineCollection, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
            => viewEngineCollection.FindView(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), controllerContext, viewName, masterName, useCache);

        /// <summary>Finds the specified view by using the specified controller context.</summary>
        /// <param name="viewEngineCollection">The view engine collection.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="masterName">The name of the master.</param>
        /// <param name="useCache"><see langword="true"/> to specify that the view engine returns the cached view, if a cached view exists; otherwise, <see langword="false"/>.</param>
        /// <returns>The page view.</returns>
        public static ViewEngineResult FindView(this ViewEngineCollection viewEngineCollection, IHostSettings hostSettings, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            try
            {
                var cacheKey = CreateCacheKey(controllerContext, "View", viewName, masterName, (controllerContext.Controller as IDnnController)?.ModuleContext.PortalId ?? 0);
                var parameters = new object[]
                {
                    new Func<IViewEngine, ViewEngineResult>(e => e.FindView(controllerContext, viewName, masterName, false)),
                    false,
                };
                var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default, "Find", viewEngineCollection, parameters);

                return useCache ? CBO.GetCachedObject<ViewEngineResult>(hostSettings, cacheArg, CallFind) : CallFind(cacheArg);
            }
            catch (Exception)
            {
                return viewEngineCollection.FindView(controllerContext, viewName, masterName);
            }
        }

        /// <summary>Finds the specified partial view by using the specified controller context.</summary>
        /// <param name="viewEngineCollection">The view engine collection.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="partialViewName">The name of the partial view.</param>
        /// <param name="useCache"><see langword="true"/> to specify that the view engine returns the cached view, if a cached view exists; otherwise, <see langword="true"/>.</param>
        /// <returns>The partial view.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial ViewEngineResult FindPartialView(this ViewEngineCollection viewEngineCollection, ControllerContext controllerContext, string partialViewName, bool useCache)
            => viewEngineCollection.FindPartialView(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), controllerContext, partialViewName, useCache);

        public static ViewEngineResult FindPartialView(this ViewEngineCollection viewEngineCollection, IHostSettings hostSettings, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            try
            {
                var cacheKey = CreateCacheKey(controllerContext, "Partial", partialViewName, string.Empty, (controllerContext.Controller as IDnnController)?.ModuleContext.PortalId ?? 0);
                var parameters = new object[]
                {
                    new Func<IViewEngine, ViewEngineResult>(e => e.FindPartialView(controllerContext, partialViewName, false)),
                    false,
                };
                var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default, "Find", viewEngineCollection, parameters);

                return useCache ? CBO.GetCachedObject<ViewEngineResult>(hostSettings, cacheArg, CallFind) : CallFind(cacheArg);
            }
            catch (Exception)
            {
                return viewEngineCollection.FindPartialView(controllerContext, partialViewName);
            }
        }

        private static ViewEngineResult CallFind(CacheItemArgs cacheItem)
        {
            var factoryType = Reflection.CreateType("System.Web.Mvc.ViewEngineCollection");
            var name = cacheItem.Params[0] as string;
            var target = cacheItem.Params[1];
            var parameters = cacheItem.Params[2] as object[];
            var result = factoryType.InvokeMember(name, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, target, parameters, CultureInfo.InvariantCulture);
            return result as ViewEngineResult;
        }

        private static string CreateCacheKey(ControllerContext controllerContext, string section, string name, string areaName, int portalId)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}",
                ((string[])controllerContext.RouteData.DataTokens["namespaces"]).FirstOrDefault(),
                section,
                name,
                controllerContext.RouteData.Values["controller"],
                areaName,
                portalId);
        }
    }
}
