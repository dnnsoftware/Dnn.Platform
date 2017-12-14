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
using System.Web.Http.Routing;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Api
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generate WebAPI Links compatible with DNN Services Framework
        /// </summary>
        /// <param name="urlHelper">The UrlHelper</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route</param>
        /// <param name="routeName">RouteName for the route</param>
        /// <param name="routeValues">Values to be passed to the route</param>
        /// <returns>a url</returns>
        public static string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues)
        {
            return DnnLink(urlHelper, moduleFolderName, routeName, routeValues, PortalController.Instance.GetCurrentPortalSettings().PortalAlias);
        }

        /// <summary>
        /// Generate WebAPI Links compatible with DNN Services Framework
        /// </summary>
        /// <param name="urlHelper">The UrlHelper</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route</param>
        /// <param name="routeName">RouteName for the route</param>
        /// <param name="routeValues">Values to be passed to the route</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the Url </param>
        /// <returns>a url</returns>
        public static string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues, PortalAliasInfo portalAliasInfo)
        {
            var parm = new PortalAliasRouteManager();
            var fullName = parm.GetRouteName(moduleFolderName, routeName, PortalController.Instance.GetCurrentPortalSettings().PortalAlias);
            HttpRouteValueDictionary allRouteValues = parm.GetAllRouteValues(portalAliasInfo, routeValues);

            return urlHelper.Link(fullName, allRouteValues);
        }
    }
}