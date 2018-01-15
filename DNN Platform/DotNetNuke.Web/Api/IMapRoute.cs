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
using System.Web.Routing;

namespace DotNetNuke.Web.Api
{
    public interface IMapRoute
    {
        /// <summary>
        /// Sets up the route(s) for DotNetNuke services
        /// </summary>
        /// <param name="moduleFolderName">The name of the folder under DesktopModules in which your module resides</param>
        /// <param name="routeName">A unique name for the route</param>
        /// <param name="url">The parameterized portion of the route</param>
        /// <param name="defaults">Default values for the route parameters</param>
        /// <param name="constraints">The constraints</param>
        /// <param name="namespaces">The namespace(s) in which to search for the controllers for this route</param>
        /// <returns>A list of all routes that were registered.</returns>
        /// <remarks>The combination of moduleFolderName and routeName must be unique for each route</remarks>
        IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, object constraints, string[] namespaces);

        /// <summary>
        /// Sets up the route(s) for DotNetNuke services
        /// </summary>
        /// <param name="moduleFolderName">The name of the folder under DesktopModules in which your module resides</param>
        /// <param name="routeName">A unique name for the route</param>
        /// <param name="url">The parameterized portion of the route</param>
        /// <param name="defaults">Default values for the route parameters</param>
        /// <param name="namespaces">The namespace(s) in which to search for the controllers for this route</param>
        /// <returns>A list of all routes that were registered.</returns>
        /// <remarks>The combination of moduleFolderName and routeName must be unique for each route</remarks>
        IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces);

        /// <summary>
        /// Sets up the route(s) for DotNetNuke services
        /// </summary>
        /// <param name="moduleFolderName">The name of the folder under DesktopModules in which your module resides</param>
        /// <param name="routeName">A unique name for the route</param>
        /// <param name="url">The parameterized portion of the route</param>
        /// <param name="namespaces">The namespace(s) in which to search for the controllers for this route</param>
        /// <returns>A list of all routes that were registered.</returns>
        /// <remarks>The combination of moduleFolderName and routeName must be unique for each route</remarks> 
        IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, string[] namespaces);
    }
}