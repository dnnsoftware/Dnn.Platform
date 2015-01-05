#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
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

using System.Web.Mvc;
using System.Web.Routing;

namespace DotNetNuke.Web.Mvc.Routing
{
    public static class RouteCollectionExtensions
    {
        private const string DefaultControllerName = "Home";
        private const string DefaultActionName = "Index";
        private const string DefaultId = "";
        private const string DefaultRouteName = "Default";

        public static void RegisterDefaultRoute(this RouteCollection routes, string controllerNamespace)
        {
            routes.RegisterDefaultRoute(DefaultControllerName, DefaultActionName, DefaultId, new[] { controllerNamespace });
        }

        public static void RegisterDefaultRoute(this RouteCollection routes, string[] namespaces)
        {
            routes.RegisterDefaultRoute(DefaultControllerName, DefaultActionName, DefaultId, namespaces);
        }

        public static void RegisterDefaultRoute(this RouteCollection routes, string defaultController, string[] namespaces)
        {
            routes.RegisterDefaultRoute(defaultController, DefaultActionName, DefaultId, namespaces);
        }

        public static void RegisterDefaultRoute(this RouteCollection routes, string defaultController, string defaultAction, string[] namespaces)
        {
            routes.RegisterDefaultRoute(defaultController, defaultAction, DefaultId, namespaces);
        }

        public static void RegisterDefaultRoute(this RouteCollection routes, string defaultController, string defaultAction, string defaultId, string[] namespaces)
        {
            routes.RegisterDefaultRoute(DefaultRouteName, defaultController, defaultAction, defaultId, namespaces);
        }

        public static void RegisterDefaultRoute(this RouteCollection routes, string routeName, string defaultController, string defaultAction, string defaultId, string[] namespaces)
        {
            routes.MapRoute(
                routeName,
                "{controller}/{action}/{id}",
                new { controller = defaultController, action = defaultAction, id = defaultId },
                new {moduleId = @"[0-9]*"},
                namespaces
                );
        }
    }
}