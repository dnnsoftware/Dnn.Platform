using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DotNetNuke.Framework;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework
{
    public static class ViewEngineCollectionExt
    {
        //Enable the call to ViewEngineCollection FindView method with useCache=false
        public static ViewEngineResult FindView(this ViewEngineCollection viewEngineCollection,
            ControllerContext controllerContext,
            string viewName, string masterName, bool useCache)
        {
            try
            {
                var cacheKey = CreateCacheKey(controllerContext, "View", viewName, masterName);
                var cachArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default,
                    "Find", viewEngineCollection,
                    new object[]
                    {
                        new Func<IViewEngine, ViewEngineResult>(
                            e => e.FindView(controllerContext, viewName, masterName, false)),
                        false
                    });

                return useCache ? CBO.GetCachedObject<ViewEngineResult>(cachArg, CallFind) : CallFind(cachArg);
            }
            catch (Exception)
            {
                return viewEngineCollection.FindView(controllerContext, viewName, masterName);
            }
        }

        //Enable the call to ViewEngineCollection FindPartialView method with useCache=false
        public static ViewEngineResult FindPartialView(this ViewEngineCollection viewEngineCollection,
            ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            try
            {
                var cacheKey = CreateCacheKey(controllerContext, "Partial", partialViewName, string.Empty);
                var cachArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default,
                    "Find", viewEngineCollection,
                    new object[]
                    {
                        new Func<IViewEngine, ViewEngineResult>(
                            e => e.FindPartialView(controllerContext, partialViewName, false)),
                        false
                    });

                return useCache ? CBO.GetCachedObject<ViewEngineResult>(cachArg, CallFind) : CallFind(cachArg);
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
            return
                factoryType.InvokeMember(name,
                    BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, target, parameters)
                    as ViewEngineResult;
        }

        private static string CreateCacheKey(ControllerContext controllerContext, string section, string name, string areaName)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:",
                ((string[])controllerContext.RouteData.DataTokens["namespaces"]).FirstOrDefault(), section, name, controllerContext.RouteData.Values["controller"], areaName);
        }
    }
}
